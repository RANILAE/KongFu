using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;

public class UIManager : MonoBehaviour
{
    [Header("Battle Info")]
    public TMP_Text roundText;
    public TMP_Text battleLogText;

    [Header("Player Status")]
    public TMP_Text playerHealthText;
    public TMP_Text playerAttackText;
    public TMP_Text playerDefenseText;

    [Header("Player Health Bar")]
    public Image playerHealthBarBackground; // Health bar background
    public Image playerHealthBarForeground; // Health bar foreground
    // We no longer need to record the original width, as we are using fillAmount

    [Header("Enemy Status")]
    public TMP_Text enemyHealthText;

    [Header("Enemy Health Bar")]
    public Image enemyHealthBarBackground; // Enemy health bar background
    public Image enemyHealthBarForeground; // Enemy health bar foreground
    // We no longer need to record the original width

    [Header("Yin-Yang State")]
    public TMP_Text yinYangStateText;

    [Header("Yin-Yang Setup")]
    public GameObject yinYangSetupPanel;
    public TMP_Text currentYangText;
    public TMP_Text currentYinText;

    [Header("End Button")]
    public Button endButton;

    // 新增：回合提示UI
    [Header("Turn Indicator")]
    public GameObject turnIndicatorPanel;
    public TMP_Text turnIndicatorText;

    // 新增：游戏胜利面板
    [Header("Win Panel")]
    public GameObject winPanel;
    public TMP_Text winMessageText;
    public Button restartButton;

    // 新增：游戏失败面板
    [Header("Lose Panel")]
    public GameObject losePanel;
    public TMP_Text loseMessageText;
    public Button loseRestartButton;

    // 新增：叠层不足提示面板
    [Header("Stack Insufficient Panel")]
    public GameObject stackInsufficientPanel;
    public TMP_Text stackInsufficientText;
    public Button stackInsufficientConfirmButton;

    // 回合提示控制
    private Coroutine turnIndicatorCoroutine;

    private List<string> battleLog = new List<string>();
    private const int MAX_LOG_LINES = 10;

    void Awake()
    {
        // Bind the end button event to the method in BattleSystem
        if (endButton != null)
        {
            endButton.onClick.AddListener(() => BattleSystem.Instance.OnEndButtonClick());
        }

        // 初始化所有面板为隐藏
        if (winPanel != null)
        {
            winPanel.SetActive(false);
        }

        if (losePanel != null)
        {
            losePanel.SetActive(false);
        }

        // 初始化回合提示面板为隐藏
        if (turnIndicatorPanel != null)
        {
            turnIndicatorPanel.SetActive(false);
        }

        // 初始化叠层不足提示面板为隐藏
        if (stackInsufficientPanel != null)
        {
            stackInsufficientPanel.SetActive(false);
        }
    }

    void Start()
    {
        // 绑定重新开始按钮事件
        if (restartButton != null)
        {
            restartButton.onClick.AddListener(RestartBattle);
        }

        if (loseRestartButton != null)
        {
            loseRestartButton.onClick.AddListener(RestartBattle);
        }

        // 绑定叠层不足确认按钮事件
        if (stackInsufficientConfirmButton != null)
        {
            stackInsufficientConfirmButton.onClick.AddListener(HideStackInsufficientPanel);
        }
    }

    public void Initialize()
    {
        ClearBattleLog();
        SetHealthBarFull();

        // 确保面板隐藏
        if (winPanel != null) winPanel.SetActive(false);
        if (losePanel != null) losePanel.SetActive(false);
        if (stackInsufficientPanel != null) stackInsufficientPanel.SetActive(false);
    }

    // Set health bars to full
    private void SetHealthBarFull()
    {
        if (playerHealthBarForeground != null)
        {
            playerHealthBarForeground.fillAmount = 1.0f;
        }

        if (enemyHealthBarForeground != null)
        {
            enemyHealthBarForeground.fillAmount = 1.0f;
        }
    }

    // Update health bar display
    private void UpdateHealthBar(Image healthBar, float currentHealth, float maxHealth)
    {
        if (healthBar == null || maxHealth <= 0) return;

        float healthRatio = Mathf.Clamp01(currentHealth / maxHealth);
        healthBar.fillAmount = healthRatio;
    }

    public void UpdateRoundDisplay(int round)
    {
        if (roundText) roundText.text = $"Round: {round}";
    }

    public void UpdateBattleLog(string message)
    {
        battleLog.Add(message);

        if (battleLog.Count > MAX_LOG_LINES)
        {
            battleLog.RemoveAt(0);
        }

        battleLogText.text = string.Join("\n", battleLog.ToArray());
    }

    public void ClearBattleLog()
    {
        battleLog.Clear();
        if (battleLogText) battleLogText.text = "";
    }

    public void UpdatePlayerStatus(float health, float maxHealth, float attack, float defense)
    {
        if (playerHealthText) playerHealthText.text = $"HP: {health:F1}/{maxHealth:F1}";
        if (playerAttackText) playerAttackText.text = $"ATK: {attack:F1}";
        if (playerDefenseText) playerDefenseText.text = $"DEF: {defense:F1}";

        UpdateHealthBar(playerHealthBarForeground, health, maxHealth);
    }

    public void UpdatePlayerAttackDefense(float attack, float defense)
    {
        if (playerAttackText) playerAttackText.text = $"ATK: {attack:F1}";
        if (playerDefenseText) playerDefenseText.text = $"DEF: {defense:F1}";
    }

