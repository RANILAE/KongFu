using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Text;

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
    private List<string> battleLog = new List<string>();
    private const int MAX_LOG_LINES = 10;
    private string lastLog = ""; // ��¼��һ����־

    void Start()
    {
        BindEvents();
        ResetPreviewValues();
        UpdateUI();
    }

    void BindEvents()
    {
        endTurnButton.onClick.AddListener(OnEndTurnClick);

        increaseYinBtn.onClick.AddListener(() => AdjustYin(1));
        decreaseYinBtn.onClick.AddListener(() => AdjustYin(-1));
        increaseYangBtn.onClick.AddListener(() => AdjustYang(1));
        decreaseYangBtn.onClick.AddListener(() => AdjustYang(-1));

        battleSystem.onPlayerTurnStart.AddListener(OnPlayerTurnStart);
        battleSystem.onDamageCalculated.AddListener(UpdateUI);
        battleSystem.onBattleLog.AddListener(UpdateBattleLog);
    }

    void OnPlayerTurnStart()
    {
        yinPoints = 0;
        yangPoints = 0;
        ResetPreviewValues();
        UpdateUI();
    }

    void AdjustYin(int amount)
    {
        int newYin = Mathf.Clamp(yinPoints + amount, 0, playerData.qiPoints - yangPoints);
        yinPoints = newYin;
        UpdatePreviewStats();
        UpdateUI();
    }

    void AdjustYang(int amount)
    {
        int newYang = Mathf.Clamp(yangPoints + amount, 0, playerData.qiPoints - yinPoints);
        yangPoints = newYang;
        UpdatePreviewStats();
        UpdateUI();
    }

    void OnEndTurnClick()
    {
        playerData.defense = yinPoints;
        playerData.attack = yangPoints;

        battleSystem.EndPlayerTurn();
    }

    void ResetPreviewValues()
    {
        if (previewAttack != null) previewAttack.text = "ATK: 0";
        if (previewDefense != null) previewDefense.text = "DEF: 0";
        if (previewState != null) previewState.text = "Balance";
    }

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

    void SetButtonStates(int remainingQi)
    {
        if (increaseYinBtn == null || increaseYangBtn == null) return;

        increaseYinBtn.interactable = remainingQi > 0;
        increaseYangBtn.interactable = remainingQi > 0;
        decreaseYinBtn.interactable = yinPoints > 0;
        decreaseYangBtn.interactable = yangPoints > 0;

        if (endTurnButton != null)
        {
            // ����Ƿ���Խ����غϣ�����״̬�����Ƿ����㣩
            bool canEndTurn = CanEndTurn();
            endTurnButton.interactable = battleSystem.currentState == BattleSystem.BattleState.PlayerTurn && canEndTurn;

            // ����޷������غϣ������ʾ
            if (!canEndTurn)
            {
                // ʹ��BattleSystem��AddLog���������־
                battleSystem.AddLog("Cannot end turn: Need more critical stacks to unlock extreme states");
            }
        }
    }

    // ����Ƿ���Խ����غϣ�����״̬�����Ƿ����㣩
    bool CanEndTurn()
    {
        // ����������
        int diff = yangPoints - yinPoints;

        // ����Ƿ����㼫����״̬���������㲻��
        if (diff >= 5 && playerData.extremeYangStack < 3)
        {
            return false;
        }

        // ����Ƿ����㼫����״̬���������㲻��
        if (diff <= -5 && playerData.extremeYinStack < 3)
        {
            return false;
        }

        return true;
    }

    // �����־�������޸�����
    void AddLog(string message)
    {
        // �����ظ���־
        if (battleLog.Count > 0 && battleLog[battleLog.Count - 1] == message)
        {
            return;
        }

        battleLog.Add(message);

        while (battleLog.Count > MAX_LOG_LINES)
        {
            battleLog.RemoveAt(0);
        }

        UpdateBattleLogDisplay();
    }

    void UpdateBattleLogDisplay()
    {
        if (battleLogText == null) return;

        StringBuilder formattedLog = new StringBuilder();
        foreach (string line in battleLog)
        {
            formattedLog.AppendLine(line);
        }

        battleLogText.text = formattedLog.ToString();
    }

    void UpdateBattleLog(string log)
    {
        // �����ظ���־
        if (lastLog == log) return;
        lastLog = log;

        // ʹ�ñ��ص�AddLog����
        AddLog(log);
    }

    void UpdatePreviewStats()
    {
        int tempYin = yinPoints;
        int tempYang = yangPoints;

        int attack = tempYang;
        int defense = tempYin;
        string stateName = "Default";
        string stateColor = "white";
        bool stateLocked = false; // ״̬�Ƿ�����

        int diff = tempYang - tempYin;

        // 1. ������״̬ (��-�� �� 5)
        if (diff >= 5)
        {
            // ��鼫���������Ƿ�����
            if (playerData.extremeYangStack < 3)
            {
                stateName = "Extreme Yang (Locked)";
                stateColor = "gray";
                attack = tempYang;
                defense = tempYin;
                stateLocked = true;
            }
            else
            {
                attack = Mathf.FloorToInt(battleSystem.config.extremeYangAttackMultiplier * tempYang);
                defense = Mathf.FloorToInt(battleSystem.config.extremeYangDefenseMultiplier * tempYin);
                stateName = "Extreme Yang";
                stateColor = "orange";
            }
        }
        // 2. ������״̬ (��-�� �� 5)
        else if (diff <= -5)
        {
            // ��鼫���������Ƿ�����
            if (playerData.extremeYinStack < 3)
            {
                stateName = "Extreme Yin (Locked)";
                stateColor = "gray";
                attack = tempYang;
                defense = tempYin;
                stateLocked = true;
            }
            else
            {
                attack = tempYang;
                defense = Mathf.FloorToInt(battleSystem.config.extremeYinDefenseMultiplier * tempYin);
                stateName = "Extreme Yin";
                stateColor = "purple";
            }
        }
        // 3. ��ʢ״̬ (��-�� = 3 or 4)
        else if (diff >= 3)
        {
            attack = Mathf.FloorToInt(battleSystem.config.yangShengAttackMultiplier * tempYang);
            defense = Mathf.FloorToInt(battleSystem.config.yangShengDefenseMultiplier * tempYin);
            stateName = "Yang Sheng";
            stateColor = "orange";
        }
        // 4. ��ʢ״̬ (��-�� = 3 or 4)
        else if (diff <= -3)
        {
            attack = tempYang;
            defense = Mathf.FloorToInt(battleSystem.config.yinShengDefenseMultiplier * tempYin);
            stateName = "Yin Sheng";
            stateColor = "purple";
        }
        // 5. �ٽ���״̬ (��-�� = 2)
        else if (diff == 2)
        {
            attack = Mathf.FloorToInt(battleSystem.config.criticalYangAttackMultiplier * tempYang);
            defense = Mathf.FloorToInt(battleSystem.config.criticalYangDefenseMultiplier * tempYin);
            stateName = "Critical Yang";
            stateColor = "orange";
        }
        // 6. �ٽ���״̬ (��-�� = 2)
        else if (diff == -2)
        {
            attack = Mathf.FloorToInt(battleSystem.config.criticalYinAttackMultiplier * tempYang);
            defense = Mathf.FloorToInt(battleSystem.config.criticalYinDefenseMultiplier * tempYin);
            stateName = "Critical Yin";
            stateColor = "purple";
        }
        // 7. ƽ��״̬ (��-���ľ���ֵ = 0 or 1)
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

        if (previewAttack != null) previewAttack.text = $"ATK: {attack}";
        if (previewDefense != null) previewDefense.text = $"DEF: {defense}";
        if (previewState != null) previewState.text = $"<color={stateColor}>{stateName}</color>";

        // ���״̬����������End��ť
        if (stateLocked)
        {
            endTurnButton.interactable = false;
        }
    }
}