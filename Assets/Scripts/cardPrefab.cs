using System;
using TMPro;
using UnityEngine;
using static Card;
using static CardLibrary;

public class cardPrefab : MonoBehaviour
{
    //TODO: add size modifier for the entire card's size based on the terrain/unit size
    public enum placement
    {
        HAND,
        BATTLE,
        DISCARD
    }
    public Card curCard;
    public GameObject border; //to set color
    public TextMeshProUGUI AttackText, DefenseText, AetherText, SizeText, Abilitytext, nameText;
    private Boolean blank; //is the card blank?
    private Sprite curSprite;
    public void setCardTo(Card card)
    {
        curCard = card;
        nameText.text = card.name;
        AetherText.text = card.cost.ToString();
        if(card is Unit unit) //the one time c# is genius
        {
            AttackText.text = unit.ATTACK.ToString();
            DefenseText.text = unit.DEFENSE.ToString();
            SizeText.text = unit.size.ToString();
        }
        if(card is Terrain terrain)
        {
            SizeText.text = terrain.size.ToString();
            Abilitytext.text = terrain.effect.ToString();
        }
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
