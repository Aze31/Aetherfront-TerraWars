using System.Collections.Generic;
using static CardLibrary;
public class Card
{
    public string name;
    public int cost;
}

public class Unit : Card
{
    public int size;
    public int DEFENSE;
    public int ATTACK;
    public Element type;
    public Ability ability;
    public Unit(string name, int s, int d, int a, int cost, Element type)
    {
        ATTACK = a;
        DEFENSE = d;
        size = s;
        this.name = name;
        this.cost = cost;
        ability = Ability.NONE;
    }
    public Unit(string name, int s, int d, int a, int cost, Element type, Ability ability)
    {
        ATTACK = a;
        DEFENSE = d;
        size = s;
        this.name = name;
        this.cost = cost;
        this.ability = ability;
    }

}

public class Tactic : Card
{
    public List<string> abilities = new List<string>();
    public Tactic(string name, int cost, string[] abilities)
    {
        this.name = name;
        this.cost = cost;
        foreach(string ability in abilities)
        {
            this.abilities.Add(ability);
        }
    }
}

public class Terrain : Card
{
    public int size;
    public string effect;
    public Element type;
    public PassCondition condition;
    public Element overlayType;
    //use this constructor if anyone can pass the terrain
    public Terrain(string name, int size, Element type, string effect, int cost)
    {
        this.name = name;
        this.size = size;
        this.type = type;
        this.effect = effect;
        this.cost = cost;
        condition = PassCondition.NONE;
    }
    //use this one to specify
    public Terrain(string name, int size, Element type, string effect, int cost, PassCondition pass)
    {
        this.name = name;
        this.size = size;
        this.type = type;
        this.effect = effect;
        this.cost = cost;
        condition = pass;
    }
    //to add an overlay type:
    public Terrain(string name, int size, Element type, string effect, int cost, Element overlayType)
    {
        this.name = name;
        this.size = size;
        this.type = type;
        this.effect = effect;
        this.cost = cost;
        this.overlayType = overlayType;
        condition = PassCondition.NONE;
    }
    //for both:
    public Terrain(string name, int size, Element type, string effect, int cost, PassCondition pass, Element overlayType)
    {
        this.name = name;
        this.size = size;
        this.type = type;
        this.effect = effect;
        this.cost = cost;
        condition = pass;
        this.overlayType = overlayType;
    }
}