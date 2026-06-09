using System.Collections.Generic;

public class Player
{
    public string name; //prompt user for this at the beginning of the game
    public List<Card> allCards = new List<Card>(); //collection (deck and outside)
    public List<Card> cardCollection = new List<Card>(); //all cards - deck
    public List<Card> curDeck = new List<Card>(); //deck
    public double Health;
    public double Aether;

    //for battle
    public List<Card> curhand = new List<Card>();
    public List<Card> battleDeck = new List<Card>();
    
    public Card drawCard()
    {
        System.Random rand = new System.Random();
        Card randomCard = battleDeck[rand.Next(1,battleDeck.Count)];
        battleDeck.Remove(randomCard);
        return randomCard;
    }

    public Card getCardByName(string name)
    {
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
    public void payAether(int a){Aether -=a;}
    public void createStartingDeck()
    {
        addToDeck(CardLibrary.getTerrainByName("Volcano"), 4);
        addToDeck(CardLibrary.getUnitByName("Ember Drake"), 4);
        addToDeck(CardLibrary.getTacticByName("The Forked Advance"), 2);
    }
    public void createStartingCollection()
    {
        addToCollection(CardLibrary.getUnitByName("Dwarf"),2);
    }

}