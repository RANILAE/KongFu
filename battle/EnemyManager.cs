using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    // �����ֶ� (ʹ��float)
    public float Health { get; private set; }
    public float MaxHealth { get; private set; }
    public float BaseAttack { get; private set; }
    public float CurrentAttack { get; set; }
    public float Defense { get; private set; }
    public float CurrentDefense { get; set; }
    public int DamageCounter { get; private set; } // ���ڴ�������

    public class EnemyAction
    {
        public enum Type { Attack, Defend, Charge }
        public Type type;
    }

    public void Initialize(BattleConfig config)
    {
        MaxHealth = config.enemyBaseHealth;
        Health = MaxHealth;
        BaseAttack = config.enemyBaseAttack;
        CurrentAttack = BaseAttack;
        Defense = 0;
        CurrentDefense = 0;
        DamageCounter = 0;
    }

    public void TakeDamage(float damage)
    {
        Health = Mathf.Max(0, Health - damage);
        DamageCounter++;
        BattleSystem.Instance.uiManager.UpdateEnemyStatus(Health, MaxHealth);
    }

    public void EnableDefense()
    {
        CurrentDefense = BaseAttack * 0.2f; // 20%�˺�����
        BattleSystem.Instance.uiManager.UpdateEnemyStatus(Health, MaxHealth);
    }

    public void ChargeAttack()
    {
        CurrentAttack += 3; // �������ӹ�����
        BattleSystem.Instance.uiManager.UpdateEnemyStatus(Health, MaxHealth);
    }

    public float CalculateDamage(float playerDefense)
    {
        return Mathf.Max(0, CurrentAttack - playerDefense);
    }

    public void AdjustAttack(float amount)
    {
        CurrentAttack = Mathf.Max(0, CurrentAttack + amount);
        BattleSystem.Instance.uiManager.UpdateEnemyStatus(Health, MaxHealth);
    }

    // �ϸ���ͼƬ�е����ȼ�ʵ�ֵ���AI
    public EnemyAction ChooseAction(int currentRound, int playerDamageTaken)
    {
        // 1. �������ܣ�������ȼ���ÿ4�غϣ�
        if (currentRound % 4 == 1)
        {
            // ������ǵ�һ�غ�
            if (currentRound > 1)
            {
                return new EnemyAction { type = EnemyAction.Type.Charge };
            }
        }

        // 2. �������ܣ����ܵ�2���˺��󴥷�
        if (DamageCounter >= 2)
        {
            DamageCounter = 0; // ���ü�����
            return new EnemyAction { type = EnemyAction.Type.Defend };
        }

        // 3. �˺����ܣ�Ĭ����Ϊ
        return new EnemyAction { type = EnemyAction.Type.Attack };
    }
}