using UnityEngine;
using System;
using static CardLibrary;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using System.Collections;
using UnityEngine.InputSystem;


public class BattlefieldManager : MonoBehaviour
{

    private static BattlefieldManager instance;
    public static BattlefieldManager getInstance()
    {
        return instance;
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);
    }
    private void Start()
    {
        player = Player.GetInstance(); // to make sure we're getting the right instance of player
        if (tacticZone == null)
        {
            Transform battleCanvas = transform.Find("BattleCanvas");
            tacticZone = battleCanvas != null
                ? battleCanvas.Find("TacticZone") as RectTransform
                : null;
        }
        if (tacticZone == null)
        {
            Debug.LogError("BattlefieldManager requires a tactic zone RectTransform.", this);
        }

        gameObject.SetActive(false);
        passPhaseButton.onClick.AddListener(PassPhasePressed);
        passTurnButton.onClick.AddListener(passTurnPressed);
    }
    private void PassPhasePressed()
    {
        switch (currentPhase)
        {
            case gamePhase.YOUR_TURN:
                currentPhase = gamePhase.YOUR_ATTACKS;
                break;
            case gamePhase.YOUR_ATTACKS:
                currentPhase = gamePhase.ENEMY_TURN;

                break;
        }
    }
    private void passTurnPressed()
    {
        currentPhase = gamePhase.ENEMY_TURN;
    }
    

    private void Update()
    {
        bool isPlayerActionPhase =
            currentPhase == gamePhase.YOUR_TURN ||
            currentPhase == gamePhase.YOUR_ATTACKS;
        bool arrowPressed = ReadArrowKeyPress(out int deltaX, out int deltaY);
        if (!isPlayerActionPhase)
        {
            DeselectSelectedUnit();
        }
        else
        {
            HandleSelectionClick();
            if (selectedUnit != null && arrowPressed)
            {
                if (currentPhase == gamePhase.YOUR_TURN)
                {
                    TryMoveSelectedUnit(deltaX, deltaY);
                }
                else
                {
                    TryAttackSelectedUnit(deltaX, deltaY);
                }
            }
        }

        //also update the phase UI while your at it
        turnDisplay.text = currentPhase switch
        {
            gamePhase.YOUR_TURN => "your main",
            gamePhase.YOUR_ATTACKS => "your attacks",
            gamePhase.ENEMY_TURN => "enemy main",
            gamePhase.ENEMY_ATTACKS => "enemy attacks",
            _=> "error?"
        };
        //add display stuff in here
        enemyLife.text = opponent.Health.ToString();
        playerLife.text = player.Health.ToString();
    }

    public TextMeshProUGUI AetherDisplay; 
    public TextMeshProUGUI turnDisplay;
    //make an array of buttons to assign so this is easy
    public Button passTurnButton;
    public Button passPhaseButton;
    private const int BoardSize = 5;
    public static Tile[,] battleField = new Tile[BoardSize + 1, BoardSize + 1]; // !!!!!!!!!!!!!!!!!!!!!!!!!!!!!
    private readonly TilePrefab[,] tileViews = new TilePrefab[BoardSize + 1, BoardSize + 1];
    private Tile selectedUnitTile;
    private Unit selectedUnit;
    private TilePrefab selectedUnitView;
    private bool upArrowHeld;
    private bool downArrowHeld;
    private bool leftArrowHeld;
    private bool rightArrowHeld;
    private Opponent opponent;
    [SerializeField]
    private Transform HandScrollView; // for hand visualization
    [SerializeField]
    private cardPrefab cardPrefabForHand;
    public RectTransform tacticZone;
    private Player player = Player.GetInstance();
    private const float HandCardDisplayScale = 35f;
    private const float HandCardSpacing = 8f;
    private const int HandCardsPerRow = 2;
    public bool TerrainThisTurn; // whether the player has placed a terrain this turn
    public enum gamePhase
    {
        YOUR_TURN, ENEMY_TURN, YOUR_ATTACKS, ENEMY_ATTACKS, GAME_END, ERROR, OUT_OF_GAME
    }
    public gamePhase currentPhase;
    public TextMeshProUGUI playerLife;
    public TextMeshProUGUI enemyLife;

    //use this to set the current state of an empty battlefield
    public void initTiles(){
        for(int i = 1; i <= BoardSize; i++){
            for(int j = 1; j <= BoardSize; j++){
                Tile curTile = new Tile(i,j);
                battleField[i,j] = curTile;
            }
        }
    }

    public void setUpBattlefield(int i)
    {
        DeselectSelectedUnit();
        initTiles();
        gameObject.SetActive(true);
        if(i == 1)
        {
            Player.GetInstance().DrawHand();
            updateHand();
            setAetherCount(0);
        }
        currentPhase = gamePhase.YOUR_TURN; // player goes first
        TerrainThisTurn = false;
        ResetUnitMovement();
    }
    public void startPlayerTurn()
    {
        if (player == null)
        {
            player = Player.GetInstance();
        }

        if (player == null)
        {
            Debug.LogError("Cannot start the player's turn because no Player instance exists.", this);
            return;
        }

        currentPhase = gamePhase.YOUR_TURN;
        TerrainThisTurn = false;
        ResetUnitMovement();
        DeselectSelectedUnit();

        int controlledTerrainCount = 0;
        foreach (Tile tile in battleField)
        {
            if (tile != null && tile.curTerrain != null && tile.playerControlsTerrain)
            {
                controlledTerrainCount++;
            }
        }

        player.Aether += controlledTerrainCount;
        Debug.Log("Aether just incremented by: " + controlledTerrainCount);
        updateAetherDisplay();

        Card drawnCard = player.drawCard();
        if (drawnCard != null)
        {
            player.curhand.Add(drawnCard);
        }
        updateHand();
    }
    public void setAetherCount(int c)
    {
        player.Aether = c;
        updateAetherDisplay();
    }

    public void updateAetherDisplay()
    {
        if (AetherDisplay == null)
        {
            Debug.LogError("BattlefieldManager requires an Aether display.", this);
            return;
        }

        AetherDisplay.text = player.Aether.ToString();
    }

    public bool IsPointerOverTacticZone(Vector2 screenPosition)
    {
        if (tacticZone == null)
        {
            return false;
        }

        Canvas zoneCanvas = tacticZone.GetComponentInParent<Canvas>();
        if (zoneCanvas == null)
        {
            Debug.LogError("The tactic zone must be under a Canvas.", tacticZone);
            return false;
        }

        Canvas rootCanvas = zoneCanvas.rootCanvas;
        Camera camera = rootCanvas.renderMode == RenderMode.ScreenSpaceOverlay
            ? null
            : rootCanvas.worldCamera;
        if (rootCanvas.renderMode != RenderMode.ScreenSpaceOverlay && camera == null)
        {
            Debug.LogError("The tactic zone Canvas requires a world camera.", rootCanvas);
            return false;
        }

        return RectTransformUtility.RectangleContainsScreenPoint(
            tacticZone,
            screenPosition,
            camera);
    }

    //checks if two tiles are adjacent to each other
    public Boolean checkifAdjacent(Tile t1, Tile t2)
    {
        return (t1.getX()-t2.getX()==1) || (t1.getY()-t2.getY()==1) || (t1.getX()-t2.getX()==-1) || (t1.getY()-t2.getY()==-1);
    }

    //check if two tiles are next to each other, essential for battle logic
    public Boolean checkifNext(Tile t1, Tile t2)
    {
        return(
            //dispx=1 and same y, or dispy=1 and same x
            (Math.Abs(t1.getX()-t2.getX()) == 1 && t1.getY()-t2.getY() == 0) ||
            (Math.Abs(t1.getY()-t2.getY())==1 && t1.getX()-t2.getX()==0)
        );
    }

    public void placeUnitAt(int x, int y, Unit unit, bool playerControlsUnit = true)
    {
        Tile tile = battleField[x, y];
        if (tile == null || unit == null)
        {
            Debug.LogError("Cannot place a unit on a missing tile or with a null unit.", this);
            return;
        }

        if (tile.curTerrain == null || tile.playerControlsTerrain != playerControlsUnit)
        {
            Debug.LogError("A unit can only be placed on terrain controlled by the same side.", this);
            return;
        }

        if (!tile.canHoldUnit(unit))
        {
            Debug.LogError("The tile cannot hold this unit.", this);
            return;
        }

        tile.putUnitHere(unit);
        if (unit.ability == Ability.FLYING)
        {
            tile.skyOccupied = true;
            tile.playerControlsFlyingUnit = playerControlsUnit;
            tile.flyingUnitMovedThisTurn = false;
        }
        else
        {
            tile.groundOccupied = true;
            tile.playerControlsGroundUnit = playerControlsUnit;
            tile.groundUnitMovedThisTurn = false;
        }
    }
    public void removeUnitsAt(int x, int y)
    {
        Tile tile = battleField[x, y];
        if (tile != null)
        {
            if (selectedUnitTile == tile)
            {
                DeselectSelectedUnit();
            }

            if (tile.unitHere != null)
            {
                RemoveUnitFromTile(tile, tile.unitHere);
            }

            if (tile.unitFlyingHere != null)
            {
                RemoveUnitFromTile(tile, tile.unitFlyingHere);
            }

            tile.unitHere = null;
            tile.unitFlyingHere = null;
            tile.groundOccupied = false;
            tile.skyOccupied = false;
            tile.playerControlsGroundUnit = false;
            tile.playerControlsFlyingUnit = false;
            tile.groundUnitMovedThisTurn = false;
            tile.flyingUnitMovedThisTurn = false;
        }
    }
    public void damageUnitsAt(int x, int y, int ATTACK)
    {
        Tile tile = battleField[x, y];
        if (tile == null)
        {
            return;
        }

        if (tile.unitHere != null && tile.unitHere.DEFENSE <= ATTACK)
        {
            RemoveUnitFromTile(tile, tile.unitHere);
        }
        if (tile.unitFlyingHere != null && tile.unitFlyingHere.DEFENSE <= ATTACK)
        {
            RemoveUnitFromTile(tile, tile.unitFlyingHere);
        }
    }

    private void RemoveUnitFromTile(Tile tile, Unit unit)
    {
        if (tile == null || unit == null)
        {
            return;
        }

        bool isFlyingUnit = tile.unitFlyingHere == unit;
        if (!isFlyingUnit && tile.unitHere != unit)
        {
            return;
        }

        if (selectedUnitTile == tile && selectedUnit == unit)
        {
            DeselectSelectedUnit();
        }

        TilePrefab tileView = tileViews[tile.x, tile.y];
        if (tileView != null)
        {
            tileView.ClearUnitSprite(unit);
        }
        else
        {
            Debug.LogError("Cannot clear a removed unit's sprite because its tile view is not registered.", this);
        }

        if (isFlyingUnit)
        {
            tile.unitFlyingHere = null;
            tile.skyOccupied = false;
            tile.playerControlsFlyingUnit = false;
            tile.flyingUnitMovedThisTurn = false;
        }
        else
        {
            tile.unitHere = null;
            tile.groundOccupied = false;
            tile.playerControlsGroundUnit = false;
            tile.groundUnitMovedThisTurn = false;
        }
    }

    public bool TryRemoveUnitFromTile(Tile tile, Unit unit)
    {
        if (tile == null ||
            unit == null ||
            (tile.unitHere != unit && tile.unitFlyingHere != unit))
        {
            return false;
        }

        RemoveUnitFromTile(tile, unit);
        return true;
    }

    public void RegisterTilePrefab(TilePrefab tileView)
    {
        if (tileView == null ||
            tileView.x < 1 || tileView.x > BoardSize ||
            tileView.y < 1 || tileView.y > BoardSize)
        {
            Debug.LogError("Cannot register a tile view with invalid board coordinates.", tileView);
            return;
        }

        tileViews[tileView.x, tileView.y] = tileView;
    }

    public bool TrySetTileCardSpriteAt(int x, int y, Card card, Sprite cardSprite)
    {
        if (x < 1 || x > BoardSize || y < 1 || y > BoardSize)
        {
            Debug.LogError("Cannot set a card sprite at invalid tile coordinates.", this);
            return false;
        }

        TilePrefab tileView = tileViews[x, y];
        if (tileView == null)
        {
            Debug.LogError("Cannot set a card sprite because the tile view is not registered.", this);
            return false;
        }

        return tileView.TrySetCardSprite(card, cardSprite);
    }

    public void SelectUnit(TilePrefab tileView, Unit unit)
    {
        if ((currentPhase != gamePhase.YOUR_TURN &&
             currentPhase != gamePhase.YOUR_ATTACKS) ||
            tileView == null ||
            unit == null ||
            tileView.x < 1 || tileView.x > BoardSize ||
            tileView.y < 1 || tileView.y > BoardSize)
        {
            DeselectSelectedUnit();
            return;
        }

        Tile tile = battleField[tileView.x, tileView.y];
        if (!IsPlayerUnitOnTile(tile, unit))
        {
            DeselectSelectedUnit();
            return;
        }

        if (selectedUnit == unit && selectedUnitTile == tile)
        {
            return;
        }

        DeselectSelectedUnit();
        if (!tileView.SetUnitSelected(unit, true))
        {
            return;
        }

        selectedUnitTile = tile;
        selectedUnit = unit;
        selectedUnitView = tileView;
    }

    public void DeselectSelectedUnit()
    {
        if (selectedUnitView != null && selectedUnit != null)
        {
            selectedUnitView.SetUnitSelected(selectedUnit, false);
        }

        selectedUnitTile = null;
        selectedUnit = null;
        selectedUnitView = null;
    }

    public void HandleTileClicked(TilePrefab clickedTileView)
    {
        if ((currentPhase != gamePhase.YOUR_TURN &&
             currentPhase != gamePhase.YOUR_ATTACKS) ||
            clickedTileView == null ||
            clickedTileView.x < 1 || clickedTileView.x > BoardSize ||
            clickedTileView.y < 1 || clickedTileView.y > BoardSize)
        {
            DeselectSelectedUnit();
            return;
        }

        Tile tile = battleField[clickedTileView.x, clickedTileView.y];
        Unit clickedUnit = tile != null && tile.unitHere != null && tile.playerControlsGroundUnit
            ? tile.unitHere
            : tile != null && tile.unitFlyingHere != null && tile.playerControlsFlyingUnit
                ? tile.unitFlyingHere
                : null;
        if (clickedUnit == null)
        {
            DeselectSelectedUnit();
            return;
        }

        SelectUnit(clickedTileView, clickedUnit);
    }

    private void HandleSelectionClick()
    {
        if (Mouse.current == null ||
            !Mouse.current.leftButton.wasPressedThisFrame)
        {
            return;
        }

        Camera camera = Camera.main;
        if (camera == null)
        {
            Debug.LogError("Cannot detect a clicked board tile because no Main Camera exists.", this);
            DeselectSelectedUnit();
            return;
        }

        RaycastHit2D[] hits = Physics2D.GetRayIntersectionAll(
            camera.ScreenPointToRay(Mouse.current.position.ReadValue()));
        foreach (RaycastHit2D hit in hits)
        {
            TilePrefab clickedTileView = hit.collider != null
                ? hit.collider.GetComponentInParent<TilePrefab>()
                : null;
            if (clickedTileView != null)
            {
                HandleTileClicked(clickedTileView);
                return;
            }
        }

        DeselectSelectedUnit();
    }

    private bool ReadArrowKeyPress(out int deltaX, out int deltaY)
    {
        deltaX = 0;
        deltaY = 0;
        Keyboard keyboard = Keyboard.current;
        bool upPressed = keyboard != null && keyboard.upArrowKey.isPressed;
        bool downPressed = keyboard != null && keyboard.downArrowKey.isPressed;
        bool leftPressed = keyboard != null && keyboard.leftArrowKey.isPressed;
        bool rightPressed = keyboard != null && keyboard.rightArrowKey.isPressed;

        bool upJustPressed = upPressed && !upArrowHeld;
        bool downJustPressed = downPressed && !downArrowHeld;
        bool leftJustPressed = leftPressed && !leftArrowHeld;
        bool rightJustPressed = rightPressed && !rightArrowHeld;
        upArrowHeld = upPressed;
        downArrowHeld = downPressed;
        leftArrowHeld = leftPressed;
        rightArrowHeld = rightPressed;

        if (upJustPressed)
        {
            deltaY = 1;
        }
        else if (downJustPressed)
        {
            deltaY = -1;
        }
        else if (leftJustPressed)
        {
            deltaX = -1;
        }
        else if (rightJustPressed)
        {
            deltaX = 1;
        }
        else
        {
            return false;
        }

        return true;
    }

    private void TryAttackSelectedUnit(int deltaX, int deltaY)
    {
        if (currentPhase != gamePhase.YOUR_ATTACKS ||
            selectedUnit == null ||
            selectedUnitTile == null ||
            !IsPlayerUnitOnTile(selectedUnitTile, selectedUnit))
        {
            return;
        }

        if (deltaX == 0 &&
            deltaY == 1 &&
            HasClearPathToOpponent(selectedUnit, selectedUnitTile))
        {
            Opponent targetOpponent = GetOpponent();
            if (targetOpponent != null)
            {
                targetOpponent.loseHealth(selectedUnit.ATTACK);
            }

            return;
        }

        int targetX = selectedUnitTile.x + deltaX;
        int targetY = selectedUnitTile.y + deltaY;
        if (targetX < 1 || targetX > BoardSize || targetY < 1 || targetY > BoardSize)
        {
            return;
        }

        Tile targetTile = battleField[targetX, targetY];
        if (targetTile == null)
        {
            return;
        }

        Unit defender = null;
        if (selectedUnit.ability == Ability.FLYING)
        {
            if (targetTile.unitFlyingHere != null && !targetTile.playerControlsFlyingUnit)
            {
                defender = targetTile.unitFlyingHere;
            }
            else if (targetTile.unitHere != null && !targetTile.playerControlsGroundUnit)
            {
                defender = targetTile.unitHere;
            }
        }
        else if (targetTile.unitHere != null && !targetTile.playerControlsGroundUnit)
        {
            defender = targetTile.unitHere;
        }

        if (defender != null && selectedUnit.ATTACK >= defender.DEFENSE)
        {
            RemoveUnitFromTile(targetTile, defender);
        }
    }

    private bool HasClearPathToOpponent(Unit unit, Tile sourceTile)
    {
        for (int y = sourceTile.y + 1; y <= BoardSize; y++)
        {
            Tile pathTile = battleField[sourceTile.x, y];
            if (pathTile == null ||
                pathTile.unitHere != null ||
                pathTile.unitFlyingHere != null ||
                !pathTile.canCross(unit.ability))
            {
                return false;
            }
        }

        return true;
    }

    private Opponent GetOpponent()
    {
        if (opponent == null)
        {
            opponent = FindAnyObjectByType<Opponent>();
        }

        if (opponent == null)
        {
            Debug.LogError("Cannot damage the opponent because no Opponent component exists.", this);
        }

        return opponent;
    }

    private bool CanMoveUnitTo(
        Unit unit,
        int sourceX,
        int sourceY,
        int destinationX,
        int destinationY,
        bool playerControlsUnit)
    {
        gamePhase requiredPhase = playerControlsUnit
            ? gamePhase.YOUR_TURN
            : gamePhase.ENEMY_TURN;
        if (currentPhase != requiredPhase ||
            unit == null ||
            sourceX < 1 || sourceX > BoardSize ||
            sourceY < 1 || sourceY > BoardSize ||
            destinationX < 1 || destinationX > BoardSize ||
            destinationY < 1 || destinationY > BoardSize ||
            Math.Abs(sourceX - destinationX) + Math.Abs(sourceY - destinationY) != 1)
        {
            return false;
        }

        Tile source = battleField[sourceX, sourceY];
        Tile destination = battleField[destinationX, destinationY];
        if (source == null || destination == null)
        {
            return false;
        }

        bool isFlying = unit.ability == Ability.FLYING;
        bool unitIsOnSource = isFlying
            ? source.unitFlyingHere == unit && source.playerControlsFlyingUnit == playerControlsUnit
            : source.unitHere == unit && source.playerControlsGroundUnit == playerControlsUnit;
        return unitIsOnSource &&
            !HasUnitMovedThisTurn(source, unit) &&
            destination.canHoldUnit(unit);
    }

    public bool TryMoveUnitTo(
        Unit unit,
        int sourceX,
        int sourceY,
        int destinationX,
        int destinationY,
        bool playerControlsUnit)
    {
        if (!CanMoveUnitTo(
                unit,
                sourceX,
                sourceY,
                destinationX,
                destinationY,
                playerControlsUnit))
        {
            return false;
        }

        Tile source = battleField[sourceX, sourceY];
        Tile destination = battleField[destinationX, destinationY];
        TilePrefab sourceView = tileViews[sourceX, sourceY];
        TilePrefab destinationView = tileViews[destinationX, destinationY];
        if (sourceView == null || destinationView == null)
        {
            Debug.LogError("Cannot move a unit because a source or destination tile view is missing.", this);
            return false;
        }

        bool isSelectedUnit = selectedUnit == unit && selectedUnitTile == source;
        if (isSelectedUnit)
        {
            sourceView.SetUnitSelected(unit, false);
        }

        if (!sourceView.TryMoveUnitSpriteTo(destinationView, unit))
        {
            if (isSelectedUnit)
            {
                sourceView.SetUnitSelected(unit, true);
            }

            return false;
        }

        if (unit.ability == Ability.FLYING)
        {
            source.unitFlyingHere = null;
            source.skyOccupied = false;
            source.playerControlsFlyingUnit = false;
            source.flyingUnitMovedThisTurn = false;
            destination.unitFlyingHere = unit;
            destination.skyOccupied = true;
            destination.playerControlsFlyingUnit = playerControlsUnit;
            destination.flyingUnitMovedThisTurn = true;
        }
        else
        {
            source.unitHere = null;
            source.groundOccupied = false;
            source.playerControlsGroundUnit = false;
            source.groundUnitMovedThisTurn = false;
            destination.unitHere = unit;
            destination.groundOccupied = true;
            destination.playerControlsGroundUnit = playerControlsUnit;
            destination.groundUnitMovedThisTurn = true;
        }

        return true;
    }

    private void TryMoveSelectedUnit(int deltaX, int deltaY)
    {
        if (selectedUnitTile == null || selectedUnit == null || selectedUnitView == null)
        {
            return;
        }

        Tile source = selectedUnitTile;
        Unit unit = selectedUnit;
        int destinationX = source.x + deltaX;
        int destinationY = source.y + deltaY;
        if (!TryMoveUnitTo(
                unit,
                source.x,
                source.y,
                destinationX,
                destinationY,
                true))
        {
            return;
        }

        selectedUnitTile = battleField[destinationX, destinationY];
        selectedUnitView = tileViews[destinationX, destinationY];
        if (selectedUnitView == null || !selectedUnitView.SetUnitSelected(unit, true))
        {
            DeselectSelectedUnit();
        }
    }

    private static bool IsPlayerUnitOnTile(Tile tile, Unit unit)
    {
        return tile != null &&
            ((tile.unitHere == unit && tile.playerControlsGroundUnit) ||
             (tile.unitFlyingHere == unit && tile.playerControlsFlyingUnit));
    }

    private static bool HasUnitMovedThisTurn(Tile tile, Unit unit)
    {
        return tile.unitHere == unit
            ? tile.groundUnitMovedThisTurn
            : tile.flyingUnitMovedThisTurn;
    }

    private static void ResetUnitMovement()
    {
        foreach (Tile tile in battleField)
        {
            if (tile != null)
            {
                tile.groundUnitMovedThisTurn = false;
                tile.flyingUnitMovedThisTurn = false;
            }
        }
    }

    public void updateHand()
    {
        if (HandScrollView == null)
        {
            Debug.LogError("BattlefieldManager requires its Hand Scroll View transform.", this);
            return;
        }

        ScrollRect scrollRect = HandScrollView.GetComponent<ScrollRect>();
        RectTransform content = scrollRect != null
            ? scrollRect.content
            : HandScrollView as RectTransform;

        if (content == null)
        {
            Debug.LogError("Hand Scroll View must have a ScrollRect with content assigned.", HandScrollView);
            return;
        }

        if (cardPrefabForHand == null)
        {
            Debug.LogError("BattlefieldManager requires a card prefab for the hand.", this);
            return;
        }

        Canvas prefabCanvas = cardPrefabForHand.GetComponentInChildren<Canvas>();
        RectTransform cardRect = prefabCanvas != null
            ? prefabCanvas.GetComponent<RectTransform>()
            : null;

        if (cardRect == null)
        {
            Debug.LogError("Hand card prefab must contain a Canvas with a RectTransform.", cardPrefabForHand);
            return;
        }

        RectTransform viewport = scrollRect != null && scrollRect.viewport != null
            ? scrollRect.viewport
            : content;

        Canvas.ForceUpdateCanvases();

        if (viewport.rect.width <= HandCardSpacing ||
            cardRect.rect.width <= 0f ||
            cardRect.rect.height <= 0f)
        {
            Debug.LogError("Hand viewport must fit two cards and the card prefab dimensions must be non-zero.", this);
            return;
        }

        for (int i = content.childCount - 1; i >= 0; i--)
        {
            Destroy(content.GetChild(i).gameObject);
        }

        float maxCardWidth = (viewport.rect.width - HandCardSpacing) / HandCardsPerRow;
        float scale = Mathf.Min(HandCardDisplayScale, maxCardWidth / cardRect.rect.width);
        Vector2 displaySize = cardRect.rect.size * scale;
        float columnStep = displaySize.x + HandCardSpacing;
        float rowStep = displaySize.y + HandCardSpacing;

        int cardCount = 0;
        foreach (Card card in Player.GetInstance().curhand)
        {
            if (card != null)
            {
                cardCount++;
            }
        }

        int rowCount = (cardCount + HandCardsPerRow - 1) / HandCardsPerRow;
        float gridWidth = HandCardsPerRow * displaySize.x + (HandCardsPerRow - 1) * HandCardSpacing;
        float contentWidth = Mathf.Max(viewport.rect.width, gridWidth);
        float contentHeight = rowCount > 0
            ? rowCount * displaySize.y + (rowCount - 1) * HandCardSpacing
            : viewport.rect.height;

        content.anchorMin = new Vector2(0.5f, 1f);
        content.anchorMax = new Vector2(0.5f, 1f);
        content.pivot = new Vector2(0.5f, 1f);
        content.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, contentWidth);
        content.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, Mathf.Max(viewport.rect.height, contentHeight));

        int cardIndex = 0;
        foreach (Card card in Player.GetInstance().curhand)
        {
            if (card == null)
            {
                Debug.LogWarning("Skipping a null card in the player's hand.", this);
                continue;
            }

            cardPrefab prefab = Instantiate(cardPrefabForHand, content, false);
            prefab.name = card.name;
            prefab.transform.localPosition = new Vector3(
                -(gridWidth * 0.5f) + (cardIndex % HandCardsPerRow) * columnStep + displaySize.x * 0.5f,
                -(cardIndex / HandCardsPerRow * rowStep + displaySize.y * 0.5f),
                0f);
            prefab.transform.localRotation = Quaternion.identity;
            prefab.transform.localScale = Vector3.one * scale;

            prefab.setCardTo(card);
            prefab.place = cardPrefab.placement.HAND;
            cardIndex++;
        }
        if (scrollRect != null)
        {
            scrollRect.horizontalNormalizedPosition = 0f;
            scrollRect.verticalNormalizedPosition = 1f;
        }
        Debug.Log("Updated hand UI.");
    }
    public bool hasTerrain(int x, int y)
    {
        return battleField[x,y].curTerrain != null;
    }
    public void placeTerrainAt(int x, int y, Terrain terrain, bool playerControlsTerrain = true)
    {
        Tile tile = battleField[x, y];
        bool passConditionChanged = tile.curTerrain != null &&
            tile.curTerrain.condition != terrain.condition;

        tile.curTerrain = terrain;
        tile.playerControlsTerrain = playerControlsTerrain;

        if (!passConditionChanged)
        {
            return;
        }

        if (tile.unitHere != null &&
            !tile.canPassTerrain(tile.unitHere.ability, terrain))
        {
            RemoveUnitFromTile(tile, tile.unitHere);
        }

        if (tile.unitFlyingHere != null &&
            !tile.canPassTerrain(tile.unitFlyingHere.ability, terrain))
        {
            RemoveUnitFromTile(tile, tile.unitFlyingHere);
        }
    }
    public Unit selectAnyUnit()
    {
        //TODO: fill this in later with a visual where the player gets to click a Unit (or more technically a tile with unit on it) and it returns that unit
        return new Unit("0",0,0,0,Element.COSMIC);
    }
    public Tile selectAnyUnitTile()
    {
        //TODO: fill this in later with a visual where the player can select any tile with a unit on it
        return new Tile(0,0);
    }
}

