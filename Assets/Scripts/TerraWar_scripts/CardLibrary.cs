using UnityEngine;
using System.Collections.Generic;
using JetBrains.Annotations;
public class CardLibrary
{
    private static CardLibrary instance;
    public static CardLibrary getInstance()
    {
        if(instance == null){
            instance = new CardLibrary();
            initializeLibrary();
        }
        return instance;
    }
    public enum Element
    {
        FIRE,
        WATER,
        NATURE,
        LIGHTNING,
        EARTH,
        WIND,
        COSMIC,
        RUIN
    }
    
    public enum Ability
    {
        FLYING,
        DIG,
        INVISIBLE, 
        SWIM, //to differentiate between land+ocean and ocean only
        SWIMONLY,
        RANGE1,
        RANGE2,
        BURN, //defaults to 1
        NONE
    }
    public enum PassCondition
    {
        SWIM,
        CANSWIM, // swimonly, swim, and non-swimming ground units can cross
        DIG,
        FLYING,
        NONE // default i think
    }
    public static List<Unit> Units = new List<Unit>();
    public static List<Tactic> Tactics = new List<Tactic>();
    public static List<Terrain> Terrains = new List<Terrain>();
    
    public static void initializeLibrary()
    {        
        //there may be a problem with AddRange
        //  size defaults to 1, abilites to .NONE
        //name, size, defense, attack, cost, type, abilities (if any)
        //I will likely end up increasing cost during playtesting and tuning before anything else
        Units.AddRange(new Unit[]
        {
            new Unit("Ember Drake", 1, 2, 1, Element.FIRE, Ability.FLYING),
            new Unit("HeatQuill Hedgehog",2,2,2,Element.FIRE, Ability.BURN),
            new Unit("Aqua Scout",2,1,1, Element.WATER), //should this guy know how to swim?
            new Unit("Marsh Serpent", 3,2,5, Element.WATER, Ability.SWIM),
            new Unit("Grove Sentinel",3, 1, 1, Element.NATURE),
            new Unit("Dwarf", 2, 1, 2, Element.NATURE),
            new Unit("Grave-Burrow Lurker", 1, 2, 2, 1, Element.RUIN, Ability.DIG),
            new Unit("Ancient Ruins Dragon", 5, 3, 9,Element.RUIN, Ability.FLYING),
            new Unit("Blazehorn Charger", 2, 3, 4, Element.FIRE),
            new Unit("Tidebender Serpent", 1, 3, 2, 3, Element.WATER, Ability.SWIMONLY),
            new Unit("Great Green Shark",4,4,7,Element.WATER, Ability.SWIMONLY),
            new Unit("Brave Seagull",1,1,2,Element.WATER, Ability.FLYING),
            new Unit("Catacomb Stalker", 2, 2, 4, Element.RUIN),
            new Unit("AshArrow Archer",1,2,1,Element.FIRE, Ability.RANGE1),
            new Unit("Keen-eyed Shark",1,3,2,3,Element.WATER, Ability.SWIMONLY),
            new Unit("Airborne Guardian", 1,1,3,2,Element.WIND, Ability.FLYING),
            new Unit("Molten Golem", 4,4,8,Element.FIRE, Ability.NONE),
            new Unit("Flame Warrior",1,2,2,3,Element.FIRE),
            new Unit("Lightning Artist",1,2,2,2,Element.LIGHTNING, Ability.RANGE1), //remove r1?
            new Unit("Echoed Wyvern", 0, 2, 3, 3, Element.COSMIC, Ability.INVISIBLE),
            new Unit("Mystic Elf",3,1,3,Element.NATURE)
            
        });
        Terrains.AddRange(new Terrain[]
        {
            //name, size, type, effect, cost, passCondition/overlay type
            //how overlay works is that a terrain of that element can 'cover up' this terrain. It should rarely come up but still be relevant and interesting nuance
            new Terrain("Volcano", 1, Element.FIRE, "", 0, Element.WATER),
            new Terrain("Mountains",2,Element.EARTH, "Earth and Wind Units here gain +1 Defense.", 1),
            new Terrain("Ocean", 1, Element.WATER, "Water Units here gain +1 Defense.", 0, PassCondition.SWIM),
            new Terrain("Beach", 1, Element.WATER, "", 0),
            new Terrain("Verdant Meadow", 1, Element.NATURE, "", 0),
            new Terrain("Marsh", 1, Element.WATER, "", 0, PassCondition.CANSWIM),
            new Terrain("Grave Ruins", 1, Element.RUIN, "Ruin Units you control take up no space here.", 0),
            new Terrain("Thunderbroken Plain", 2, Element.LIGHTNING, "Lightning and Water Units here gain Swim.",1, PassCondition.SWIM),
            new Terrain("Ancient Temple", 1, Element.RUIN, "Ruin and Nature Units next to this gain +1 Defense.", 0, Element.NATURE),
            new Terrain("Desert", 2, Element.FIRE, "Fire Units here gain +1 Attack", 0),
            new Terrain("Thunderfalls",1, Element.LIGHTNING, "Lightning Units here (with RANGE0) gain RANGE1 and +1 Attack",0),
            new Terrain("Frozen Lake",1, Element.WATER, "Non-water Units here get -1 Defense",1, Element.WATER),
        });
        Tactics.AddRange(new Tactic[]
        {
            //name, cost, list of effects
            new Tactic("The Forked Advance", 2, new string[]{"Selected Unit moves one space.", "All Units next to it then take 1 damage."}),
            new Tactic("The Blazing Gambit", 2, new string[]{"All Units next to a Fire Terrain that are not a Fire type take 1 damage.", "All Fire Units you control gain +1 Attack."}),
            new Tactic("Sudden Shield", 1, new string[]{"Selected Unit you control gains +2 Defense this turn."}),
            new Tactic("Draconic Flare", 4, new string[]{"Selected Fire Unit you control gains +1 Attack.", "That unit deals damage to all areas."}),
            new Tactic("Ruinous Rampage", 2, new string[]{"Selected Ruin Unit you control gains +3 Attack this turn and attacks every unit next to it."}),
            new Tactic("High Tide", 2, new string[]{"Gain 4 Aether"}),
            new Tactic("Water Blast",4,new string[]{"Selected Unit takes 3 damage."}),
            new Tactic("Knowledge Vial",5,new string[]{"Draw three cards."}),
            new Tactic("Burning Flare", 4, new string[]{"Opponent takes 4 damage."})
        });
    }


