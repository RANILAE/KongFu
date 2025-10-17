using UnityEngine;
using UnityEngine.Events;
using System.Text;
using System.Collections.Generic;
using TMPro;

public class BattleSystem : MonoBehaviour
{
    public static BattleSystem Instance { get; private set; }

    [Header("Battle Entities")]
    public PlayerData player;
    public EnemyData enemy;
    public YinYangSystem yinYangSystem;

    [Header("Battle Config")]
    public BattleConfig config;

    [Header("Battle Log")]
    public TMP_Text battleLogText;

    [Header("Battle State")]
    public BattleState currentState;
    public enum BattleState { PlayerTurn, EnemyTurn, BattleEnd }

    [Header("Event Callbacks")]
    public UnityEvent onPlayerTurnStart;
    public UnityEvent onEnemyTurnStart;
    public UnityEvent onBattleEnd;
    public UnityEvent onDamageCalculated;
    public UnityEvent<string> onBattleLog;

    private List<string> battleLog = new List<string>();
    private const int MAX_LOG_LINES = 10;
    private int damageTakenCount = 0;
    private float playerDamageFactor = 1.0f;
    private int currentRound = 1;
    private string lastLog = ""; // 记录上一条日志

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        if (config == null)
        {
            Debug.LogError("BattleConfig not assigned in BattleSystem!");
            return;
        }

        StartBattle();
    }

    public void StartBattle()
    {
        player.ResetPlayer(config);
        enemy.ResetEnemy(config);
        damageTakenCount = 0;
        playerDamageFactor = 1.0f;
        currentRound = 1;

        battleLog.Clear();
        AddLog("=== BATTLE STARTED ===");
        AddLog($"Round {currentRound}");
        StartPlayerTurn();
    }

    public void AddLog(string message)
    {
        // 避免重复日志
        if (battleLog.Count > 0 && battleLog[battleLog.Count - 1] == message)
        {
            return;
        }

        battleLog.Add(message);

        while (battleLog.Count > MAX_LOG_LINES)
        {
            battleLog.RemoveAt(0);
        }

        StringBuilder formattedLog = new StringBuilder();
        foreach (string line in battleLog)
        {
            formattedLog.AppendLine(line);
        }

        onBattleLog?.Invoke(formattedLog.ToString());
        lastLog = message;
    }

    void StartPlayerTurn()
    {
        // 重置玩家状态
        player.ResetForNewTurn(config);

        // 更新阴阳系统冷却
        yinYangSystem.UpdateBalanceCooldown();

        currentState = BattleState.PlayerTurn;
        onPlayerTurnStart?.Invoke();
        AddLog("Player turn started");

        // 添加当前叠层信息
        AddLog($"Current Stacks: Yang Critical: {player.yangCriticalCounter}, Yin Critical: {player.yinCriticalCounter}, Extreme Yang: {player.extremeYangStack}/3, Extreme Yin: {player.extremeYinStack}/3");
    }

    public void EndPlayerTurn()
    {
        if (currentState != BattleState.PlayerTurn) return;

        // 应用阴阳效果
        yinYangSystem.ApplyBattleEffects(player, enemy, config);

        // 记录状态
        string stateName = yinYangSystem.GetCurrentStateName();
        AddLog($"Yin-Yang State: {stateName}");
        AddLog($"Player ATK: {player.attack}, DEF: {player.defense}");

        // 计算玩家伤害
        int baseDamage = player.attack - enemy.defense;
        int calculatedDamage = Mathf.FloorToInt(baseDamage * playerDamageFactor);

        if (calculatedDamage > 0)
        {
            enemy.health = Mathf.Max(0, enemy.health - calculatedDamage);
            AddLog($"Player dealt {calculatedDamage} damage");
            damageTakenCount++;
        }
        else
        {
            AddLog("Player attack dealt no damage");
        }

        onDamageCalculated?.Invoke();

        if (enemy.health <= 0)
        {
            EndBattle(true);
            return;
        }

        StartEnemyTurn();
    }

    void StartEnemyTurn()
    {
        currentState = BattleState.EnemyTurn;
        onEnemyTurnStart?.Invoke();
        AddLog("Enemy turn started");

        enemy.turnCount++;
        ChooseEnemyAction();
    }

    void ChooseEnemyAction()
    {
        if (enemy.turnCount % 3 == 0)
        {
            EnemyCharge();
        }
        else if (damageTakenCount % 2 == 1)
        {
            EnemyDefense();
        }
        else
        {
            EnemyAttack();
        }

        ProcessPlayerDots();

        onDamageCalculated?.Invoke();

        if (player.health <= 0)
        {
            EndBattle(false);
            return;
        }

        // 增加回合计数
        currentRound++;
        AddLog($"Round {currentRound}");

        // 开始下一回合的玩家回合
        StartPlayerTurn();
    }

    void EnemyCharge()
    {
        enemy.baseAttack += 3;
        enemy.currentAttack = enemy.baseAttack;
        AddLog($"Enemy used Power Charge! ATK permanently increased to {enemy.currentAttack}");
    }

    void EnemyDefense()
    {
        playerDamageFactor = 0.8f;
        AddLog("Enemy entered Defense Stance! Player damage reduced by 20%");
    }

    void EnemyAttack()
    {
        int damage = enemy.currentAttack;
        int actualDamage = Mathf.Max(0, damage - player.defense);

        if (actualDamage > 0)
        {
            player.health = Mathf.Max(0, player.health - actualDamage);
            AddLog($"Enemy dealt {actualDamage} damage");
        }
        else
        {
            AddLog($"Enemy attack failed (Player DEF: {player.defense})");
        }

        // 检查并应用反震效果
        ApplyCounterStrike(damage);
    }

    // 修复：添加单独的方法处理反震效果
    void ApplyCounterStrike(int enemyDamage)
    {
        // 如果反震激活且敌人攻击 < 玩家防御
        if (player.counterStrikeActive && enemyDamage < player.defense)
        {
            int counterDamage = Mathf.FloorToInt(player.defense * player.counterStrikeMultiplier);
            enemy.health = Mathf.Max(0, enemy.health - counterDamage);
            AddLog($"Counter Strike! Enemy took {counterDamage} damage");
        }
    }

    void ProcessPlayerDots()
    {
        for (int i = player.activeDots.Count - 1; i >= 0; i--)
        {
            var dot = player.activeDots[i];
            player.health -= dot.damage;
            dot.duration--;

            AddLog($"Player took {dot.damage} DOT damage");

            if (dot.duration <= 0) player.activeDots.RemoveAt(i);
            else player.activeDots[i] = dot;
        }

        player.health = Mathf.Max(0, player.health);
    }

    void EndBattle(bool playerWins)
    {
        currentState = BattleState.BattleEnd;
        onBattleEnd?.Invoke();
        AddLog(playerWins ? "VICTORY!" : "DEFEAT...");
    }
}