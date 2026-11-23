using UnityEngine;
using System;
using static CardLibrary;
using static Player;
using UnityEngine.UI;
using JetBrains.Annotations;
using UnityEditor.Rendering.LookDev;
using UnityEngine.InputSystem;
using System.Collections;

public class BattlefieldManager : MonoBehaviour
{

    private static BattlefieldManager instance;
    public static BattlefieldManager getInstance()
    {
        return instance;
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);
    }
    private void Start()
    {
        player = Player.GetInstance(); // to make sure we're getting the right instance of player
    }

    //another thing that could work is if I assign an object to attach to each tile object.
    //this is probably simpler
    public GameObject[,] fieldObject;
    public GameObject AetherDisplay; 
    //make an array of buttons to assign so this is easy
    public Button[] playerButtons;
    public Tile[,] battleField = new Tile[5,5];
    [SerializeField]
    private Transform HandScrollView; // for hand visualization
    [SerializeField]
    private cardPrefab cardPrefabForHand;
    private Player player = Player.GetInstance();
    private const float HandCardDisplayScale = 35f;
    private const float HandCardSpacing = 8f;
    private const int HandCardsPerRow = 2;


    //use this to set the current state of an empty battlefield
    public void initTiles(){
        for(int i = 1; i < 5; i++){
            for(int j = 1; j < 5; j++){
                Tile curTile = new Tile(i,j);
                battleField[i,j] = curTile;
            }
        }
    }

    public void setUpBattlefield()
    {
        for(int i = 0; i < 5; i++)
        {
            for (int j = 1; j < 5; j++)
            {
                //why does this error
                //fieldObject[i,j].GetComponent<Sprite>().SetSpriteID("path");
                fieldObject[i,j].SetActive(true);
            }
        }
    }

    //checks if two tiles are adjacent to each other
    public Boolean checkifAdjacent(Tile t1, Tile t2)
    {
        return (t1.getX()-t2.getX()==1) || (t1.getY()-t2.getY()==1) || (t1.getX()-t2.getX()==-1) || (t1.getY()-t2.getY()==-1);
    }

    //check if two tiles are next to each other, essential for battle logic
    public Boolean checkifNext(Tile t1, Tile t2)
    {
        return(
            //dispx=1 and same y, or dispy=1 and same x
            (Math.Abs(t1.getX()-t2.getX()) == 1 && t1.getY()-t2.getY() == 0) ||
            (Math.Abs(t1.getY()-t2.getY())==1 && t1.getX()-t2.getX()==0)
        );
    }

    public void placeUnitAt(int x, int y, Unit unit)
    {
        battleField[x,y].putUnitHere(unit);
    }
    public void removeUnitsAt(int x, int y)
    {
        if(battleField[x,y].unitHere != null || battleField[x,y].unitFlyingHere != null)
        {
            battleField[x,y].unitHere = null;
            battleField[x,y].unitFlyingHere = null;
        }
    }
    public void damageUnitsAt(int x, int y, int ATTACK)
    {
        //remove all Units at this location if their defense is too low
        if(battleField[x,y].unitHere != null && battleField[x,y].unitHere.DEFENSE <= ATTACK)
        {
            battleField[x,y].unitHere = null;
        }
        if(battleField[x,y].unitFlyingHere != null && battleField[x,y].unitFlyingHere.DEFENSE <= ATTACK)
        {
            battleField[x,y].unitFlyingHere = null;
        }
    }

    public void updateHand()
    {
        if (HandScrollView == null)
        {
            Debug.LogError("BattlefieldManager requires its Hand Scroll View transform.", this);
            return;
        }

        ScrollRect scrollRect = HandScrollView.GetComponent<ScrollRect>();
        RectTransform content = scrollRect != null
            ? scrollRect.content
            : HandScrollView as RectTransform;

        if (content == null)
        {
            Debug.LogError("Hand Scroll View must have a ScrollRect with content assigned.", HandScrollView);
            return;
        }

        if (cardPrefabForHand == null)
        {
            Debug.LogError("BattlefieldManager requires a card prefab for the hand.", this);
            return;
        }

        Canvas prefabCanvas = cardPrefabForHand.GetComponentInChildren<Canvas>();
        RectTransform cardRect = prefabCanvas != null
            ? prefabCanvas.GetComponent<RectTransform>()
            : null;

        if (cardRect == null)
        {
            Debug.LogError("Hand card prefab must contain a Canvas with a RectTransform.", cardPrefabForHand);
            return;
        }

        RectTransform viewport = scrollRect != null && scrollRect.viewport != null
            ? scrollRect.viewport
            : content;

        if (viewport.rect.width <= HandCardSpacing ||
            cardRect.rect.width <= 0f ||
            cardRect.rect.height <= 0f)
        {
            Debug.LogError("Hand viewport must fit two cards and the card prefab dimensions must be non-zero.", this);
            return;
        }

        for (int i = content.childCount - 1; i >= 0; i--)
        {
            Destroy(content.GetChild(i).gameObject);
        }

        float maxCardWidth = (viewport.rect.width - HandCardSpacing) / HandCardsPerRow;
        float scale = Mathf.Min(HandCardDisplayScale, maxCardWidth / cardRect.rect.width);
        Vector2 displaySize = cardRect.rect.size * scale;
        float columnStep = displaySize.x + HandCardSpacing;
        float rowStep = displaySize.y + HandCardSpacing;

        int cardCount = 0;
        foreach (Card card in Player.GetInstance().curhand)
        {
            if (card != null)
            {
                cardCount++;
            }
        }

        int rowCount = (cardCount + HandCardsPerRow - 1) / HandCardsPerRow;
        float gridWidth = HandCardsPerRow * displaySize.x + (HandCardsPerRow - 1) * HandCardSpacing;
        float contentWidth = Mathf.Max(viewport.rect.width, gridWidth);
        float contentHeight = rowCount > 0
            ? rowCount * displaySize.y + (rowCount - 1) * HandCardSpacing
            : viewport.rect.height;

        content.anchorMin = new Vector2(0.5f, 1f);
        content.anchorMax = new Vector2(0.5f, 1f);
        content.pivot = new Vector2(0.5f, 1f);
        content.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, contentWidth);
        content.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, Mathf.Max(viewport.rect.height, contentHeight));

        int cardIndex = 0;
        foreach (Card card in Player.GetInstance().curhand)
        {
            if (card == null)
            {
                Debug.LogWarning("Skipping a null card in the player's hand.", this);
                continue;
            }

            cardPrefab prefab = Instantiate(cardPrefabForHand, content, false);
            prefab.name = card.name;
            prefab.transform.localPosition = new Vector3(
                -(gridWidth * 0.5f) + (cardIndex % HandCardsPerRow) * columnStep + displaySize.x * 0.5f,
                -(cardIndex / HandCardsPerRow * rowStep + displaySize.y * 0.5f),
                0f);
            prefab.transform.localRotation = Quaternion.identity;
            prefab.transform.localScale = Vector3.one * scale;

            prefab.setCardTo(card);
            prefab.place = cardPrefab.placement.HAND;
            cardIndex++;
        }

        if (scrollRect != null)
        {
            scrollRect.horizontalNormalizedPosition = 0f;
            scrollRect.verticalNormalizedPosition = 1f;
        }

        Debug.Log("Updated hand UI.");
    }
    public bool hasTerrain(int x, int y)
    {
        return battleField[x,y].curTerrain != null;
    }
    public Unit selectAnyUnit()
    {
        //TODO: fill this in with a visual where the player gets to click a Unit and it returns that unit
        return new Unit("0",0,0,0,Element.COSMIC);
    }
}