    //currently none of these things are working currently and all are throwing warning sand returning null
    public Unit getUnitByName(string name){
        foreach (Unit unit in Units)
        {
            if(unit.name == name){return unit;}
        }    
        Debug.LogWarning("Unit " + name + " not found!");
        return null;
    }

    public Tactic getTacticByName(string name)
    {
        foreach(Tactic tactic in Tactics)
        {
            if(tactic.name == name)
            {
                return tactic;
            }
        }
        Debug.LogWarning("Tactic "+ name + " not found");
        return null;
    }
    public Terrain getTerrainByName(string name)
    {
        foreach (Terrain terrain in Terrains)
        {
            if(terrain.name == name){return terrain;}
        }    
        Debug.LogWarning("Terrain " + name + " not found!");
        return null;
    }
//for the player to resolve tactics. Opponent's tactics are resolved in Opponent.cs
    public void resolveTacticByName(string name)
    {
        BattlefieldManager manager = BattlefieldManager.getInstance();
        if (manager == null)
        {
            Debug.LogError("Cannot resolve tactic '" + name + "' without a BattlefieldManager.");
            return;
        }

        switch (name)
        {
            case "The Forked Advance":
                manager.selectAnyUnit(name, (tile, unit) =>
                    manager.selectAdjacentDestination(
                        tile,
                        unit,
                        name,
                        destination => manager.damageUnitsAdjacentTo(destination, 1)));
                break;
            case "The Blazing Gambit":
                ResolveBlazingGambit(manager);
                break;
            case "Sudden Shield":
                manager.selectAnyUnit(
                    name,
                    (tile, unit) => BattlefieldManager.IsPlayerUnitOnTile(tile, unit),
                    (tile, unit) => manager.AddTemporaryStatModifiers(unit, 0, 2));
                break;
            case "Draconic Flare":
                manager.selectAnyUnit(
                    name,
                    (tile, unit) =>
                        unit.type == Element.FIRE &&
                        BattlefieldManager.IsPlayerUnitOnTile(tile, unit),
                    (tile, unit) =>
                    {
                        unit.ATTACK += 1;
                        manager.damageAllUnits(unit.ATTACK, unit);
                    });
                break;
            case "Ruinous Rampage":
                manager.selectAnyUnit(
                    name,
                    (tile, unit) =>
                        unit.type == Element.RUIN &&
                        BattlefieldManager.IsPlayerUnitOnTile(tile, unit),
                    (tile, unit) =>
                    {
                        manager.AddTemporaryStatModifiers(unit, 3, 0);
                        manager.damageUnitsAdjacentTo(tile, unit.ATTACK);
                    });
                break;
            case "High Tide":
                Player player = Player.GetInstance();
                if (player == null)
                {
                    Debug.LogError("Cannot resolve High Tide because no Player instance exists.");
                    return;
                }

                player.Aether += 4;
                break;
            case "Water Blast":
                manager.selectAnyUnitTile(name, (tile, unit) =>
                    manager.damageUnitAt(tile, unit, 3));
                break;
            case "Knowledge Vial":
                ResolveKnowledgeVial(manager);
                break;
            case "Burning Flare":
                manager.damageOpponent(4);
                break;
            default:
                Debug.LogWarning("No tactic effect is defined for '" + name + "'.");
                break;
        }
    }

