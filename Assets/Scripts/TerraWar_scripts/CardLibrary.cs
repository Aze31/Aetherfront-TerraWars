using UnityEngine;
using System.Collections.Generic;
public static class CardLibrary
{
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
        //to differentiate between land+ocean and ocean
        SWIM,
        SWIMONLY,
        RANGE1,
        RANGE2,
        NONE
    }
    public enum PassCondition
    {
        SWIM,
        DIG,
        FLYING,
        NONE // default i think
    }
    public static List<Unit> Units = new List<Unit>();
    public static List<Tactic> Tactics = new List<Tactic>();
    public static List<Terrain> Terrains = new List<Terrain>();
    
    public static void initializeLibrary()
    {        
        //name, size, defense, attack, cost
        Units.AddRange(new Unit[]
        {
            new Unit("Ember Drake", 1, 1, 2, 1, Element.FIRE, Ability.FLYING),
            new Unit("Aqua Scout",1,2,1,1, Element.WATER), //should this guy know how to swim?
            new Unit("Grove Sentinel", 1, 3, 1, 1, Element.NATURE),
            new Unit("Grave-Burrow Lurker", 1, 2, 2, 1, Element.RUIN, Ability.DIG),
            new Unit("Ancient Ruins Dragon",5,5,3,4,Element.RUIN, Ability.FLYING),
            new Unit("Blazehorn Charger", 2, 2, 3, 1, Element.FIRE),
            new Unit("Tidebender Serpent", 2, 3, 2, 2, Element.WATER, Ability.SWIMONLY),
            new Unit("Catacomb Stalker", 2, 2, 2, 1, Element.RUIN),
            new Unit("AshArrow Archer",2,1,2,1,Element.FIRE, Ability.RANGE1),
            new Unit("Keen-eyed Shark",1,3,2,3,Element.WATER, Ability.SWIMONLY),
            new Unit("Airborne Guardian", 1,1,3,2,Element.WIND, Ability.FLYING),
        });
        Terrains.AddRange(new Terrain[]
        {
            //name, size, type, effect, cost, passCondition/overlay type
            new Terrain("Volcano", 1, Element.FIRE, "Fire Units here gain +1 Attack.", 0, Element.WATER),
            new Terrain("Mountains",2,Element.EARTH, "Earth and Wind Units here gain +1 Defense.", 1),
            new Terrain("Ocean", 1, Element.WATER, "Water Units here gain +1 Defense.", 0, PassCondition.SWIM),
            new Terrain("Grave Ruins", 1, Element.RUIN, "Ruin Units you control take up no space here.", 0),
            new Terrain("Thunderbroken Falls", 2, Element.LIGHTNING, "Lightning and Water Units here gain Swim.",1, PassCondition.SWIM),
            new Terrain("Ancient Temple", 1, Element.RUIN, "Ruin and Nature Units next to this gain +1 Defense.", 0, Element.NATURE),
            new Terrain("Desert",2, Element.FIRE, "Fire Units here gain +1 Defense",0),
            new Terrain("Thunderfalls",1,Element.LIGHTNING,"Lightning Units here (with RANGE0) gain RANGE1 and +1 Attack",0),
        });
        Tactics.AddRange(new Tactic[]
        {
            //name, cost, list of effects
            new Tactic("The Forked Advance", 2, new string[]{"Selected Unit moves one space.", "All Units next to it then take 1 damage."}),
            new Tactic("The Blazing Gambit", 2, new string[]{"All Units next to a Fire Terrain that are not a Fire type take 1 damage.", "All Fire Units you control gain +1 Attack."}),
            new Tactic("Sudden Shield", 1, new string[]{"Selected Unit you control gains +2 Defense this turn."}),
            new Tactic("Ruinous Rampage", 2, new string[]{"Selected Ruin Unit you control gains +3 Attack this turn and attacks every unit next to it."}),
        });
    }

    public static Unit getUnitByName(string name){
        foreach (Unit unit in Units)
        {
            if(unit.name == name){return unit;}
        }    
        Debug.LogWarning("Unit" + name + " not found!");
        return null;
    }

    public static Tactic getTacticByName(string name)
    {
        foreach(Tactic tactic in Tactics)
        {
            if(tactic.name == name)
            {
                return tactic;
            }
        }
        return null;
    }
    public static Terrain getTerrainByName(string name)
    {
        foreach (Terrain terrain in Terrains)
        {
            if(terrain.name == name){return terrain;}
        }    
        UnityEngine.Debug.LogWarning("Terrain" + name + " not found!");
        return null;
    }

}