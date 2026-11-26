using UnityEngine;
using static CardLibrary;

public class TilePrefab : MonoBehaviour
{
    private const float UnitSpriteTileScale = 0.8f;

    //bottom left is 1,1 and top right is 5,5
    public int x;
    public int y;

    public GameObject sprite;
    public GameObject tileSquare;
    private BattlefieldManager manager;
    private bool isMouseOver;
    private SpriteRenderer groundUnitSpriteRenderer;
    private SpriteRenderer flyingUnitSpriteRenderer;
    private Unit highlightedUnit;
    private SpriteRenderer highlightedUnitRenderer;
    private Color highlightedOriginalColor;
    private Vector3 highlightedOriginalScale;

    private void Start()
    {
        manager = BattlefieldManager.getInstance();
        if (manager != null)
        {
            manager.RegisterTilePrefab(this);
        }
    }

    private void OnMouseEnter()
    {
        isMouseOver = true;
    }

    private void OnMouseExit()
    {
        isMouseOver = false;
    }

    private void OnMouseDown()
    {
        if (manager == null)
        {
            manager = BattlefieldManager.getInstance();
        }

        if (manager != null)
        {
            manager.HandleTileClicked(this);
        }
        Debug.LogWarning("Tile (" + this.x + ", " + this.y + ") was pressed! IT WORKED", this);
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
        Player player = Player.GetInstance();
        if (manager == null ||
            player == null ||
            card == null ||
            player.Aether < card.cost ||
            manager.currentPhase != BattlefieldManager.gamePhase.YOUR_TURN ||
            x < 1 || x >= battlefield.GetLength(0) ||
            y < 1 || y >= battlefield.GetLength(1))
        {
            return false;
        }

        if (card is Terrain && manager.TerrainThisTurn)
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
            return tile.playerControlsTerrain && tile.canHoldUnit(unit);
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
        if (cardSprite == null)
        {
            Debug.LogError("Cannot place a card whose image has no sprite.", cardView);
            return false;
        }

        if (!TryPrepareCardSprite(cardView.getCard(), cardSprite, out SpriteRenderer targetSpriteRenderer))
        {
            return false;
        }

        Player player = Player.GetInstance();
        if (player == null || !player.payAether(cardView.getCard().cost))
        {
            return false;
        }
        manager.updateAetherDisplay();

        //stupid don't force properties onto a local copy of tile set it via manager so it actually matters
        if (cardView.getCard() is Terrain terrain)
        {
            manager.placeTerrainAt(this.x, this.y, terrain);
            manager.TerrainThisTurn = true;
        }
        else if (cardView.getCard() is Unit placedUnit)
        {
            manager.placeUnitAt(this.x, this.y, placedUnit);
        }

        targetSpriteRenderer.sprite = cardSprite;
        return true;
    }

    public bool TrySetCardSprite(Card card, Sprite cardSprite)
    {
        if (!TryPrepareCardSprite(card, cardSprite, out SpriteRenderer targetSpriteRenderer))
        {
            return false;
        }

        targetSpriteRenderer.sprite = cardSprite;
        return true;
    }

    private bool TryPrepareCardSprite(
        Card card,
        Sprite cardSprite,
        out SpriteRenderer targetSpriteRenderer)
    {
        targetSpriteRenderer = null;
        if (card == null || cardSprite == null)
        {
            Debug.LogError("Cannot display a null card or sprite on a tile.", this);
            return false;
        }

        SpriteRenderer tileSpriteRenderer = sprite != null
            ? sprite.GetComponent<SpriteRenderer>()
            : null;
        if (tileSpriteRenderer == null)
        {
            Debug.LogError("TilePrefab requires a SpriteRenderer on its sprite object.", this);
            return false;
        }

        targetSpriteRenderer = tileSpriteRenderer;
        if (card is Unit unitCard)
        {
            targetSpriteRenderer = GetUnitSpriteRenderer(
                tileSpriteRenderer,
                unitCard.ability == Ability.FLYING);
        }
        else if (!(card is Terrain))
        {
            Debug.LogError("Only unit and terrain cards can be displayed on a tile.", this);
            return false;
        }

        float sizeScale = card is Unit ? UnitSpriteTileScale : 1f;
        return FitSpriteToTile(targetSpriteRenderer, cardSprite, sizeScale);
    }

