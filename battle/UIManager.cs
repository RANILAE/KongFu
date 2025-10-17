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

    // �������غ���ʾUI
    [Header("Turn Indicator")]
    public GameObject turnIndicatorPanel;
    public TMP_Text turnIndicatorText;

    // ��������Ϸʤ�����
    [Header("Win Panel")]
    public GameObject winPanel;
    public TMP_Text winMessageText;
    public Button restartButton;

    // ��������Ϸʧ�����
    [Header("Lose Panel")]
    public GameObject losePanel;
    public TMP_Text loseMessageText;
    public Button loseRestartButton;

    // ���������㲻����ʾ���
    [Header("Stack Insufficient Panel")]
    public GameObject stackInsufficientPanel;
    public TMP_Text stackInsufficientText;
    public Button stackInsufficientConfirmButton;

    // �غ���ʾ����
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

        // ��ʼ���������Ϊ����
        if (winPanel != null)
        {
            winPanel.SetActive(false);
        }

        if (losePanel != null)
        {
            losePanel.SetActive(false);
        }

        // ��ʼ���غ���ʾ���Ϊ����
        if (turnIndicatorPanel != null)
        {
            turnIndicatorPanel.SetActive(false);
        }

        // ��ʼ�����㲻����ʾ���Ϊ����
        if (stackInsufficientPanel != null)
        {
            stackInsufficientPanel.SetActive(false);
        }
    }

    void Start()
    {
        // �����¿�ʼ��ť�¼�
        if (restartButton != null)
        {
            restartButton.onClick.AddListener(RestartBattle);
        }

        if (loseRestartButton != null)
        {
            loseRestartButton.onClick.AddListener(RestartBattle);
        }

        // �󶨵��㲻��ȷ�ϰ�ť�¼�
        if (stackInsufficientConfirmButton != null)
        {
            stackInsufficientConfirmButton.onClick.AddListener(HideStackInsufficientPanel);
        }
    }

    public void Initialize()
    {
        ClearBattleLog();
        SetHealthBarFull();

        // ȷ���������
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

    // ��������ʾ�غ���ʾ���ɿ��Ƴ���ʱ�䣩
    public void ShowTurnIndicator(string turnText, float duration)
    {
        // ��������������е�Э�̣���ֹͣ��
        if (turnIndicatorCoroutine != null)
        {
            StopCoroutine(turnIndicatorCoroutine);
        }

        // �����µ�Э��
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

    // ��������ʾ�غ���ʾ��������ʾ�����Զ���ʧ��
    public void ShowPersistentTurnIndicator(string turnText)
    {
        // ��������������е�Э�̣���ֹͣ��
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

    // ���������ػغ���ʾ
    public void HideTurnIndicator()
    {
        // ��������������е�Э�̣���ֹͣ��
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

    // �������������»غ���ʾ�ı������ı���ʾ״̬��
    public void UpdateTurnIndicatorText(string turnText)
    {
        if (turnIndicatorText != null)
        {
            turnIndicatorText.text = turnText;
        }
    }

    // ��������ʾʤ�����
    public void ShowWinPanel()
    {
        // ���ػغ���ʾ
        HideTurnIndicator();

        // ��������UIԪ��
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

    // ��������ʾʧ�����
    public void ShowLosePanel()
    {
        // ���ػغ���ʾ
        HideTurnIndicator();

        // ��������UIԪ��
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

    // ��������ʾ���㲻����ʾ���
    public void ShowStackInsufficientPanel(string message)
    {
        if (stackInsufficientPanel == null) return;

        // ��ʾ���㲻����ʾ���
        stackInsufficientPanel.SetActive(true);

        // ������ʾ�ı�
        if (stackInsufficientText != null)
        {
            stackInsufficientText.text = message;
        }

        Debug.Log($"Showing stack insufficient panel with message: {message}");
    }

    // ���������ص��㲻����ʾ���
    public void HideStackInsufficientPanel()
    {
        if (stackInsufficientPanel != null)
        {
            stackInsufficientPanel.SetActive(false);
        }

        Debug.Log("Hiding stack insufficient panel");
    }

    // ���������¿�ʼս��
    public void RestartBattle()
    {
        Debug.Log("Restarting battle...");

        // �����������
        if (winPanel != null) winPanel.SetActive(false);
        if (losePanel != null) losePanel.SetActive(false);
        if (stackInsufficientPanel != null) stackInsufficientPanel.SetActive(false);

        // ���¿�ʼս��
        if (BattleSystem.Instance != null)
        {
            BattleSystem.Instance.RestartBattle();
        }
    }
}