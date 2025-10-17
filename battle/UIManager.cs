using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class UIManager : MonoBehaviour
{
    [Header("Battle Info")]
    public TMP_Text roundText;
    public TMP_Text battleLogText;

    [Header("Player Status")]
    public TMP_Text playerHealthText;
    public TMP_Text playerAttackText;
    public TMP_Text playerDefenseText;

    [Header("Enemy Status")]
    public TMP_Text enemyHealthText;

    [Header("Yin-Yang State")]
    public TMP_Text yinYangStateText;

    [Header("Yin-Yang Setup")]
    public GameObject yinYangSetupPanel;
    public TMP_Text currentYangText;
    public TMP_Text currentYinText;

    [Header("End Button")]
    public Button endButton;

    private List<string> battleLog = new List<string>();
    private const int MAX_LOG_LINES = 10;

    public void Initialize()
    {
        // 绑定结束按钮事件
        if (endButton != null)
        {
            endButton.onClick.AddListener(OnEndButtonClick);
        }

        ClearBattleLog();
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
    }

    public void UpdatePlayerAttackDefense(float attack, float defense)
    {
        if (playerAttackText) playerAttackText.text = $"ATK: {attack:F1}";
        if (playerDefenseText) playerDefenseText.text = $"DEF: {defense:F1}";
    }

    public void UpdateEnemyStatus(float health, float maxHealth)
    {
        if (enemyHealthText) enemyHealthText.text = $"HP: {health:F1}/{maxHealth:F1}";
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

    public void UpdateYinYangFields(int yang, int yin)
    {
        if (currentYangText) currentYangText.text = $"Yang: {yang}";
        if (currentYinText) currentYinText.text = $"Yin: {yin}";
    }

    private void OnEndButtonClick()
    {
        BattleSystem.Instance.OnEndButtonClick();
    }
}