using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static CardLibrary;
using Math = System.Math;
public class Opponent : MonoBehaviour
{
    //this opponent behaviour script is simplistic and actions it takes are mostly random
    public List<Card> curDeck = new List<Card>();
    public List<Card> curHand = new List<Card>();
    public double Health = 15;
    public double Aether;
    public Sprite defaultSprite;
    public float timeInterval = 1f;
    private CardLibrary lib;
    private BattlefieldManager manager;
    private Dictionary<string, Sprite> spritesByName;
    public bool started = false;

    public void loseHealth(int amount)
    {
        Health -= amount;
    }

    private struct UnitStartPosition
    {
        public int x;
        public int y;
        public Unit unit;

        public UnitStartPosition(int x, int y, Unit unit)
        {
            this.x = x;
            this.y = y;
            this.unit = unit;
        }
    }

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
            lib.getUnitByName("Flame Warrior"),
            lib.getUnitByName("Flame Warrior"),
            lib.getUnitByName("Flame Warrior"),

        });
    }
    public void clearDeck()
    {
        curDeck = new List<Card>();
    }
    public void initRuinDeck()
    {
        curDeck.AddRange(new Card[]
        {
            
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
        if(Health <= 1)
        {
            manager.playerWon();
            clearDeck();
            Player.GetInstance().rewardCollection();
        }
        if (started)
        {
            return;
        }

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
        StartCoroutine(PerformTurn());
    }

    private IEnumerator PerformTurn()
    {
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

        bool playedCard = TryPlaceTerrain(overlaysOnly: true);
        if (!playedCard)
        {
            playedCard = TryPlaceTerrain(overlaysOnly: false);
        }
        if (!playedCard)
        {
            playedCard = TryPlaceUnit();
        }

        if (playedCard && timeInterval > 0f)
        {
            yield return new WaitForSeconds(timeInterval);
        }

        foreach (UnitStartPosition unitStart in GetOpponentUnitsFrontToBack())
        {
            if (TryMoveUnitTowardPlayer(unitStart) && timeInterval > 0f)
            {
                yield return new WaitForSeconds(timeInterval);
            }
        }

        GoToMain();
    }

    private List<UnitStartPosition> GetOpponentUnitsFrontToBack()
    {
        List<UnitStartPosition> units = new List<UnitStartPosition>();
        Tile[,] battlefield = BattlefieldManager.battleField;
        for (int y = 1; y < battlefield.GetLength(1); y++)
        {
            for (int x = 1; x < battlefield.GetLength(0); x++)
            {
                Tile tile = battlefield[x, y];
                if (tile == null)
                {
                    continue;
                }

                if (tile.unitHere != null && !tile.playerControlsGroundUnit)
                {
                    units.Add(new UnitStartPosition(x, y, tile.unitHere));
                }

                if (tile.unitFlyingHere != null && !tile.playerControlsFlyingUnit)
                {
                    units.Add(new UnitStartPosition(x, y, tile.unitFlyingHere));
                }
            }
        }

        return units;
    }

    private bool TryMoveUnitTowardPlayer(UnitStartPosition unitStart)
    {
        int destinationY = unitStart.y - 1;
        if (destinationY < 1)
        {
            return false;
        }

        Tile destination = BattlefieldManager.battleField[unitStart.x, destinationY];
        if (destination == null || !destination.canHoldUnit(unitStart.unit))
        {
            return false;
        }

        if (!manager.TryMoveUnitTo(
                unitStart.unit,
                unitStart.x,
                unitStart.y,
                unitStart.x,
                destinationY,
                false))
        {
            return false;
        }

        Debug.Log(
            "Opponent: Moving " + unitStart.unit.name +
            " from (" + unitStart.x + ", " + unitStart.y +
            ") to (" + unitStart.x + ", " + destinationY + ")",
            this);
        return true;
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
        //check if there are any useful movements to be made (forward)
        goToAttacks();
        // endMyTurn();
    }
    public void goToAttacks()
    {
        manager.currentPhase = BattlefieldManager.gamePhase.ENEMY_ATTACKS;
        StartCoroutine(ResolveAttacksAndEndTurn());
    }

    private IEnumerator ResolveAttacksAndEndTurn()
    {
        foreach (UnitStartPosition attacker in GetOpponentUnitsFrontToBack())
        {
            bool performedAttack = TryDamagePlayerDirectly(attacker);
            if (!performedAttack)
            {
                performedAttack = TryAttackPlayerUnit(attacker);
            }

            if (performedAttack && timeInterval > 0f)
            {
                yield return new WaitForSeconds(timeInterval);
            }
        }

        endMyTurn();
    }

    private bool TryAttackPlayerUnit(UnitStartPosition attackerPosition)
    {
        Tile[,] battlefield = BattlefieldManager.battleField;
        Tile attackerTile = battlefield[attackerPosition.x, attackerPosition.y];
        Unit attacker = attackerPosition.unit;
        if (attackerTile == null || attacker == null)
        {
            return false;
        }

        if (manager.HasUnitAttackedThisTurn(attackerTile, attacker))
        {
            return false;
        }

        bool attackerStillOnTile = attacker.ability == Ability.FLYING
            ? attackerTile.unitFlyingHere == attacker && !attackerTile.playerControlsFlyingUnit
            : attackerTile.unitHere == attacker && !attackerTile.playerControlsGroundUnit;
        if (!attackerStillOnTile)
        {
            return false;
        }

        for (int x = Math.Max(1, attackerTile.x - 1);
             x <= Math.Min(battlefield.GetLength(0) - 1, attackerTile.x + 1);
             x++)
        {
            for (int y = Math.Max(1, attackerTile.y - 1);
                 y <= Math.Min(battlefield.GetLength(1) - 1, attackerTile.y + 1);
                 y++)
            {
                Tile targetTile = battlefield[x, y];
                if (targetTile == null || !manager.checkifNext(attackerTile, targetTile))
                {
                    continue;
                }

                Unit target = null;
                if (attacker.ability == Ability.FLYING &&
                    targetTile.unitFlyingHere != null &&
                    targetTile.playerControlsFlyingUnit)
                {
                    target = targetTile.unitFlyingHere;
                }
                else if (targetTile.unitHere != null && targetTile.playerControlsGroundUnit)
                {
                    target = targetTile.unitHere;
                }

                if (target != null &&
                    attacker.ATTACK > target.DEFENSE &&
                    manager.MarkUnitAttackedThisTurn(attackerTile, attacker))
                {
                    return manager.TryRemoveUnitFromTile(targetTile, target);
                }
            }
        }

        return false;
    }

    private bool TryDamagePlayerDirectly(UnitStartPosition attackerPosition)
    {
        if (manager.currentPhase != BattlefieldManager.gamePhase.ENEMY_ATTACKS)
        {
            return false;
        }

        Tile[,] battlefield = BattlefieldManager.battleField;
        Tile attackerTile = battlefield[attackerPosition.x, attackerPosition.y];
        Unit attacker = attackerPosition.unit;
        if (attackerTile == null || attacker == null)
        {
            return false;
        }

        if (manager.HasUnitAttackedThisTurn(attackerTile, attacker))
        {
            return false;
        }

        bool attackerStillOnTile = attacker.ability == Ability.FLYING
            ? attackerTile.unitFlyingHere == attacker && !attackerTile.playerControlsFlyingUnit
            : attackerTile.unitHere == attacker && !attackerTile.playerControlsGroundUnit;
        if (!attackerStillOnTile ||
            (attackerTile.y != 1 && !HasClearPathToPlayer(attacker, attackerTile)))
        {
            return false;
        }

        Player player = Player.GetInstance();
        if (player == null)
        {
            Debug.LogError("Cannot damage the player because no Player instance exists.", this);
            return false;
        }

        if (!manager.MarkUnitAttackedThisTurn(attackerTile, attacker))
        {
            return false;
        }

        player.loseHealth(attacker.ATTACK);
        Debug.Log(
            "Opponent: " + attacker.name + " dealt " + attacker.ATTACK +
            " direct damage. Player health: " + player.Health,
            this);
        return true;
    }

    private bool HasClearPathToPlayer(Unit attacker, Tile attackerTile)
    {
        Tile[,] battlefield = BattlefieldManager.battleField;
        for (int y = attackerTile.y - 1; y >= 1; y--)
        {
            Tile pathTile = battlefield[attackerTile.x, y];
            if (pathTile == null ||
                pathTile.curTerrain == null ||
                pathTile.unitHere != null ||
                pathTile.unitFlyingHere != null ||
                !pathTile.canCross(attacker.ability))
            {
                return false;
            }
        }

        return true;
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