    private SpriteRenderer GetUnitSpriteRenderer(
        SpriteRenderer terrainSpriteRenderer,
        bool isFlying)
    {
        SpriteRenderer unitSpriteRenderer = isFlying
            ? flyingUnitSpriteRenderer
            : groundUnitSpriteRenderer;
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
        unitSpriteRenderer.sortingOrder = terrainSpriteRenderer.sortingOrder + (isFlying ? 2 : 1);
        if (isFlying)
        {
            flyingUnitSpriteRenderer = unitSpriteRenderer;
        }
        else
        {
            groundUnitSpriteRenderer = unitSpriteRenderer;
        }

        return unitSpriteRenderer;
    }

    public bool SetUnitSelected(Unit unit, bool selected)
    {
        if (!selected)
        {
            if (highlightedUnit != unit)
            {
                return true;
            }

            RestoreUnitHighlight();
            return true;
        }

        SpriteRenderer unitRenderer = GetUnitSpriteRenderer(unit);
        if (unitRenderer == null || unitRenderer.sprite == null)
        {
            Debug.LogError("Cannot highlight a unit without its tile sprite.", this);
            return false;
        }

        if (highlightedUnit == unit && highlightedUnitRenderer == unitRenderer)
        {
            return true;
        }

        RestoreUnitHighlight();
        highlightedUnit = unit;
        highlightedUnitRenderer = unitRenderer;
        highlightedOriginalColor = unitRenderer.color;
        highlightedOriginalScale = unitRenderer.transform.localScale;
        Color selectionTint = new Color(1f, 0.96f, 0.62f, highlightedOriginalColor.a);
        unitRenderer.color = Color.Lerp(highlightedOriginalColor, selectionTint, 0.5f);
        unitRenderer.transform.localScale = new Vector3(
            highlightedOriginalScale.x * 1.05f,
            highlightedOriginalScale.y * 1.05f,
            highlightedOriginalScale.z);
        return true;
    }

    public void ClearUnitSprite(Unit unit)
    {
        if (unit == null)
        {
            return;
        }

        if (highlightedUnit == unit)
        {
            RestoreUnitHighlight();
        }

        SpriteRenderer unitRenderer = GetUnitSpriteRenderer(unit);
        if (unitRenderer != null)
        {
            unitRenderer.sprite = null;
        }
    }

    private SpriteRenderer GetUnitSpriteRenderer(Unit unit)
    {
        if (unit == null)
        {
            return null;
        }

        return unit.ability == Ability.FLYING
            ? flyingUnitSpriteRenderer
            : groundUnitSpriteRenderer;
    }

    private void RestoreUnitHighlight()
    {
        if (highlightedUnitRenderer != null)
        {
            highlightedUnitRenderer.color = highlightedOriginalColor;
            highlightedUnitRenderer.transform.localScale = highlightedOriginalScale;
        }

        highlightedUnit = null;
        highlightedUnitRenderer = null;
    }

    public bool TryMoveUnitSpriteTo(TilePrefab destination, Unit unit)
    {
        if (destination == null || unit == null)
        {
            return false;
        }

        bool isFlying = unit.ability == Ability.FLYING;
        SpriteRenderer sourceRenderer = isFlying
            ? flyingUnitSpriteRenderer
            : groundUnitSpriteRenderer;
        SpriteRenderer destinationTerrainRenderer = destination.sprite != null
            ? destination.sprite.GetComponent<SpriteRenderer>()
            : null;
        if (sourceRenderer == null || sourceRenderer.sprite == null)
        {
            Debug.LogError("Cannot move a unit without its source sprite.", this);
            return false;
        }

        if (destinationTerrainRenderer == null)
        {
            Debug.LogError("Destination TilePrefab requires a SpriteRenderer on its sprite object.", destination);
            return false;
        }

        SpriteRenderer destinationRenderer = destination.GetUnitSpriteRenderer(
            destinationTerrainRenderer,
            isFlying);
        Sprite unitSprite = sourceRenderer.sprite;
        if (!destination.FitSpriteToTile(
                destinationRenderer,
                unitSprite,
                UnitSpriteTileScale))
        {
            return false;
        }

        destinationRenderer.sprite = unitSprite;
        destinationRenderer.color = sourceRenderer.color;
        sourceRenderer.sprite = null;
        return true;
    }

    private bool FitSpriteToTile(SpriteRenderer spriteRenderer, Sprite cardSprite, float sizeScale)
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

        float scale = Mathf.Min(boxSize.x / spriteSize.x, boxSize.y / spriteSize.y) * sizeScale;
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
