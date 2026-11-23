using System;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using static Card;
using static CardLibrary;

public class cardPrefab : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    //TODO: later, add size modifier for the entire card's size based on the terrain/unit size
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
    public GameObject aText, dText, sText, abText; // these are the objects for enabling/disableing
    public GameObject imageObject;
    private Boolean blank; //is the card blank? ig
    private Sprite curSprite;
    public placement place;
    private Vector2 originalPose;
    private bool beingHeld = false;
    private Vector3 originalScale;
    private bool pointerOverDisplayCard;
    
    public void setCardTo(Card card)
    {
        curCard = card;
        nameText.text = card.name;
        AetherText.text = card.cost.ToString();
        if(card is Unit unit) //c# is genius
        {
            AttackText.text = unit.ATTACK.ToString();
            DefenseText.text = unit.DEFENSE.ToString();
            SizeText.text = unit.size.ToString();
            abText.SetActive(false);
        }
        if(card is Terrain terrain)
        {
            SizeText.text = terrain.size.ToString();
            // abText.SetActive(true);
            aText.SetActive(false);
            dText.SetActive(false);
            Abilitytext.text = terrain.effect.ToString();
        }
        if(card is Tactic tactic)
        { //record the first ability for now, fix later
            Abilitytext.text = tactic.abilities[0];
            aText.SetActive(false);
            dText.SetActive(false);
            sText.SetActive(false);
            //abText.SetActive(true);
        }
        //get rid of the if here when you get enough sprites for all cards and are more certain
        if(card.name == "Aqua Scout" || card.name == "Ocean" || card.name == "Beach")
        {
            imageObject.GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("Sprites/Beach");
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
        originalScale = transform.localScale;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (place != placement.DISPLAY || pointerOverDisplayCard)
        {
            return;
        }

        originalScale = transform.localScale;
        transform.localScale = originalScale * 1.05f;
        pointerOverDisplayCard = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!pointerOverDisplayCard)
        {
            return;
        }

        transform.localScale = originalScale;
        pointerOverDisplayCard = false;
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
