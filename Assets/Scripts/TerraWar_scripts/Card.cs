//just set up each card object in this class, and each card type.
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
    public Unit( string name, int s, int d, int a, int cost)
    {
        ATTACK = a;
        DEFENSE = d;
        size = s;
        this.name = name;
        this.cost = cost;
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
    public Terrain(string name, int size, Element type, string effect, int cost)
    {
        this.name = name;
        this.size = size;
        this.type = type;
        this.effect = effect;
        this.cost = cost;
    }
}