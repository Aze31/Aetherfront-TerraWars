using System;
using UnityEngine;
using static Card;
using static CardLibrary;

public class cardPrefab : MonoBehaviour
{
    public enum placement
    {
        HAND,
        BATTLE,
        DISCARD
    }
    public Card curCard;
    public Boolean blank; //is the card blank?
    public Sprite curSprite;
    public void setCardTo(Card card)
    {
        curCard = card;
    }
    void Start()
    {
        
    }
    void Update()
    {
        if(curCard != null){
        //set sprite based on curCard 
        }
    }

}
