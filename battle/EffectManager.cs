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

    // ����ϵͳ
    private int yangPenetrationStacks = 0;
    private int yinCoverStacks = 0;
    private int balanceHealCooldown = 0;

    // Ч���б�
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
        ProcessCooldowns(); // ȷ������CD
    }

    #region ���㷽��
    public void AddYangPenetrationStack()
    {
        yangPenetrationStacks++;
        iconManager.AddPlayerIcon(IconManager.IconType.YangStack, yangPenetrationStacks);
    }

    public int GetYangPenetrationStacks()
    {
        return yangPenetrationStacks;
    }

    public void ResetYangPenetrationStacks()
    {
        yangPenetrationStacks = 0;
        iconManager.RemovePlayerIcon(IconManager.IconType.YangStack);
    }

    public void AddYinCoverStack()
    {
        yinCoverStacks++;
        iconManager.AddPlayerIcon(IconManager.IconType.YinStack, yinCoverStacks);
    }

    public int GetYinCoverStacks()
    {
        return yinCoverStacks;
    }

    public void ResetYinCoverStacks()
    {
        yinCoverStacks = 0;
        iconManager.RemovePlayerIcon(IconManager.IconType.YinStack);
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

    #region DOT����
    public void AddPlayerDotEffect(float damage, int duration)
    {
        activeDots.Add(new DotEffect { damage = damage, remainingTurns = duration, isPlayer = true });
        iconManager.AddPlayerIcon(IconManager.IconType.PlayerDot, damage, duration);
    }

    public void AddEnemyDotEffect(float damage, int duration)
    {
        activeDots.Add(new DotEffect { damage = damage, remainingTurns = duration, isPlayer = false });
        iconManager.AddEnemyIcon(IconManager.IconType.EnemyDot, damage, duration);
    }

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
        // ����ͼ����ʾ
        iconManager.UpdateDotIcons(activeDots);
    }
    #endregion

    #region DEBUFF����
    public void ApplyEnemyAttackDebuff(float amount, int duration)
    {
        activeDebuffs.Add(new DebuffEffect { amount = amount, remainingTurns = duration, isAttackDebuff = true });
        iconManager.AddEnemyIcon(IconManager.IconType.AttackDebuff, amount, duration);
    }

    public void ApplyPlayerDefenseDebuff(float amount, int duration)
    {
        activeDebuffs.Add(new DebuffEffect { amount = amount, remainingTurns = duration, isAttackDebuff = false });
        iconManager.AddPlayerIcon(IconManager.IconType.DefenseDebuff, amount, duration);
    }

    private void ProcessDebuffs()
    {
        for (int i = activeDebuffs.Count - 1; i >= 0; i--)
        {
            DebuffEffect debuff = activeDebuffs[i];
            debuff.remainingTurns--;
            if (debuff.isAttackDebuff)
            {
                // ���˹���������
                BattleSystem.Instance.enemyManager.AdjustAttack(-debuff.amount);
            }
            else
            {
                // ��ҷ���������
                BattleSystem.Instance.playerManager.AdjustDefense(-debuff.amount);
            }

            if (debuff.remainingTurns <= 0)
            {
                activeDebuffs.RemoveAt(i);
                // ���Ƴ���Ч��ͼ����µ���
            }
        }
    }
    #endregion

    #region ��ȴ����
    private void ProcessCooldowns()
    {
        // ����Balance��ѪCD
        if (balanceHealCooldown > 0)
        {
            balanceHealCooldown--;
            Debug.Log($"Balance heal cooldown decreased to: {balanceHealCooldown}");
        }

        // ȷ������YinYangSystem��CD�����ؼ��޸���
        if (BattleSystem.Instance != null && BattleSystem.Instance.yinYangSystem != null)
        {
            BattleSystem.Instance.yinYangSystem.ProcessCooldowns();
        }
    }
    #endregion

    #region ������ͼ��ʾ
    public void ShowEnemyIntent(EnemyManager.EnemyAction action)
    {
        // ���������ͼͼ��
        iconManager.RemoveEnemyIcon(IconManager.IconType.AttackIntent);
        iconManager.RemoveEnemyIcon(IconManager.IconType.DefendIntent);
        iconManager.RemoveEnemyIcon(IconManager.IconType.ChargeIntent);

        // �������ͼͼ��
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

    // ��������ȡ��ǰBalance��ѪCDֵ�����ڵ��ԣ�
    public int GetBalanceHealCooldown()
    {
        return balanceHealCooldown;
    }

    // ����������Balance��ѪCD�����ڵ��ԣ�
    public void ResetBalanceHealCooldown()
    {
        balanceHealCooldown = 0;
    }
}