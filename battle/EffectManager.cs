using UnityEngine;
using System.Collections.Generic;

public class EffectManager : MonoBehaviour
{
    [System.Serializable]
    public class DotEffect
    {
        public float damage;
        public int remainingTurns;
        public bool isPlayer;
    }

    [System.Serializable]
    public class DebuffEffect
    {
        public float amount;
        public int remainingTurns;
        public bool isAttackDebuff; // true表示攻击Debuff, false表示防御Debuff
        public bool isPlayer; // true表示作用于玩家, false表示作用于敌人
    }

    // 叠层系统
    private int yangPenetrationStacks = 0;
    private int yinCoverStacks = 0;
    private int balanceHealCooldown = 0;

    // 效果列表
    private List<DotEffect> activeDots = new List<DotEffect>();
    private List<DebuffEffect> activeDebuffs = new List<DebuffEffect>();


    public void Initialize()
    {

        yangPenetrationStacks = 0;
        yinCoverStacks = 0;
        balanceHealCooldown = 0;
        activeDots.Clear();
        activeDebuffs.Clear();
    }

    // 新增：处理回合开始的DOT效果
    public void ProcessTurnStartDots()
    {
        Debug.Log($"Processing {activeDots.Count} active DOT effects at start of turn");
        // 从后往前遍历，因为可能会移除元素
        for (int i = activeDots.Count - 1; i >= 0; i--)
        {
            DotEffect dot = activeDots[i];
            if (dot.isPlayer)
            {
                BattleSystem.Instance.playerManager.TakeDamage(dot.damage);
                BattleSystem.Instance.uiManager.UpdateBattleLog($"[DOT] Player takes {dot.damage:F1} damage. ({dot.remainingTurns} turns remaining)");
                Debug.Log($"Player took {dot.damage} DOT damage, {dot.remainingTurns} turns remaining");
            }
            else
            {
                BattleSystem.Instance.enemyManager.TakeDamage(dot.damage);
                BattleSystem.Instance.uiManager.UpdateBattleLog($"[DOT] Enemy takes {dot.damage:F1} damage. ({dot.remainingTurns} turns remaining)");
                Debug.Log($"Enemy took {dot.damage} DOT damage, {dot.remainingTurns} turns remaining");
            }
            dot.remainingTurns--;
            if (dot.remainingTurns <= 0)
            {
                activeDots.RemoveAt(i);
                BattleSystem.Instance.uiManager.UpdateBattleLog($"DOT effect expired.");
                Debug.Log("DOT effect expired and removed");
            }
        }
    }

    // 新增：处理非DOT效果
    public void ProcessNonDotEffects()
    {
        Debug.Log("Processing non-DOT effects");

        // 处理Debuff效果
        ProcessDebuffs();

        // 处理冷却时间
        ProcessCooldowns();
    }

    public void ProcessAllEffects()
    {
        ProcessDotEffects();
        ProcessDebuffs();
        ProcessCooldowns(); // 确保处理CD
    }

    #region 叠层方法
    public void AddYangPenetrationStack()
    {
        yangPenetrationStacks++;

    }

    public int GetYangPenetrationStacks()
    {
        return yangPenetrationStacks;
    }

    public void ResetYangPenetrationStacks()
    {
        yangPenetrationStacks = 0;

    }

    public void AddYinCoverStack()
    {
        yinCoverStacks++;

    }

    public int GetYinCoverStacks()
    {
        return yinCoverStacks;
    }

    public void ResetYinCoverStacks()
    {
        yinCoverStacks = 0;

    }

    public void SetBalanceHealCooldown()
    {
        balanceHealCooldown = BattleSystem.Instance.config.balanceHealCooldown;

    }

    public bool CanHealFromBalance()
    {
        return balanceHealCooldown <= 0;
    }
    #endregion

    #region DOT处理
    public void AddPlayerDotEffect(float damage, int duration)
    {
        // 避免添加0回合或负回合的DOT
        if (duration <= 0)
        {
            Debug.LogWarning("Attempted to add DOT effect with non-positive duration. Ignoring.");
            return;
        }
        activeDots.Add(new DotEffect { damage = damage, remainingTurns = duration, isPlayer = true });
        Debug.Log($"Added player DOT effect: {damage} damage for {duration} turns");
    }

    public void AddEnemyDotEffect(float damage, int duration)
    {
        // 避免添加0回合或负回合的DOT
        if (duration <= 0)
        {
            Debug.LogWarning("Attempted to add DOT effect with non-positive duration. Ignoring.");
            return;
        }
        activeDots.Add(new DotEffect { damage = damage, remainingTurns = duration, isPlayer = false });
        Debug.Log($"Added enemy DOT effect: {damage} damage for {duration} turns");
    }

