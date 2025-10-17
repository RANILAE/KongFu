using UnityEngine;
using UnityEngine.UI; // 确保包含此命名空间

public class YinYangSystem : MonoBehaviour
{
    private BattleConfig config;
    private int balanceHealCooldown = 0;
    private bool ultimateQiUsed = false;

    // 新增：用于追踪“临界阳”和“临界阴”状态被触发的次数
    private int criticalYangTriggerCount = 0;
    private int criticalYinTriggerCount = 0;

    // 新增：标记玩家是否已解锁“极端阳”和“极端阴”能力
    private bool isExtremeYangUnlocked = false;
    private bool isExtremeYinUnlocked = false;

    public void Initialize(BattleConfig config)
    {
        this.config = config;
        balanceHealCooldown = 0;
        ultimateQiUsed = false;
        // 初始化计数器和解锁状态
        criticalYangTriggerCount = 0;
        criticalYinTriggerCount = 0;
        isExtremeYangUnlocked = false;
        isExtremeYinUnlocked = false;
        Debug.Log("YinYangSystem initialized. Balance heal cooldown set to: " + balanceHealCooldown);
    }

    public void ApplyEffects(float yangPoints, float yinPoints)
    {
        float diff = yangPoints - yinPoints;
        float absDiff = Mathf.Abs(diff);

        // 应用点数给玩家
        ApplyFinalAttributes(yangPoints, yinPoints, diff);

        // 应用效果基于差值 - 严格按照新规则
        if (absDiff < 1f)
        {
            ApplyBalanceEffect(yangPoints, yinPoints);
        }
        else if (diff >= 1f && diff <= 2.5f)
        {
            // 进入“临界阳”状态
            criticalYangTriggerCount++;
            CheckAndUnlockExtremeYang();
            ApplyCriticalYangEffect(yangPoints, yinPoints);
        }
        else if (diff <= -1f && diff >= -2.5f)
        {
            // 进入“临界阴”状态
            criticalYinTriggerCount++;
            CheckAndUnlockExtremeYin();
            ApplyCriticalYinEffect(yangPoints, yinPoints);
        }
        else if (diff > 2.5f && diff < 5f)
        {
            ApplyYangProsperityEffect(yangPoints, yinPoints); // DOT
        }
        else if (diff < -2.5f && diff > -5f)
        {
            ApplyYinProsperityEffect(yangPoints, yinPoints); // 反震
        }
        else if (diff >= 5f && diff <= 7f)
        {
            // 检查是否已解锁“极端阳”
            if (isExtremeYangUnlocked)
            {
                ApplyExtremeYangEffect(yangPoints, yinPoints); // 伤害和Debuff
            }
            else
            {
                BattleSystem.Instance.uiManager.UpdateBattleLog("Extreme Yang state activated! (Requires 3 Critical Yang triggers to unlock full effect)");
                BattleSystem.Instance.uiManager.ShowStackInsufficientPanel($"Cannot use Extreme Yang yet! Requires 3 Critical Yang uses. Current: {criticalYangTriggerCount}/3.");
            }
        }
        else if (diff <= -5f && diff >= -7f)
        {
            // 检查是否已解锁“极端阴”
            if (isExtremeYinUnlocked)
            {
                ApplyExtremeYinEffect(yangPoints, yinPoints); // 反震, Debuff, Debuff
            }
            else
            {
                BattleSystem.Instance.uiManager.UpdateBattleLog("Extreme Yin state activated! (Requires 3 Critical Yin triggers to unlock full effect)");
                BattleSystem.Instance.uiManager.ShowStackInsufficientPanel($"Cannot use Extreme Yin yet! Requires 3 Critical Yin uses. Current: {criticalYinTriggerCount}/3.");
            }
        }
        else if (absDiff > 7f && absDiff <= 10f && !ultimateQiUsed)
        {
            ApplyUltimateQiEffect(yangPoints, yinPoints); // 血量, 防御, 反震
        }

        // 更新UI显示
        BattleSystem.Instance.uiManager.UpdateYinYangState(diff, GetCurrentStateName(diff));
    }

    // 新增：检查并更新“极端阳”解锁状态
    private void CheckAndUnlockExtremeYang()
    {
        if (criticalYangTriggerCount >= 3 && !isExtremeYangUnlocked)
        {
            isExtremeYangUnlocked = true;
            Debug.Log("Extreme Yang ability unlocked!");
            BattleSystem.Instance.uiManager.UpdateBattleLog("Extreme Yang ability unlocked! Full effect now available.");
        }
    }

