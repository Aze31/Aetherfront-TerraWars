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
    private const float CardSpriteBoxWidthRatio = 0.5f;
    private const float CardSpriteBoxHeightRatio = 0.45f;

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
    public GameObject aText, dText, sText, abText; // these are the objects for enabling/disabling
    public GameObject imageObject;
    [SerializeField] private Sprite aquaScoutSprite;
    [SerializeField] private Sprite beachSprite;
    [SerializeField] private Sprite oceanSprite;
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
        SpriteRenderer cardImageRenderer = imageObject != null
            ? imageObject.GetComponent<SpriteRenderer>()
            : null;
        Sprite cardSprite = null;
        if (card.name == "Aqua Scout")
        {
            cardSprite = aquaScoutSprite;
        }
        else if (card.name == "Ocean")
        {
            cardSprite = oceanSprite;
        }
        else if (card.name == "Beach")
        {
            cardSprite = beachSprite;
        }

        if (cardImageRenderer != null)
        {
            if (cardSprite != null)
            {
                cardImageRenderer.sprite = cardSprite;
            }

            FitSpriteToCardBox(cardImageRenderer);
        }

        curSprite = cardImageRenderer != null ? cardImageRenderer.sprite : null;
    }

    private void FitSpriteToCardBox(SpriteRenderer cardImageRenderer)
    {
        Sprite sprite = cardImageRenderer.sprite;
        RectTransform cardBox = cardImageRenderer.transform.parent as RectTransform;
        if (sprite == null || cardBox == null)
        {
            return;
        }

        Vector2 spriteSize = sprite.bounds.size;
        Vector2 boxSize = cardBox.rect.size;
        if (spriteSize.x <= 0f || spriteSize.y <= 0f ||
            boxSize.x <= 0f || boxSize.y <= 0f)
        {
            Debug.LogError("Card sprite and image box dimensions must be non-zero.", cardImageRenderer);
            return;
        }

        float scale = Mathf.Min(
            boxSize.x * CardSpriteBoxWidthRatio / spriteSize.x,
            boxSize.y * CardSpriteBoxHeightRatio / spriteSize.y);
        Vector3 imageScale = cardImageRenderer.transform.localScale;
        cardImageRenderer.transform.localScale = new Vector3(scale, scale, imageScale.z);
    }

    public Sprite GetCardSprite()
    {
        return curSprite;
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
             place != placement.HAND) ||
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
        bool isHandCard = place == placement.HAND;
        bool isCollectionCard = isDraggable &&
            collectionUIManager != null &&
            (place == placement.COLLECTION || place == placement.DECK);
        if (!isHandCard && !isCollectionCard)
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
        Player player = Player.GetInstance();
        if (player != null)
        {
            player.currentlyDragging = this;
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isDragging)
        {
            return;
        }

        Player player = Player.GetInstance();
        if (player != null)
        {
            player.currentlyDragging = this;
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
        if (place == placement.HAND)
        {
            if (!TilePrefab.TryPlaceCardAtScreenPosition(
                eventData.position,
                dragCanvas.worldCamera,
                this))
            {
                RestoreAfterFailedDrag();
                return;
            }

            Player placingPlayer = Player.GetInstance();
            if (placingPlayer != null)
            {
                placingPlayer.curhand.Remove(curCard);
                placingPlayer.currentlyDragging = null;
            }

            place = placement.BATTLE;
            gameObject.SetActive(false);
            return;
        }

        if (collectionUIManager != null &&
            collectionUIManager.TryTransferCard(curCard, place, eventData.position))
        {
            Player transferPlayer = Player.GetInstance();
            if (transferPlayer != null)
            {
                transferPlayer.currentlyDragging = null;
            }

            Destroy(gameObject);
            return;
        }

        RestoreAfterFailedDrag();
    }

    private void RestoreAfterFailedDrag()
    {
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
        Player player = Player.GetInstance();
        if (player != null)
        {
            player.currentlyDragging = null;
        }
    }

    public Card getCard()
    {
        return curCard;
    }

}
