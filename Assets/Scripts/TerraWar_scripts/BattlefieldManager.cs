using UnityEngine;
using System.Collections.Generic;
using System;
//this will handle battlefield only UI and manage the gameplay mecanics pertaining to terrain placement and field stuff
public class BattlefieldManager : MonoBehaviour
{
    //first create the battlefield as a grid of Terrains
    //would be interesting to try implementing storage of which creatures are placed where
    public Terrain[,] battleField = new Terrain[5,5];
    
}

//create an object to represent a single tile on the board
public class Tile
{
    public Terrain curTerrain;
    public Boolean occupied;
    public Boolean canCross(CardLibrary.Ability creatureAvility)
    {
        return curTerrain.condition == CardLibrary.PassCondition.SWIM ? false : true;
    }
}