    // 新增：检查并更新“极端阴”解锁状态
    private void CheckAndUnlockExtremeYin()
    {
        if (criticalYinTriggerCount >= 3 && !isExtremeYinUnlocked)
        {
            isExtremeYinUnlocked = true;
            Debug.Log("Extreme Yin ability unlocked!");
            BattleSystem.Instance.uiManager.UpdateBattleLog("Extreme Yin ability unlocked! Full effect now available.");
        }
    }

    private void ApplyFinalAttributes(float yangPoints, float yinPoints, float diff)
    {
        Vector2 multipliers = Vector2.one;
        string stateName = GetCurrentStateName(diff);

        // 根据状态选择倍率 (完全按照图片规则)
        switch (stateName)
        {
            case "Balance":
                multipliers = config.balanceMultipliers;
                break;
            case "Critical Yang":
                multipliers = config.criticalYangMultipliers;
                break;
            case "Critical Yin":
                multipliers = config.criticalYinMultipliers;
                break;
            case "Yang Prosperity":
                multipliers = config.yangProsperityMultipliers;
                break;
            case "Yin Prosperity":
                multipliers = config.yinProsperityMultipliers;
                break;
            case "Extreme Yang":
                multipliers = config.extremeYangMultipliers; // 使用配置的 4.5, 0.5
                break;
            case "Extreme Yin":
                multipliers = config.extremeYinMultipliers; // 使用配置的 1.0, 3.0
                break;
            case "Ultimate Qi":
                multipliers = config.ultimateQiMultipliers;
                break;
        }

        // 计算最终属性 (使用float)
        float finalAttack = yangPoints * multipliers.x;
        float finalDefense = yinPoints * multipliers.y;

        // 应用给玩家
        BattleSystem.Instance.playerManager.ApplyAttributes(finalAttack, finalDefense);
    }

    public string GetCurrentStateName(float diff)
    {
        float absDiff = Mathf.Abs(diff);

        if (absDiff < 1f) return "Balance";
        if (diff >= 1f && diff <= 2.5f) return "Critical Yang";
        if (diff <= -1f && diff >= -2.5f) return "Critical Yin";
        if (diff > 2.5f && diff < 5f) return "Yang Prosperity";
        if (diff < -2.5f && diff > -5f) return "Yin Prosperity";
        if (diff >= 5f && diff <= 7f) return "Extreme Yang";
        if (diff <= -5f && diff >= -7f) return "Extreme Yin";
        if (absDiff > 7f && absDiff <= 10f) return "Ultimate Qi";

        return "Unknown State";
    }

    private void ApplyBalanceEffect(float yangPoints, float yinPoints)
    {
        BattleSystem.Instance.uiManager.UpdateBattleLog("Balance state activated!");

        // 检查玩家是否需要回血（不是满血状态）
        PlayerManager player = BattleSystem.Instance.playerManager;
        if (player.Health < player.MaxHealth)
        {
            Debug.Log($"Player health: {player.Health}/{player.MaxHealth}, CD: {balanceHealCooldown}");

            // 检查冷却时间
            if (balanceHealCooldown <= 0)
            {
                // 玩家恢复配置中指定的生命值（不超过最大生命值）
                float healAmount = Mathf.Min(config.balanceHealAmount, player.MaxHealth - player.Health);
                if (healAmount > 0)
                {
                    BattleSystem.Instance.playerManager.Heal(healAmount);
                    BattleSystem.Instance.uiManager.UpdateBattleLog($"Player healed {healAmount:F0} HP!");

                    // 设置冷却时间（使用配置中的CD值）
                    balanceHealCooldown = config.balanceHealCooldown;
                    Debug.Log($"Balance heal triggered. Setting cooldown to: {balanceHealCooldown}");
                }
                else
                {
                    BattleSystem.Instance.uiManager.UpdateBattleLog("Player is at full health, no healing needed!");
                }
            }
            else
            {
                BattleSystem.Instance.uiManager.UpdateBattleLog($"Balance heal effect on cooldown! ({balanceHealCooldown} turns left)");
            }
        }
        else
        {
            BattleSystem.Instance.uiManager.UpdateBattleLog("Player is at full health, balance heal skipped!");
        }
    }

