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
    public Animator enemyAnimator; // Ĭ�ϵ��˶�����
    public Animator secondEnemyAnimator; // �ڶ������˶�����
    public Animator thirdEnemyAnimator; // ���������˶�����

    // ��ǰʹ�õĵ��˶�����
    private Animator currentEnemyAnimator;

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

        // ��ʼ�����˶�����
        if (enemyAnimator != null)
        {
            currentEnemyAnimator = enemyAnimator;
            currentEnemyAnimator.SetBool(enemyIdleBool, true);
            currentEnemyAnimator.ResetTrigger(enemyAttackTrigger);
            currentEnemyAnimator.ResetTrigger(enemyHitTrigger);
            currentEnemyAnimator.ResetTrigger(enemySkillTrigger);
        }
    }

    // ���õ�ǰ���˶����������ݵ��������л���
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

        // ��ʼ���¶�����
        if (currentEnemyAnimator != null)
        {
            currentEnemyAnimator.SetBool(enemyIdleBool, true);
            currentEnemyAnimator.ResetTrigger(enemyAttackTrigger);
            currentEnemyAnimator.ResetTrigger(enemyHitTrigger);
            currentEnemyAnimator.ResetTrigger(enemySkillTrigger);
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
        if (currentEnemyAnimator == null || isEnemyAnimating) return;

        StartCoroutine(PlayEnemyAttackSequence());
    }

    private IEnumerator PlayEnemyAttackSequence()
    {
        isEnemyAnimating = true;
        currentEnemyState = "Attack";

        // ����IdleΪfalse��������������
        currentEnemyAnimator.SetBool(enemyIdleBool, false);
        currentEnemyAnimator.SetTrigger(enemyAttackTrigger);

        // �ȴ��������
        yield return new WaitForSeconds(attackAnimationDuration);

        // ����״̬�ص�Idle
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

        // ����IdleΪfalse�������ܻ�����
        currentEnemyAnimator.SetBool(enemyIdleBool, false);
        currentEnemyAnimator.SetTrigger(enemyHitTrigger);

        // �ȴ��������
        yield return new WaitForSeconds(hitAnimationDuration);

        // ����״̬�ص�Idle
        currentEnemyAnimator.ResetTrigger(enemyHitTrigger);
        currentEnemyAnimator.SetBool(enemyIdleBool, true);
        currentEnemyState = "Idle";
        isEnemyAnimating = false;
    }

    // ===== ���˼��ܶ��� =====
    public void PlayEnemySkill()
    {
        if (currentEnemyAnimator == null || isEnemyAnimating) return;

        StartCoroutine(PlayEnemySkillSequence());
    }

    private IEnumerator PlayEnemySkillSequence()
    {
        isEnemyAnimating = true;
        currentEnemyState = "Skill";

        // ����IdleΪfalse���������ܶ���
        currentEnemyAnimator.SetBool(enemyIdleBool, false);
        currentEnemyAnimator.SetTrigger(enemySkillTrigger);

        // �ȴ��������
        yield return new WaitForSeconds(skillAnimationDuration);

        // ����״̬�ص�Idle
        currentEnemyAnimator.ResetTrigger(enemySkillTrigger);
        currentEnemyAnimator.SetBool(enemyIdleBool, true);
        currentEnemyState = "Idle";
        isEnemyAnimating = false;
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

    public bool IsAnimating()
    {
        return isPlayerAnimating || isEnemyAnimating;
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