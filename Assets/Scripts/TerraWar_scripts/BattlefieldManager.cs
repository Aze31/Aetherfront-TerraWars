using UnityEngine;
using System;
using System.Collections.Generic;
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
        opponent = GetOpponent();
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
                ClearTemporaryTacticEffects();
                currentPhase = gamePhase.ENEMY_TURN;

                break;
        }
    }

    public void AddTemporaryStatModifiers(Unit unit, int attackModifier, int defenseModifier)
    {
        if (unit == null)
        {
            Debug.LogError("Cannot apply a temporary tactic effect to a null Unit.", this);
            return;
        }

        unit.AddTemporaryStatModifiers(attackModifier, defenseModifier);
        unitsWithTemporaryTacticEffects.Add(unit);
    }

    private void ClearTemporaryTacticEffects()
    {
        foreach (Unit unit in unitsWithTemporaryTacticEffects)
        {
            if (unit != null)
            {
                unit.ClearTemporaryStatModifiers();
            }
        }

        unitsWithTemporaryTacticEffects.Clear();
    }
    private void passTurnPressed()
    {
        ClearTemporaryTacticEffects();
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
            if (!IsTacticTargetSelectionPending)
            {
                HandleSelectionClick();
            }

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
    private Action<Tile, Unit> pendingUnitSelection;
    private Func<Tile, Unit, bool> pendingUnitFilter;
    private Action<Tile> pendingTileSelection;
    private Func<Tile, bool> pendingTileFilter;
    private string pendingSelectionPrompt;
    private readonly HashSet<Unit> unitsWithTemporaryTacticEffects = new HashSet<Unit>();
    private int lastTileClickFrame = -1;
    private bool upArrowHeld;
    private bool downArrowHeld;
    private bool leftArrowHeld;
    private bool rightArrowHeld;
    private Opponent opponent;
    [SerializeField]
    private Transform HandScrollView;
    [SerializeField]
    private cardPrefab cardPrefabForHand;
    public RectTransform tacticZone;
    public NarrativeText narrativeText;
    private Player player = Player.GetInstance();
    private const float HandCardDisplayScale = 35f;
    private const float HandCardSpacing = 8f;
    private const int HandCardsPerRow = 2;
    public bool TerrainThisTurn;
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
        ResetUnitTurnState();
    }
    public void startPlayerTurn()
    {
        ClearTemporaryTacticEffects();
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
        ResetUnitTurnState();
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
    public void playerWon()
    {
        gameObject.SetActive(false);
        currentPhase = gamePhase.OUT_OF_GAME;

        narrativeText.startTypingText("You have defeated the goblin! Well done! (by the way, try checking your collection)");
        //find a way to check the Wizard sprite and restore his pose
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

    public bool IsTacticTargetSelectionPending
    {
        get { return pendingUnitSelection != null || pendingTileSelection != null; }
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

        Unit battlefieldUnit = unit.CreateBattlefieldCopy();
        tile.putUnitHere(battlefieldUnit);
        if (battlefieldUnit.ability == Ability.FLYING)
        {
            tile.skyOccupied = true;
            tile.playerControlsFlyingUnit = playerControlsUnit;
            tile.flyingUnitMovedThisTurn = false;
            tile.flyingUnitAttackedThisTurn = false;
        }
        else
        {
            tile.groundOccupied = true;
            tile.playerControlsGroundUnit = playerControlsUnit;
            tile.groundUnitMovedThisTurn = false;
            tile.groundUnitAttackedThisTurn = false;
        }

        ResolveTerrainEffectsOnBattlefield();
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
            tile.groundUnitAttackedThisTurn = false;
            tile.flyingUnitAttackedThisTurn = false;
        }
    }
    public void damageUnitsAt(int x, int y, int ATTACK)
    {
        if (x < 1 || x > BoardSize || y < 1 || y > BoardSize)
        {
            return;
        }

        Tile tile = battleField[x, y];
        if (tile == null)
        {
            return;
        }

        if (tile.unitHere != null)
        {
            damageUnitAt(tile, tile.unitHere, ATTACK);
        }
        if (tile.unitFlyingHere != null)
        {
            damageUnitAt(tile, tile.unitFlyingHere, ATTACK);
        }
    }

    public bool damageUnitAt(Tile tile, Unit unit, int damage)
    {
        if (tile == null || unit == null || damage <= 0 ||
            (tile.unitHere != unit && tile.unitFlyingHere != unit) ||
            unit.DEFENSE > damage)
        {
            return false;
        }

        RemoveUnitFromTile(tile, unit);
        return true;
    }

    public void damageUnit(Unit unit, int damage)
    {
        if (unit == null)
        {
            return;
        }

        foreach (Tile tile in battleField)
        {
            if (tile != null && (tile.unitHere == unit || tile.unitFlyingHere == unit))
            {
                damageUnitAt(tile, unit, damage);
                return;
            }
        }
    }

    public void damageAllUnits(int damage, Unit excludedUnit = null)
    {
        foreach (Tile tile in battleField)
        {
            if (tile == null)
            {
                continue;
            }

            Unit groundUnit = tile.unitHere;
            Unit flyingUnit = tile.unitFlyingHere;
            if (groundUnit != excludedUnit)
            {
                damageUnitAt(tile, groundUnit, damage);
            }

            if (flyingUnit != excludedUnit)
            {
                damageUnitAt(tile, flyingUnit, damage);
            }
        }
    }

    public void damageUnitsAdjacentTo(Tile tile, int damage)
    {
        if (tile == null)
        {
            return;
        }

        damageUnitsAt(tile.x - 1, tile.y, damage);
        damageUnitsAt(tile.x + 1, tile.y, damage);
        damageUnitsAt(tile.x, tile.y - 1, damage);
        damageUnitsAt(tile.x, tile.y + 1, damage);
    }

    public void damageOpponent(int damage)
    {
        Opponent target = GetOpponent();
        if (target != null)
        {
            target.loseHealth(damage);
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
            tile.flyingUnitAttackedThisTurn = false;
        }
        else
        {
            tile.unitHere = null;
            tile.groundOccupied = false;
            tile.playerControlsGroundUnit = false;
            tile.groundUnitMovedThisTurn = false;
            tile.groundUnitAttackedThisTurn = false;
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
        if (lastTileClickFrame == Time.frameCount)
        {
            return;
        }

        lastTileClickFrame = Time.frameCount;
        if (pendingUnitSelection != null)
        {
            HandlePendingUnitSelection(clickedTileView);
            return;
        }

        if (pendingTileSelection != null)
        {
            HandlePendingTileSelection(clickedTileView);
            return;
        }

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
        Unit opponentUnit = tile != null && tile.unitHere != null && !tile.playerControlsGroundUnit
            ? tile.unitHere
            : tile != null && tile.unitFlyingHere != null && !tile.playerControlsFlyingUnit
                ? tile.unitFlyingHere
                : null;
        if (opponentUnit != null)
        {
            DeselectSelectedUnit();
            ShowOpponentUnitInfo(opponentUnit);
            return;
        }

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

    public void selectAnyUnit(string tacticName, Action<Tile, Unit> onSelected)
    {
        selectAnyUnit(tacticName, null, onSelected);
    }

    public void selectAnyUnit(
        string tacticName,
        Func<Tile, Unit, bool> isValidTarget,
        Action<Tile, Unit> onSelected)
    {
        if (onSelected == null)
        {
            Debug.LogError("Cannot select a tactic target without a callback.", this);
            return;
        }

        if (narrativeText == null)
        {
            Debug.LogError("BattlefieldManager requires NarrativeText to prompt for a tactic target.", this);
            return;
        }

        if (pendingUnitSelection != null || pendingTileSelection != null)
        {
            Debug.LogError("Cannot start a tactic target selection while another selection is active.", this);
            return;
        }

        pendingSelectionPrompt = "Select any unit as a target for " + tacticName + ".";
        pendingUnitFilter = isValidTarget;
        pendingUnitSelection = onSelected;
        DeselectSelectedUnit();
        ShowSelectionPrompt(pendingSelectionPrompt);
    }

    public void selectAnyUnitTile(string tacticName, Action<Tile, Unit> onSelected)
    {
        selectAnyUnit(tacticName, onSelected);
    }

    private void HandlePendingUnitSelection(TilePrefab tileView)
    {
        Tile tile = GetTileFromView(tileView);
        Unit target = tile != null
            ? tile.unitHere != null ? tile.unitHere : tile.unitFlyingHere
            : null;
        if (target == null ||
            (pendingUnitFilter != null && !pendingUnitFilter(tile, target)))
        {
            ShowSelectionPrompt(pendingSelectionPrompt);
            return;
        }

        Action<Tile, Unit> onSelected = pendingUnitSelection;
        string selectionPrompt = pendingSelectionPrompt;
        ClearPendingSelection();
        onSelected(tile, target);
        HideSelectionPromptIfComplete(selectionPrompt);
    }

    private void HandlePendingTileSelection(TilePrefab tileView)
    {
        Tile tile = GetTileFromView(tileView);
        if (tile == null || (pendingTileFilter != null && !pendingTileFilter(tile)))
        {
            ShowSelectionPrompt(pendingSelectionPrompt);
            return;
        }

        Action<Tile> onSelected = pendingTileSelection;
        string selectionPrompt = pendingSelectionPrompt;
        ClearPendingSelection();
        onSelected(tile);
        HideSelectionPromptIfComplete(selectionPrompt);
    }

    private Tile GetTileFromView(TilePrefab tileView)
    {
        if (tileView == null ||
            tileView.x < 1 || tileView.x > BoardSize ||
            tileView.y < 1 || tileView.y > BoardSize)
        {
            return null;
        }

        return battleField[tileView.x, tileView.y];
    }

    private void ClearPendingSelection()
    {
        pendingUnitSelection = null;
        pendingUnitFilter = null;
        pendingTileSelection = null;
        pendingTileFilter = null;
        pendingSelectionPrompt = null;
    }

    private void ShowSelectionPrompt(string prompt)
    {
        if (narrativeText == null)
        {
            Debug.LogError("BattlefieldManager requires NarrativeText to prompt for a tactic target.", this);
            return;
        }

        narrativeText.startTypingText(prompt);
    }

    private void HideSelectionPromptIfComplete(string selectionPrompt)
    {
        if (!IsTacticTargetSelectionPending &&
            narrativeText != null &&
            narrativeText.canvasObject != null &&
            narrativeText.textObject != null &&
            narrativeText.textObject.text == selectionPrompt)
        {
            narrativeText.canvasObject.SetActive(false);
        }
    }

    public void selectAdjacentDestination(
        Tile source,
        Unit unit,
        string tacticName,
        Action<Tile> onMoved)
    {
        if (narrativeText == null)
        {
            Debug.LogError("BattlefieldManager requires NarrativeText to prompt for a move destination.", this);
            return;
        }

        if (source == null || unit == null ||
            !HasAdjacentTacticMoveDestination(source, unit))
        {
            ShowSelectionPrompt("No adjacent tile is available for " + tacticName + ".");
            return;
        }

        pendingSelectionPrompt =
            "Select an adjacent tile for " + unit.name + " to move to (" + tacticName + ").";
        pendingTileFilter = destination => CanMoveUnitForTactic(source, unit, destination);
        pendingTileSelection = destination =>
        {
            if (TryMoveUnitForTactic(source, unit, destination))
            {
                onMoved?.Invoke(destination);
            }
        };
        ShowSelectionPrompt(pendingSelectionPrompt);
    }

    private bool HasAdjacentTacticMoveDestination(Tile source, Unit unit)
    {
        for (int x = source.x - 1; x <= source.x + 1; x++)
        {
            for (int y = source.y - 1; y <= source.y + 1; y++)
            {
                Tile destination = x >= 1 && x <= BoardSize && y >= 1 && y <= BoardSize
                    ? battleField[x, y]
                    : null;
                if (CanMoveUnitForTactic(source, unit, destination))
                {
                    return true;
                }
            }
        }

        return false;
    }

    private bool CanMoveUnitForTactic(Tile source, Unit unit, Tile destination)
    {
        if (source == null || unit == null || destination == null ||
            Math.Abs(source.x - destination.x) + Math.Abs(source.y - destination.y) != 1)
        {
            return false;
        }

        bool isFlying = unit.ability == Ability.FLYING;
        bool isOnSource = isFlying
            ? source.unitFlyingHere == unit
            : source.unitHere == unit;
        return isOnSource &&
            destination.curTerrain != null &&
            destination.canPassTerrain(unit.ability, destination.curTerrain) &&
            destination.canHoldUnit(unit);
    }

    private bool TryMoveUnitForTactic(Tile source, Unit unit, Tile destination)
    {
        if (!CanMoveUnitForTactic(source, unit, destination))
        {
            return false;
        }

        bool playerControlsUnit = unit.ability == Ability.FLYING
            ? source.playerControlsFlyingUnit
            : source.playerControlsGroundUnit;
        return MoveUnitBetweenTiles(source, destination, unit, playerControlsUnit);
    }

    private void ShowOpponentUnitInfo(Unit unit)
    {
        string abilityText = unit.ability == Ability.NONE
            ? string.Empty
            : ", and has " + unit.ability.ToString().ToLowerInvariant();
        narrativeText.startTypingText(
            unit.name + " has " + unit.ATTACK + " attack, and " + unit.DEFENSE +
            " defense" + abilityText + ".");
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
            !IsPlayerUnitOnTile(selectedUnitTile, selectedUnit) ||
            HasUnitAttackedThisTurn(selectedUnitTile, selectedUnit))
        {
            return;
        }

        if (deltaX == 0 &&
            deltaY == 1 &&
            HasClearPathToOpponent(selectedUnit, selectedUnitTile))
        {
            Opponent targetOpponent = GetOpponent();
            if (targetOpponent != null &&
                MarkUnitAttackedThisTurn(selectedUnitTile, selectedUnit))
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

        if (defender == null)
        {
            if (selectedUnit.ability != Ability.FLYING &&
                targetTile.unitFlyingHere != null &&
                !targetTile.playerControlsFlyingUnit)
            {
                narrativeText.startTypingText("Cannot attack target with flying!");
            }
            else if (targetTile.unitHere == null && targetTile.unitFlyingHere == null)
            {
                narrativeText.startTypingText("that tile is empty!");
            }

            return;
        }

        if (selectedUnit.ATTACK >= defender.DEFENSE &&
            MarkUnitAttackedThisTurn(selectedUnitTile, selectedUnit))
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
                pathTile.curTerrain == null ||
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
        if (!unitIsOnSource || HasUnitMovedThisTurn(source, unit))
        {
            return false;
        }

        if (playerControlsUnit && destination.curTerrain == null)
        {
            narrativeText.startTypingText("that tile is empty!");
            return false;
        }

        if (playerControlsUnit &&
            !destination.canPassTerrain(unit.ability, destination.curTerrain))
        {
            narrativeText.startTypingText("Your Unit cannot cross that Terrain!");
            return false;
        }

        return destination.canHoldUnit(unit);
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
        return MoveUnitBetweenTiles(source, destination, unit, playerControlsUnit);
    }

    private bool MoveUnitBetweenTiles(
        Tile source,
        Tile destination,
        Unit unit,
        bool playerControlsUnit)
    {
        if (source == null || destination == null || unit == null)
        {
            return false;
        }

        TilePrefab sourceView = tileViews[source.x, source.y];
        TilePrefab destinationView = tileViews[destination.x, destination.y];
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
            bool hasAttacked = source.flyingUnitAttackedThisTurn;
            source.unitFlyingHere = null;
            source.skyOccupied = false;
            source.playerControlsFlyingUnit = false;
            source.flyingUnitMovedThisTurn = false;
            source.flyingUnitAttackedThisTurn = false;
            destination.unitFlyingHere = unit;
            destination.skyOccupied = true;
            destination.playerControlsFlyingUnit = playerControlsUnit;
            destination.flyingUnitMovedThisTurn = true;
            destination.flyingUnitAttackedThisTurn = hasAttacked;
        }
        else
        {
            bool hasAttacked = source.groundUnitAttackedThisTurn;
            source.unitHere = null;
            source.groundOccupied = false;
            source.playerControlsGroundUnit = false;
            source.groundUnitMovedThisTurn = false;
            source.groundUnitAttackedThisTurn = false;
            destination.unitHere = unit;
            destination.groundOccupied = true;
            destination.playerControlsGroundUnit = playerControlsUnit;
            destination.groundUnitMovedThisTurn = true;
            destination.groundUnitAttackedThisTurn = hasAttacked;
        }

        if (isSelectedUnit)
        {
            selectedUnitTile = destination;
            selectedUnitView = destinationView;
            if (!destinationView.SetUnitSelected(unit, true))
            {
                DeselectSelectedUnit();
            }
        }

        ResolveTerrainEffectsOnBattlefield();
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

    public static bool IsPlayerUnitOnTile(Tile tile, Unit unit)
    {
        return tile != null &&
            ((tile.unitHere == unit && tile.playerControlsGroundUnit) ||
             (tile.unitFlyingHere == unit && tile.playerControlsFlyingUnit));
    }

    private void ResolveTerrainEffectsOnBattlefield()
    {
        CardLibrary library = CardLibrary.getInstance();
        foreach (Tile tile in battleField)
        {
            if (tile == null)
            {
                continue;
            }

            if (tile.unitHere != null)
            {
                library.resolveTerrainOnUnit(tile, tile.unitHere);
            }

            if (tile.unitFlyingHere != null)
            {
                library.resolveTerrainOnUnit(tile, tile.unitFlyingHere);
            }
        }
    }

    private static bool HasUnitMovedThisTurn(Tile tile, Unit unit)
    {
        return tile.unitHere == unit
            ? tile.groundUnitMovedThisTurn
            : tile.flyingUnitMovedThisTurn;
    }

    public bool HasUnitAttackedThisTurn(Tile tile, Unit unit)
    {
        if (tile == null || unit == null)
        {
            return true;
        }

        return unit.ability == Ability.FLYING
            ? tile.unitFlyingHere != unit || tile.flyingUnitAttackedThisTurn
            : tile.unitHere != unit || tile.groundUnitAttackedThisTurn;
    }

    public bool MarkUnitAttackedThisTurn(Tile tile, Unit unit)
    {
        if (tile == null || unit == null)
        {
            return false;
        }

        if (unit.ability == Ability.FLYING)
        {
            if (tile.unitFlyingHere != unit || tile.flyingUnitAttackedThisTurn)
            {
                return false;
            }

            tile.flyingUnitAttackedThisTurn = true;
            return true;
        }

        if (tile.unitHere != unit || tile.groundUnitAttackedThisTurn)
        {
            return false;
        }

        tile.groundUnitAttackedThisTurn = true;
        return true;
    }

    private static void ResetUnitTurnState()
    {
        foreach (Tile tile in battleField)
        {
            if (tile != null)
            {
                tile.groundUnitMovedThisTurn = false;
                tile.flyingUnitMovedThisTurn = false;
                tile.groundUnitAttackedThisTurn = false;
                tile.flyingUnitAttackedThisTurn = false;
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
        if (x < 1 || x > BoardSize || y < 1 || y > BoardSize ||
            terrain == null || battleField[x, y] == null)
        {
            Debug.LogError("Cannot place a null terrain or place it at invalid coordinates.", this);
            return;
        }

        Tile tile = battleField[x, y];
        bool passConditionChanged = tile.curTerrain != null &&
            tile.curTerrain.condition != terrain.condition;

        tile.curTerrain = terrain;
        tile.playerControlsTerrain = playerControlsTerrain;

        if (passConditionChanged &&
            tile.unitHere != null &&
            !tile.canPassTerrain(tile.unitHere.ability, terrain))
        {
            RemoveUnitFromTile(tile, tile.unitHere);
        }

        if (passConditionChanged &&
            tile.unitFlyingHere != null &&
            !tile.canPassTerrain(tile.unitFlyingHere.ability, terrain))
        {
            RemoveUnitFromTile(tile, tile.unitFlyingHere);
        }

        ResolveTerrainEffectsOnBattlefield();
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
    public bool groundUnitAttackedThisTurn;
    public bool flyingUnitAttackedThisTurn;
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