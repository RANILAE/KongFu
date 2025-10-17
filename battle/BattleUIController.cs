using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class BattleUIController : MonoBehaviour
{
    [Header("References")]
    public BattleSystem battleSystem;
    public PlayerData playerData;
    public EnemyData enemyData;

    [Header("Player Stats")]
    public TMP_Text playerHP;
    public TMP_Text enemyHP;

    [Header("Qi Points")]
    public TMP_Text qiPoints;

    [Header("Point Allocation")]
    public Button increaseYinBtn;
    public Button decreaseYinBtn;
    public Button increaseYangBtn;
    public Button decreaseYangBtn;
    public TMP_Text yinAllocation;
    public TMP_Text yangAllocation;

    [Header("Preview Stats")]
    public TMP_Text previewAttack;
    public TMP_Text previewDefense;
    public TMP_Text previewState;

    [Header("Battle Log")]
    public TMP_Text battleLogText;

    [Header("UI Controls")]
    public Button endTurnButton;

    private int yinPoints = 0;
    private int yangPoints = 0;
    private const int MAX_LOG_LINES = 10;

    void Start()
    {
        BindEvents();
        ResetPreviewValues();
        UpdateUI();
    }

    // Bind UI events
    void BindEvents()
    {
        // Set button listeners
        endTurnButton.onClick.AddListener(OnEndTurnClick);

        increaseYinBtn.onClick.AddListener(() => AdjustYin(1));
        decreaseYinBtn.onClick.AddListener(() => AdjustYin(-1));
        increaseYangBtn.onClick.AddListener(() => AdjustYang(1));
        decreaseYangBtn.onClick.AddListener(() => AdjustYang(-1));

        // Bind battle system events
        battleSystem.onPlayerTurnStart.AddListener(OnPlayerTurnStart);
        battleSystem.onDamageCalculated.AddListener(UpdateUI);
        battleSystem.onBattleLog.AddListener(UpdateBattleLog);
    }

    // Handle start of player's turn
    void OnPlayerTurnStart()
    {
        yinPoints = 0;
        yangPoints = 0;
        ResetPreviewValues();
        UpdateUI();
    }

    // Adjust yin point allocation
    void AdjustYin(int amount)
    {
        int newYin = Mathf.Clamp(yinPoints + amount, 0, playerData.qiPoints - yangPoints);
        yinPoints = newYin;
        UpdatePreviewStats();
        UpdateUI();
    }

    // Adjust yang point allocation
    void AdjustYang(int amount)
    {
        int newYang = Mathf.Clamp(yangPoints + amount, 0, playerData.qiPoints - yinPoints);
        yangPoints = newYang;
        UpdatePreviewStats();
        UpdateUI();
    }

    // Handle end turn button click
    void OnEndTurnClick()
    {
        // Apply allocated points to player stats
        playerData.defense = yinPoints;
        playerData.attack = yangPoints;

        // End player turn - FIXED REFERENCE
        battleSystem.EndPlayerTurn();
    }

    // Reset preview values
    void ResetPreviewValues()
    {
        if (previewAttack != null) previewAttack.text = "ATK: 0";
        if (previewDefense != null) previewDefense.text = "DEF: 0";
        if (previewState != null) previewState.text = "Balance";
    }

    // Update UI elements
    public void UpdateUI()
    {
        if (playerData == null || enemyData == null) return;

        playerHP.text = $"HP: {playerData.health}";
        enemyHP.text = $"HP: {enemyData.health}";

        yinAllocation.text = $"{yinPoints}";
        yangAllocation.text = $"{yangPoints}";

        int qi = playerData.qiPoints;
        if (qi < 0) qi = 0;

        int remainingQi = qi - (yinPoints + yangPoints);
        qiPoints.text = $"{remainingQi}/7";

        SetButtonStates(remainingQi);
    }

    // Set button interaction states
    void SetButtonStates(int remainingQi)
    {
        if (increaseYinBtn == null || increaseYangBtn == null) return;

        increaseYinBtn.interactable = remainingQi > 0;
        increaseYangBtn.interactable = remainingQi > 0;
        decreaseYinBtn.interactable = yinPoints > 0;
        decreaseYangBtn.interactable = yangPoints > 0;

        if (endTurnButton != null)
        {
            endTurnButton.interactable = battleSystem.currentState == BattleSystem.BattleState.PlayerTurn;
        }
    }

    // Update battle log display
    void UpdateBattleLog(string log)
    {
        if (battleLogText != null)
        {
            battleLogText.text = log;
        }
    }

    // Update preview stats based on allocated points
    void UpdatePreviewStats()
    {
        int tempYin = yinPoints;
        int tempYang = yangPoints;

        int attack = tempYang;
        int defense = tempYin;
        string stateName = "Default";
        string stateColor = "white";

        int diff = tempYang - tempYin;

        if (diff >= 5)
        {
            attack = Mathf.FloorToInt(battleSystem.config.extremeYangAttackMultiplier * tempYang);
            defense = Mathf.FloorToInt(battleSystem.config.extremeYangDefenseMultiplier * tempYin);
            stateName = "Extreme Yang";
            stateColor = "orange";
        }
        else if (diff <= -5)
        {
            attack = tempYang;
            defense = Mathf.FloorToInt(battleSystem.config.extremeYinDefenseMultiplier * tempYin);
            stateName = "Extreme Yin";
            stateColor = "purple";
        }
        else if (diff >= 3)
        {
            attack = Mathf.FloorToInt(battleSystem.config.yangShengAttackMultiplier * tempYang);
            defense = Mathf.FloorToInt(battleSystem.config.yangShengDefenseMultiplier * tempYin);
            stateName = "Yang Sheng";
            stateColor = "orange";
        }
        else if (diff <= -3)
        {
            attack = tempYang;
            defense = Mathf.FloorToInt(battleSystem.config.yinShengDefenseMultiplier * tempYin);
            stateName = "Yin Sheng";
            stateColor = "purple";
        }
        else if (diff == 2)
        {
            attack = Mathf.FloorToInt(battleSystem.config.criticalYangAttackMultiplier * tempYang);
            defense = Mathf.FloorToInt(battleSystem.config.criticalYangDefenseMultiplier * tempYin);
            stateName = "Critical Yang";
            stateColor = "orange";
        }
        else if (diff == -2)
        {
            attack = Mathf.FloorToInt(battleSystem.config.criticalYinAttackMultiplier * tempYang);
            defense = Mathf.FloorToInt(battleSystem.config.criticalYinDefenseMultiplier * tempYin);
            stateName = "Critical Yin";
            stateColor = "purple";
        }
        else if (Mathf.Abs(diff) <= 1)
        {
            attack = Mathf.FloorToInt(battleSystem.config.balanceMultiplier * tempYang);
            defense = Mathf.FloorToInt(battleSystem.config.balanceMultiplier * tempYin);
            stateName = "Balance";
            stateColor = "green";
        }
        else
        {
            attack = tempYang;
            defense = tempYin;
            stateName = "Default";
            stateColor = "white";
        }

        // Update preview displays
        if (previewAttack != null) previewAttack.text = $"ATK: {attack}";
        if (previewDefense != null) previewDefense.text = $"DEF: {defense}";
        if (previewState != null) previewState.text = $"<color={stateColor}>{stateName}</color>";
    }
}