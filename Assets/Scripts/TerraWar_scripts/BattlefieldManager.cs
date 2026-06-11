using UnityEngine;
using System.Collections.Generic;
using System;
using static CardLibrary;
using static Card;
using System.Runtime.InteropServices.WindowsRuntime;
//this will handle battlefield only UI and manage the gameplay mecanics pertaining to terrain placement and field stuff

public class BattlefieldManager : MonoBehaviour
{
    //first create the battlefield as a grid of Terrains
    //would be interesting to try implementing storage of which creatures are placed where
    public Tile[,] battleField = new Tile[5,5];
    
}

//create an object to represent a single tile on the board
public class Tile
{
    public Terrain curTerrain;
    public Boolean groundOccupied;
    public Boolean skyOccupied;
    public Boolean canCross(Ability creatureAbility)
    {
        if(creatureAbility == Ability.FLYING){return !skyOccupied;}
        if(creatureAbility == Ability.SWIMONLY){return curTerrain.condition == PassCondition.SWIM && !groundOccupied;}
        if(creatureAbility == Ability.SWIM){return (curTerrain.condition == PassCondition.SWIM || curTerrain.condition == null) && !groundOccupied;}
        return !groundOccupied;
    }
}