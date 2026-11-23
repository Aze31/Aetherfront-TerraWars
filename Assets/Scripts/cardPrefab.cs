using System;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static Card;
using static CardLibrary;

public class cardPrefab : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler,
    IBeginDragHandler, IDragHandler, IEndDragHandler
{
    //TODO: later, add size modifier for the entire card's size based on the terrain/unit size
    public enum placement
    {
        HAND,
        BATTLE,
        DISCARD,
        DISPLAY,
        COLLECTION,
        DECK
    }
    public Card curCard;
    public GameObject border; //to set color
    public TextMeshProUGUI AttackText, DefenseText, AetherText, SizeText, Abilitytext, nameText;
    public GameObject aText, dText, sText, abText; // these are the objects for enabling/disableing
    public GameObject imageObject;
    private Boolean blank; //is the card blank? ig
    private Sprite curSprite;
    public placement place;
    public bool isDraggable;
    private CollectionUIManager collectionUIManager;
    private Transform originalParent;
    private Vector3 originalLocalPosition;
    private Quaternion originalLocalRotation;
    private Vector3 originalLocalScale;
    private int originalSiblingIndex;
    private RectTransform dragLayer;
    private Canvas dragCanvas;
    private CanvasGroup dragCanvasGroup;
    private bool originalBlocksRaycasts;
    private bool addedCanvasGroup;
    private bool isDragging;
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
        originalScale = transform.localScale;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if ((place != placement.DISPLAY &&
             place != placement.COLLECTION &&
             place != placement.DECK) ||
            pointerOverDisplayCard)
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
    public void ConfigureDrag(CollectionUIManager manager, bool canDrag)
    {
        collectionUIManager = manager;
        isDraggable = canDrag;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!isDraggable ||
            collectionUIManager == null ||
            (place != placement.COLLECTION && place != placement.DECK))
        {
            return;
        }

        Canvas parentCanvas = GetComponentInParent<Canvas>();
        if (parentCanvas == null || parentCanvas.rootCanvas == null)
        {
            Debug.LogError("Draggable cards must be placed under a Canvas.", this);
            return;
        }

        dragCanvas = parentCanvas.rootCanvas;
        dragLayer = dragCanvas.transform as RectTransform;
        if (dragLayer == null)
        {
            Debug.LogError("The card's root Canvas must have a RectTransform.", dragCanvas);
            return;
        }

        originalParent = transform.parent;
        originalLocalPosition = transform.localPosition;
        originalLocalRotation = transform.localRotation;
        originalLocalScale = transform.localScale;
        originalSiblingIndex = transform.GetSiblingIndex();

        dragCanvasGroup = GetComponent<CanvasGroup>();
        addedCanvasGroup = dragCanvasGroup == null;
        if (addedCanvasGroup)
        {
            dragCanvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        originalBlocksRaycasts = dragCanvasGroup.blocksRaycasts;
        dragCanvasGroup.blocksRaycasts = false;
        transform.SetParent(dragLayer, true);
        transform.SetAsLastSibling();
        isDragging = true;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isDragging)
        {
            return;
        }

        if (RectTransformUtility.ScreenPointToWorldPointInRectangle(
                dragLayer,
                eventData.position,
                dragCanvas.worldCamera,
                out Vector3 worldPosition))
        {
            transform.position = worldPosition;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!isDragging)
        {
            return;
        }

        isDragging = false;
        if (collectionUIManager.TryTransferCard(curCard, place, eventData.position))
        {
            Destroy(gameObject);
            return;
        }

        transform.SetParent(originalParent, false);
        transform.localPosition = originalLocalPosition;
        transform.localRotation = originalLocalRotation;
        transform.localScale = originalLocalScale;
        transform.SetSiblingIndex(originalSiblingIndex);
        dragCanvasGroup.blocksRaycasts = originalBlocksRaycasts;
        if (addedCanvasGroup)
        {
            Destroy(dragCanvasGroup);
        }
    }

}
