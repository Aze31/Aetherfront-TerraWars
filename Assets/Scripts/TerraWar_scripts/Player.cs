using System.Collections.Generic;
using UnityEngine;
//track all of the user's information in this class
public class Player : MonoBehaviour
{
    public string playerName; //prompt user for this at the beginning of the game
    public List<Card> allCards = new List<Card>(); //collection (deck and outside)
    public List<Card> cardCollection = new List<Card>(); //all cards minus deck
    public List<Card> curDeck = new List<Card>(); //deck
    public double Health;
    public double Aether;

    //for battle
    public List<Card> curhand = new List<Card>();
    public List<Card> battleDeck = new List<Card>();

    void Start(){}
    void Update()
    {
        addToCollection(CardLibrary.getUnitByName("Blazeborn Charger"),4);
        addToCollection(CardLibrary.getTacticByName("The Forked Advance"),2);
    }
    public Card drawCard()
    {
        System.Random rand = new System.Random();
        Card randomCard = battleDeck[rand.Next(1,battleDeck.Count)];
        battleDeck.Remove(randomCard);
        return randomCard;
    }

    public Card getCardByName(string name)
    {
        //from current deck state
        foreach(Card card in battleDeck)
        {
            if(card.name == name)
            {
                return card;
            }
        }
        return null;
    }

    public void addToDeck(Card card, int copies)
    {
        for(int i=0; i < copies; i++)
        {
            curDeck.Add(card);
        }
    }
    public void addToCollection(Card card, int copies)
    {
        for(int i = 0; i < copies; i++){cardCollection.Add(card);}
    }
    public void initBattleDeck(){battleDeck = curDeck;}
    public void loseHealth(int h){Health -= h;}
    public void payAether(int a){
        if(Aether >= a){Aether -=a;} 
        else Debug.LogWarning("Can't pay Aether!");
    }
    public void createStartingDeck()
    {
        addToDeck(CardLibrary.getTerrainByName("Volcano"), 4);
        addToDeck(CardLibrary.getUnitByName("Ember Drake"), 4);
        addToDeck(CardLibrary.getTacticByName("The Forked Advance"), 2);
    }
    public void createStartingCollection()
    {
        addToCollection(CardLibrary.getUnitByName("Dwarf"),2);
        addToCollection(CardLibrary.getUnitByName("Ember Drake"),3);
    }

    //now for gameplay decision methods
    public void activateCard(Card card)
    {
        payAether(card.cost);
        //extra effects specific to a card go here from the lib
    }

    public void placeTerrain(int x, int y, int size, Terrain terrain)
    {
        
    }
    public List<Card> giveUsableCardsFromHand()
    {
        List<Card> availableCards = new List<Card>();
        foreach(Card card in curhand)
        {
            if (card.cost <= Aether)
            {
                availableCards.Add(card);
            }
        }
        return availableCards;
    }
}