    // 保留原始的 ProcessDotEffects 以防其他地方调用
    private void ProcessDotEffects()
    {
        for (int i = activeDots.Count - 1; i >= 0; i--)
        {
            DotEffect dot = activeDots[i];
            if (dot.isPlayer)
            {
                BattleSystem.Instance.playerManager.TakeDamage(dot.damage);
                BattleSystem.Instance.uiManager.UpdateBattleLog($"Player takes {dot.damage:F1} DOT damage.");
            }
            else
            {
                BattleSystem.Instance.enemyManager.TakeDamage(dot.damage);
                BattleSystem.Instance.uiManager.UpdateBattleLog($"Enemy takes {dot.damage:F1} DOT damage.");
            }
            dot.remainingTurns--;
            if (dot.remainingTurns <= 0)
            {
                activeDots.RemoveAt(i);
            }
        }

    }
    #endregion

    #region DEBUFF处理
    public void ApplyEnemyAttackDebuff(float amount, int duration)
    {
        // 注意：这里 isAttackDebuff=true, isPlayer=false，表示这是作用于敌人的攻击Debuff
        if (duration <= 0)
        {
            Debug.LogWarning("Attempted to add Debuff effect with non-positive duration. Ignoring.");
            return;
        }
        activeDebuffs.Add(new DebuffEffect { amount = amount, remainingTurns = duration, isAttackDebuff = true, isPlayer = false });

    }

    public void ApplyPlayerDefenseDebuff(float amount, int duration)
    {
        // 注意：这里 isAttackDebuff=false, isPlayer=true，表示这是作用于玩家的防御Debuff (来自极端阴)
        if (duration <= 0)
        {
            Debug.LogWarning("Attempted to add Debuff effect with non-positive duration. Ignoring.");
            return;
        }
        activeDebuffs.Add(new DebuffEffect { amount = amount, remainingTurns = duration, isAttackDebuff = false, isPlayer = true });

    }

    private void ProcessDebuffs()
    {
        for (int i = activeDebuffs.Count - 1; i >= 0; i--)
        {
            DebuffEffect debuff = activeDebuffs[i];
            debuff.remainingTurns--;
            // 应用Debuff效果
            if (debuff.isAttackDebuff)
            {
                if (debuff.isPlayer)
                {
                    // 玩家攻击力降低
                    BattleSystem.Instance.playerManager.AdjustAttack(-debuff.amount);
                }
                else
                {
                    // 敌人攻击力降低 (来自极端阴效果)
                    BattleSystem.Instance.enemyManager.AdjustAttack(-debuff.amount);
                }
            }
            else
            {
                if (debuff.isPlayer)
                {
                    // 玩家防御力降低 (来自极端阳/阴效果)
                    BattleSystem.Instance.playerManager.AdjustDefense(-debuff.amount);
                }
                else
                {
                    // 敌人防御力降低 (来自其他效果，如果有的话)
                    BattleSystem.Instance.enemyManager.AdjustDefense(-debuff.amount);
                }
            }
            if (debuff.remainingTurns <= 0)
            {
                activeDebuffs.RemoveAt(i);

            }

        }
    }
    #endregion

    #region 冷却处理
    private void ProcessCooldowns()
    {
        // 处理Balance回血CD
        if (balanceHealCooldown > 0)
        {
            balanceHealCooldown--;
            Debug.Log($"Balance heal cooldown decreased to: {balanceHealCooldown}");

        }
        // 确保调用YinYangSystem的CD处理（关键修复）
        if (BattleSystem.Instance != null && BattleSystem.Instance.yinYangSystem != null)
        {
            BattleSystem.Instance.yinYangSystem.ProcessCooldowns();
        }
    }
    #endregion

    #region 敌人意图显示

    /// <param name="action">敌人将要执行的动作</param>
    public void ShowEnemyIntent(EnemyManager.EnemyAction action)
    {
        // 调用 UIManager 来更新敌人意图显示
        // 确保 BattleSystem 实例和 UIManager 都存在
        if (BattleSystem.Instance != null && BattleSystem.Instance.uiManager != null)
        {
            BattleSystem.Instance.uiManager.ShowEnemyIntent(action);
        }
        else
        {
            Debug.LogWarning("EffectManager.ShowEnemyIntent: BattleSystem or UIManager instance is null.");
        }


    }
    #endregion

    // 新增：获取当前Balance回血CD值（用于调试）
    public int GetBalanceHealCooldown()
    {
        return balanceHealCooldown;
    }

    // 新增：重置Balance回血CD（用于调试）
    public void ResetBalanceHealCooldown()
    {
        balanceHealCooldown = 0;
        Debug.Log("EffectManager - Balance heal cooldown reset to 0");

    }
}
