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
        public bool isAttackDebuff; // true��ʾ����Debuff, false��ʾ����Debuff
        public bool isPlayer; // true��ʾ���������, false��ʾ�����ڵ���
    }

    // ����ϵͳ
    private int yangPenetrationStacks = 0;
    private int yinCoverStacks = 0;
    private int balanceHealCooldown = 0;

    // Ч���б�
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

    // ����������غϿ�ʼ��DOTЧ��
    public void ProcessTurnStartDots()
    {
        Debug.Log($"Processing {activeDots.Count} active DOT effects at start of turn");
        // �Ӻ���ǰ��������Ϊ���ܻ��Ƴ�Ԫ��
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

    // �����������DOTЧ��
    public void ProcessNonDotEffects()
    {
        Debug.Log("Processing non-DOT effects");

        // ����DebuffЧ��
        ProcessDebuffs();

        // ������ȴʱ��
        ProcessCooldowns();
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

    #region DOT����
    public void AddPlayerDotEffect(float damage, int duration)
    {
        // �������0�غϻ򸺻غϵ�DOT
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
        // �������0�غϻ򸺻غϵ�DOT
        if (duration <= 0)
        {
            Debug.LogWarning("Attempted to add DOT effect with non-positive duration. Ignoring.");
            return;
        }
        activeDots.Add(new DotEffect { damage = damage, remainingTurns = duration, isPlayer = false });
        Debug.Log($"Added enemy DOT effect: {damage} damage for {duration} turns");
    }

    // ����ԭʼ�� ProcessDotEffects �Է������ط�����
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

    #region DEBUFF����
    public void ApplyEnemyAttackDebuff(float amount, int duration)
    {
        // ע�⣺���� isAttackDebuff=true, isPlayer=false����ʾ���������ڵ��˵Ĺ���Debuff
        if (duration <= 0)
        {
            Debug.LogWarning("Attempted to add Debuff effect with non-positive duration. Ignoring.");
            return;
        }
        activeDebuffs.Add(new DebuffEffect { amount = amount, remainingTurns = duration, isAttackDebuff = true, isPlayer = false });

    }

    public void ApplyPlayerDefenseDebuff(float amount, int duration)
    {
        // ע�⣺���� isAttackDebuff=false, isPlayer=true����ʾ������������ҵķ���Debuff (���Լ�����)
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
            // Ӧ��DebuffЧ��
            if (debuff.isAttackDebuff)
            {
                if (debuff.isPlayer)
                {
                    // ��ҹ���������
                    BattleSystem.Instance.playerManager.AdjustAttack(-debuff.amount);
                }
                else
                {
                    // ���˹��������� (���Լ�����Ч��)
                    BattleSystem.Instance.enemyManager.AdjustAttack(-debuff.amount);
                }
            }
            else
            {
                if (debuff.isPlayer)
                {
                    // ��ҷ��������� (���Լ�����/��Ч��)
                    BattleSystem.Instance.playerManager.AdjustDefense(-debuff.amount);
                }
                else
                {
                    // ���˷��������� (��������Ч��������еĻ�)
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

    /// <param name="action">���˽�Ҫִ�еĶ���</param>
    public void ShowEnemyIntent(EnemyManager.EnemyAction action)
    {
        // ���� UIManager �����µ�����ͼ��ʾ
        // ȷ�� BattleSystem ʵ���� UIManager ������
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

    // ��������ȡ��ǰBalance��ѪCDֵ�����ڵ��ԣ�
    public int GetBalanceHealCooldown()
    {
        return balanceHealCooldown;
    }

    // ����������Balance��ѪCD�����ڵ��ԣ�
    public void ResetBalanceHealCooldown()
    {
        balanceHealCooldown = 0;
        Debug.Log("EffectManager - Balance heal cooldown reset to 0");

    }
}
