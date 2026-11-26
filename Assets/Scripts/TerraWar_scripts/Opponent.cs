using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
public class Opponent : MonoBehaviour
{
    //this opponent behaviour script is simplistic and actions it takes are mostly random
    public List<Card> curDeck = new List<Card>();
    public List<Card> curHand = new List<Card>();
    public double Health = 25;
    public double Aether;
    private CardLibrary lib;
    private BattlefieldManager manager;

    private void Start()
    {
        lib = CardLibrary.getInstance();
        manager = BattlefieldManager.getInstance();
    }
    //the version 1 for this machine should be select the most aether intensive play available, then put it as far down as possible

    public void initDeckOne() // this is meant to be the first deck that the player battles against
    {
        curDeck.AddRange(new Card[]
        {
            lib.getUnitByName("Ember Drake"),
            lib.getUnitByName("Ember Drake"),
            lib.getUnitByName("Ember Drake"),
            lib.getUnitByName("Ember Drake"),
            lib.getTerrainByName("Volcano"),
            lib.getTerrainByName("Volcano"),
            lib.getTerrainByName("Volcano"),
            lib.getTerrainByName("Volcano"),
            lib.getTerrainByName("Volcano"),
            lib.getTerrainByName("Volcano"),
            lib.getTerrainByName("Desert"),
            lib.getTerrainByName("Desert"),
            lib.getTerrainByName("Desert"),
            lib.getTerrainByName("Desert"),
            lib.getUnitByName("Blazeborn Charger"),
            lib.getUnitByName("Blazeborn Charger"),
            lib.getUnitByName("Blazeborn Charger"),
            lib.getUnitByName("Molten Golem"),
            lib.getUnitByName("Molten Golem"),
            lib.getUnitByName("Molten Golem"),

        });
    }
    public void prepForBattle(int num)
    {
        if(num == 1){initDeckOne();}
    }


    public Card drawCard()
    {
        if (curDeck.Count == 0)
        {
            Debug.LogWarning("The opponent has no cards left to draw.", this);
            return null;
        }

        int cardIndex = Random.Range(0, curDeck.Count);
        Card randomCard = curDeck[cardIndex];
        curDeck.RemoveAt(cardIndex);
        return randomCard;
    }

//sequence: first on start draw and place terrain at random (the first one you can pay for) on a legal position at random.
    public void MyTurnStart()
    {
        if (manager == null)
        {
            manager = BattlefieldManager.getInstance();
        }

        if (manager == null)
        {
            Debug.LogError("Opponent requires a BattlefieldManager to take its turn.", this);
            return;
        }

        Card drawnCard = drawCard();
        if (drawnCard != null)
        {
            curHand.Add(drawnCard);
        }

        Tile[,] battlefield = BattlefieldManager.battleField;
        List<Tile> candidateTiles = new List<Tile>();
        foreach(Card card in curHand)
        {
            if (!(card is Terrain terrain) || terrain.cost > Aether)
            {
                continue;
            }

            candidateTiles.Clear();
            for (int x = 1; x < battlefield.GetLength(0); x++)
            {
                for (int y = 1; y < battlefield.GetLength(1); y++)
                {
                    Tile tile = battlefield[x, y];
                    if (tile == null)
                    {
                        continue;
                    }

                    bool canReplaceTerrain = tile.curTerrain != null &&
                        terrain.hasOverlayType &&
                        tile.curTerrain.type == terrain.overlayType;
                    if (tile.curTerrain != null && !canReplaceTerrain)
                    {
                        continue;
                    }

                    bool canPlaceTerrain = tile.y == 1 || canReplaceTerrain;
                    if (!canPlaceTerrain)
                    {
                        foreach (Tile existingTile in battlefield)
                        {
                            if (existingTile != null &&
                                existingTile.curTerrain != null &&
                                manager.checkifNext(existingTile, tile))
                            {
                                canPlaceTerrain = true;
                                break;
                            }
                        }
                    }

                    if (canPlaceTerrain)
                    {
                        candidateTiles.Add(tile);
                    }
                }
            }

            if (candidateTiles.Count == 0)
            {
                continue;
            }

            Tile chosenTile = candidateTiles[Random.Range(0, candidateTiles.Count)];
            manager.placeTerrainAt(chosenTile.x, chosenTile.y, terrain);
            return;
        }

        Debug.LogWarning("Opponent has no affordable terrain card with a legal placement.", this);
        GoToMain();
    }
    public void GoToMain()
    {
        
    }
    public void endMyTurn() //universal flag setter for ending the turn, add whatever is necessary later
    {
        manager.currentPhase = BattlefieldManager.gamePhase.YOUR_TURN;
        manager.TerrainThisTurn = false;

    }
    void Update()
    {
        //do periodic checks for opponent's turn to trigger starts?
    }
}