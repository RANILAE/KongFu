using UnityEngine;

public class CounterStrikeSystem : MonoBehaviour
{
    private PlayerManager playerManager;
    private EnemyManager enemyManager;
    private EffectManager effectManager;

    public void Initialize(PlayerManager playerManager, EnemyManager enemyManager,
                           EffectManager effectManager)
    {
        this.playerManager = playerManager;
        this.enemyManager = enemyManager;
        this.effectManager = effectManager;
    }

    /// <summary>
    /// ִ�з���Ч�� - �ϸ��¹���
    /// </summary>
    /// <param name="isAttackBlocked">���������Ƿ��Ѿ����˹���</param>
    public void ExecuteCounterStrike(bool isAttackBlocked)
    {
        // --- ǰ�ü�� ---
        // 1. �������Ƿ񼤻��˷���Ч�� (��������YinYangSystem���ض�״̬������)
        //    BattleSystem �Ѿ�ȷ���� CounterStrikeActive Ϊ true �Ż���ô˷���
        //    ��Ϊ�˱��գ��ڲ��ټ��һ��
        if (!playerManager.CounterStrikeActive)
        {
            // Debug.Log("CounterStrikeSystem: Counter strike not active, skipping execution.");
            return;
        }

        // --- ��������ж� ---
        // 2. ���𴥷�����: ��ҷ����� > ���˹�����
        //    ע�⣺��ʹ�������������� (isAttackBlocked=true)��ֻҪ��������ֵ�ϴ��ڹ�����������ͳɹ���
        if (playerManager.Defense > enemyManager.CurrentAttack)
        {
            // --- ����ɹ� ---
            // 3. ���㷴���˺�
            float counterDamage = playerManager.Defense;

            // 4. ����Ƿ��Ǽ�����״̬������������˺�����1.5��
            //    playerManager.IsInExtremeYinState() �жϵ��ǵ�ǰ�������Ƿ���ϼ�������Χ
            //    playerManager.CounterStrikeActive ��ʾ���غϼ����˷��𣨿�������ʢ���������򾿼�����
            //    ������Ҫͬʱ���㡰���ڼ�����������Χ���͡������˷��𡱲���Ӧ��1.5���˺�
            if (playerManager.IsInExtremeYinState())
            {
                counterDamage *= 1.5f;
                BattleSystem.Instance.uiManager.UpdateBattleLog($"Enhanced Counter strike! Dealt {counterDamage:F1} damage to enemy");
            }
            else
            {
                BattleSystem.Instance.uiManager.UpdateBattleLog($"Counter strike! Dealt {counterDamage:F1} damage to enemy");
            }

            // 5. �Ե�����ɷ����˺�
            enemyManager.TakeDamage(counterDamage);

            // 6. ���ŷ�����Ч
            if (BattleSystem.Instance.animationManager != null)
            {
                BattleSystem.Instance.animationManager.PlayCounterStrikeEffect();
            }

        }
        else
        {
            // --- ����ʧ�� ---
            BattleSystem.Instance.uiManager.UpdateBattleLog("Enemy's attack power is too high for counter strike!");

            // 7. ���ʧ�ܳͷ���ֻ������ʢ�򼫶���״̬�£�����ʧ�ܲŻ��ܵ�DOT�˺�
            //    playerManager.IsInYinProsperityState() �� playerManager.IsInExtremeYinState()
            //    �жϵ��ǵ�ǰ�������Ƿ������Щ��Χ���� CounterStrikeActive ��ʾ�����Ѽ���
            //    ������������ YinYangSystem Ӧ��Ч��ʱ��һ�µģ����������������Χ����
            if (playerManager.IsInYinProsperityState() || playerManager.IsInExtremeYinState())
            {
                BattleSystem.Instance.uiManager.UpdateBattleLog("Player suffers backlash! Takes DOT damage.");
                // ʩ��2��DOT�˺�������2�غ�
                effectManager.AddPlayerDotEffect(2, 2);
            }
            else
            {
                // ����Ǿ�����״̬����ķ���ʧ�ܣ����ݹ��򲻴�������ض�DOT
                // ��ͨ�˺����� BattleSystem �д���
            }
        }

        // 8. ע�⣺����ĳ���ʱ�� (CounterStrikeDuration) ��״̬ (CounterStrikeActive)
        //    �Ĺ����� PlayerManager.ResetForNewTurn() ����
        //    ���ε��ý����󣬱��ι����ķ����ж��ͽ����ˡ�
        //    YinYangSystem ��ÿ�غϿ�ʼʱ���ݵ��������¼����
    }

    // --- ����ԭ�з����Լ��ݾɴ������ (��Ȼ���ܲ���ֱ��ʹ��) ---
    // ��Щ�������ڻ���ú��ĵ� ExecuteCounterStrike(bool) ����

    public void HandleCounterStrike()
    {
        // Ϊ�˼��ݣ������������δ���񵲣���Ϊ������ˣ�ͨ��������������
        // ����׼ȷ�ķ�ʽ������ BattleSystem ������ȷ�� isAttackBlocked ״̬
        // ����Ϊ�˼����ԣ����Ǽ���δ��
        ExecuteCounterStrike(false);
    }

    public void HandleCounterStrike(float damageTaken)
    {
        // damageTaken �����ڴ��߼��в���ֱ��ʹ�ã���Ϊ�ж����ڷ������͹�����
        // ͬ������δ��
        ExecuteCounterStrike(false);
    }

    public void HandleCounterStrike(float damageTaken, bool isInCounterStrikeState)
    {
        // isInCounterStrikeState �����ڴ��߼������࣬��Ϊ PlayerManager.CounterStrikeActive ��Ȩ��Դ
        // �� BattleSystem �Ѿ�����ˡ�Ϊ�˼��ݣ����ǵ��ú����߼���
        // ����δ��
        ExecuteCounterStrike(false);
    }
}