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
        public bool isAttackDebuff;
    }

    // 叠层系统
    private int yangPenetrationStacks = 0;
    private int yinCoverStacks = 0;
    private int balanceHealCooldown = 0;

    // 效果列表
    private List<DotEffect> activeDots = new List<DotEffect>();
    private List<DebuffEffect> activeDebuffs = new List<DebuffEffect>();

    private IconManager iconManager;

    public void Initialize(IconManager iconManager)
    {
        this.iconManager = iconManager;
        yangPenetrationStacks = 0;
        yinCoverStacks = 0;
        balanceHealCooldown = 0;
        activeDots.Clear();
        activeDebuffs.Clear();
    }

    public void ProcessAllEffects()
    {
        ProcessDotEffects();
        ProcessDebuffs();
        ProcessCooldowns();
    }

    #region 叠层方法
    public void AddYangPenetrationStack()
    {
        yangPenetrationStacks++;
        iconManager.AddEnemyIcon(IconManager.IconType.YangPenetration, yangPenetrationStacks);
    }

    public void AddYinCoverStack()
    {
        yinCoverStacks++;
        iconManager.AddEnemyIcon(IconManager.IconType.YinCover, yinCoverStacks);
    }

    public int GetYangPenetrationStacks()
    {
        return yangPenetrationStacks;
    }

    public int GetYinCoverStacks()
    {
        return yinCoverStacks;
    }

    public void ResetYangPenetrationStacks()
    {
        yangPenetrationStacks = 0;
        iconManager.RemoveEnemyIcon(IconManager.IconType.YangPenetration);
    }

    public void ResetYinCoverStacks()
    {
        yinCoverStacks = 0;
        iconManager.RemoveEnemyIcon(IconManager.IconType.YinCover);
    }
    #endregion

    #region DOT效果
    public void AddEnemyDotEffect(float damage, int duration)
    {
        activeDots.Add(new DotEffect
        {
            damage = damage,
            remainingTurns = duration,
            isPlayer = false
        });
        iconManager.AddEnemyIcon(IconManager.IconType.EnemyDot, 0, duration);
    }

    public void AddPlayerDotEffect(float damage, int duration)
    {
        activeDots.Add(new DotEffect
        {
            damage = damage,
            remainingTurns = duration,
            isPlayer = true
        });
        iconManager.AddPlayerIcon(IconManager.IconType.PlayerDot, 0, duration);
    }

    private void ProcessDotEffects()
    {
        for (int i = activeDots.Count - 1; i >= 0; i--)
        {
            DotEffect dot = activeDots[i];

            if (dot.isPlayer)
            {
                BattleSystem.Instance.playerManager.TakeDamage(dot.damage);
                iconManager.UpdatePlayerIcon(IconManager.IconType.PlayerDot, 0, dot.remainingTurns);
            }
            else
            {
                BattleSystem.Instance.enemyManager.TakeDamage(dot.damage);
                iconManager.UpdateEnemyIcon(IconManager.IconType.EnemyDot, 0, dot.remainingTurns);
            }

            dot.remainingTurns--;

            if (dot.remainingTurns <= 0)
            {
                activeDots.RemoveAt(i);
                if (dot.isPlayer)
                {
                    iconManager.RemovePlayerIcon(IconManager.IconType.PlayerDot);
                }
                else
                {
                    iconManager.RemoveEnemyIcon(IconManager.IconType.EnemyDot);
                }
            }
            else
            {
                activeDots[i] = dot;
            }
        }
    }
    #endregion

    #region 反震系统
    public void HandleCounterStrike(PlayerManager player, EnemyManager enemy)
    {
        if (!player.CounterStrikeActive) return;

        BattleSystem.Instance.uiManager.UpdateBattleLog("尝试发动反震效果...");

        // 反震成功条件: 敌人攻击力 < 玩家防御力
        if (enemy.CurrentAttack < player.Defense)
        {
            float counterDamage = player.Defense * player.CounterStrikeMultiplier;
            enemy.TakeDamage(counterDamage);
            BattleSystem.Instance.uiManager.UpdateBattleLog($"反震成功! 造成{counterDamage:F1}伤害");
        }
        else
        {
            BattleSystem.Instance.uiManager.UpdateBattleLog("反震失败! 玩家受到持续伤害");
            // 规则: 失败时受到2点DOT伤害持续2回合
            AddPlayerDotEffect(2, 2);
        }

        // 无论成功失败，反震效果只在本回合有效
        player.CounterStrikeActive = false;
        iconManager.RemovePlayerIcon(IconManager.IconType.CounterStrike);
    }
    #endregion

    #region 平衡状态治疗
    public bool CanApplyBalanceHeal()
    {
        return balanceHealCooldown == 0;
    }

    public void ApplyBalanceHeal()
    {
        balanceHealCooldown = 2; // 2回合冷却
    }
    #endregion

    #region 减益效果
    public void ApplyEnemyAttackDebuff(float reduction, int duration)
    {
        activeDebuffs.Add(new DebuffEffect
        {
            amount = reduction,
            remainingTurns = duration,
            isAttackDebuff = true
        });

        // 立即应用减益效果
        BattleSystem.Instance.enemyManager.AdjustAttack(-reduction);

        iconManager.AddEnemyIcon(IconManager.IconType.AttackDebuff, 0, duration);
    }

    public void ApplyPlayerAttackDebuff(float reduction, int duration)
    {
        activeDebuffs.Add(new DebuffEffect
        {
            amount = reduction,
            remainingTurns = duration,
            isAttackDebuff = true
        });

        // 立即应用减益效果
        BattleSystem.Instance.playerManager.AdjustAttack(-reduction);

        iconManager.AddPlayerIcon(IconManager.IconType.AttackDebuff, 0, duration);
    }

    public void ApplyPlayerDefenseDebuff(float reduction, int duration)
    {
        activeDebuffs.Add(new DebuffEffect
        {
            amount = reduction,
            remainingTurns = duration,
            isAttackDebuff = false
        });

        // 立即应用减益效果
        BattleSystem.Instance.playerManager.AdjustDefense(-reduction);

        iconManager.AddPlayerIcon(IconManager.IconType.DefenseDebuff, 0, duration);
    }

    private void ProcessDebuffs()
    {
        for (int i = activeDebuffs.Count - 1; i >= 0; i--)
        {
            DebuffEffect debuff = activeDebuffs[i];

            // 减少剩余回合
            debuff.remainingTurns--;

            if (debuff.remainingTurns <= 0)
            {
                // 移除效果时恢复原值
                if (debuff.isAttackDebuff)
                {
                    BattleSystem.Instance.enemyManager.AdjustAttack(debuff.amount);
                    iconManager.RemoveEnemyIcon(IconManager.IconType.AttackDebuff);
                }
                else
                {
                    BattleSystem.Instance.playerManager.AdjustDefense(debuff.amount);
                    iconManager.RemovePlayerIcon(IconManager.IconType.DefenseDebuff);
                }

                activeDebuffs.RemoveAt(i);
            }
            else
            {
                activeDebuffs[i] = debuff;
                // 更新图标显示
                if (debuff.isAttackDebuff)
                {
                    iconManager.UpdateEnemyIcon(IconManager.IconType.AttackDebuff, 0, debuff.remainingTurns);
                }
                else
                {
                    iconManager.UpdatePlayerIcon(IconManager.IconType.DefenseDebuff, 0, debuff.remainingTurns);
                }
            }
        }
    }
    #endregion

    #region 冷却处理
    private void ProcessCooldowns()
    {
        if (balanceHealCooldown > 0) balanceHealCooldown--;
    }
    #endregion

    #region 敌人意图显示
    public void ShowEnemyIntent(EnemyManager.EnemyAction action)
    {
        // 清除所有意图图标
        iconManager.RemoveEnemyIcon(IconManager.IconType.AttackIntent);
        iconManager.RemoveEnemyIcon(IconManager.IconType.DefendIntent);
        iconManager.RemoveEnemyIcon(IconManager.IconType.ChargeIntent);

        // 添加新意图图标
        switch (action.type)
        {
            case EnemyManager.EnemyAction.Type.Attack:
                iconManager.AddEnemyIcon(IconManager.IconType.AttackIntent);
                break;
            case EnemyManager.EnemyAction.Type.Defend:
                iconManager.AddEnemyIcon(IconManager.IconType.DefendIntent);
                break;
            case EnemyManager.EnemyAction.Type.Charge:
                iconManager.AddEnemyIcon(IconManager.IconType.ChargeIntent);
                break;
        }
    }
    #endregion
}