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
    public IconManager iconManager;

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
    private string lastLog = "";
    private int balanceHealCooldown = 0; // 平衡状态恢复冷却

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
        balanceHealCooldown = 0;

        // 清除所有效果
        if (iconManager != null) iconManager.ClearAllEffects();

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

        // 清除上回合的下降效果
        if (iconManager != null)
        {
            iconManager.RemovePlayerEffect(IconManager.EffectCategory.AttackDebuff);
            iconManager.RemovePlayerEffect(IconManager.EffectCategory.DefenseDebuff);
        }

        // 重置反震效果
        player.counterStrikeActive = false;
        iconManager.RemovePlayerEffect(IconManager.EffectCategory.CounterStrike);

        // 更新阴阳系统冷却
        yinYangSystem.UpdateBalanceCooldown();

        currentState = BattleState.PlayerTurn;
        onPlayerTurnStart?.Invoke();
        AddLog("Player turn started");

        // 添加当前叠层信息
        AddLog($"Current Stacks: Yang Critical: {player.yangCriticalCounter}, Yin Critical: {player.yinCriticalCounter}, Extreme Yang: {player.extremeYangStack}/3, Extreme Yin: {player.extremeYinStack}/3");

        // 更新叠层显示
        UpdatePlayerStacksDisplay();
    }

    // 更新玩家叠层显示
    void UpdatePlayerStacksDisplay()
    {
        if (iconManager == null) return;

        // 更新阳叠层（只在有叠层时显示）
        if (player.yangCriticalCounter > 0)
        {
            iconManager.AddPlayerEffect(
                IconManager.EffectCategory.YangStack,
                player.yangCriticalCounter
            );
        }

        // 更新阴叠层（只在有叠层时显示）
        if (player.yinCriticalCounter > 0)
        {
            iconManager.AddPlayerEffect(
                IconManager.EffectCategory.YinStack,
                player.yinCriticalCounter
            );
        }

        // 显示攻击下降效果
        if (player.nextTurnAttackDebuff)
        {
            iconManager.AddPlayerEffect(IconManager.EffectCategory.AttackDebuff);
        }

        // 显示防御下降效果
        if (player.nextTurnDefenseDebuff)
        {
            iconManager.AddPlayerEffect(IconManager.EffectCategory.DefenseDebuff);
        }
    }

    public void EndPlayerTurn()
    {
        if (currentState != BattleState.PlayerTurn) return;

        // 保存原始点数用于DOT计算
        int originalAttack = player.attack;
        int originalDefense = player.defense;

        // 应用阴阳效果
        ApplyYinYangEffects(originalAttack, originalDefense);

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

    // 应用阴阳效果（根据图片机制）
    void ApplyYinYangEffects(int originalAttack, int originalDefense)
    {
        int yangPoints = originalAttack;
        int yinPoints = originalDefense;
        int diff = yangPoints - yinPoints; // 阳点数 - 阴点数

        // 1. 极端阳状态 (阳-阴 ≥ 5)
        if (diff >= 5)
        {
            // 检查极端阳叠层是否满足
            if (player.extremeYangStack >= 3)
            {
                AddLog("Extreme Yang activated!");

                // 计算攻击防御值
                player.attack = Mathf.FloorToInt(config.extremeYangAttackMultiplier * yangPoints);
                player.defense = Mathf.FloorToInt(config.extremeYangDefenseMultiplier * yinPoints);

                // 重置叠层
                player.ResetExtremeYangStack();

                // 设置下回合攻击下降
                player.nextTurnAttackDebuff = true;

                // 引爆阳穿透效果
                if (enemy.yangPenetrationStacks > 0)
                {
                    int yangDamage = enemy.yangPenetrationStacks * yangPoints;
                    enemy.health = Mathf.Max(0, enemy.health - yangDamage);
                    enemy.yangPenetrationStacks = 0;
                    iconManager.RemoveEnemyEffect(IconManager.EffectCategory.YangPenetration);
                    AddLog($"Extreme Yang detonation! Dealt {yangDamage} damage to enemy");
                }
            }
            else
            {
                // 叠层不足无法激活
                AddLog($"Extreme Yang locked! Requires 3 critical yang stacks (current: {player.extremeYangStack}/3)");
                player.attack = yangPoints;
                player.defense = yinPoints;
            }
            return;
        }

        // 2. 极端阴状态 (阴-阳 ≥ 5)
        if (diff <= -5)
        {
            // 检查极端阴叠层是否满足
            if (player.extremeYinStack >= 3)
            {
                AddLog("Extreme Yin activated!");

                // 计算攻击防御值
                player.attack = yangPoints;
                player.defense = Mathf.FloorToInt(config.extremeYinDefenseMultiplier * yinPoints);

                // 重置叠层
                player.ResetExtremeYinStack();

                // 设置下回合防御下降
                player.nextTurnDefenseDebuff = true;

                // 激活反震效果
                player.ActivateCounterStrike(1.5f);
                iconManager.AddPlayerEffect(IconManager.EffectCategory.CounterStrike);
                AddLog("Counter Strike effect activated! (1.5x multiplier)");

                // 引爆阴覆盖效果
                if (enemy.yinCoverStacks > 0)
                {
                    int defenseReduce = enemy.yinCoverStacks * 2;
                    enemy.defense = Mathf.Max(0, enemy.defense - defenseReduce);
                    enemy.ResetYinCoverStacks();
                    iconManager.RemoveEnemyEffect(IconManager.EffectCategory.YinCover);
                    AddLog($"Extreme Yin detonation! Reduced enemy defense by {defenseReduce}");
                }
            }
            else
            {
                // 叠层不足无法激活
                AddLog($"Extreme Yin locked! Requires 3 critical yin stacks (current: {player.extremeYinStack}/3)");
                player.attack = yangPoints;
                player.defense = yinPoints;
            }
            return;
        }

        // 3. 阳盛状态 (4 >= 阳-阴 > 2)
        if (diff > 2 && diff <= 4)
        {
            AddLog("Yang Sheng activated!");

            // 计算攻击防御值
            player.attack = Mathf.FloorToInt(config.yangShengAttackMultiplier * yangPoints);
            player.defense = Mathf.FloorToInt(config.yangShengDefenseMultiplier * yinPoints);

            // 添加敌人DOT效果
            int dotDamage = Mathf.FloorToInt(yangPoints / 2);
            AddEnemyDotEffect(dotDamage, 2);
            return;
        }

        // 4. 阴盛状态 (4 >= 阴-阳 > 2)
        if (diff < -2 && diff >= -4)
        {
            AddLog("Yin Sheng activated!");

            // 计算攻击防御值
            player.attack = yangPoints;
            player.defense = Mathf.FloorToInt(config.yinShengDefenseMultiplier * yinPoints);

            // 激活反震效果
            player.ActivateCounterStrike(1.0f);
            iconManager.AddPlayerEffect(IconManager.EffectCategory.CounterStrike);
            AddLog("Counter Strike effect activated! (1.0x multiplier)");
            return;
        }

        // 5. 临界阳状态 (阳-阴 = 2)
        if (diff == 2)
        {
            AddLog("Critical Yang activated!");

            // 计算攻击防御值
            player.attack = Mathf.FloorToInt(config.criticalYangAttackMultiplier * yangPoints);
            player.defense = Mathf.FloorToInt(config.criticalYangDefenseMultiplier * yinPoints);

            // 增加临界阳计数器和极端阳叠层
            player.IncrementYangCriticalCounter();

            // 给敌人添加阳穿透效果
            enemy.AddYangPenetrationStack();
            iconManager.AddEnemyEffect(
                IconManager.EffectCategory.YangPenetration,
                enemy.yangPenetrationStacks
            );
            AddLog($"Applied Yang Penetration stack to enemy (current: {enemy.yangPenetrationStacks})");
            return;
        }

        // 6. 临界阴状态 (阴-阳 = 2)
        if (diff == -2)
        {
            AddLog("Critical Yin activated!");

            // 计算攻击防御值
            player.attack = Mathf.FloorToInt(config.criticalYinAttackMultiplier * yangPoints);
            player.defense = Mathf.FloorToInt(config.criticalYinDefenseMultiplier * yinPoints);

            // 增加临界阴计数器和极端阴叠层
            player.IncrementYinCriticalCounter();

            // 给敌人添加阴覆盖效果
            enemy.AddYinCoverStack();
            iconManager.AddEnemyEffect(
                IconManager.EffectCategory.YinCover,
                enemy.yinCoverStacks
            );
            AddLog($"Applied Yin Cover stack to enemy (current: {enemy.yinCoverStacks})");

            // 激活反震效果
            player.ActivateCounterStrike(1.0f);
            iconManager.AddPlayerEffect(IconManager.EffectCategory.CounterStrike);
            AddLog("Counter Strike effect activated! (1.0x multiplier)");
            return;
        }

        // 7. 平衡状态 (阳-阴的绝对值 < 1)
        if (Mathf.Abs(diff) < 1)
        {
            AddLog("Balance state activated!");

            // 计算攻击防御值
            player.attack = Mathf.FloorToInt(config.balanceMultiplier * yangPoints);
            player.defense = Mathf.FloorToInt(config.balanceMultiplier * yinPoints);

            // 处理恢复效果
            if (balanceHealCooldown == 0)
            {
                player.health = Mathf.Min(player.health + 5, player.maxHealth);
                AddLog("Balance state healed 5 HP!");
                balanceHealCooldown = 2;
            }
            else
            {
                balanceHealCooldown--;
                AddLog($"Balance state heal on cooldown. {balanceHealCooldown} turns left");
            }
            return;
        }

        // 默认状态
        AddLog("No special effect from Yin-Yang energies");
        player.attack = yangPoints;
        player.defense = yinPoints;
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
        ProcessEnemyDots();

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

        // 敌人回合结束，移除反震图标
        iconManager.RemovePlayerEffect(IconManager.EffectCategory.CounterStrike);
    }

    void EnemyDefense()
    {
        playerDamageFactor = 0.8f;
        AddLog("Enemy entered Defense Stance! Player damage reduced by 20%");

        // 敌人回合结束，移除反震图标
        iconManager.RemovePlayerEffect(IconManager.EffectCategory.CounterStrike);
    }

    void EnemyAttack()
    {
        int damage = enemy.currentAttack;
        int actualDamage = Mathf.Max(0, damage - player.defense);

        if (actualDamage > 0)
        {
            player.health = Mathf.Max(0, player.health - actualDamage);
            AddLog($"Enemy dealt {actualDamage} damage");

            // 如果反震激活
            if (player.counterStrikeActive)
            {
                // 敌人攻击 < 玩家防御：反震成功
                if (damage < player.defense)
                {
                    int counterDamage = Mathf.FloorToInt(player.defense * player.counterStrikeMultiplier);
                    enemy.health = Mathf.Max(0, enemy.health - counterDamage);
                    AddLog($"Counter Strike! Enemy took {counterDamage} damage");
                }
                // 敌人攻击 > 玩家防御：反震失败，添加玩家DOT
                else
                {
                    int dotDamage = Mathf.FloorToInt(damage * 0.5f);
                    AddPlayerDotEffect(dotDamage, 2);
                    AddLog("Counter Strike failed! Player receives DOT damage");
                }
            }
        }
        else
        {
            AddLog($"Enemy attack failed (Player DEF: {player.defense})");
        }

        // 敌人回合结束，移除反震图标
        iconManager.RemovePlayerEffect(IconManager.EffectCategory.CounterStrike);
    }

    // 添加玩家DOT效果
    void AddPlayerDotEffect(int damage, int duration)
    {
        player.activeDots.Add(new PlayerData.DotEffect
        {
            damage = damage,
            duration = duration
        });

        // 显示玩家DOT图标
        iconManager.AddPlayerEffect(
            IconManager.EffectCategory.PlayerDot,
            0, // 叠层数
            duration
        );

        AddLog($"Player received DOT effect: {damage} damage per turn for {duration} turns");
    }

    // 添加敌人DOT效果
    void AddEnemyDotEffect(int damage, int duration)
    {
        enemy.activeDots.Add(new EnemyData.DotEffect
        {
            damage = damage,
            duration = duration
        });

        // 显示敌人DOT图标
        iconManager.AddEnemyEffect(
            IconManager.EffectCategory.EnemyDot,
            0, // 叠层数
            duration
        );

        AddLog($"Enemy received DOT effect: {damage} damage per turn for {duration} turns");
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

            // 更新玩家DOT效果显示
            UpdatePlayerDotEffectDisplay(dot, i);
        }

        player.health = Mathf.Max(0, player.health);
    }

    void ProcessEnemyDots()
    {
        for (int i = enemy.activeDots.Count - 1; i >= 0; i--)
        {
            var dot = enemy.activeDots[i];
            enemy.health -= dot.damage;
            dot.duration--;

            AddLog($"Enemy took {dot.damage} DOT damage");

            if (dot.duration <= 0) enemy.activeDots.RemoveAt(i);
            else enemy.activeDots[i] = dot;

            // 更新敌人DOT效果显示
            UpdateEnemyDotEffectDisplay(dot, i);
        }

        enemy.health = Mathf.Max(0, enemy.health);
    }

    // 更新玩家DOT效果显示
    void UpdatePlayerDotEffectDisplay(PlayerData.DotEffect dot, int index)
    {
        if (iconManager == null) return;

        if (dot.duration <= 0)
        {
            // 移除过期的DOT效果
            iconManager.RemovePlayerEffect(IconManager.EffectCategory.PlayerDot);
        }
        else
        {
            // 更新DOT效果
            iconManager.UpdatePlayerEffect(
                IconManager.EffectCategory.PlayerDot,
                0, // 叠层数
                dot.duration
            );
        }
    }

    // 更新敌人DOT效果显示
    void UpdateEnemyDotEffectDisplay(EnemyData.DotEffect dot, int index)
    {
        if (iconManager == null) return;

        if (dot.duration <= 0)
        {
            // 移除过期的DOT效果
            iconManager.RemoveEnemyEffect(IconManager.EffectCategory.EnemyDot);
        }
        else
        {
            // 更新DOT效果
            iconManager.UpdateEnemyEffect(
                IconManager.EffectCategory.EnemyDot,
                0, // 叠层数
                dot.duration
            );
        }
    }

    void EndBattle(bool playerWins)
    {
        currentState = BattleState.BattleEnd;
        onBattleEnd?.Invoke();
        AddLog(playerWins ? "VICTORY!" : "DEFEAT...");

        // 清除所有效果
        if (iconManager != null) iconManager.ClearAllEffects();
    }
}