//create an object to represent a single tile on the board
public class Tile
{
    public Terrain curTerrain;
    public bool playerControlsTerrain;
    public Unit unitHere; //track the unit on this terrain
    public Unit unitFlyingHere; //track flying unit here (if applicable)
    public bool playerControlsGroundUnit;
    public bool playerControlsFlyingUnit;
    public bool groundUnitMovedThisTurn;
    public bool flyingUnitMovedThisTurn;
    //the above two store the current state of the unit, so stat mods will happen to those objects up there
    public Boolean groundOccupied; //counts underwater and underground
    public Boolean skyOccupied; //could literally just check if unitFlyingHere is null
    public int x;
    public int y;
    public Tile(int x, int y)
    {
        this.x = x;
        this.y = y;
    }
    public Boolean canCross(Ability creatureAbility)
    //return if the passed creature.ability can pass this tile
    {
        if(creatureAbility == Ability.FLYING){return !skyOccupied;}
        if (groundOccupied)
        {
            return false;
        }

        if (curTerrain == null)
        {
            return false;
        }

        return canPassTerrain(creatureAbility, curTerrain);
    }

    public bool canPassTerrain(Ability creatureAbility, Terrain terrain)
    {
        if (creatureAbility == Ability.FLYING)
        {
            return true;
        }

        if (terrain == null)
        {
            return false;
        }

        if (terrain.condition == PassCondition.CANSWIM)
        {
            return true;
        }

        if(creatureAbility == Ability.SWIMONLY){return terrain.condition == PassCondition.SWIM;}
        if(creatureAbility == Ability.SWIM){return (terrain.condition == PassCondition.SWIM || 
            terrain.condition == PassCondition.NONE);}
        return terrain.condition == PassCondition.NONE;
    }
    public bool canHoldUnit(Unit unit)
    {
        return unit != null &&
            curTerrain != null &&
            (unit.ability == Ability.FLYING || unit.size <= curTerrain.size) &&
            canCross(unit.ability);
    }
    public int getX(){return this.x;}
    public int getY(){return this.y;}
    public void setXY(int x, int y){this.x = x; this.y = y;}
    public void putUnitHere(Unit unit)
    {
        if(unit.ability == Ability.FLYING){unitFlyingHere = unit;}
        else {unitHere = unit;}
    }
    public void clearUnitHere(Boolean ground)
    {
        if(ground){unitHere = null;} else {unitFlyingHere = null;}
    }
}