    private static void ResolveBlazingGambit(BattlefieldManager manager)
    {
        Tile[,] battlefield = BattlefieldManager.battleField;
        HashSet<Unit> unitsToDamage = new HashSet<Unit>();

        for (int x = 1; x < battlefield.GetLength(0); x++)
        {
            for (int y = 1; y < battlefield.GetLength(1); y++)
            {
                Tile terrainTile = battlefield[x, y];
                if (terrainTile == null ||
                    terrainTile.curTerrain == null ||
                    terrainTile.curTerrain.type != Element.FIRE)
                {
                    continue;
                }

                AddNonFireUnitsAt(battlefield, x - 1, y, unitsToDamage);
                AddNonFireUnitsAt(battlefield, x + 1, y, unitsToDamage);
                AddNonFireUnitsAt(battlefield, x, y - 1, unitsToDamage);
                AddNonFireUnitsAt(battlefield, x, y + 1, unitsToDamage);
            }
        }

        foreach (Unit unit in unitsToDamage)
        {
            manager.damageUnit(unit, 1);
        }

        for (int x = 1; x < battlefield.GetLength(0); x++)
        {
            for (int y = 1; y < battlefield.GetLength(1); y++)
            {
                Tile tile = battlefield[x, y];
                if (tile == null)
                {
                    continue;
                }

                if (tile.unitHere != null &&
                    tile.playerControlsGroundUnit &&
                    tile.unitHere.type == Element.FIRE)
                {
                    tile.unitHere.ATTACK += 1;
                }

                if (tile.unitFlyingHere != null &&
                    tile.playerControlsFlyingUnit &&
                    tile.unitFlyingHere.type == Element.FIRE)
                {
                    tile.unitFlyingHere.ATTACK += 1;
                }
            }
        }
    }

