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
        // ������ʹ�� IconManager �ж������ȷ���� YangStack (ԭΪ YangPenetrationStack)
        iconManager.AddPlayerIcon(IconManager.IconType.YangStack, yangPenetrationStacks);
    }

    public int GetYangPenetrationStacks()
    {
        return yangPenetrationStacks;
    }

    public void ResetYangPenetrationStacks()
    {
        yangPenetrationStacks = 0;
        // ������ʹ�� IconManager �ж������ȷ���� YangStack (ԭΪ YangPenetrationStack)
        iconManager.RemovePlayerIcon(IconManager.IconType.YangStack);
    }

    public void AddYinCoverStack()
    {
        yinCoverStacks++;
        // ������ʹ�� IconManager �ж������ȷ���� YinStack (ԭΪ YinCoverStack)
        iconManager.AddPlayerIcon(IconManager.IconType.YinStack, yinCoverStacks);
    }

    public int GetYinCoverStacks()
    {
        return yinCoverStacks;
    }

    public void ResetYinCoverStacks()
    {
        yinCoverStacks = 0;
        // ������ʹ�� IconManager �ж������ȷ���� YinStack (ԭΪ YinCoverStack)
        iconManager.RemovePlayerIcon(IconManager.IconType.YinStack);
    }

    public void SetBalanceHealCooldown()
    {
        balanceHealCooldown = BattleSystem.Instance.config.balanceHealCooldown;
        // ������������� BalanceHealCD ͼ�꣬�����Ҫ��ʾ��ʼCD
        // iconManager.AddPlayerIcon(IconManager.IconType.BalanceHealCD, 0, balanceHealCooldown);
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
        // ������ʹ�� IconManager �ж������ȷ���� PlayerDot (������ PlayerDot_DamageOverTime)
        iconManager.AddPlayerIcon(IconManager.IconType.PlayerDot, damage, duration);
    }

    public void AddEnemyDotEffect(float damage, int duration)
    {
        activeDots.Add(new DotEffect { damage = damage, remainingTurns = duration, isPlayer = false });
        // ������ʹ�� IconManager �ж������ȷ���� EnemyDot (������ EnemyDot_DamageOverTime)
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
        // ע�⣺���� isAttackDebuff=true, isPlayer=false����ʾ���������ڵ��˵Ĺ���Debuff
        activeDebuffs.Add(new DebuffEffect { amount = amount, remainingTurns = duration, isAttackDebuff = true, isPlayer = false });
        // ������ʹ�� IconManager �ж������ȷ���� EnemyDebuff_DefenseDown (ԭΪ EnemyDebuff_AttackDown)
        // ע�⣺����������������߼���������Ϊ��ƥ�� IconManager.cs �еĶ��壬����ʹ�� EnemyDebuff_DefenseDown
        // *** ǿ�ҽ����� IconManager.cs �� IconType ���� ***
        iconManager.AddEnemyIcon(IconManager.IconType.EnemyDebuff_DefenseDown, amount, duration);
    }

    public void ApplyPlayerDefenseDebuff(float amount, int duration)
    {
        // ע�⣺���� isAttackDebuff=false, isPlayer=true����ʾ������������ҵķ���Debuff (���Լ�����)
        activeDebuffs.Add(new DebuffEffect { amount = amount, remainingTurns = duration, isAttackDebuff = false, isPlayer = true });
        // ������ʹ�� IconManager �ж������ȷ���� ExtremeYangDebuff_AttackDown (ԭΪ ExtremeYinDebuff_DefenseDown)
        // ע�⣺����������������߼���������Ϊ��ƥ�� IconManager.cs �еĶ��壬����ʹ�� ExtremeYangDebuff_AttackDown
        // *** ǿ�ҽ����� IconManager.cs �� IconType ���� ***
        iconManager.AddPlayerIcon(IconManager.IconType.ExtremeYangDebuff_AttackDown, amount, duration);
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
                // �Ƴ���Ӧ��ͼ��
                if (debuff.isPlayer && !debuff.isAttackDebuff)
                {
                    // �Ƴ���ҵļ�����Debuffͼ�� (���������������)
                    iconManager.RemovePlayerIcon(IconManager.IconType.ExtremeYangDebuff_AttackDown);
                }
                else if (!debuff.isPlayer && debuff.isAttackDebuff)
                {
                    // �Ƴ����˵Ĺ���Debuffͼ�� (���������������)
                    iconManager.RemoveEnemyIcon(IconManager.IconType.EnemyDebuff_DefenseDown);
                }
                // �������������ϣ�������ҹ���Debuff����˷���Debuff����Ҳ��Ҫ��Ӧ����
                else if (debuff.isPlayer && debuff.isAttackDebuff)
                {
                    // ���磬������Ҳ�й���Debuff (Ŀǰû������������������߼�����)
                    iconManager.RemovePlayerIcon(IconManager.IconType.EnemyDebuff_DefenseDown); // ����ʹ����ͬ�����ͻ����������
                }
                else if (!debuff.isPlayer && !debuff.isAttackDebuff)
                {
                    // ���磬��������з���Debuff (����������Դ)
                    // ע�⣺�������󣬼����������˵��ǹ���Debuff������������ܲ��ᴥ��
                    // ��Ϊ�������ԣ����Ǽ����� EnemyDefenseDebuff ����
                    iconManager.RemoveEnemyIcon(IconManager.IconType.ExtremeYangDebuff_AttackDown); // ����ʹ����ͬ�����ͻ����������
                }
            }
            else
            {
                // ����ͼ����ʾ����̬�������ݣ�
                if (debuff.isPlayer && !debuff.isAttackDebuff)
                {
                    // ������ҵļ�����Debuffͼ�� (���������������)
                    iconManager.AddPlayerIcon(IconManager.IconType.ExtremeYangDebuff_AttackDown, debuff.amount, debuff.remainingTurns);
                }
                else if (!debuff.isPlayer && debuff.isAttackDebuff)
                {
                    // ���µ��˵Ĺ���Debuffͼ�� (���������������)
                    iconManager.AddEnemyIcon(IconManager.IconType.EnemyDebuff_DefenseDown, debuff.amount, debuff.remainingTurns);
                }
                else if (debuff.isPlayer && debuff.isAttackDebuff)
                {
                    // ������ҵĹ���Debuffͼ�� (�������)
                    iconManager.AddPlayerIcon(IconManager.IconType.EnemyDebuff_DefenseDown, debuff.amount, debuff.remainingTurns); // ��������
                }
                else if (!debuff.isPlayer && !debuff.isAttackDebuff)
                {
                    // ���µ��˵ķ���Debuffͼ�� (�������)
                    iconManager.AddEnemyIcon(IconManager.IconType.ExtremeYangDebuff_AttackDown, debuff.amount, debuff.remainingTurns); // ��������
                }
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
            // �����Ҫ��̬���� BalanceHealCD ͼ�꣬�������������
            // iconManager.AddPlayerIcon(IconManager.IconType.BalanceHealCD, 0, balanceHealCooldown);
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
        // *** ���� IconType ���� ***
        // ����������������ʹ��ͳһ�� EnemyIntent ͼ��
        iconManager.RemoveEnemyIcon(IconManager.IconType.EnemyIntent);
        // �Ƴ��� AttackIntent, DefendIntent, ChargeIntent �ĵ��ã���Ϊ����ֻʹ��һ��ͳһͼ��
        // iconManager.RemoveEnemyIcon(IconManager.IconType.AttackIntent);
        // iconManager.RemoveEnemyIcon(IconManager.IconType.DefendIntent);
        // iconManager.RemoveEnemyIcon(IconManager.IconType.ChargeIntent);

        // �������ͼͼ��
        // *** ���� IconType ���� ***
        // ����������������ʹ��ͳһ�� EnemyIntent ͼ��
        iconManager.AddEnemyIcon(IconManager.IconType.EnemyIntent, (float)action.type); // ���Դ�����ͼ������Ϊ value �� Tooltip ʹ��
        // �Ƴ��Բ�ͬ��ͼ���͵ķ�֧���ã���Ϊ����ֻʹ��һ��ͳһͼ��
        /*
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
        */
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
        // ������Ҫ���»��Ƴ� Balance CD ͼ��
        // iconManager.RemovePlayerIcon(IconManager.IconType.BalanceHealCD);
    }
}