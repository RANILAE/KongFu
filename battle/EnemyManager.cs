using UnityEngine;
using System.Collections.Generic; // ��Ӵ������ռ�

public class EnemyManager : MonoBehaviour
{
    // ״̬�ֶ�
    private bool IsDefending = false; // ����״̬���� DefenseDuration �ƶϣ�
    private int DefenseDuration = 0; // ���������غ��� (˽�У��ڲ�����)
    private int DamageDealtCount = 0; // ���ڴ�����������������
    private int DotDuration = 0; // DOT�����غ��� (������OnTurnStart�д���)
    private float AttackMultiplier = 1f; // �������� (������ExecuteAttack������)
    private int DefenseUsedCount = 0; // ��������ʹ�ô���
    private int DamageCounter = 0; // ���ڴ������� (������TakeDamage�е���)

    // ========== �޸ģ�׷���ض�����Ч����״̬ ==========
    private bool NextAttackCharged = false; // �ڶ������ˣ������һ�ι����Ƿ�����
    private bool NextAttackAppliesDot = false; // ���������ˣ������һ�ι����Ƿ񸽴�����DOT
    private int ThirdEnemyNextDefendDotBonus = 0; // ���������ˣ��´η���DOT�˺��ӳ�
    // ========== �޸Ľ��� ==========

    // ========== ������Inspector���޸ĵ������ֶ� ==========
    [Header("ͨ��Ч������")]
    public int DefaultDefenseDuration = 1; // Ĭ�Ϸ��������غ���
    public int SecondEnemyDefenseDuration = 2; // �ڶ������˷��������غ���
    public int ThirdEnemyDefenseDuration = 1; // ���������˷��������غ��� (����)
    public int DotTurns = 2; // DOT�����غ���
    public float DotDamage = 1f; // DOTÿ�غ��˺�
    public float ChargedAttackMultiplier = 2.0f; // ������������
    public int ThirdEnemyChargeDotDamage = 3; // ����������������������DOT�˺�
    public int ThirdEnemyDefenseDotBaseDamage = 1; // ���������˷���ʩ��DOT�Ļ����˺�
    // ========== �������� ==========

    // �����ֶ� (ʹ��float)
    public float Health { get; private set; }
    public float MaxHealth { get; private set; }
    public float BaseAttack { get; private set; }
    public float CurrentAttack { get; set; }
    public float Defense { get; private set; }
    public float CurrentDefense { get; set; }

    public class EnemyAction
    {
        public enum Type { Attack, Defend, Charge }
        public Type type;
    }

    public enum EnemyType { Default, Second, Third }

    private EnemyType currentEnemyType = EnemyType.Default;
    private BattleConfig config;

    public void Initialize(BattleConfig config)
    {
        this.config = config;
        SetEnemyProperties(config);
        ResetEnemyState(config);
    }

    private void SetEnemyProperties(BattleConfig config)
    {
        // ���ݵ�ǰ����������������
        switch (currentEnemyType)
        {
            case EnemyType.Second:
                MaxHealth = config.secondEnemyMaxHealth;
                BaseAttack = config.secondEnemyBaseAttack;
                Defense = config.secondEnemyBaseDefense;
                break;
            case EnemyType.Third:
                MaxHealth = config.thirdEnemyMaxHealth;
                BaseAttack = config.thirdEnemyBaseAttack;
                Defense = config.thirdEnemyBaseDefense;
                break;
            case EnemyType.Default:
            default:
                MaxHealth = config.enemyMaxHealth;
                BaseAttack = config.enemyBaseAttack;
                Defense = config.enemyBaseDefense;
                break;
        }
        Health = MaxHealth;
        CurrentAttack = BaseAttack;
        CurrentDefense = Defense;
    }

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

