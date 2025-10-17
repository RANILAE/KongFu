using UnityEngine;

public class AnimationManager : MonoBehaviour
{
    [Header("��Ҷ�������")]
    public Animator playerAnimator;

    [Header("���˶�������")]
    public Animator enemyAnimator;

    private BattleConfig config;

    public void Initialize(BattleConfig config)
    {
        this.config = config;

        // ���Animatorû�����ã�����Ĭ�����
        if (playerAnimator == null) playerAnimator = gameObject.AddComponent<Animator>();
        if (enemyAnimator == null) enemyAnimator = gameObject.AddComponent<Animator>();
    }

    public void PlayPlayerAttack()
    {
        if (playerAnimator != null && config.playerAttackAnim != null)
        {
            playerAnimator.Play(config.playerAttackAnim.name);
        }
    }

    public void PlayPlayerHit()
    {
        if (playerAnimator != null && config.playerHitAnim != null)
        {
            playerAnimator.Play(config.playerHitAnim.name);
        }
    }

    public void PlayEnemyAttack()
    {
        if (enemyAnimator != null && config.enemyAttackAnim != null)
        {
            enemyAnimator.Play(config.enemyAttackAnim.name);
        }
    }

    public void PlayEnemyDefend()
    {
        if (enemyAnimator != null && config.enemyDefendAnim != null)
        {
            enemyAnimator.Play(config.enemyDefendAnim.name);
        }
    }

    public void PlayEnemyCharge()
    {
        if (enemyAnimator != null && config.enemyChargeAnim != null)
        {
            enemyAnimator.Play(config.enemyChargeAnim.name);
        }
    }

    public void PlayEnemyHit()
    {
        if (enemyAnimator != null && config.enemyHitAnim != null)
        {
            enemyAnimator.Play(config.enemyHitAnim.name);
        }
    }

    public void PlayCounterStrikeEffect()
    {
        // �����������Զ���ķ�����Ч
    }

    public void PlayBattleEnd(bool victory)
    {
        if (victory)
        {
            if (playerAnimator != null && config.victoryAnim != null)
            {
                playerAnimator.Play(config.victoryAnim.name);
            }
        }
        else
        {
            // �޸�: ʹ����ȷ��defeatAnim�ֶ�
            if (playerAnimator != null && config.defeatAnim != null)
            {
                playerAnimator.Play(config.defeatAnim.name);
            }
        }
    }
}