    private void ApplyCriticalYangEffect(float yangPoints, float yinPoints)
    {
        BattleSystem.Instance.uiManager.UpdateBattleLog("Critical Yang state activated!");

        // 给敌方施加阳穿透BUFF
        BattleSystem.Instance.effectManager.AddYangPenetrationStack();
        BattleSystem.Instance.uiManager.UpdateBattleLog("Enemy gained Yang Penetration BUFF!");
    }

    private void ApplyCriticalYinEffect(float yangPoints, float yinPoints)
    {
        BattleSystem.Instance.uiManager.UpdateBattleLog("Critical Yin state activated!");

        // 给敌方施加阴覆盖BUFF
        BattleSystem.Instance.effectManager.AddYinCoverStack();
        BattleSystem.Instance.uiManager.UpdateBattleLog("Enemy gained Yin Cover BUFF!");
    }

    private void ApplyYangProsperityEffect(float yangPoints, float yinPoints)
    {
        BattleSystem.Instance.uiManager.UpdateBattleLog("Yang Prosperity state activated!");

        // 给敌人添加DOT伤害（阳点数/2）
        float dotDamage = yangPoints / 2f;
        BattleSystem.Instance.effectManager.AddEnemyDotEffect(dotDamage, 2);
        BattleSystem.Instance.uiManager.UpdateBattleLog($"Enemy takes continuous damage: {dotDamage:F1}/turn, lasts 2 turns");
    }

    private void ApplyYinProsperityEffect(float yangPoints, float yinPoints)
    {
        BattleSystem.Instance.uiManager.UpdateBattleLog("Yin Prosperity state activated!");

        // 效果：激活反震效果（仅在本回合生效）
        // 持续时间应为1回合，在当前回合的敌人攻击后生效，下一回合开始前失效
        // PlayerManager.ResetForNewTurn 会处理 Duration--
        BattleSystem.Instance.playerManager.ActivateCounterStrike(1.0f, 1); // 倍率1.0，持续1回合
        BattleSystem.Instance.uiManager.UpdateBattleLog($"Player gained Counter Strike effect for this turn!");
    }


    private void ApplyExtremeYangEffect(float yangPoints, float yinPoints)
    {
        BattleSystem.Instance.uiManager.UpdateBattleLog("Extreme Yang state activated! (Full effect applied)");

        // 计算额外伤害（层数*2）
        // 注意：这里使用 EffectManager 的 GetYangPenetrationStacks() 方法获取当前叠层
        int yangStacks = BattleSystem.Instance.effectManager.GetYangPenetrationStacks();
        if (yangStacks > 0) // 只有在有层数时才应用伤害
        {
            float extraDamage = yangStacks * config.extremeYangBonusPerStack; // 使用配置的值
            float totalDamage = BattleSystem.Instance.playerManager.Attack + extraDamage;

            // 对敌人造成伤害
            BattleSystem.Instance.enemyManager.TakeDamage(totalDamage);
            BattleSystem.Instance.uiManager.UpdateBattleLog($"Dealt Extreme Yang damage: {totalDamage:F1}!");
        }
        else
        {
            BattleSystem.Instance.uiManager.UpdateBattleLog($"No Yang Penetration stacks, no extra damage dealt.");
        }


        // 设置下回合攻击减半 (这是一个标记，由PlayerManager在下回合开始时处理)
        BattleSystem.Instance.playerManager.NextTurnAttackDebuff = true;
        BattleSystem.Instance.uiManager.UpdateBattleLog("Next turn attack will be reduced by half!");

        // 重置叠层
        BattleSystem.Instance.effectManager.ResetYangPenetrationStacks();
    }

