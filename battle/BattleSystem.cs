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
    private int damageReceivedCounter = 0; // Tracks number of times enemy received damage
    private float playerDamageFactor = 1.0f; // Damage multiplier for player
    private int currentRound = 1;

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
        damageReceivedCounter = 0;
        playerDamageFactor = 1.0f;
        currentRound = 1;

        battleLog.Clear();
        AddLog("=== BATTLE STARTED ===");
        AddLog($"Round {currentRound}");
        StartPlayerTurn();
    }

    void AddLog(string message)
    {
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
    }

    void StartPlayerTurn()
    {
        // Apply debuffs from previous turn
        if (player.nextTurnAttackDebuff)
        {
            player.attack = Mathf.FloorToInt(player.attack * 0.5f);
            AddLog("Extreme Yang debuff: Attack halved");
            player.nextTurnAttackDebuff = false;
        }

        if (player.nextTurnDefenseDebuff)
        {
            player.defense = Mathf.FloorToInt(player.defense * 0.5f);
            AddLog("Extreme Yin debuff: Defense halved");
            player.nextTurnDefenseDebuff = false;
        }

        // Reset player state
        player.ResetForNewTurn(config);

        // Reset player damage multiplier at start of player turn
        playerDamageFactor = 1.0f;

        // Update cooldowns
        yinYangSystem.UpdateBalanceCooldown();

        currentState = BattleState.PlayerTurn;
        onPlayerTurnStart?.Invoke();
        AddLog("Player turn started");
    }

    public void EndPlayerTurn()
    {
        if (currentState != BattleState.PlayerTurn) return;

        // Apply Yin-Yang effects
        yinYangSystem.ApplyBattleEffects(player, enemy, config);

        // Log state
        string stateName = yinYangSystem.GetCurrentStateName();
        AddLog($"Yin-Yang State: {stateName}");
        AddLog($"Player ATK: {player.attack}, DEF: {player.defense}");

        // Calculate player damage to enemy
        int baseDamage = player.attack - enemy.defense;
        int calculatedDamage = Mathf.FloorToInt(baseDamage * playerDamageFactor);

        if (calculatedDamage > 0)
        {
            enemy.health = Mathf.Max(0, enemy.health - calculatedDamage);
            AddLog($"Player dealt {calculatedDamage} damage");

            // Increase damage received counter (for defense skill triggering)
            damageReceivedCounter++;
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

        ChooseEnemyAction();
    }

    // ENEMY LOGIC UPDATED BASED ON IMAGE
    void ChooseEnemyAction()
    {
        /* 图像中的敌人技能优先级:
         * 1. 蓄力技能 (Charge Skill) - 每三回合使用一次（最高优先级）
         * 2. 防御技能 (Defense Skill) - 每受到两次伤害后使用（中等优先级）
         * 3. 伤害技能 (Damage Skill) - 默认动作（最低优先级）
         */

        // 1. Charge Skill: Used every 3 rounds - Priority #1
        if (currentRound % 3 == 1)
        {
            EnemyCharge();
        }
        // 2. Defense Skill: Used after receiving 2 damages - Priority #2
        else if (damageReceivedCounter >= 2)
        {
            EnemyDefense();
        }
        // 3. Damage Skill: Default action - Priority #3
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

        // Next round
        currentRound++;
        AddLog($"Round {currentRound}");

        // Start next player turn
        StartPlayerTurn();
    }

    // Enemy Charge Skill - per image description
    void EnemyCharge()
    {
        enemy.baseAttack += 3; // Permanently increase base attack by 3
        enemy.currentAttack = enemy.baseAttack; // Apply the increase
        AddLog($"Enemy used Charge Skill! ATK permanently increased by 3 to {enemy.currentAttack}");
    }

    // Enemy Defense Skill - per image description
    void EnemyDefense()
    {
        // Reduce player damage by 20% until enemy's next turn starts
        playerDamageFactor = 0.8f;
        damageReceivedCounter = 0; // Reset counter after use
        AddLog("Enemy used Defense Skill! Player damage reduced by 20% until enemy's next turn");
    }

    // Enemy Damage Skill - per image description
    void EnemyAttack()
    {
        // Attack damage equals enemy's current attack value
        int damage = enemy.currentAttack;
        int actualDamage = Mathf.Max(0, damage - player.defense);

        if (actualDamage > 0)
        {
            player.health = Mathf.Max(0, player.health - actualDamage);
            AddLog($"Enemy used Damage Skill! Dealt {actualDamage} damage");

            // Apply counter strike if active and enemy attack < player defense
            if (player.counterStrikeActive && damage < player.defense)
            {
                int counterDamage = Mathf.FloorToInt(player.defense * player.counterStrikeMultiplier);
                enemy.health = Mathf.Max(0, enemy.health - counterDamage);
                AddLog($"Counter Strike! Enemy took {counterDamage} damage");
            }
        }
        else
        {
            AddLog($"Enemy attack failed (Player DEF: {player.defense})");
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