    private static void AddNonFireUnitsAt(
        Tile[,] battlefield,
        int x,
        int y,
        HashSet<Unit> unitsToDamage)
    {
        if (x < 1 || x >= battlefield.GetLength(0) ||
            y < 1 || y >= battlefield.GetLength(1))
        {
            return;
        }

        Tile tile = battlefield[x, y];
        if (tile == null)
        {
            return;
        }

        if (tile.unitHere != null && tile.unitHere.type != Element.FIRE)
        {
            unitsToDamage.Add(tile.unitHere);
        }

        if (tile.unitFlyingHere != null && tile.unitFlyingHere.type != Element.FIRE)
        {
            unitsToDamage.Add(tile.unitFlyingHere);
        }
    }

    private static void ResolveKnowledgeVial(BattlefieldManager manager)
    {
        Player player = Player.GetInstance();
        if (player == null)
        {
            Debug.LogError("Cannot resolve Knowledge Vial because no Player instance exists.");
            return;
        }

        for (int i = 0; i < 3; i++)
        {
            Card drawnCard = player.drawCard();
            if (drawnCard != null)
            {
                player.curhand.Add(drawnCard);
            }
        }

        manager.updateHand();
    }

    public Unit resolveTerrainOnUnit(string terrainName, Unit unit)
    {
        return resolveTerrainOnUnit(null, terrainName, unit);
    }

    public Unit resolveTerrainOnUnit(Tile tile, Unit unit)
    {
        return resolveTerrainOnUnit(tile, tile != null && tile.curTerrain != null
            ? tile.curTerrain.name
            : null, unit);
    }

    private Unit resolveTerrainOnUnit(Tile tile, string terrainName, Unit unit)
    {
        if (unit == null)
        {
            Debug.LogError("Cannot resolve terrain effects for a null Unit.");
            return null;
        }

        int attackModifier = 0;
        int defenseModifier = 0;
        switch (terrainName)
        {
            case "Mountains":
                if (unit.type == Element.EARTH || unit.type == Element.WIND)
                {
                    defenseModifier++;
                }
                break;
            case "Ocean":
                if (unit.type == Element.WATER)
                {
                    defenseModifier++;
                }
                break;
            case "Frozen Lake":
                if (unit.type != Element.WATER)
                {
                    defenseModifier--;
                }
                break;
            case "Desert":
                if (unit.type == Element.FIRE)
                {
                    attackModifier++;
                }
                break;
            case "Thunderfalls":
                if (unit.type == Element.LIGHTNING &&
                    unit.ability != Ability.RANGE1 &&
                    unit.ability != Ability.RANGE2 &&
                    unit.ability1 != Ability.RANGE1 &&
                    unit.ability1 != Ability.RANGE2)
                {
                    attackModifier++;
                }
                break;
        }

        if (tile != null && (unit.type == Element.RUIN || unit.type == Element.NATURE))
        {
            Tile[,] battlefield = BattlefieldManager.battleField;
            AddTempleDefenseModifier(battlefield, tile.x - 1, tile.y, ref defenseModifier);
            AddTempleDefenseModifier(battlefield, tile.x + 1, tile.y, ref defenseModifier);
            AddTempleDefenseModifier(battlefield, tile.x, tile.y - 1, ref defenseModifier);
            AddTempleDefenseModifier(battlefield, tile.x, tile.y + 1, ref defenseModifier);
        }

        unit.SetTerrainStatModifiers(attackModifier, defenseModifier);
        return unit;
    }

    private static void AddTempleDefenseModifier(
        Tile[,] battlefield,
        int x,
        int y,
        ref int defenseModifier)
    {
        if (x >= 1 && x < battlefield.GetLength(0) &&
            y >= 1 && y < battlefield.GetLength(1) &&
            battlefield[x, y] != null &&
            battlefield[x, y].curTerrain != null &&
            battlefield[x, y].curTerrain.name == "Ancient Temple")
        {
            defenseModifier++;
        }
    }

}