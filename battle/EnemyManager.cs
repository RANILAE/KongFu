using UnityEngine;
// ... ���� using ��䱣�ֲ��� ...

public class EnemyManager : MonoBehaviour
{
    // ��������������ö��
    public enum EnemyType { Default, Second, Third }
    private EnemyType currentEnemyType = EnemyType.Default;
    // ������״̬�ֶ�
    public bool IsDefending { get; private set; }
    public int DefenseDuration { get; private set; } // ���������غ���
    public int DamageDealtCount { get; private set; } // ����˺�������
    public int DotDuration { get; private set; } // DOT�����غ���
    public float AttackMultiplier { get; private set; } = 1f; // ��������
    public int DefenseUsedCount { get; private set; } // ��������ʹ�ô���
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
        // ��������״̬
        IsDefending = false;
        DefenseDuration = 0;
        DamageDealtCount = 0;
        DotDuration = 0;
        AttackMultiplier = 1f;
        DefenseUsedCount = 0;
        DamageCounter = 0;
        // ���ݵ���������������
        SetEnemyProperties(config);
    }

    // ���������õ�������
    public void SetEnemyType(EnemyType type)
    {
        currentEnemyType = type;
    }

    // ��������ȡ��ǰ��������
    public EnemyType GetCurrentEnemyType()
    {
        return currentEnemyType;
    }

    // ���������ݵ���������������
    private void SetEnemyProperties(BattleConfig config)
    {
        switch (currentEnemyType)
        {
            case EnemyType.Second:
                MaxHealth = config.secondEnemy.health;
                Health = config.secondEnemy.health;
                BaseAttack = config.secondEnemy.attack;
                Defense = config.secondEnemy.defense;
                CurrentDefense = config.secondEnemy.defense;
                break;
            case EnemyType.Third:
                MaxHealth = config.thirdEnemy.health;
                Health = config.thirdEnemy.health;
                BaseAttack = config.thirdEnemy.attack;
                Defense = config.thirdEnemy.defense;
                CurrentDefense = config.thirdEnemy.defense;
                break;
            case EnemyType.Default:
            default:
                MaxHealth = config.defaultEnemy.health;
                Health = config.defaultEnemy.health;
                BaseAttack = config.defaultEnemy.attack;
                Defense = config.defaultEnemy.defense;
                CurrentDefense = config.defaultEnemy.defense;
                break;
        }
        CurrentAttack = BaseAttack;
    }

    // ���������õ���״̬�������޸�BUG��
    public void ResetEnemyState(BattleConfig config)
    {
        // ��������״̬
        IsDefending = false;
        DefenseDuration = 0;
        DamageDealtCount = 0;
        DotDuration = 0;
        AttackMultiplier = 1f;
        DefenseUsedCount = 0;
        DamageCounter = 0;
        // ������������
        SetEnemyProperties(config);
    }

    public void TakeDamage(float damage)
    {
        float actualDamage = damage;
        // ������ڷ��������ݵ�������Ӧ�ò�ͬ�ķ���Ч��
        if (IsDefending)
        {
            switch (currentEnemyType)
            {
                case EnemyType.Second:
                    // �ڶ������ˣ�����50%�˺��������ã�
                    if (BattleSystem.Instance != null && BattleSystem.Instance.config != null)
                    {
                        float reduction = BattleSystem.Instance.config.secondEnemyDefenseReduction;
                        actualDamage = damage * (1f - reduction);
                        Debug.Log($"Enemy is defending and reducing damage by {reduction * 100}%! Original: {damage}, Reduced: {actualDamage}");
                    }
                    else
                    {
                        actualDamage = damage * 0.5f; // Ĭ��50%����
                    }
                    break;
                case EnemyType.Default:
                case EnemyType.Third:
                default:
                    // �������ˣ���ȫ�����˺�
                    Debug.Log("Enemy is defending and ignoring attack!");
                    return;
            }
        }
        // ��ͨ�˺�����
        Health = Mathf.Max(0, Health - actualDamage);
        DamageCounter++;
        // ע�⣺���ﲻ��ֱ�ӵ��ö�������BattleSystem�����ƶ���˳��
        // ���ŵ����ܻ��������߼��Ƶ�BattleSystem��
        // ����UI
        if (BattleSystem.Instance != null && BattleSystem.Instance.uiManager != null)
        {
            BattleSystem.Instance.uiManager.UpdateEnemyStatus(Health, MaxHealth);
        }
    }

    public void EnableDefense()
    {
        IsDefending = true;
        DefenseDuration = 1; // ��ʼ����1�غ�
        DefenseUsedCount++; // ���ӷ���ʹ�ô���
        // ����UI
        if (BattleSystem.Instance != null && BattleSystem.Instance.uiManager != null)
        {
            BattleSystem.Instance.uiManager.UpdateEnemyStatus(Health, MaxHealth);
        }
    }

    public void ChargeAttack()
    {
        // ���ݵ�������ִ�в�ͬ�������߼�
        switch (currentEnemyType)
        {
            case EnemyType.Second:
                // �ڶ������ˣ��ӳ�����ʱ��
                if (DefenseDuration > 0)
                {
                    DefenseDuration++; // �ӳ���������ʱ��
                }
                break;
            case EnemyType.Third:
                // ���������ˣ��������DOT
                DotDuration = 2; // DOT����2�غ�
                break;
            case EnemyType.Default:
            default:
                // Ĭ�ϵ��ˣ����ӹ�����
                CurrentAttack += 3; // �������ӹ�����
                break;
        }
        // ����UI
        if (BattleSystem.Instance != null && BattleSystem.Instance.uiManager != null)
        {
            BattleSystem.Instance.uiManager.UpdateEnemyStatus(Health, MaxHealth);
        }
    }

    public float CalculateDamage(float playerDefense)
    {
        // Ӧ�ù�������
        float actualDamage = CurrentAttack * AttackMultiplier;
        return Mathf.Max(0, actualDamage - playerDefense);
    }

    public void AdjustAttack(float amount)
    {
        CurrentAttack = Mathf.Max(0, CurrentAttack + amount);
        // ����UI
        if (BattleSystem.Instance != null && BattleSystem.Instance.uiManager != null)
        {
            BattleSystem.Instance.uiManager.UpdateEnemyStatus(Health, MaxHealth);
        }
    }

    // �������������˷������������޸�BUG��
    public void AdjustDefense(float amount)
    {
        CurrentDefense = Mathf.Max(0, CurrentDefense + amount);
        // ����UI
        if (BattleSystem.Instance != null && BattleSystem.Instance.uiManager != null)
        {
            BattleSystem.Instance.uiManager.UpdateEnemyStatus(Health, MaxHealth);
        }
    }

    // ========== �޸ģ�ChooseAction ���� ==========
    // �ϸ��ղ�ͬ���˵����ȼ�ʵ�ֵ���AI
    public EnemyAction ChooseAction(int currentRound, float playerDamageTaken)
    {
        EnemyAction action;
        switch (currentEnemyType)
        {
            case EnemyType.Second:
                action = ChooseSecondEnemyAction(currentRound, playerDamageTaken);
                break;
            case EnemyType.Third:
                action = ChooseThirdEnemyAction(currentRound, playerDamageTaken);
                break;
            case EnemyType.Default:
            default:
                action = ChooseDefaultEnemyAction(currentRound, playerDamageTaken);
                break;
        }

        // ========== �ؼ��޸ģ��� ChooseAction �ڲ�ֱ�Ӹ���UI��ͼ ==========
        // ��������ȷ��ÿ��ѡ���ж�ʱ����ͼͼ�궼����������
        // ���ǵ��� EffectManager �� ShowEnemyIntent ������������ͼ��ʾ
        if (BattleSystem.Instance != null && BattleSystem.Instance.effectManager != null)
        {
            // ���� EffectManager �ķ�����������ͼ
            BattleSystem.Instance.effectManager.ShowEnemyIntent(action);
            Debug.Log($"[Enemy Intent Updated Internally] Round: {currentRound}, Action: {action.type}");
        }
        else
        {
            Debug.LogWarning("EnemyManager.ChooseAction: Cannot update intent display. BattleSystem.Instance or effectManager is null.");
        }
        // ========== �޸Ľ��� ==========

        return action;
    }
    // ========== �޸Ľ��� ==========

    // ========== ������ִ�е��˶��������״̬ ==========
    /// <summary>
    /// ִ�е��˶�������ô˷����������״̬������
    /// </summary>
    public void ExecuteAction(EnemyAction.Type actionType)
    {
        switch (actionType)
        {
            case EnemyAction.Type.Attack:
                // ����ʱ��������˺�������
                DamageDealtCount++;
                break;
            case EnemyAction.Type.Defend:
                // ����ʱ�����ܵ��˺�������
                DamageCounter = 0;
                DefenseUsedCount++; // Ҳ���ӷ���ʹ�ô���
                break;
            case EnemyAction.Type.Charge:
                // ������������ֱ�������ض���������
                // ����Ч���������ӹ��������ӳ�������ʩ��DOT������ChargeAttack�д���
                // �����Ҫ����������������ض��������ļ�������
                break;
        }
        Debug.Log($"Enemy executed action: {actionType}. DamageDealtCount: {DamageDealtCount}, DamageCounter: {DamageCounter}, DefenseUsedCount: {DefenseUsedCount}");
    }
    // ========== �������� ==========

    // Ĭ�ϵ���AI
    private EnemyAction ChooseDefaultEnemyAction(int currentRound, float playerDamageTaken)
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
        // 2. �������ܣ��ܵ������˺�ʱʹ�ã����ȼ��ڶ���
        if (DamageCounter >= 2)
        {
            DamageCounter = 0; // ���ü�����
            return new EnemyAction { type = EnemyAction.Type.Defend };
        }
        // 3. �˺����ܣ�����״̬��������ʱʹ�ã����ȼ�������
        return new EnemyAction { type = EnemyAction.Type.Attack };
    }

    // �ڶ�������AI
    private EnemyAction ChooseSecondEnemyAction(int currentRound, float playerDamageTaken)
    {
        // ���ȼ���һ���������ܣ�����������ÿʹ��2��֮��ʹ�øü��ܣ�
        if (DefenseUsedCount >= 2 && currentRound > 1)
        {
            DefenseUsedCount = 0; // ���ü�����
            return new EnemyAction { type = EnemyAction.Type.Charge };
        }
        // ���ȼ��ڶ����������ܣ��ܵ������˺�ʱʹ�ã�
        if (DamageCounter >= 2)
        {
            DamageCounter = 0; // ���ü�����
            return new EnemyAction { type = EnemyAction.Type.Defend };
        }
        // ���ȼ��������˺�����
        return new EnemyAction { type = EnemyAction.Type.Attack };
    }

    // ����������AI
    private EnemyAction ChooseThirdEnemyAction(int currentRound, float playerDamageTaken)
    {
        // ���ȼ���һ���������ܣ�ÿ��������2���˺���ʹ�ã�
        if (DamageDealtCount >= 2)
        {
            DamageDealtCount = 0; // ���ü�����
            return new EnemyAction { type = EnemyAction.Type.Charge };
        }
        // ���ȼ��ڶ������˼��ܣ��ܵ������˺�ʱʹ�ã�
        if (DamageCounter >= 2)
        {
            DamageCounter = 0; // ���ü�����
            // �������ù�����
            BaseAttack += 2;
            CurrentAttack = BaseAttack;
            // ���ӹ�������
            AttackMultiplier = 1.5f;
            return new EnemyAction { type = EnemyAction.Type.Charge };
        }
        // ���ȼ��������˺�����
        return new EnemyAction { type = EnemyAction.Type.Attack };
    }

    // ÿ�غϿ�ʼʱ����
    public void OnTurnStart()
    {
        // �����������ʱ��
        if (DefenseDuration > 0)
        {
            DefenseDuration--;
            if (DefenseDuration <= 0)
            {
                IsDefending = false; // ��������
            }
        }
        // ����DOTЧ��
        if (DotDuration > 0)
        {
            DotDuration--;
            // ��������DOT�˺�
            if (BattleSystem.Instance != null && BattleSystem.Instance.playerManager != null)
            {
                BattleSystem.Instance.playerManager.TakeDamage(2f); // 2��DOT�˺�
                // ����UI
                if (BattleSystem.Instance.uiManager != null)
                {
                    BattleSystem.Instance.uiManager.UpdatePlayerStatus(
                        BattleSystem.Instance.playerManager.Health,
                        BattleSystem.Instance.playerManager.MaxHealth,
                        BattleSystem.Instance.playerManager.Attack,
                        BattleSystem.Instance.playerManager.Defense
                    );
                }
            }
        }
        // ����������˥��
        if (AttackMultiplier > 1f)
        {
            AttackMultiplier = Mathf.Max(1f, AttackMultiplier - 0.25f); // ÿ�غϼ���0.25����
        }
    }

    // ÿ�غϽ���ʱ����
    public void OnTurnEnd()
    {
        // ���������˺��������˺�������
        if (BattleSystem.Instance != null && BattleSystem.Instance.playerManager != null)
        {
            // �����������˺������߼�
        }
    }

    // ��ȡ����ID������PersistentBattleData��
    public int GetEnemyId()
    {
        switch (currentEnemyType)
        {
            case EnemyType.Default: return 0;
            case EnemyType.Second: return 1;
            case EnemyType.Third: return 2;
            default: return 0;
        }
    }
}