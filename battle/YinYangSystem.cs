using UnityEngine;

public class YinYangSystem : MonoBehaviour
{
    private BattleConfig config;
    private int balanceHealCooldown = 0;
    private bool ultimateQiUsed = false;

    public void Initialize(BattleConfig config)
    {
        this.config = config;
        balanceHealCooldown = 0;
        ultimateQiUsed = false;
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
            ApplyCriticalYangEffect(yangPoints, yinPoints);
        }
        else if (diff <= -1f && diff >= -2.5f)
        {
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
            ApplyExtremeYangEffect(yangPoints, yinPoints);
        }
        else if (diff <= -5f && diff >= -7f)
        {
            ApplyExtremeYinEffect(yangPoints, yinPoints);
        }
        else if (absDiff > 7f && absDiff <= 10f && !ultimateQiUsed)
        {
            ApplyUltimateQiEffect(yangPoints, yinPoints);
        }

        // 更新UI显示
        BattleSystem.Instance.uiManager.UpdateYinYangState(diff, GetCurrentStateName(diff));
    }

    private void ApplyFinalAttributes(float yangPoints, float yinPoints, float diff)
    {
        Vector2 multipliers = Vector2.one;
        string stateName = GetCurrentStateName(diff);

        // 根据状态选择倍率 (完全按照图片规则)
        switch (stateName)
        {
            case "Balance":
                multipliers = new Vector2(1.25f, 1.25f);
                break;
            case "Critical Yang":
                multipliers = new Vector2(1.75f, 1.25f);
                break;
            case "Critical Yin":
                multipliers = new Vector2(1.25f, 1.75f);
                break;
            case "Yang Prosperity":
                multipliers = new Vector2(2.75f, 1.25f);
                break;
            case "Yin Prosperity":
                multipliers = new Vector2(1.0f, 2.5f);
                break;
            case "Extreme Yang":
                multipliers = new Vector2(4.5f, 0.5f);
                break;
            case "Extreme Yin":
                multipliers = new Vector2(1.0f, 3.0f);
                break;
            case "Ultimate Qi":
                multipliers = new Vector2(7.0f, 7.0f);
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

        // 激活反震效果
        BattleSystem.Instance.playerManager.ActivateCounterStrike(1.0f);
        BattleSystem.Instance.uiManager.UpdateBattleLog("Player gained Counter Strike effect!");
    }

    private void ApplyExtremeYangEffect(float yangPoints, float yinPoints)
    {
        BattleSystem.Instance.uiManager.UpdateBattleLog("Extreme Yang state activated!");

        // 检查叠层数
        int yangStacks = BattleSystem.Instance.effectManager.GetYangPenetrationStacks();
        if (yangStacks >= 3)
        {
            // 计算额外伤害（层数*2）
            float extraDamage = yangStacks * 2;
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
        else
        {
            BattleSystem.Instance.uiManager.UpdateBattleLog("Insufficient Yang Penetration stacks (need 3)!");
        }
    }

    private void ApplyExtremeYinEffect(float yangPoints, float yinPoints)
    {
        BattleSystem.Instance.uiManager.UpdateBattleLog("Extreme Yin state activated!");

        // 检查叠层数
        int yinStacks = BattleSystem.Instance.effectManager.GetYinCoverStacks();
        if (yinStacks >= 3)
        {
            // 激活增强反震（1.5倍）
            BattleSystem.Instance.playerManager.ActivateCounterStrike(1.5f);
            BattleSystem.Instance.uiManager.UpdateBattleLog("Player gained enhanced Counter Strike (1.5x)!");

            // 减少敌人攻击力（层数*2）
            float attackReduction = yinStacks * 2;
            BattleSystem.Instance.effectManager.ApplyEnemyAttackDebuff(attackReduction, 2);
            BattleSystem.Instance.uiManager.UpdateBattleLog($"Enemy attack reduced by {attackReduction:F1}, lasts 2 turns!");

            // 设置下回合防御减半
            BattleSystem.Instance.playerManager.NextTurnDefenseDebuff = true;
            BattleSystem.Instance.uiManager.UpdateBattleLog("Next turn defense reduced by half!");

            // 重置叠层
            BattleSystem.Instance.effectManager.ResetYinCoverStacks();
        }
        else
        {
            BattleSystem.Instance.uiManager.UpdateBattleLog("Insufficient Yin Cover stacks (need 3)!");
        }
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
        BattleSystem.Instance.playerManager.SetHealth(1);
        BattleSystem.Instance.uiManager.UpdateBattleLog("Player health set to 1!");

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
}