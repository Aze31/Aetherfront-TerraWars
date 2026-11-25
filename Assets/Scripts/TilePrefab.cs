using UnityEngine;
using static CardLibrary;

public class TilePrefab : MonoBehaviour
{
    //bottom left is 1,1 and top right is 5,5
    public int x;
    public int y;

    public GameObject sprite;
    public GameObject tileSquare;
    private BattlefieldManager manager;
    private bool isMouseOver;
    private SpriteRenderer unitSpriteRenderer;

    private void Start()
    {
        manager = BattlefieldManager.getInstance();
    }

    private void OnMouseEnter()
    {
        isMouseOver = true;
    }

    private void OnMouseExit()
    {
        isMouseOver = false;
    }

    private bool nextToTerrain()
    {
        if (manager == null)
        {
            return false;
        }

        Tile[,] battlefield = BattlefieldManager.battleField;
        Tile target = new Tile(x, y);
        for (int tileX = 1; tileX < battlefield.GetLength(0); tileX++)
        {
            for (int tileY = 1; tileY < battlefield.GetLength(1); tileY++)
            {
                Tile existingTile = battlefield[tileX, tileY];
                if (existingTile != null &&
                    existingTile.curTerrain != null &&
                    manager.checkifNext(existingTile, target))
                {
                    return true;
                }
            }
        }

        return false;
    }

    public bool CanAcceptCard(Card card)
    {
        Tile[,] battlefield = BattlefieldManager.battleField;
        if (manager == null ||
            card == null ||
            x < 1 || x >= battlefield.GetLength(0) ||
            y < 1 || y >= battlefield.GetLength(1))
        {
            return false;
        }

        Tile tile = battlefield[x, y];
        if (tile == null)
        {
            return false;
        }

        if (card is Terrain)
        {
            return y == 1 || nextToTerrain();
        }

        if (card is Unit unit)
        {
            return tile.curTerrain != null &&
                (unit.ability == Ability.FLYING || unit.size <= tile.curTerrain.size) &&
                tile.canCross(unit.ability);
        }

        return false;
    }

    public bool TryPlaceCard(cardPrefab cardView)
    {
        if (cardView == null || !CanAcceptCard(cardView.getCard()))
        {
            return false;
        }

        Sprite cardSprite = cardView.GetCardSprite();
        SpriteRenderer tileSpriteRenderer = sprite != null
            ? sprite.GetComponent<SpriteRenderer>()
            : null;
        if (cardSprite == null)
        {
            Debug.LogError("Cannot place a card whose image has no sprite.", cardView);
            return false;
        }

        if (tileSpriteRenderer == null)
        {
            Debug.LogError("TilePrefab requires a SpriteRenderer on its sprite object.", this);
            return false;
        }

        SpriteRenderer targetSpriteRenderer = tileSpriteRenderer;
        if (cardView.getCard() is Unit)
        {
            targetSpriteRenderer = GetUnitSpriteRenderer(tileSpriteRenderer);
        }

        if (!FitSpriteToTile(targetSpriteRenderer, cardSprite))
        {
            return false;
        }

        //stupid don't force properties onto a local copy of tile set it via manager so it actually matters
        Tile tile = BattlefieldManager.battleField[x, y];
        if (cardView.getCard() is Terrain terrain)
        {
            //tile.curTerrain = terrain;
            manager.placeTerrainAt(this.x, this.y, terrain);
        }
        else if (cardView.getCard() is Unit unit)
        {
            //tile.putUnitHere(unit);
            manager.placeUnitAt(this.x, this.y, unit);
            if (unit.ability == Ability.FLYING)
            {
                tile.skyOccupied = true;
            }
            else
            {
                tile.groundOccupied = true;
            }
        }

        targetSpriteRenderer.sprite = cardSprite;
        return true;
    }

    private SpriteRenderer GetUnitSpriteRenderer(SpriteRenderer terrainSpriteRenderer)
    {
        if (unitSpriteRenderer != null)
        {
            return unitSpriteRenderer;
        }

        Transform terrainTransform = terrainSpriteRenderer.transform;
        GameObject unitSpriteObject = new GameObject("Unit Sprite", typeof(SpriteRenderer));
        unitSpriteObject.layer = terrainTransform.gameObject.layer;
        unitSpriteObject.transform.SetParent(terrainTransform.parent, false);
        unitSpriteObject.transform.localPosition = terrainTransform.localPosition;
        unitSpriteObject.transform.localRotation = terrainTransform.localRotation;
        unitSpriteObject.transform.localScale = terrainTransform.localScale;

        unitSpriteRenderer = unitSpriteObject.GetComponent<SpriteRenderer>();
        unitSpriteRenderer.sharedMaterial = terrainSpriteRenderer.sharedMaterial;
        unitSpriteRenderer.sortingLayerID = terrainSpriteRenderer.sortingLayerID;
        unitSpriteRenderer.sortingOrder = terrainSpriteRenderer.sortingOrder + 1;
        return unitSpriteRenderer;
    }

    private bool FitSpriteToTile(SpriteRenderer spriteRenderer, Sprite cardSprite)
    {
        RectTransform tileBox = tileSquare != null
            ? tileSquare.GetComponent<RectTransform>()
            : spriteRenderer.transform.parent as RectTransform;
        if (tileBox == null)
        {
            Debug.LogError("TilePrefab requires a RectTransform for its tile sprite box.", this);
            return false;
        }

        Vector2 spriteSize = cardSprite.bounds.size;
        Vector2 boxSize = tileBox.rect.size;
        if (spriteSize.x <= 0f || spriteSize.y <= 0f ||
            boxSize.x <= 0f || boxSize.y <= 0f)
        {
            Debug.LogError("Tile sprite and tile box dimensions must be non-zero.", this);
            return false;
        }

        float scale = Mathf.Min(boxSize.x / spriteSize.x, boxSize.y / spriteSize.y);
        Vector3 imageScale = spriteRenderer.transform.localScale;
        spriteRenderer.transform.localScale = new Vector3(scale, scale, imageScale.z);
        return true;
    }

    public static bool TryPlaceCardAtScreenPosition(
        Vector2 screenPosition,
        Camera camera,
        cardPrefab cardView)
    {
        if (camera == null)
        {
            camera = Camera.main;
        }

        if (camera == null)
        {
            Debug.LogError("Cannot place a hand card because no camera is available.");
            return false;
        }

        Vector3 worldPosition = camera.ScreenToWorldPoint(
            new Vector3(screenPosition.x, screenPosition.y, 0f));
        Collider2D[] hits = Physics2D.OverlapPointAll(worldPosition);
        foreach (Collider2D hit in hits)
        {
            TilePrefab tile = hit.GetComponentInParent<TilePrefab>();
            if (tile != null && tile.TryPlaceCard(cardView))
            {
                return true;
            }
        }

        return false;
    }

    private void Update()
    {
        if (!isMouseOver || manager == null)
        {
            return;
        }

        Player player = Player.GetInstance();
        cardPrefab draggingCard = player != null ? player.currentlyDragging : null;
        if (draggingCard == null ||
            draggingCard.place != cardPrefab.placement.HAND ||
            !CanAcceptCard(draggingCard.getCard()))
        {
            return;
        }

        RectTransform rectTransform = GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 1.5f);
            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 1.5f);
        }
    }
}