//create an object to represent a single tile on the board
public class Tile
{
    public Terrain curTerrain;
    public Unit unitHere; //track the unit on this terrain
    public Unit unitFlyingHere; //track flying unit here (if applicable)
    //the above two store the current state of the unit, so stat mods will happen to those objects up there
    public Boolean groundOccupied; //counts underwater and underground
    public Boolean skyOccupied; //could literally just check if unitFlyingHere is null
    public int x;
    public int y;
    public Tile(int x, int y)
    {
        this.x = x;
        this.y = y;
    }
    public Boolean canCross(Ability creatureAbility)
    //return if the passed creature.ability can pass this tile
    {
        if(creatureAbility == Ability.FLYING){return !skyOccupied;}
        if(creatureAbility == Ability.SWIMONLY){return curTerrain.condition == PassCondition.SWIM && !groundOccupied;}
        if(creatureAbility == Ability.SWIM){return (curTerrain.condition == PassCondition.SWIM || 
            curTerrain.condition == PassCondition.NONE) && !groundOccupied;}
        return !groundOccupied && curTerrain.condition == PassCondition.NONE;
    }
    public int getX(){return this.x;}
    public int getY(){return this.y;}
    public void setXY(int x, int y){this.x = x; this.y = y;}
    public void putUnitHere(Unit unit)
    {
        if(unit.ability == Ability.FLYING){unitFlyingHere = unit;}
        else {unitHere = unit;}
    }
    public void clearUnitHere(Boolean ground)
    {
        if(ground){unitHere = null;} else {unitFlyingHere = null;}
    }
}