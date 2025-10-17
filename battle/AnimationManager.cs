using UnityEngine;

public class AnimationManager : MonoBehaviour
{
    [Header("玩家动画引用")]
    public Animator playerAnimator;

    [Header("敌人动画引用")]
    public Animator enemyAnimator;

    private BattleConfig config;

    public void Initialize(BattleConfig config)
    {
        this.config = config;

        // 如果Animator没有设置，创建默认组件
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
        // 这里可以添加自定义的反震特效
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
            // 修复: 使用正确的defeatAnim字段
            if (playerAnimator != null && config.defeatAnim != null)
            {
                playerAnimator.Play(config.defeatAnim.name);
            }
        }
    }
}