        // ========== ��������״̬ ==========
        NextAttackCharged = false;
        NextAttackAppliesDot = false;
        ThirdEnemyNextDefendDotBonus = 0; // ���÷���DOT�ӳ�
        // ========== ���ý��� ==========

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
                        actualDamage *= (1f - BattleSystem.Instance.config.secondEnemyDefenseReduction);
                    }
                    break;
                case EnemyType.Third:
                    // ���������ˣ���ȫ��
                    actualDamage = 0f;
                    break;
                case EnemyType.Default:
                default:
                    // Ĭ�ϵ��ˣ�����25%�˺�
                    actualDamage *= 0.75f;
                    break;
            }
        }

        Health = Mathf.Max(0, Health - actualDamage);
        if (actualDamage > 0)
        {
            DamageCounter++; // �����ܵ��˺�������
        }

        // ���ŵ����ܻ��������߼��Ƶ�BattleSystem��

        // ����UI
        if (BattleSystem.Instance != null && BattleSystem.Instance.uiManager != null)
        {
            BattleSystem.Instance.uiManager.UpdateEnemyStatus(Health, MaxHealth);
        }
    }

    public void EnableDefense()
    {
        // ���÷���״̬
        IsDefending = true;
        // ���ݵ����������÷��������غ���
        switch (currentEnemyType)
        {
            case EnemyType.Second:
                DefenseDuration = SecondEnemyDefenseDuration; // ʹ��Inspector�����õ�ֵ
                break;
            case EnemyType.Third: // **�޸���Ϊ�����ֵ����������**
                DefenseDuration = ThirdEnemyDefenseDuration; // ʹ��Inspector�����õ�ֵ
                break;
            case EnemyType.Default:
            default:
                DefenseDuration = DefaultDefenseDuration; // ʹ��Inspector�����õ�ֵ
                break;
        }
        DefenseUsedCount++; // ���ӷ���ʹ�ô���

        // ========== �޸ģ����������˷���Ч�� ==========
        if (currentEnemyType == EnemyType.Third)
        {
            // ���������ʩ��DOT�������˺�Ϊ1���������üӳ�
            int dotDamage = ThirdEnemyDefenseDotBaseDamage + ThirdEnemyNextDefendDotBonus; // ʹ��Inspector�����õ�ֵ
            if (BattleSystem.Instance != null && BattleSystem.Instance.effectManager != null)
            {
                BattleSystem.Instance.effectManager.AddPlayerDotEffect(dotDamage, DotTurns); // ʹ��Inspector�����õ�ֵ
                BattleSystem.Instance.uiManager.UpdateBattleLog($"Enemy applies {dotDamage} DOT damage to player for {DotTurns} turns!");
            }
            // �������üӳɣ���Ϊ���Ѿ�Ӧ���ˣ�
            ThirdEnemyNextDefendDotBonus = 0;
        }
        // ========== �޸Ľ��� ==========

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
                // ========== �޸ģ��ڶ�����������Ч�� ==========
                // �����һ�ι���Ϊ�����������˺�*2��
                NextAttackCharged = true;
                // �������ӹ�����1��
                CurrentAttack += 1;
                BattleSystem.Instance.uiManager.UpdateBattleLog($"Enemy charged! Next attack will deal {ChargedAttackMultiplier}x damage and gains 1 permanent attack.");
                // ========== �޸Ľ��� ==========
                break;
            case EnemyType.Third:
                // ========== �޸ģ���������������Ч�� ==========
                // �����һ�ι�������DOT��3���˺���
                NextAttackAppliesDot = true;
                BattleSystem.Instance.uiManager.UpdateBattleLog($"Enemy charged! Next attack will apply {ThirdEnemyChargeDotDamage} DOT.");
                // ========== �޸Ľ��� ==========
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

    // ========== �޸ģ�ִ�й������������ڴ�������Ч���� ==========
    public void ExecuteAttack()
    {
        // ��鲢Ӧ������Ч��
        if (currentEnemyType == EnemyType.Second && NextAttackCharged)
        {
            AttackMultiplier = ChargedAttackMultiplier; // ʹ��Inspector�����õı���
            NextAttackCharged = false; // ��������Ч��
            BattleSystem.Instance.uiManager.UpdateBattleLog($"Enemy uses charged attack ({ChargedAttackMultiplier}x damage)!");
        }
        else
        {
            AttackMultiplier = 1f; // ���ñ���
        }

        // ��鲢Ӧ�õ�����������������������DOT
        if (currentEnemyType == EnemyType.Third && NextAttackAppliesDot)
        {
            // ��������󣬸����ʩ��3��DOT������2�غ�
            if (BattleSystem.Instance != null && BattleSystem.Instance.effectManager != null)
            {
                BattleSystem.Instance.effectManager.AddPlayerDotEffect(ThirdEnemyChargeDotDamage, DotTurns); // ʹ��Inspector�����õ�ֵ
                BattleSystem.Instance.uiManager.UpdateBattleLog($"Enemy's attack applies {ThirdEnemyChargeDotDamage} DOT damage to player for {DotTurns} turns!");
            }
            NextAttackAppliesDot = false; // ���ı��
        }
    }
    // ========== �޸Ľ��� ==========

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

        // ========== �ؼ��޸ģ����ڲ�������ͼ��ʾ ==========
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

    // ========== �޸ģ�ִ�е��˶��������״̬ ==========
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
                ExecuteAttack(); // ����������Ч��
                break;
            case EnemyAction.Type.Defend:
                // ����ʱ�����ܵ��˺�������
                DamageCounter = 0;
                DefenseUsedCount++; // Ҳ���ӷ���ʹ�ô���
                // ========== ���������������˷�����������DOT�ӳ� ==========
                if (currentEnemyType == EnemyType.Third)
                {
                    ThirdEnemyNextDefendDotBonus += 1;
                    BattleSystem.Instance.uiManager.UpdateBattleLog($"Enemy's next defense DOT bonus increased by 1 (Total bonus: {ThirdEnemyNextDefendDotBonus}).");
                }
                // ========== �������� ==========
                break;
            case EnemyAction.Type.Charge:
                // ������������ֱ�������ض���������
                // ����Ч���������ӹ��������ӳ�������ʩ��DOT������ChargeAttack�д���
                // �����Ҫ����������������ض��������ļ�������
                break;
        }
        Debug.Log($"Enemy executed action: {actionType}. DamageDealtCount: {DamageDealtCount}, DamageCounter: {DamageCounter}, DefenseUsedCount: {DefenseUsedCount}");
    }
    // ========== �޸Ľ��� ==========

    // Ĭ�ϵ���AI
    private EnemyAction ChooseDefaultEnemyAction(int currentRound, float playerDamageTaken)
    {
        // 1. �������ܣ�������ȼ���ÿ4�غ�ʹ��һ�Σ�
        if (currentRound % 4 == 1 && currentRound > 1) // ȷ�����ǵ�һ�غ�
        {
            return new EnemyAction { type = EnemyAction.Type.Charge };
        }
        // 2. �������ܣ�ÿ���غ�ʹ��һ�Σ����ȼ��ڶ���
        else if (currentRound % 2 == 0)
        {
            return new EnemyAction { type = EnemyAction.Type.Defend };
        }
        // 3. �˺����ܣ�����״̬��������ʱʹ�ã����ȼ�������
        else
        {
            return new EnemyAction { type = EnemyAction.Type.Attack };
        }
    }

    // ========== �޸ģ��ڶ�������AI ==========
    private EnemyAction ChooseSecondEnemyAction(int currentRound, float playerDamageTaken)
    {
        // ���ȼ���һ���������ܣ�ÿ3�غ�ʹ��һ�Σ�������ȼ�
        if (currentRound % 3 == 0 && currentRound > 0) // �����3, 6, 9...�غ�
        {
            return new EnemyAction { type = EnemyAction.Type.Charge };
        }
        // ���ȼ��ڶ����������ܣ�ÿ4�غ�ʹ��һ�Σ������ȼ��ڶ���
        else if (currentRound % 4 == 0 && currentRound > 0) // �����4, 8, 12...�غ�
        {
            return new EnemyAction { type = EnemyAction.Type.Defend };
        }
        // ���ȼ��������˺����ܣ�����״̬��������ʱʹ�ã������ȼ�������
        else
        {
            return new EnemyAction { type = EnemyAction.Type.Attack };
        }
    }
    // ========== �޸Ľ��� ==========

    // ����������AI
    private EnemyAction ChooseThirdEnemyAction(int currentRound, float playerDamageTaken)
    {
        // ���ȼ���һ���������ܣ�ÿ3�غ�ʹ��һ�Σ�������ȼ�
        if (currentRound % 3 == 1 && currentRound > 1) // �����1, 4, 7...�غϣ���������1�غ�
        {
            return new EnemyAction { type = EnemyAction.Type.Charge };
        }
        // ���ȼ��ڶ����������ܣ�ÿ2�غ�ʹ��һ�Σ������ȼ��ڶ���
        else if (currentRound % 2 == 0)
        {
            return new EnemyAction { type = EnemyAction.Type.Defend };
        }
        // ���ȼ��������˺����ܣ�����״̬��������ʱʹ�ã������ȼ�������
        else
        {
            return new EnemyAction { type = EnemyAction.Type.Attack };
        }
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
                float damageThisTurn = DotDamage; // ʹ��Inspector�����õ�ֵ
                BattleSystem.Instance.playerManager.TakeDamage(damageThisTurn);
                BattleSystem.Instance.uiManager.UpdateBattleLog($"Enemy applies {damageThisTurn} DOT to player! {DotDuration} turns remaining.");
                // �����������˺������߼�
            }
        }
    }

    public void SetEnemyType(EnemyType type)
    {
        currentEnemyType = type;
        if (config != null)
        {
            SetEnemyProperties(config);
        }
    }

    public EnemyType GetCurrentEnemyType()
    {
        return currentEnemyType;
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