    public void UpdateEnemyStatus(float health, float maxHealth)
    {
        if (enemyHealthText) enemyHealthText.text = $"HP: {health:F1}/{maxHealth:F1}";
        UpdateHealthBar(enemyHealthBarForeground, health, maxHealth);
    }

    public void UpdateYinYangState(float diff, string stateName)
    {
        if (yinYangStateText)
            yinYangStateText.text = $"State: {stateName} | Diff: {diff:F1}";
    }

    public void ShowYinYangSetup()
    {
        if (yinYangSetupPanel)
        {
            yinYangSetupPanel.SetActive(true);
            UpdateYinYangFields(0, 0);
        }
    }

    public void HideYinYangSetup()
    {
        if (yinYangSetupPanel) yinYangSetupPanel.SetActive(false);
    }

    // Fix: changed parameter types from int to float to match other scripts
    public void UpdateYinYangFields(float yang, float yin)
    {
        if (currentYangText) currentYangText.text = $"Yang: {yang:F1}";
        if (currentYinText) currentYinText.text = $"Yin: {yin:F1}";
    }

    // 新增：显示回合提示（可控制持续时间）
    public void ShowTurnIndicator(string turnText, float duration)
    {
        // 如果已有正在运行的协程，先停止它
        if (turnIndicatorCoroutine != null)
        {
            StopCoroutine(turnIndicatorCoroutine);
        }

        // 启动新的协程
        turnIndicatorCoroutine = StartCoroutine(ShowTurnIndicatorCoroutine(turnText, duration));
    }

    private IEnumerator ShowTurnIndicatorCoroutine(string turnText, float duration)
    {
        if (turnIndicatorPanel != null && turnIndicatorText != null)
        {
            turnIndicatorText.text = turnText;
            turnIndicatorPanel.SetActive(true);
            yield return new WaitForSeconds(duration);
            turnIndicatorPanel.SetActive(false);
        }

        turnIndicatorCoroutine = null;
    }

    // 新增：显示回合提示（持续显示，不自动消失）
    public void ShowPersistentTurnIndicator(string turnText)
    {
        // 如果已有正在运行的协程，先停止它
        if (turnIndicatorCoroutine != null)
        {
            StopCoroutine(turnIndicatorCoroutine);
            turnIndicatorCoroutine = null;
        }

        if (turnIndicatorPanel != null && turnIndicatorText != null)
        {
            turnIndicatorText.text = turnText;
            turnIndicatorPanel.SetActive(true);
        }
    }

    // 新增：隐藏回合提示
    public void HideTurnIndicator()
    {
        // 如果已有正在运行的协程，先停止它
        if (turnIndicatorCoroutine != null)
        {
            StopCoroutine(turnIndicatorCoroutine);
            turnIndicatorCoroutine = null;
        }

        if (turnIndicatorPanel != null)
        {
            turnIndicatorPanel.SetActive(false);
        }
    }

    // 新增：立即更新回合提示文本（不改变显示状态）
    public void UpdateTurnIndicatorText(string turnText)
    {
        if (turnIndicatorText != null)
        {
            turnIndicatorText.text = turnText;
        }
    }

    // 新增：显示胜利面板
    public void ShowWinPanel()
    {
        // 隐藏回合提示
        HideTurnIndicator();

        // 隐藏其他UI元素
        if (yinYangSetupPanel) yinYangSetupPanel.SetActive(false);

        if (winPanel == null) return;

        // Show the win panel
        winPanel.SetActive(true);

        // Update the message text
        if (winMessageText != null)
        {
            winMessageText.text = "VICTORY!";
        }
    }

    // 新增：显示失败面板
    public void ShowLosePanel()
    {
        // 隐藏回合提示
        HideTurnIndicator();

        // 隐藏其他UI元素
        if (yinYangSetupPanel) yinYangSetupPanel.SetActive(false);

        if (losePanel == null) return;

        // Show the lose panel
        losePanel.SetActive(true);

        // Update the message text
        if (loseMessageText != null)
        {
            loseMessageText.text = "DEFEAT...";
        }
    }

    // 新增：显示叠层不足提示面板
    public void ShowStackInsufficientPanel(string message)
    {
        if (stackInsufficientPanel == null) return;

        // 显示叠层不足提示面板
        stackInsufficientPanel.SetActive(true);

        // 更新提示文本
        if (stackInsufficientText != null)
        {
            stackInsufficientText.text = message;
        }

        Debug.Log($"Showing stack insufficient panel with message: {message}");
    }

    // 新增：隐藏叠层不足提示面板
    public void HideStackInsufficientPanel()
    {
        if (stackInsufficientPanel != null)
        {
            stackInsufficientPanel.SetActive(false);
        }

        Debug.Log("Hiding stack insufficient panel");
    }

    // 新增：重新开始战斗
    public void RestartBattle()
    {
        Debug.Log("Restarting battle...");

        // 隐藏所有面板
        if (winPanel != null) winPanel.SetActive(false);
        if (losePanel != null) losePanel.SetActive(false);
        if (stackInsufficientPanel != null) stackInsufficientPanel.SetActive(false);

        // 重新开始战斗
        if (BattleSystem.Instance != null)
        {
            BattleSystem.Instance.RestartBattle();
        }
    }
}