    private void ApplyExtremeYinEffect(float yangPoints, float yinPoints)
    {
        BattleSystem.Instance.uiManager.UpdateBattleLog("Extreme Yin state activated! (Full effect applied)");

        // 效果1：激活增强反震（1.5倍伤害逻辑在CounterStrikeSystem中处理，基于状态判断，这里只需激活）
        // 持续时间应为1回合
        BattleSystem.Instance.playerManager.ActivateCounterStrike(1.5f, 1); // 激活，持续1回合
        BattleSystem.Instance.uiManager.UpdateBattleLog($"Player gained enhanced Counter Strike for this turn!");

        // 效果2：减少敌人攻击力（层数*2，持续2回合）
        int yinStacks = BattleSystem.Instance.effectManager.GetYinCoverStacks();
        if (yinStacks > 0) // 只有在有层数时才应用
        {
            float attackReduction = yinStacks * config.extremeYinAttackReducePerStack;
            BattleSystem.Instance.effectManager.ApplyEnemyAttackDebuff(attackReduction, config.extremeDebuffDuration);
            BattleSystem.Instance.uiManager.UpdateBattleLog($"Enemy attack reduced by {attackReduction:F1}, lasts {config.extremeDebuffDuration} turns!");
        }
        else
        {
            BattleSystem.Instance.uiManager.UpdateBattleLog($"No Yin Cover stacks, no enemy attack reduction applied.");
        }

        // 效果3：设置下回合防御减半 (这是一个标记，由PlayerManager在下回合开始时处理)
        BattleSystem.Instance.playerManager.NextTurnDefenseDebuff = true;
        BattleSystem.Instance.uiManager.UpdateBattleLog("Next turn defense will be reduced by half!");

        // 效果4：重置叠层
        BattleSystem.Instance.effectManager.ResetYinCoverStacks();
    }

    private void ApplyUltimateQiEffect(float yangPoints, float yinPoints)
    {
        if (ultimateQiUsed)
        {
            BattleSystem.Instance.uiManager.UpdateBattleLog("Ultimate Qi can only be used once per battle!");
            return;
        }

        BattleSystem.Instance.uiManager.UpdateBattleLog("Ultimate Qi state activated!");

        // 效果1：设置玩家生命值为1
        BattleSystem.Instance.playerManager.SetHealth(config.ultimateQiHealthSet);
        BattleSystem.Instance.uiManager.UpdateBattleLog("Player health set to 1!");

        // 效果2：计算究极气状态下的防御力（7*阴点数）
        float qiDefense = 7 * yinPoints;

        // 确保防御力至少为15
        if (qiDefense < 15f)
        {
            qiDefense = 15f;
            BattleSystem.Instance.uiManager.UpdateBattleLog("Qi defense boosted to minimum 15!");
        }

        // 临时设置玩家防御力为究极气防御力 (这个值会在PlayerManager.ResetForNewTurn中被重置，除非有NextTurnDebuff)
        BattleSystem.Instance.playerManager.Defense = qiDefense;
        BattleSystem.Instance.uiManager.UpdatePlayerAttackDefense(BattleSystem.Instance.playerManager.Attack, BattleSystem.Instance.playerManager.Defense);

        // 效果3：获得反震效果（持续3回合）
        BattleSystem.Instance.playerManager.ActivateCounterStrike(1.0f, config.ultimateQiCounterStrikeDuration); // 持续3回合
        BattleSystem.Instance.uiManager.UpdateBattleLog($"Player gained Counter Strike effect for {config.ultimateQiCounterStrikeDuration} turns!");

        // 标记已使用
        ultimateQiUsed = true;
    }

    // 每回合结束时调用
    public void ProcessCooldowns()
    {
        if (balanceHealCooldown > 0)
        {
            balanceHealCooldown--;
            Debug.Log($"YinYangSystem - Balance heal cooldown decreased to: {balanceHealCooldown}");
        }
    }

    // 新增：获取当前CD值（用于调试）
    public int GetBalanceHealCooldown()
    {
        return balanceHealCooldown;
    }

    // 新增：重置CD（用于调试）
    public void ResetBalanceHealCooldown()
    {
        balanceHealCooldown = 0;
        Debug.Log("YinYangSystem - Balance heal cooldown reset to 0");
    }

    // 新增：获取“临界阳”触发次数（用于调试或UI）
    public int GetCriticalYangTriggerCount()
    {
        return criticalYangTriggerCount;
    }

    // 新增：获取“临界阴”触发次数（用于调试或UI）
    public int GetCriticalYinTriggerCount()
    {
        return criticalYinTriggerCount;
    }

    // 新增：检查“极端阳”是否已解锁（用于调试或UI）
    public bool IsExtremeYangUnlocked()
    {
        return isExtremeYangUnlocked;
    }

    // 新增：检查“极端阴”是否已解锁（用于调试或UI）
    public bool IsExtremeYinUnlocked()
    {
        return isExtremeYinUnlocked;
    }
}
