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

    }
    public static List<Unit> Units = new List<Unit>();
    public static List<Tactic> Tactics = new List<Tactic>();
    public static List<Terrain> Terrains = new List<Terrain>();


    public static void initializeLibrary()
    {
        //name, cost, defense, attack, size
        Units.Add(new Unit("Ember Drake", 1, 1, 2, 2));
        Units.Add(new Unit("Aqua Falcon",));
        
    }

    public static Unit getUnitByName(string name){
        foreach (Unit unit in Units)
        {
            if(unit.name == name){return unit;}
        }    
        UnityEngine.Debug.LogWarning("Unit" + name + " not found!");
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
    public static Terrain GetTerrainByName(string name)
    {
        foreach (Terrain terrain in Terrains)
        {
            if(terrain.name == name){return terrain;}
        }    
        UnityEngine.Debug.LogWarning("Terrain" + name + " not found!");
        return null;
    }

}