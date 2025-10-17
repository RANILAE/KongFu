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
    private int balanceHealCooldown = 0; // ƽ��״̬�ָ���ȴ

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

        // �������Ч��
        if (iconManager != null) iconManager.ClearAllEffects();

        battleLog.Clear();
        AddLog("=== BATTLE STARTED ===");
        AddLog($"Round {currentRound}");
        StartPlayerTurn();
    }

    public void AddLog(string message)
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
        // �������״̬
        player.ResetForNewTurn(config);

        // ����ϻغϵ��½�Ч��
        if (iconManager != null)
        {
            iconManager.RemovePlayerEffect(IconManager.EffectCategory.AttackDebuff);
            iconManager.RemovePlayerEffect(IconManager.EffectCategory.DefenseDebuff);
        }

        // ���÷���Ч��
        player.counterStrikeActive = false;
        iconManager.RemovePlayerEffect(IconManager.EffectCategory.CounterStrike);

        // ��������ϵͳ��ȴ
        yinYangSystem.UpdateBalanceCooldown();

        currentState = BattleState.PlayerTurn;
        onPlayerTurnStart?.Invoke();
        AddLog("Player turn started");

        // ��ӵ�ǰ������Ϣ
        AddLog($"Current Stacks: Yang Critical: {player.yangCriticalCounter}, Yin Critical: {player.yinCriticalCounter}, Extreme Yang: {player.extremeYangStack}/3, Extreme Yin: {player.extremeYinStack}/3");

        // ���µ�����ʾ
        UpdatePlayerStacksDisplay();
    }

    // ������ҵ�����ʾ
    void UpdatePlayerStacksDisplay()
    {
        if (iconManager == null) return;

        // ���������㣨ֻ���е���ʱ��ʾ��
        if (player.yangCriticalCounter > 0)
        {
            iconManager.AddPlayerEffect(
                IconManager.EffectCategory.YangStack,
                player.yangCriticalCounter
            );
        }

        // ���������㣨ֻ���е���ʱ��ʾ��
        if (player.yinCriticalCounter > 0)
        {
            iconManager.AddPlayerEffect(
                IconManager.EffectCategory.YinStack,
                player.yinCriticalCounter
            );
        }

        // ��ʾ�����½�Ч��
        if (player.nextTurnAttackDebuff)
        {
            iconManager.AddPlayerEffect(IconManager.EffectCategory.AttackDebuff);
        }

        // ��ʾ�����½�Ч��
        if (player.nextTurnDefenseDebuff)
        {
            iconManager.AddPlayerEffect(IconManager.EffectCategory.DefenseDebuff);
        }
    }

    public void EndPlayerTurn()
    {
        if (currentState != BattleState.PlayerTurn) return;

        // ����ԭʼ��������DOT����
        int originalAttack = player.attack;
        int originalDefense = player.defense;

        // Ӧ������Ч��
        ApplyYinYangEffects(originalAttack, originalDefense);

        // ��������˺�
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

    // Ӧ������Ч��������ͼƬ���ƣ�
    void ApplyYinYangEffects(int originalAttack, int originalDefense)
    {
        int yangPoints = originalAttack;
        int yinPoints = originalDefense;
        int diff = yangPoints - yinPoints; // ������ - ������

        // 1. ������״̬ (��-�� �� 5)
        if (diff >= 5)
        {
            // ��鼫���������Ƿ�����
            if (player.extremeYangStack >= 3)
            {
                AddLog("Extreme Yang activated!");

                // ���㹥������ֵ
                player.attack = Mathf.FloorToInt(config.extremeYangAttackMultiplier * yangPoints);
                player.defense = Mathf.FloorToInt(config.extremeYangDefenseMultiplier * yinPoints);

                // ���õ���
                player.ResetExtremeYangStack();

                // �����»غϹ����½�
                player.nextTurnAttackDebuff = true;

                // ��������͸Ч��
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
                // ���㲻���޷�����
                AddLog($"Extreme Yang locked! Requires 3 critical yang stacks (current: {player.extremeYangStack}/3)");
                player.attack = yangPoints;
                player.defense = yinPoints;
            }
            return;
        }

        // 2. ������״̬ (��-�� �� 5)
        if (diff <= -5)
        {
            // ��鼫���������Ƿ�����
            if (player.extremeYinStack >= 3)
            {
                AddLog("Extreme Yin activated!");

                // ���㹥������ֵ
                player.attack = yangPoints;
                player.defense = Mathf.FloorToInt(config.extremeYinDefenseMultiplier * yinPoints);

                // ���õ���
                player.ResetExtremeYinStack();

                // �����»غϷ����½�
                player.nextTurnDefenseDebuff = true;

                // �����Ч��
                player.ActivateCounterStrike(1.5f);
                iconManager.AddPlayerEffect(IconManager.EffectCategory.CounterStrike);
                AddLog("Counter Strike effect activated! (1.5x multiplier)");

                // ����������Ч��
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
                // ���㲻���޷�����
                AddLog($"Extreme Yin locked! Requires 3 critical yin stacks (current: {player.extremeYinStack}/3)");
                player.attack = yangPoints;
                player.defense = yinPoints;
            }
            return;
        }

        // 3. ��ʢ״̬ (4 >= ��-�� > 2)
        if (diff > 2 && diff <= 4)
        {
            AddLog("Yang Sheng activated!");

            // ���㹥������ֵ
            player.attack = Mathf.FloorToInt(config.yangShengAttackMultiplier * yangPoints);
            player.defense = Mathf.FloorToInt(config.yangShengDefenseMultiplier * yinPoints);

            // ��ӵ���DOTЧ��
            int dotDamage = Mathf.FloorToInt(yangPoints / 2);
            AddEnemyDotEffect(dotDamage, 2);
            return;
        }

        // 4. ��ʢ״̬ (4 >= ��-�� > 2)
        if (diff < -2 && diff >= -4)
        {
            AddLog("Yin Sheng activated!");

            // ���㹥������ֵ
            player.attack = yangPoints;
            player.defense = Mathf.FloorToInt(config.yinShengDefenseMultiplier * yinPoints);

            // �����Ч��
            player.ActivateCounterStrike(1.0f);
            iconManager.AddPlayerEffect(IconManager.EffectCategory.CounterStrike);
            AddLog("Counter Strike effect activated! (1.0x multiplier)");
            return;
        }

        // 5. �ٽ���״̬ (��-�� = 2)
        if (diff == 2)
        {
            AddLog("Critical Yang activated!");

            // ���㹥������ֵ
            player.attack = Mathf.FloorToInt(config.criticalYangAttackMultiplier * yangPoints);
            player.defense = Mathf.FloorToInt(config.criticalYangDefenseMultiplier * yinPoints);

            // �����ٽ����������ͼ���������
            player.IncrementYangCriticalCounter();

            // �������������͸Ч��
            enemy.AddYangPenetrationStack();
            iconManager.AddEnemyEffect(
                IconManager.EffectCategory.YangPenetration,
                enemy.yangPenetrationStacks
            );
            AddLog($"Applied Yang Penetration stack to enemy (current: {enemy.yangPenetrationStacks})");
            return;
        }

        // 6. �ٽ���״̬ (��-�� = 2)
        if (diff == -2)
        {
            AddLog("Critical Yin activated!");

            // ���㹥������ֵ
            player.attack = Mathf.FloorToInt(config.criticalYinAttackMultiplier * yangPoints);
            player.defense = Mathf.FloorToInt(config.criticalYinDefenseMultiplier * yinPoints);

            // �����ٽ����������ͼ���������
            player.IncrementYinCriticalCounter();

            // ���������������Ч��
            enemy.AddYinCoverStack();
            iconManager.AddEnemyEffect(
                IconManager.EffectCategory.YinCover,
                enemy.yinCoverStacks
            );
            AddLog($"Applied Yin Cover stack to enemy (current: {enemy.yinCoverStacks})");

            // �����Ч��
            player.ActivateCounterStrike(1.0f);
            iconManager.AddPlayerEffect(IconManager.EffectCategory.CounterStrike);
            AddLog("Counter Strike effect activated! (1.0x multiplier)");
            return;
        }

        // 7. ƽ��״̬ (��-���ľ���ֵ < 1)
        if (Mathf.Abs(diff) < 1)
        {
            AddLog("Balance state activated!");

            // ���㹥������ֵ
            player.attack = Mathf.FloorToInt(config.balanceMultiplier * yangPoints);
            player.defense = Mathf.FloorToInt(config.balanceMultiplier * yinPoints);

            // ����ָ�Ч��
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

        // Ĭ��״̬
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

        // ���ӻغϼ���
        currentRound++;
        AddLog($"Round {currentRound}");

        // ��ʼ��һ�غϵ���һغ�
        StartPlayerTurn();
    }

    void EnemyCharge()
    {
        enemy.baseAttack += 3;
        enemy.currentAttack = enemy.baseAttack;
        AddLog($"Enemy used Power Charge! ATK permanently increased to {enemy.currentAttack}");

        // ���˻غϽ������Ƴ�����ͼ��
        iconManager.RemovePlayerEffect(IconManager.EffectCategory.CounterStrike);
    }

    void EnemyDefense()
    {
        playerDamageFactor = 0.8f;
        AddLog("Enemy entered Defense Stance! Player damage reduced by 20%");

        // ���˻غϽ������Ƴ�����ͼ��
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

            // ������𼤻�
            if (player.counterStrikeActive)
            {
                // ���˹��� < ��ҷ���������ɹ�
                if (damage < player.defense)
                {
                    int counterDamage = Mathf.FloorToInt(player.defense * player.counterStrikeMultiplier);
                    enemy.health = Mathf.Max(0, enemy.health - counterDamage);
                    AddLog($"Counter Strike! Enemy took {counterDamage} damage");
                }
                // ���˹��� > ��ҷ���������ʧ�ܣ�������DOT
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

        // ���˻غϽ������Ƴ�����ͼ��
        iconManager.RemovePlayerEffect(IconManager.EffectCategory.CounterStrike);
    }

    // ������DOTЧ��
    void AddPlayerDotEffect(int damage, int duration)
    {
        player.activeDots.Add(new PlayerData.DotEffect
        {
            damage = damage,
            duration = duration
        });

        // ��ʾ���DOTͼ��
        iconManager.AddPlayerEffect(
            IconManager.EffectCategory.PlayerDot,
            0, // ������
            duration
        );

        AddLog($"Player received DOT effect: {damage} damage per turn for {duration} turns");
    }

    // ��ӵ���DOTЧ��
    void AddEnemyDotEffect(int damage, int duration)
    {
        enemy.activeDots.Add(new EnemyData.DotEffect
        {
            damage = damage,
            duration = duration
        });

        // ��ʾ����DOTͼ��
        iconManager.AddEnemyEffect(
            IconManager.EffectCategory.EnemyDot,
            0, // ������
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

            // �������DOTЧ����ʾ
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

            // ���µ���DOTЧ����ʾ
            UpdateEnemyDotEffectDisplay(dot, i);
        }

        enemy.health = Mathf.Max(0, enemy.health);
    }

    // �������DOTЧ����ʾ
    void UpdatePlayerDotEffectDisplay(PlayerData.DotEffect dot, int index)
    {
        if (iconManager == null) return;

        if (dot.duration <= 0)
        {
            // �Ƴ����ڵ�DOTЧ��
            iconManager.RemovePlayerEffect(IconManager.EffectCategory.PlayerDot);
        }
        else
        {
            // ����DOTЧ��
            iconManager.UpdatePlayerEffect(
                IconManager.EffectCategory.PlayerDot,
                0, // ������
                dot.duration
            );
        }
    }

    // ���µ���DOTЧ����ʾ
    void UpdateEnemyDotEffectDisplay(EnemyData.DotEffect dot, int index)
    {
        if (iconManager == null) return;

        if (dot.duration <= 0)
        {
            // �Ƴ����ڵ�DOTЧ��
            iconManager.RemoveEnemyEffect(IconManager.EffectCategory.EnemyDot);
        }
        else
        {
            // ����DOTЧ��
            iconManager.UpdateEnemyEffect(
                IconManager.EffectCategory.EnemyDot,
                0, // ������
                dot.duration
            );
        }
    }

    void EndBattle(bool playerWins)
    {
        currentState = BattleState.BattleEnd;
        onBattleEnd?.Invoke();
        AddLog(playerWins ? "VICTORY!" : "DEFEAT...");

        // �������Ч��
        if (iconManager != null) iconManager.ClearAllEffects();
    }
}