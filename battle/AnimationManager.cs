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
    public Animator enemyAnimator;
    public string enemyIdleBool = "IsIdle";
    public string enemyAttackTrigger = "Attack";
    public string enemyHitTrigger = "Hit";
    public string enemySkillTrigger = "Skill";

    [Header("Animation Settings")]
    public float attackAnimationDuration = 0.8f;
    public float hitAnimationDuration = 0.5f;
    public float skillAnimationDuration = 0.6f;

    // ״̬����
    private string currentPlayerState = "Idle";
    private string currentEnemyState = "Idle";
    private bool isPlayerAnimating = false;
    private bool isEnemyAnimating = false;

    public void Initialize()
    {
        // ȷ����ʼ״̬ΪIdle
        if (playerAnimator != null)
        {
            playerAnimator.SetBool(playerIdleBool, true);
            playerAnimator.ResetTrigger(playerAttackTrigger);
            playerAnimator.ResetTrigger(playerHitTrigger);
        }

        if (enemyAnimator != null)
        {
            enemyAnimator.SetBool(enemyIdleBool, true);
            enemyAnimator.ResetTrigger(enemyAttackTrigger);
            enemyAnimator.ResetTrigger(enemyHitTrigger);
            enemyAnimator.ResetTrigger(enemySkillTrigger);
        }
    }

    // ===== ��Ҷ��� =====
    public void PlayPlayerAttack()
    {
        if (playerAnimator == null || isPlayerAnimating) return;

        StartCoroutine(PlayPlayerAttackSequence());
    }

    private IEnumerator PlayPlayerAttackSequence()
    {
        isPlayerAnimating = true;
        currentPlayerState = "Attack";

        // ����IdleΪfalse��������������
        playerAnimator.SetBool(playerIdleBool, false);
        playerAnimator.SetTrigger(playerAttackTrigger);

        // �ȴ��������
        yield return new WaitForSeconds(attackAnimationDuration);

        // ����״̬�ص�Idle
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

        // ����IdleΪfalse�������ܻ�����
        playerAnimator.SetBool(playerIdleBool, false);
        playerAnimator.SetTrigger(playerHitTrigger);

        // �ȴ��������
        yield return new WaitForSeconds(hitAnimationDuration);

        // ����״̬�ص�Idle
        playerAnimator.ResetTrigger(playerHitTrigger);
        playerAnimator.SetBool(playerIdleBool, true);
        currentPlayerState = "Idle";
        isPlayerAnimating = false;
    }

    // ===== ���˶��� =====
    public void PlayEnemyAttack()
    {
        if (enemyAnimator == null || isEnemyAnimating) return;

        StartCoroutine(PlayEnemyAttackSequence());
    }

    private IEnumerator PlayEnemyAttackSequence()
    {
        isEnemyAnimating = true;
        currentEnemyState = "Attack";

        // ����IdleΪfalse��������������
        enemyAnimator.SetBool(enemyIdleBool, false);
        enemyAnimator.SetTrigger(enemyAttackTrigger);

        // �ȴ��������
        yield return new WaitForSeconds(attackAnimationDuration);

        // ����״̬�ص�Idle
        enemyAnimator.ResetTrigger(enemyAttackTrigger);
        enemyAnimator.SetBool(enemyIdleBool, true);
        currentEnemyState = "Idle";
        isEnemyAnimating = false;
    }

    public void PlayEnemyHit()
    {
        if (enemyAnimator == null || isEnemyAnimating) return;

        StartCoroutine(PlayEnemyHitSequence());
    }

    private IEnumerator PlayEnemyHitSequence()
    {
        isEnemyAnimating = true;
        currentEnemyState = "Hit";

        // ����IdleΪfalse�������ܻ�����
        enemyAnimator.SetBool(enemyIdleBool, false);
        enemyAnimator.SetTrigger(enemyHitTrigger);

        // �ȴ��������
        yield return new WaitForSeconds(hitAnimationDuration);

        // ����״̬�ص�Idle
        enemyAnimator.ResetTrigger(enemyHitTrigger);
        enemyAnimator.SetBool(enemyIdleBool, true);
        currentEnemyState = "Idle";
        isEnemyAnimating = false;

        Debug.Log("Enemy hit animation completed, returning to Idle state");
    }

    // ===== ���˼��ܶ��� =====
    public void PlayEnemySkill()
    {
        if (enemyAnimator == null || isEnemyAnimating) return;

        StartCoroutine(PlayEnemySkillSequence());
    }

    private IEnumerator PlayEnemySkillSequence()
    {
        isEnemyAnimating = true;
        currentEnemyState = "Skill";

        // ����IdleΪfalse���������ܶ���
        enemyAnimator.SetBool(enemyIdleBool, false);
        enemyAnimator.SetTrigger(enemySkillTrigger);

        // �ȴ��������
        yield return new WaitForSeconds(skillAnimationDuration);

        // ����״̬�ص�Idle
        enemyAnimator.ResetTrigger(enemySkillTrigger);
        enemyAnimator.SetBool(enemyIdleBool, true);
        currentEnemyState = "Idle";
        isEnemyAnimating = false;

        Debug.Log("Enemy skill animation completed, returning to Idle state");
    }

    // ===== ����Ч�� =====
    public void PlayCounterStrikeEffect()
    {
        // ���������ӷ�����Ч��ʵ��
        Debug.Log("Counter strike effect played");
    }

    // ===== �������� =====
    public bool IsPlayerAnimating()
    {
        return isPlayerAnimating;
    }

    public bool IsEnemyAnimating()
    {
        return isEnemyAnimating;
    }

    // ���Է���
    public string GetPlayerAnimationState()
    {
        return currentPlayerState;
    }

    public string GetEnemyAnimationState()
    {
        return currentEnemyState;
    }
}