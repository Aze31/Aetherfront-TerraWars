using System.Collections.Generic;
using UnityEngine;
public class Opponent : MonoBehaviour
{
    public List<Card> curDeck = new List<Card>();
    public double Health = 25;
    public double Aether;
    private CardLibrary lib;

    private void Start()
    {
        lib = CardLibrary.getInstance();
    }

    public void initDeckOne() // this is meant to be the first deck that the player battles against
    {
        curDeck.AddRange(new Card[]
        {
            lib.getUnitByName("Ember Drake"),
            lib.getUnitByName("Ember Drake"),
            lib.getUnitByName("Ember Drake"),
            lib.getUnitByName("Ember Drake"),
        });
    }
    public void prepForBattle(int num)
    {
        if(num == 1){initDeckOne();}
    }

}