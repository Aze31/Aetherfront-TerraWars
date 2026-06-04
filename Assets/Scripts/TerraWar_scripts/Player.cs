using UnityEngine;
using static Card;
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using System;

public class Player
{
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

}