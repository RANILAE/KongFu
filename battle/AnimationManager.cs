using UnityEngine;
using System.Collections;

public class AnimationManager : MonoBehaviour
{
    [Header("Player Animations")]
    public Animator playerAnimator;
    public string playerIdleBool = "IsIdle";
    public string playerAttackTrigger = "Attack";
    public string playerHitTrigger = "Hit";

    [Header("Enemy Animations")]
    public Animator enemyAnimator; // 默认敌人动画器
    public Animator secondEnemyAnimator; // 第二个敌人动画器
    public Animator thirdEnemyAnimator; // 第三个敌人动画器

    // 当前使用的敌人动画器
    private Animator currentEnemyAnimator;

    public string enemyIdleBool = "IsIdle";
    public string enemyAttackTrigger = "Attack";
    public string enemyHitTrigger = "Hit";
    public string enemySkillTrigger = "Skill";

    [Header("Animation Settings")]
    public float attackAnimationDuration = 0.8f;
    public float hitAnimationDuration = 0.5f;
    public float skillAnimationDuration = 0.6f;

    // 状态管理
    private string currentPlayerState = "Idle";
    private string currentEnemyState = "Idle";
    private bool isPlayerAnimating = false;
    private bool isEnemyAnimating = false;

    public void Initialize()
    {
        // 确保初始状态为Idle
        if (playerAnimator != null)
        {
            playerAnimator.SetBool(playerIdleBool, true);
            playerAnimator.ResetTrigger(playerAttackTrigger);
            playerAnimator.ResetTrigger(playerHitTrigger);
        }

        // 初始化敌人动画器
        if (enemyAnimator != null)
        {
            currentEnemyAnimator = enemyAnimator;
            currentEnemyAnimator.SetBool(enemyIdleBool, true);
            currentEnemyAnimator.ResetTrigger(enemyAttackTrigger);
            currentEnemyAnimator.ResetTrigger(enemyHitTrigger);
            currentEnemyAnimator.ResetTrigger(enemySkillTrigger);
        }
    }

    // 设置当前敌人动画器（根据敌人类型切换）
    public void SetCurrentEnemyAnimator(EnemyManager.EnemyType enemyType)
    {
        switch (enemyType)
        {
            case EnemyManager.EnemyType.Second:
                currentEnemyAnimator = secondEnemyAnimator;
                break;
            case EnemyManager.EnemyType.Third:
                currentEnemyAnimator = thirdEnemyAnimator;
                break;
            case EnemyManager.EnemyType.Default:
            default:
                currentEnemyAnimator = enemyAnimator;
                break;
        }

        // 初始化新动画器
        if (currentEnemyAnimator != null)
        {
            currentEnemyAnimator.SetBool(enemyIdleBool, true);
            currentEnemyAnimator.ResetTrigger(enemyAttackTrigger);
            currentEnemyAnimator.ResetTrigger(enemyHitTrigger);
            currentEnemyAnimator.ResetTrigger(enemySkillTrigger);
        }
    }

    // ===== 玩家动画 =====
    public void PlayPlayerAttack()
    {
        if (playerAnimator == null || isPlayerAnimating) return;

        StartCoroutine(PlayPlayerAttackSequence());
    }

    private IEnumerator PlayPlayerAttackSequence()
    {
        isPlayerAnimating = true;
        currentPlayerState = "Attack";

        // 设置Idle为false，触发攻击动画
        playerAnimator.SetBool(playerIdleBool, false);
        playerAnimator.SetTrigger(playerAttackTrigger);

        // 等待动画完成
        yield return new WaitForSeconds(attackAnimationDuration);

        // 重置状态回到Idle
        playerAnimator.ResetTrigger(playerAttackTrigger);
        playerAnimator.SetBool(playerIdleBool, true);
        currentPlayerState = "Idle";
        isPlayerAnimating = false;
    }

    public void PlayPlayerHit()
    {
        if (playerAnimator == null || isPlayerAnimating) return;

        StartCoroutine(PlayPlayerHitSequence());
    }

