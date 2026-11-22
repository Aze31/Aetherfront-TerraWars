using System;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using static Card;
using static CardLibrary;

public class cardPrefab : MonoBehaviour
{
    //TODO: add size modifier for the entire card's size based on the terrain/unit size
    public enum placement
    {
        HAND,
        BATTLE,
        DISCARD,
        DISPLAY
    }
    public Card curCard;
    public GameObject border; //to set color
    public TextMeshProUGUI AttackText, DefenseText, AetherText, SizeText, Abilitytext, nameText;
    private Boolean blank; //is the card blank? ig
    private Sprite curSprite;
    public placement place;
    private Vector2 originalPose;
    private bool beingHeld = false;
    
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
        if(card is Tactic tactic)
        { //record the first ability for now, fix later
            Abilitytext.text = tactic.abilities[0];
        }
    }
    public void resetSize() //<------------
    {
        //check if card size is not 1 and then change its battiefield appearance
    }
    void Start()
    {
        //track its original position(good since it always re-inits all cards)
        originalPose = transform.position;
    }
    void Update()
    {
        if(place == placement.HAND)
        {
            // if in hand, make the object draggable

        }
    }
    //also im adding in root and parent to 
    public void OnBeginDrag(PointerEventData eventData)
    {
        //will add other conditions later
        if(place == placement.HAND){
            beingHeld = true;
            originalPose = transform.position;
        }

    }
    public void OnDrag(PointerEventData eventData)
    {
        if(place == placement.HAND)
        {
            transform.position = Input.mousePosition;
        }
    }
    //now based on end pose snap back to original pose once let go
    public void OnEndDrag(PointerEventData eventData)
    {
        beingHeld = false;
        transform.position = originalPose;
    }

}
