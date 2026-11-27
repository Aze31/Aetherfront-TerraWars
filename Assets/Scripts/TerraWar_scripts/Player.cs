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
    public int collectionLevel = 1;
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
    public Card drawCard()
    {
        if (battleDeck.Count == 0)
        {
            Debug.LogWarning("The player's battle deck is empty.", this);
            return null;
        }

        int cardIndex = Random.Range(0, battleDeck.Count);
        Card randomCard = battleDeck[cardIndex];
        battleDeck.RemoveAt(cardIndex);
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
    public void rewardCollection()
    {
        if(collectionLevel == 1)
        {
            collectionLevel++;
            addToCollection(lib.getUnitByName("Mystic Elf"), 3);
            addToCollection(lib.getUnitByName("Blazehorn Charger"), 3);
            addToCollection(lib.getUnitByName("Desert"), 1);
        }
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
    public bool payAether(int a){
        if(Aether >= a)
        {
            Aether -= a;
            return true;
        }

        Debug.LogWarning("Can't pay Aether!");
        return false;
    }
    public void createStartingDeck()
    {
        //the player should tune this deck before starting g1
        addToDeck(lib.getTerrainByName("Ocean"), 6);
        addToDeck(lib.getUnitByName("Aqua Scout"), 4);
        addToDeck(lib.getUnitByName("Brave Seagull"), 3);
        addToDeck(lib.getUnitByName("Tidebender Serpent"), 4);
        addToDeck(lib.getUnitByName("Great Green Shark"), 2);
        addToDeck(lib.getTerrainByName("Beach"), 6);
        addToDeck(lib.getTerrainByName("Frozen Lake"), 4);
        addToDeck(lib.getTacticByName("High Tide"), 2);
        addToDeck(lib.getTacticByName("Sudden Shield"), 2);
        addToDeck(lib.getTerrainByName("Marsh"), 3);
        //above is 37
    }
    public void createStartingCollection()
    {
        //in the player's collection but not inside their starting deck
        addToCollection(lib.getUnitByName("Grove Sentinel"), 3);
        addToCollection(lib.getUnitByName("Dwarf"), 3);
        addToCollection(lib.getTerrainByName("Verdant Meadow"), 4);
        addToCollection(lib.getTacticByName("Knowledge Vial"), 2);
        addToCollection(lib.getTacticByName("Water Blast"), 2);
        addToCollection(lib.getUnitByName("Marsh Serpent"), 4);
        addToCollection(lib.getTerrainByName("Ocean"), 2);
    }

    //now for gameplay decision methods
    public void activateCard(Card card)
    {
        if (!payAether(card.cost))
        {
            return;
        }
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
