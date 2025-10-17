using UnityEngine;

public class CounterStrikeSystem : MonoBehaviour
{
    private PlayerManager playerManager;
    private EnemyManager enemyManager;
    private EffectManager effectManager;
    private IconManager iconManager;

    public void Initialize(PlayerManager playerManager, EnemyManager enemyManager,
                           EffectManager effectManager, IconManager iconManager)
    {
        this.playerManager = playerManager;
        this.enemyManager = enemyManager;
        this.effectManager = effectManager;
        this.iconManager = iconManager;
    }

    public void HandleCounterStrike()
    {
        if (!playerManager.CounterStrikeActive) return;

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

        // ���۳ɹ�ʧ�ܣ�����Ч��ֻ�ڱ��غ���Ч
        playerManager.CounterStrikeActive = false;
        iconManager.RemovePlayerIcon(IconManager.IconType.CounterStrike);
    }
}