using System;
using System.Collections.Generic;
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
    public TextMeshProUGUI unitAbilityText;
    public GameObject aText, dText, sText, abText; // these are the objects for enabling/disabling
    public GameObject imageObject;

    private Boolean blank; //is the card blank? ig
    private Sprite curSprite;
    private Sprite defaultCardSprite;
    private static Dictionary<string, Sprite> spritesByName;
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
            bool hasAbility = unitAbilityText != null && unit.ability != Ability.NONE;
            abText.SetActive(hasAbility);
            if (unitAbilityText != null)
            {
                unitAbilityText.text = hasAbility ? unit.ability.ToString() : string.Empty;
                unitAbilityText.gameObject.SetActive(hasAbility);
            }
        }
        if(card is Terrain terrain)
        {
            if (unitAbilityText != null && terrain.effect.Length >= 1)
            {
                unitAbilityText.gameObject.SetActive(false);
            } else
            {
                abText.SetActive(true);
            }
            SizeText.text = terrain.size.ToString();
            aText.SetActive(false);
            dText.SetActive(false);
            Abilitytext.text = terrain.effect.ToString();
        }
        if(card is Tactic tactic)
        { //record the first ability for now, fix later
            if (unitAbilityText != null)
            {
                unitAbilityText.gameObject.SetActive(false);
            }
            Abilitytext.text = tactic.abilities[0];
            aText.SetActive(false);
            dText.SetActive(false);
            sText.SetActive(false);
            abText.SetActive(true);
        }
        SpriteRenderer cardImageRenderer = imageObject != null
            ? imageObject.GetComponent<SpriteRenderer>()
            : null;
        if (cardImageRenderer != null)
        {
            Sprite cardSprite = FindSpriteByName(card.name);
            cardImageRenderer.sprite = cardSprite != null ? cardSprite : defaultCardSprite;
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
        if ((curCard is Unit unit && unit.size !=1) || (curCard is Terrain terrain &&terrain.size != 1))
        {
            //check if card size is not 1 and then change its battiefield appearance
        }
    }
    
    void Start()
    {
        originalScale = transform.localScale;
    }

    private void Awake()
    {
        SpriteRenderer cardImageRenderer = imageObject != null
            ? imageObject.GetComponent<SpriteRenderer>()
            : null;
        defaultCardSprite = cardImageRenderer != null ? cardImageRenderer.sprite : null;
    }

    private static Sprite FindSpriteByName(string spriteName)
    {
        if (spritesByName == null)
        {
            spritesByName = new Dictionary<string, Sprite>();
            foreach (Sprite sprite in Resources.LoadAll<Sprite>("Sprites"))
            {
                if (sprite == null)
                {
                    continue;
                }

                string spriteKey = sprite.texture != null ? sprite.texture.name : sprite.name;
                if (!spritesByName.ContainsKey(spriteKey) || sprite.name == spriteKey)
                {
                    spritesByName[spriteKey] = sprite;
                }
            }
        }

        return spritesByName.TryGetValue(spriteName, out Sprite matchingSprite)
            ? matchingSprite
            : null;
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
        if (isHandCard)
        {
            BattlefieldManager manager = BattlefieldManager.getInstance();
            if (manager == null ||
                manager.currentPhase != BattlefieldManager.gamePhase.YOUR_TURN)
            {
                return;
            }
        }

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

        if (pointerOverDisplayCard)
        {
            transform.localScale = originalScale;
            pointerOverDisplayCard = false;
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
            if (TryPlayTacticAtScreenPosition(eventData.position))
            {
                return;
            }

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

    private bool TryPlayTacticAtScreenPosition(Vector2 screenPosition)
    {
        if (!(curCard is Tactic tactic))
        {
            return false;
        }

        BattlefieldManager manager = BattlefieldManager.getInstance();
        Player player = Player.GetInstance();
        if (manager == null ||
            player == null ||
            manager.IsTacticTargetSelectionPending ||
            !manager.IsPointerOverTacticZone(screenPosition) ||
            !player.payAether(tactic.cost))
        {
            return false;
        }

        CardLibrary.getInstance().resolveTacticByName(tactic.name);
        manager.updateAetherDisplay();
        player.curhand.Remove(curCard);
        player.currentlyDragging = null;
        Destroy(gameObject);
        return true;
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
