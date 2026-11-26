using System.Collections.Generic;
using System.Transactions;
using Unity.Collections;
using UnityEngine;
public class Opponent : MonoBehaviour
{
    //this opponent behaviour script is simplistic and actions it takes are mostly random
    public List<Card> curDeck = new List<Card>();
    public List<Card> curHand = new List<Card>();
    public double Health = 25;
    public double Aether;
    public Sprite defaultSprite;
    private CardLibrary lib;
    private BattlefieldManager manager;
    private Dictionary<string, Sprite> spritesByName;
    public bool started = false;

    private void Start()
    {
        lib = CardLibrary.getInstance();
        manager = BattlefieldManager.getInstance();
        initDeckOne();
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
            lib.getUnitByName("Blazehorn Charger"),
            lib.getUnitByName("Blazehorn Charger"),
            lib.getUnitByName("Blazehorn Charger"),
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

    public Sprite getSpriteByName(string name)
    {
        if (spritesByName == null)
        {
            spritesByName = new Dictionary<string, Sprite>();
            foreach (Sprite sprite in Resources.LoadAll<Sprite>("Sprites"))
            {
                if (sprite == null)
                {
                    continue;
                }

                string spriteKey = sprite.texture != null ? sprite.texture.name : sprite.name;
                if (!spritesByName.ContainsKey(spriteKey) || sprite.name == spriteKey)
                {
                    spritesByName[spriteKey] = sprite;
                }

                if (!spritesByName.ContainsKey(sprite.name))
                {
                    spritesByName[sprite.name] = sprite;
                }
            }
        }

        if (!string.IsNullOrEmpty(name) &&
            spritesByName.TryGetValue(name, out Sprite matchingSprite))
        {
            return matchingSprite;
        }

        Debug.LogWarning(
            "Opponent could not find a sprite named '" + name + "' in Resources/Sprites; using the default sprite.",
            this);
        if (defaultSprite == null)
        {
            Debug.LogError("Opponent requires a default sprite when a card sprite cannot be found.", this);
        }

        return defaultSprite;
    }

    //The opponent gains one Aether for each terrain it controls before its turn.
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

        started = true;
        foreach (Tile tile in BattlefieldManager.battleField)
        {
            if (tile != null && tile.curTerrain != null && !tile.playerControlsTerrain)
            {
                Aether++;
            }
        }

        Card drawnCard = drawCard();
        if (drawnCard != null)
        {
            curHand.Add(drawnCard);
        }

        if (!TryPlaceTerrain(overlaysOnly: true) &&
            !TryPlaceTerrain(overlaysOnly: false))
        {
            TryPlaceUnit();
        }

        GoToMain();
    }

    private bool TryPlaceTerrain(bool overlaysOnly)
    {
        Tile[,] battlefield = BattlefieldManager.battleField;
        for (int cardIndex = 0; cardIndex < curHand.Count; cardIndex++)
        {
            Card card = curHand[cardIndex];
            if (!(card is Terrain terrain) || terrain.cost > Aether)
            {
                continue;
            }

            List<Tile> candidateTiles = new List<Tile>();
            for (int x = 1; x < battlefield.GetLength(0); x++)
            {
                for (int y = 1; y < battlefield.GetLength(1); y++)
                {
                    Tile tile = battlefield[x, y];
                    if (tile == null)
                    {
                        continue;
                    }

                    bool canOverlay = CanOverlayTerrain(tile, terrain);
                    bool isCandidate = overlaysOnly
                        ? canOverlay
                        : !canOverlay && CanPlaceTerrainNormally(tile);
                    if (isCandidate)
                    {
                        candidateTiles.Add(tile);
                    }
                }
            }

            if (candidateTiles.Count == 0)
            {
                continue;
            }

            Sprite terrainSprite = getSpriteByName(terrain.name);
            if (terrainSprite == null)
            {
                continue;
            }

            Tile chosenTile = candidateTiles[Random.Range(0, candidateTiles.Count)];
            if (!manager.TrySetTileCardSpriteAt(
                    chosenTile.x,
                    chosenTile.y,
                    terrain,
                    terrainSprite))
            {
                return false;
            }

            manager.placeTerrainAt(chosenTile.x, chosenTile.y, terrain, false);
            Aether -= terrain.cost;
            curHand.RemoveAt(cardIndex);
            Debug.Log(
                "Opponent: Placing " + terrain.name + " at (" + chosenTile.x + ", " + chosenTile.y + ")");
            return true;
        }

        return false;
    }

