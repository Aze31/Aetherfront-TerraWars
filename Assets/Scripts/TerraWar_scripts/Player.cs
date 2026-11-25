using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    private static Player instance;

    public static Player GetInstance()
    {
        return instance;
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;

        // Optional if you want the player object to survive scene loads
        DontDestroyOnLoad(gameObject);
    }
    public string playerName; //prompt user for this at the beginning of the game
    public List<Card> allCards = new List<Card>(); //collection (deck and outside)
    public List<Card> cardCollection = new List<Card>(); //all cards minus deck
    public List<Card> curDeck = new List<Card>(); //deck
    public double Health = 50; //dead if below zero
    public double Aether;
    private CardLibrary lib = CardLibrary.getInstance();
    public cardPrefab currentlyDragging = null;

    //for battle
    public List<Card> curhand = new List<Card>();
    public List<Card> battleDeck = new List<Card>();

    void Start()
    {
        createStartingDeck();
        createStartingCollection();
        initBattleDeck();
    }
    void Update()
    {
        
    }
    public Card drawCard()
    {
        System.Random rand = new System.Random();
        // Card randomCard = battleDeck[rand.Next(battleDeck.Count)];
        Card randomCard = battleDeck[Random.Range(1,battleDeck.Count)];
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
        //tune this deck in the sims
        addToDeck(lib.getTerrainByName("Ocean"), 4);
        addToDeck(lib.getUnitByName("Aqua Scout"), 4);
        addToDeck(lib.getUnitByName("Brave Seagull"), 3);
        addToDeck(lib.getUnitByName("Tidebender Serpent"), 4);
        addToDeck(lib.getUnitByName("Great Green Shark"), 2);
        addToDeck(lib.getTerrainByName("Beach"),6);
        addToDeck(lib.getTerrainByName("Frozen Lake"),4);
        addToDeck(lib.getTacticByName("High Tide"),2);
        addToDeck(lib.getTacticByName("Sudden Shield"),2);
        addToDeck(lib.getTerrainByName("Marsh"),3);
        //above is 35
    }
    public void createStartingCollection()
    {
        //in the player's collection but not inside their starting deck
        addToCollection(lib.getUnitByName("Grove Sentinel"), 3);
        addToCollection(lib.getUnitByName("Dwarf"), 3);
        addToCollection(lib.getTerrainByName("Verdant Meadow"), 4);

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
    public void DrawHand()
    {
        //a full 6 starting hand
        List<Card> tempList = new List<Card>
        {
            drawCard(),drawCard(),drawCard(),drawCard(),drawCard(),drawCard()
        };
        curhand = tempList;
        foreach(Card card in curhand)
        {
            Debug.Log("you drew " + card.name + " in starting hand.");
        }
    }
}
