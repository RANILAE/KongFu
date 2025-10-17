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

        // 应用效果基于差值
        if (absDiff < 1f)
        {
            ApplyBalanceEffect(yangPoints, yinPoints);
        }
        else if (diff >= 1f && diff <= 2.5f)
        {
            // 进入“临界阳”状态，增加计数器并检查解锁
            criticalYangTriggerCount++;
            CheckAndUnlockExtremeYang(); // 检查是否解锁极端阳
            ApplyCriticalYangEffect(yangPoints, yinPoints);
        }
        else if (diff <= -1f && diff >= -2.5f)
        {
            // 进入“临界阴”状态，增加计数器并检查解锁
            criticalYinTriggerCount++;
            CheckAndUnlockExtremeYin(); // 检查是否解锁极端阴
            ApplyCriticalYinEffect(yangPoints, yinPoints);
        }
        else if (diff > 2.5f && diff < 5f)
        {
            ApplyYangProsperityEffect(yangPoints, yinPoints);
        }
        else if (diff < -2.5f && diff > -5f)
        {
            ApplyYinProsperityEffect(yangPoints, yinPoints);
        }
        else if (diff >= 5f && diff <= 7f)
        {
            // 检查是否已解锁“极端阳”
            if (isExtremeYangUnlocked)
            {
                ApplyExtremeYangEffect(yangPoints, yinPoints);
            }
            else
            {
                // 如果未解锁，仍然应用属性倍率，但不执行其特殊效果
                // ApplyFinalAttributes 已在上面调用，这里无需重复
                BattleSystem.Instance.uiManager.UpdateBattleLog("Extreme Yang state activated! (Requires 3 Critical Yang triggers to unlock full effect)");
                // 使用通用的 Stack Insufficient Panel 显示提示
                BattleSystem.Instance.uiManager.ShowStackInsufficientPanel($"Cannot use Extreme Yang yet! Requires 3 Critical Yang uses. Current: {criticalYangTriggerCount}/3.");
            }
        }
        else if (diff <= -5f && diff >= -7f)
        {
            // 检查是否已解锁“极端阴”
            if (isExtremeYinUnlocked)
            {
                ApplyExtremeYinEffect(yangPoints, yinPoints);
            }
            else
            {
                // 如果未解锁，仍然应用属性倍率，但不执行其特殊效果
                // ApplyFinalAttributes 已在上面调用，这里无需重复
                BattleSystem.Instance.uiManager.UpdateBattleLog("Extreme Yin state activated! (Requires 3 Critical Yin triggers to unlock full effect)");
                // 使用通用的 Stack Insufficient Panel 显示提示
                BattleSystem.Instance.uiManager.ShowStackInsufficientPanel($"Cannot use Extreme Yin yet! Requires 3 Critical Yin uses. Current: {criticalYinTriggerCount}/3.");
            }
        }
        else if (absDiff > 7f && absDiff <= 10f && !ultimateQiUsed)
        {
            ApplyUltimateQiEffect(yangPoints, yinPoints);
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

        // 激活反震效果（从BattleConfig读取持续时间）
        int counterStrikeDuration = config.yinProsperityCounterStrikeDuration;
        BattleSystem.Instance.playerManager.ActivateCounterStrike(1.0f, counterStrikeDuration);
        BattleSystem.Instance.uiManager.UpdateBattleLog($"Player gained Counter Strike effect for {counterStrikeDuration} turn(s)!");
    }

    private void ApplyExtremeYangEffect(float yangPoints, float yinPoints)
    {
        BattleSystem.Instance.uiManager.UpdateBattleLog("Extreme Yang state activated! (Full effect applied)");

        // 计算额外伤害（层数*2）
        // 注意：这里使用 EffectManager 的 GetYangPenetrationStacks() 方法获取当前叠层
        int yangStacks = BattleSystem.Instance.effectManager.GetYangPenetrationStacks();
        float extraDamage = yangStacks * config.extremeYangBonusPerStack; // 使用配置的值
        float totalDamage = BattleSystem.Instance.playerManager.Attack + extraDamage;

        // 对敌人造成伤害
        BattleSystem.Instance.enemyManager.TakeDamage(totalDamage);
        BattleSystem.Instance.uiManager.UpdateBattleLog($"Dealt Extreme Yang damage: {totalDamage:F1}!");

        // 设置下回合攻击减半
        BattleSystem.Instance.playerManager.NextTurnAttackDebuff = true;
        BattleSystem.Instance.uiManager.UpdateBattleLog("Next turn attack reduced by half!");

        // 重置叠层
        BattleSystem.Instance.effectManager.ResetYangPenetrationStacks();
    }

    private void ApplyExtremeYinEffect(float yangPoints, float yinPoints)
    {
        BattleSystem.Instance.uiManager.UpdateBattleLog("Extreme Yin state activated! (Full effect applied)");

        // 激活增强反震（1.5倍，从BattleConfig读取持续时间）
        int counterStrikeDuration = config.extremeYinCounterStrikeDuration;
        BattleSystem.Instance.playerManager.ActivateCounterStrike(1.5f, counterStrikeDuration);
        BattleSystem.Instance.uiManager.UpdateBattleLog($"Player gained enhanced Counter Strike (1.5x) for {counterStrikeDuration} turn(s)!");

        // 减少敌人攻击力（层数*2）
        // 注意：这里使用 EffectManager 的 GetYinCoverStacks() 方法获取当前叠层
        int yinStacks = BattleSystem.Instance.effectManager.GetYinCoverStacks();
        float attackReduction = yinStacks * config.extremeYinAttackReducePerStack; // 使用配置的值
        BattleSystem.Instance.effectManager.ApplyEnemyAttackDebuff(attackReduction, config.extremeDebuffDuration); // 使用配置的持续时间
        BattleSystem.Instance.uiManager.UpdateBattleLog($"Enemy attack reduced by {attackReduction:F1}, lasts {config.extremeDebuffDuration} turns!");

        // 设置下回合防御减半
        BattleSystem.Instance.playerManager.NextTurnDefenseDebuff = true;
        BattleSystem.Instance.uiManager.UpdateBattleLog("Next turn defense reduced by half!");

        // 重置叠层
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

        // 设置玩家生命值为1
        BattleSystem.Instance.playerManager.SetHealth(config.ultimateQiHealthSet); // 使用配置的值
        BattleSystem.Instance.uiManager.UpdateBattleLog("Player health set to 1!");

        // 计算究极气状态下的防御力（7*阴点数）
        float qiDefense = 7 * yinPoints;

        // 确保防御力至少为15
        if (qiDefense < 15f)
        {
            qiDefense = 15f;
            BattleSystem.Instance.uiManager.UpdateBattleLog("Qi defense boosted to minimum 15!");
        }

        // 临时设置玩家防御力为究极气防御力
        BattleSystem.Instance.playerManager.Defense = qiDefense;
        BattleSystem.Instance.uiManager.UpdatePlayerAttackDefense(BattleSystem.Instance.playerManager.Attack, BattleSystem.Instance.playerManager.Defense);

        // 获得反震效果（从BattleConfig读取持续时间）
        int counterStrikeDuration = config.ultimateQiCounterStrikeDuration;
        BattleSystem.Instance.playerManager.ActivateCounterStrike(1.0f, counterStrikeDuration);
        BattleSystem.Instance.uiManager.UpdateBattleLog($"Player gained Counter Strike effect for {counterStrikeDuration} turn(s)!");

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