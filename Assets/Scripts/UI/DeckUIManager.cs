using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DeckUIManager : MonoBehaviour
{
    private const float CardDisplayScale = 35f;
    private const float CardSpacing = 8f;
    private const int CardsPerRow = 2;

    public GameObject deckPanel;
    public GameObject cardPrefab;
    public TextMeshProUGUI healthText;
    public TextMeshProUGUI deckSize;
    public TextMeshProUGUI colSize;

    private readonly List<GameObject> displayedCards = new List<GameObject>();

    private void OnEnable()
    {
        RefreshDeck();
        updateHealth();
        updateSizes();
    }
    private void updateHealth()
    {
        healthText.text = "Health: " + Player.GetInstance().Health.ToString();
    }
    private void updateSizes()
    {
        colSize.text = "Collection Size: " + Player.GetInstance().cardCollection.Count.ToString();
        deckSize.text = "Deck size: " + Player.GetInstance().curDeck.Count.ToString();
    }

    private void RefreshDeck()
    {
        if (deckPanel == null)
        {
            Debug.LogError("DeckUIManager requires a deck panel.", this);
            return;
        }

        if (cardPrefab == null)
        {
            Debug.LogError("DeckUIManager requires a card prefab.", this);
            return;
        }

        Player player = Player.GetInstance();
        if (player == null)
        {
            Debug.LogError("Cannot display the deck because no Player instance exists.", this);
            return;
        }

        ScrollRect scrollRect = deckPanel.GetComponent<ScrollRect>();
        RectTransform content = scrollRect != null
            ? scrollRect.content
            : deckPanel.GetComponent<RectTransform>();
        if (content == null)
        {
            Debug.LogError("Deck panel must have a RectTransform or a ScrollRect with content assigned.", deckPanel);
            return;
        }

        Canvas prefabCanvas = cardPrefab.GetComponentInChildren<Canvas>();
        RectTransform cardRect = prefabCanvas != null
            ? prefabCanvas.GetComponent<RectTransform>()
            : null;
        if (cardRect == null)
        {
            Debug.LogError("Card prefab must contain a Canvas with a RectTransform.", cardPrefab);
            return;
        }

        RectTransform viewport = scrollRect != null && scrollRect.viewport != null
            ? scrollRect.viewport
            : content;
        if (viewport.rect.width <= CardSpacing || cardRect.rect.width <= 0f || cardRect.rect.height <= 0f)
        {
            Debug.LogError("Deck viewport must fit two cards and card prefab dimensions must be non-zero.", this);
            return;
        }

        float maxCardWidth = (viewport.rect.width - CardSpacing) / CardsPerRow;
        float scale = Mathf.Min(CardDisplayScale, maxCardWidth / cardRect.rect.width);
        Vector2 displaySize = cardRect.rect.size * scale;
        float columnStep = displaySize.x + CardSpacing;
        float rowStep = displaySize.y + CardSpacing;

        ClearDisplayedCards();
        int rowCount = (player.curDeck.Count + CardsPerRow - 1) / CardsPerRow;
        float gridWidth = CardsPerRow * displaySize.x + (CardsPerRow - 1) * CardSpacing;
        float contentWidth = Mathf.Max(viewport.rect.width, gridWidth);
        float contentHeight = rowCount > 0
            ? rowCount * displaySize.y + (rowCount - 1) * CardSpacing
            : viewport.rect.height;
        content.SetSizeWithCurrentAnchors(
            RectTransform.Axis.Horizontal,
            contentWidth);
        content.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, Mathf.Max(viewport.rect.height, contentHeight));

        for (int i = 0; i < player.curDeck.Count; i++)
        {
            Card card = player.curDeck[i];
            if (card == null)
            {
                Debug.LogWarning("Skipping a null card in the player's deck.", player);
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
                (contentWidth - gridWidth) * 0.5f + (i % CardsPerRow) * columnStep + displaySize.x * 0.5f,
                -(i / CardsPerRow * rowStep + displaySize.y * 0.5f),
                0f);
            cardObject.transform.localRotation = Quaternion.identity;
            cardObject.transform.localScale = Vector3.one * scale;

            cardView.place = global::cardPrefab.placement.DISPLAY;
            cardView.setCardTo(card);
            AddPointerTarget(cardObject);
            displayedCards.Add(cardObject);
        }

        if (scrollRect != null)
        {
            scrollRect.horizontalNormalizedPosition = 0f;
            scrollRect.verticalNormalizedPosition = 1f;
        }
    }

    private void ClearDisplayedCards()
    {
        foreach (GameObject card in displayedCards)
        {
            if (card != null)
            {
                Destroy(card);
            }
        }

        displayedCards.Clear();
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