    private IEnumerator PlayPlayerHitSequence()
    {
        isPlayerAnimating = true;
        currentPlayerState = "Hit";

        // 设置Idle为false，触发受击动画
        playerAnimator.SetBool(playerIdleBool, false);
        playerAnimator.SetTrigger(playerHitTrigger);

        // 等待动画完成
        yield return new WaitForSeconds(hitAnimationDuration);

        // 重置状态回到Idle
        playerAnimator.ResetTrigger(playerHitTrigger);
        playerAnimator.SetBool(playerIdleBool, true);
        currentPlayerState = "Idle";
        isPlayerAnimating = false;
    }

    // ===== 敌人动画 =====
    public void PlayEnemyAttack()
    {
        if (currentEnemyAnimator == null || isEnemyAnimating) return;

        StartCoroutine(PlayEnemyAttackSequence());
    }

    private IEnumerator PlayEnemyAttackSequence()
    {
        isEnemyAnimating = true;
        currentEnemyState = "Attack";

        // 设置Idle为false，触发攻击动画
        currentEnemyAnimator.SetBool(enemyIdleBool, false);
        currentEnemyAnimator.SetTrigger(enemyAttackTrigger);

        // 等待动画完成
        yield return new WaitForSeconds(attackAnimationDuration);

        // 重置状态回到Idle
        currentEnemyAnimator.ResetTrigger(enemyAttackTrigger);
        currentEnemyAnimator.SetBool(enemyIdleBool, true);
        currentEnemyState = "Idle";
        isEnemyAnimating = false;
    }

    public void PlayEnemyHit()
    {
        if (currentEnemyAnimator == null || isEnemyAnimating) return;

        StartCoroutine(PlayEnemyHitSequence());
    }

    private IEnumerator PlayEnemyHitSequence()
    {
        isEnemyAnimating = true;
        currentEnemyState = "Hit";

        // 设置Idle为false，触发受击动画
        currentEnemyAnimator.SetBool(enemyIdleBool, false);
        currentEnemyAnimator.SetTrigger(enemyHitTrigger);

        // 等待动画完成
        yield return new WaitForSeconds(hitAnimationDuration);

        // 重置状态回到Idle
        currentEnemyAnimator.ResetTrigger(enemyHitTrigger);
        currentEnemyAnimator.SetBool(enemyIdleBool, true);
        currentEnemyState = "Idle";
        isEnemyAnimating = false;
    }

    // ===== 敌人技能动画 =====
    public void PlayEnemySkill()
    {
        if (currentEnemyAnimator == null || isEnemyAnimating) return;

        StartCoroutine(PlayEnemySkillSequence());
    }

    private IEnumerator PlayEnemySkillSequence()
    {
        isEnemyAnimating = true;
        currentEnemyState = "Skill";

        // 设置Idle为false，触发技能动画
        currentEnemyAnimator.SetBool(enemyIdleBool, false);
        currentEnemyAnimator.SetTrigger(enemySkillTrigger);

        // 等待动画完成
        yield return new WaitForSeconds(skillAnimationDuration);

        // 重置状态回到Idle
        currentEnemyAnimator.ResetTrigger(enemySkillTrigger);
        currentEnemyAnimator.SetBool(enemyIdleBool, true);
        currentEnemyState = "Idle";
        isEnemyAnimating = false;
    }

    // ===== 特殊效果 =====
    public void PlayCounterStrikeEffect()
    {
        // 这里可以添加反震特效的实现
        Debug.Log("Counter strike effect played");
    }

    // ===== 辅助方法 =====
    public bool IsPlayerAnimating()
    {
        return isPlayerAnimating;
    }

    public bool IsEnemyAnimating()
    {
        return isEnemyAnimating;
    }

    public bool IsAnimating()
    {
        return isPlayerAnimating || isEnemyAnimating;
    }

    // 调试方法
    public string GetPlayerAnimationState()
    {
        return currentPlayerState;
    }

    public string GetEnemyAnimationState()
    {
        return currentEnemyState;
    }
}