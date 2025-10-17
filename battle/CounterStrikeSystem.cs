using UnityEngine;

public class CounterStrikeSystem : MonoBehaviour
{
    private PlayerManager playerManager;
    private EnemyManager enemyManager;
    private EffectManager effectManager;
    // �Ƴ��� private IconManager iconManager;

    // �޸��˷���ǩ�����Ƴ��� IconManager ����
    public void Initialize(PlayerManager playerManager, EnemyManager enemyManager,
                           EffectManager effectManager)
    {
        this.playerManager = playerManager;
        this.enemyManager = enemyManager;
        this.effectManager = effectManager;
        // �Ƴ��� this.iconManager = iconManager;
    }

    public void HandleCounterStrike()
    {
        // ��鷴��Ч���Ƿ���Ȼ��Ч�����ڳ���ʱ�䣩
        if (!playerManager.CounterStrikeActive || playerManager.CounterStrikeDuration <= 0)
        {
            // �Ƴ���ͼ���Ƴ��߼�
            return;
        }
        BattleSystem.Instance.uiManager.UpdateBattleLog("Attempting counter strike...");
        // ����ɹ�����: ���˹����� < ��ҷ�����
        if (enemyManager.CurrentAttack < playerManager.Defense)
        {
            // ���㷴���˺� (������ + ������)
            float counterDamage = playerManager.Attack + playerManager.Defense;
            // Ӧ�ñ���
            counterDamage *= playerManager.CounterStrikeMultiplier;
            enemyManager.TakeDamage(counterDamage);
            BattleSystem.Instance.uiManager.UpdateBattleLog($"Counter strike successful! Dealt {counterDamage:F1} damage");
        }
        else
        {
            BattleSystem.Instance.uiManager.UpdateBattleLog("Counter strike failed! Player takes DOT damage");
            // ����: ʧ��ʱ�ܵ�2��DOT�˺�����2�غ�
            effectManager.AddPlayerDotEffect(2, 2);
        }
        // ע�⣺�����������Ƴ�����Ч������PlayerManager.ResetForNewTurn()���������ʱ��
        // ����Ч�����ڳ���ʱ��������Զ��Ƴ�
    }
}