    private bool TryPlaceUnit()
    {
        Tile[,] battlefield = BattlefieldManager.battleField;
        for (int cardIndex = 0; cardIndex < curHand.Count; cardIndex++)
        {
            Card card = curHand[cardIndex];
            if (!(card is Unit unit) || unit.cost > Aether)
            {
                continue;
            }

            List<Tile> candidateTiles = new List<Tile>();
            for (int x = 1; x < battlefield.GetLength(0); x++)
            {
                for (int y = 1; y < battlefield.GetLength(1); y++)
                {
                    Tile tile = battlefield[x, y];
                    if (tile != null &&
                        !tile.playerControlsTerrain &&
                        tile.canHoldUnit(unit))
                    {
                        candidateTiles.Add(tile);
                    }
                }
            }

            if (candidateTiles.Count == 0)
            {
                continue;
            }

            Sprite unitSprite = getSpriteByName(unit.name);
            if (unitSprite == null)
            {
                continue;
            }

            Tile chosenTile = candidateTiles[Random.Range(0, candidateTiles.Count)];
            if (!manager.TrySetTileCardSpriteAt(
                    chosenTile.x,
                    chosenTile.y,
                    unit,
                    unitSprite))
            {
                return false;
            }

            manager.placeUnitAt(chosenTile.x, chosenTile.y, unit, false);
            Aether -= unit.cost;
            curHand.RemoveAt(cardIndex);
            Debug.Log(
                "Opponent: Placing " + unit.name + " at (" + chosenTile.x + ", " + chosenTile.y + ")");
            return true;
        }

        return false;
    }

    private bool CanPlaceTerrainNormally(Tile tile)
    {
        if (tile.curTerrain != null)
        {
            return false;
        }

        int opponentHomeRow = BattlefieldManager.battleField.GetLength(1) - 1;
        return tile.y == opponentHomeRow || IsAdjacentToOpponentTerrain(tile);
    }

    private bool CanOverlayTerrain(Tile tile, Terrain terrain)
    {
        return tile.curTerrain != null &&
            tile.playerControlsTerrain &&
            terrain.hasOverlayType &&
            tile.curTerrain.type == terrain.overlayType &&
            IsAdjacentToOpponentTerrain(tile);
    }

    private bool IsAdjacentToOpponentTerrain(Tile target)
    {
        Tile[,] battlefield = BattlefieldManager.battleField;
        for (int x = 1; x < battlefield.GetLength(0); x++)
        {
            for (int y = 1; y < battlefield.GetLength(1); y++)
            {
                Tile existingTile = battlefield[x, y];
                if (existingTile != null &&
                    existingTile.curTerrain != null &&
                    !existingTile.playerControlsTerrain &&
                    manager.checkifNext(existingTile, target))
                {
                    return true;
                }
            }
        }

        return false;
    }

    public void GoToMain()
    {
        manager.currentPhase = BattlefieldManager.gamePhase.ENEMY_TURN;
        //check if units are available to attack directly, or other units can be killed
        //the first bot isn't for bathbuilding, so meh
        // foreach(Tile t in BattlefieldManager.battleField)
        // {
        //     if(t.curTerrain != null && !t.playerControlsTerrain && t.unitHere != null || t.unitFlyingHere != null)
        //     {
        //         goToAttacks();
        //     }
        // }
        endMyTurn();
    }
    public void goToAttacks()
    {
        manager.currentPhase = BattlefieldManager.gamePhase.ENEMY_ATTACKS;
        //remember: ground/sea Units can attack each other, neither can attack flying units, and flying Units can attack everything.
        endMyTurn();
    }
    public void endMyTurn() //universal flag setter for ending the opponent's turn, add whatever is necessary later
    {
        if (manager == null)
        {
            manager = BattlefieldManager.getInstance();
        }
        if (manager == null)
        {
            Debug.LogError("Cannot start the player's turn because no BattlefieldManager exists.", this);
            return;
        }

        manager.startPlayerTurn();
        manager.currentPhase = BattlefieldManager.gamePhase.YOUR_TURN; //extra check because of parallel runs
        started = false;
    }
    void Update()
    {
        //do periodic checks for opponent's turn to trigger starts?
        if (manager == null)
        {
            manager = BattlefieldManager.getInstance();
        }

        if (manager != null &&
            manager.currentPhase == BattlefieldManager.gamePhase.ENEMY_TURN &&
            !started)
        {
            MyTurnStart();
            // started is a small lock to avoid looping myTurnStart calls :(
        }
    }
}