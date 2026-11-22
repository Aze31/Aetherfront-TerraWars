using UnityEngine;
using System;
using static CardLibrary;
using static Player;
using UnityEngine.UI;
using JetBrains.Annotations;
using UnityEditor.Rendering.LookDev;

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
    //another thing that could work is if I assign an object to attach to each tile object.
    //this is probably simpler
    public GameObject[,] fieldObject;
    public GameObject selectionObject; 
    //make an array of buttons to assign so this is easy
    public Button[] playerButtons;
    public Tile[,] battleField = new Tile[5,5];
    [SerializeField]
    private Transform HandScrollView; // for hand visualization
    [SerializeField]
    private cardPrefab cardPrefabForHand;
    private Player player = Player.GetInstance();


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
        foreach (Transform child in HandScrollView)
        {
            Destroy(child.gameObject);
        }
        foreach(Card card in Player.GetInstance().curhand)
        {
                //idu so this may not work
                cardPrefab prefab = Instantiate(cardPrefabForHand, HandScrollView);

                prefab.setCardTo(card); // this handles the card's type
                prefab.place = cardPrefab.placement.HAND;
                Debug.Log("attempted to update hand ui");
            
        }
    }
    public bool hasTerrain(int x, int y)
    {
        return battleField[x,y].curTerrain != null;
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