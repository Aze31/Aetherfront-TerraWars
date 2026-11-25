using System.Collections.Generic;
using UnityEngine;
public class Opponent : MonoBehaviour
{
    //this opponent behaviour script is simplistic and actions it takes are mostly random
    public List<Card> curDeck = new List<Card>();
    public List<Card> curHand = new List<Card>();
    public double Health = 25;
    public double Aether;
    private CardLibrary lib;

    private void Start()
    {
        lib = CardLibrary.getInstance();
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
        System.Random rand = new System.Random();
        // Card randomCard = battleDeck[rand.Next(battleDeck.Count)];
        Card randomCard = curDeck[Random.Range(1, curDeck.Count)];
        curDeck.Remove(randomCard);
        return randomCard;
    }

//sequence: first on start draw and place terrain at random (the first one you can pay for) on a legal position at random.
    public void MyTurnStart()
    {
        curHand.Add(drawCard());
        foreach(Card card in curHand)
        {
            if(card is Terrain terrain && terrain.cost <= Aether){
                
            }
        }
    }
}