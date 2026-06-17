using UnityEngine;
using System;
using static CardLibrary;
using static Player;

public class BattlefieldManager : MonoBehaviour
{
    
    public Tile[,] battleField = new Tile[5,5];

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

    public void placeUnit(int x, int y, Unit unit)
    {
        if(unit.ability == Ability.FLYING)
        {
            battleField[x,y].unitFlyingHere = unit;
        } else {
            battleField[x,y].unitHere = unit;
        }
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
        return !groundOccupied;
    }
    public int getX(){return this.x;}
    public int getY(){return this.y;}
    public void setXY(int x, int y){this.x = x; this.y = y;}
}