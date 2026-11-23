using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CollectionUIManager : MonoBehaviour
{
    private const float CardDisplayScale = 35f;
    private const float CardSpacing = 8f;
    private const int CardsPerRow = 2;

    [SerializeField]
    private ScrollRect deckScrollView;
    [SerializeField]
    private ScrollRect collectionScrollView;
    [SerializeField]
    private GameObject cardPrefab;
    [SerializeField]
    private float hDisplacement;

    private readonly List<GameObject> displayedDeckCards = new List<GameObject>();
    private readonly List<GameObject> displayedCollectionCards = new List<GameObject>();
    private Player player;
    private bool initialized;

    private void Start()
    {
        player = Player.GetInstance();
        if (player == null)
        {
            Debug.LogError("Cannot display the collection because no Player instance exists.", this);
            return;
        }

        if (deckScrollView == null || collectionScrollView == null || cardPrefab == null)
        {
            Debug.LogError("CollectionUIManager requires deck and collection ScrollRects, and a card prefab.", this);
            return;
        }

        if (deckScrollView.content == null ||
            deckScrollView.viewport == null ||
            collectionScrollView.content == null ||
            collectionScrollView.viewport == null ||
            deckScrollView == collectionScrollView)
        {
            Debug.LogError("CollectionUIManager requires distinct ScrollRects with content and viewport assigned.", this);
            return;
        }

        initialized = true;
        RefreshViews();
    }

    private void OnEnable()
    {
        if (initialized)
        {
            RefreshViews();
        }
    }

    public bool TryTransferCard(Card card, global::cardPrefab.placement source, Vector2 screenPosition)
    {
        if (!initialized || card == null)
        {
            return false;
        }

        bool movingToDeck = source == global::cardPrefab.placement.COLLECTION;
        if (!movingToDeck && source != global::cardPrefab.placement.DECK)
        {
            return false;
        }

        ScrollRect targetScrollView = movingToDeck ? deckScrollView : collectionScrollView;
        if (targetScrollView == null || targetScrollView.viewport == null)
        {
            Debug.LogError("Both collection and deck ScrollRects must have viewports assigned.", this);
            return false;
        }

        Canvas targetCanvas = targetScrollView.GetComponentInParent<Canvas>();
        Camera camera = targetCanvas != null && targetCanvas.rootCanvas.renderMode != RenderMode.ScreenSpaceOverlay
            ? targetCanvas.rootCanvas.worldCamera
            : null;

        if (!RectTransformUtility.RectangleContainsScreenPoint(
                targetScrollView.viewport,
                screenPosition,
                camera))
        {
            return false;
        }

        List<Card> sourceCards = movingToDeck ? player.cardCollection : player.curDeck;
        List<Card> destinationCards = movingToDeck ? player.curDeck : player.cardCollection;
        if (!sourceCards.Remove(card))
        {
            Debug.LogWarning("The dragged card is no longer in its source list.", this);
            return false;
        }

        destinationCards.Add(card);
        RefreshViews();
        return true;
    }

    private void RefreshViews()
    {
        RefreshScrollView(
            deckScrollView,
            player.curDeck,
            global::cardPrefab.placement.DECK,
            displayedDeckCards);
        RefreshScrollView(
            collectionScrollView,
            player.cardCollection,
            global::cardPrefab.placement.COLLECTION,
            displayedCollectionCards);
    }

    private void RefreshScrollView(
        ScrollRect scrollView,
        List<Card> cards,
        global::cardPrefab.placement placement,
        List<GameObject> displayedCards)
    {
        if (scrollView == null || scrollView.content == null || scrollView.viewport == null)
        {
            Debug.LogError("Deck and collection ScrollRects must have content and viewport assigned.", this);
            return;
        }

        RectTransform content = scrollView.content;
        RectTransform viewport = scrollView.viewport;
        RectTransform cardRect = cardPrefab != null
            ? cardPrefab.GetComponentInChildren<Canvas>()?.GetComponent<RectTransform>()
            : null;

        if (cardRect == null)
        {
            Debug.LogError("Card prefab must contain a Canvas with a RectTransform.", cardPrefab);
            return;
        }

        if (viewport.rect.width <= CardSpacing || cardRect.rect.width <= 0f || cardRect.rect.height <= 0f)
        {
            Debug.LogError("Each viewport must fit two cards and card prefab dimensions must be non-zero.", this);
            return;
        }

        ClearDisplayedCards(displayedCards);

        float maxCardWidth = (viewport.rect.width - CardSpacing) / CardsPerRow;
        float scale = Mathf.Min(CardDisplayScale, maxCardWidth / cardRect.rect.width);
        Vector2 displaySize = cardRect.rect.size * scale;
        float columnStep = displaySize.x + CardSpacing;
        float rowStep = displaySize.y + CardSpacing;

        int cardCount = 0;
        foreach (Card card in cards)
        {
            if (card != null)
            {
                cardCount++;
            }
        }

        int rowCount = (cardCount + CardsPerRow - 1) / CardsPerRow;
        float gridWidth = CardsPerRow * displaySize.x + (CardsPerRow - 1) * CardSpacing;
        float contentWidth = Mathf.Max(viewport.rect.width, gridWidth);
        float contentHeight = rowCount > 0
            ? rowCount * displaySize.y + (rowCount - 1) * CardSpacing
            : viewport.rect.height;

        content.anchorMin = new Vector2(0.5f, 1f);
        content.anchorMax = new Vector2(0.5f, 1f);
        content.pivot = new Vector2(0.5f, 1f);
        content.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, contentWidth);
        content.SetSizeWithCurrentAnchors(
            RectTransform.Axis.Vertical,
            Mathf.Max(viewport.rect.height, contentHeight));

        int cardIndex = 0;
        foreach (Card card in cards)
        {
            if (card == null)
            {
                Debug.LogWarning("Skipping a null card while populating a card view.", this);
                continue;
            }

            GameObject cardObject = Instantiate(cardPrefab, content, false);
            cardObject.name = card.name;

            global::cardPrefab cardView = cardObject.GetComponent<global::cardPrefab>();
            if (cardView == null)
            {
                Debug.LogError("Card prefab is missing its cardPrefab component.", cardObject);
                Destroy(cardObject);
                continue;
            }

            cardObject.transform.localPosition = new Vector3(
                (contentWidth - gridWidth) * 0.5f +
                    (cardIndex % CardsPerRow) * columnStep +
                    displaySize.x * 0.5f +
                    hDisplacement,
                -(cardIndex / CardsPerRow * rowStep + displaySize.y * 0.5f),
                0f);
            cardObject.transform.localRotation = Quaternion.identity;
            cardObject.transform.localScale = Vector3.one * scale;

            cardView.place = placement;
            cardView.ConfigureDrag(this, true);
            cardView.setCardTo(card);
            AddPointerTarget(cardObject);
            displayedCards.Add(cardObject);
            cardIndex++;
        }

        scrollView.horizontalNormalizedPosition = 0f;
        scrollView.verticalNormalizedPosition = 1f;
    }

    private void ClearDisplayedCards(List<GameObject> cards)
    {
        foreach (GameObject card in cards)
        {
            if (card != null)
            {
                Destroy(card);
            }
        }

        cards.Clear();
    }

    private static void AddPointerTarget(GameObject cardObject)
    {
        Canvas instanceCanvas = cardObject.GetComponentInChildren<Canvas>();
        if (instanceCanvas == null)
        {
            Debug.LogError("Card instance is missing its Canvas.", cardObject);
            return;
        }

        Image pointerTarget = instanceCanvas.GetComponent<Image>();
        if (pointerTarget == null)
        {
            pointerTarget = instanceCanvas.gameObject.AddComponent<Image>();
        }

        pointerTarget.color = new Color(1f, 1f, 1f, 0f);
        pointerTarget.raycastTarget = true;
    }
}
