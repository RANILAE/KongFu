using UnityEngine;

public class BattleAnimator : MonoBehaviour
{
    [Header("References")]
    public Animator playerAnimator;
    public Animator enemyAnimator;

    [Header("Animation Parameters")]
    public string attackParam = "Attack";
    public string defendParam = "Defend";
    public string takeDamageParam = "TakeDamage";
    public string idleParam = "Idle";
    public string chargeParam = "Charge";
    public string victoryParam = "Victory";
    public string defeatParam = "Defeat";

    void Start()
    {
        // 确保BattleSystem实例存在
        if (BattleSystem.Instance == null)
        {
            Debug.LogError("BattleSystem.Instance is null in BattleAnimator.Start!");
            return;
        }

        // 注册事件监听
        BattleSystem.Instance.onPlayerTurnStart.AddListener(OnPlayerTurnStart);
        BattleSystem.Instance.onEnemyTurnStart.AddListener(OnEnemyTurnStart);
        BattleSystem.Instance.onDamageCalculated.AddListener(OnDamageCalculated);
        BattleSystem.Instance.onBattleEnd.AddListener(OnBattleEnd);
    }

    void OnPlayerTurnStart()
    {
        if (playerAnimator != null) playerAnimator.SetTrigger(idleParam);
        if (enemyAnimator != null) enemyAnimator.SetTrigger(defendParam);
    }

    void OnEnemyTurnStart()
    {
        if (playerAnimator != null) playerAnimator.SetTrigger(defendParam);
        if (enemyAnimator != null) enemyAnimator.SetTrigger(idleParam);
    }

    void OnDamageCalculated()
    {
        if (BattleSystem.Instance.currentState == BattleSystem.BattleState.PlayerTurn)
        {
            if (playerAnimator != null) playerAnimator.SetTrigger(attackParam);
            if (enemyAnimator != null) enemyAnimator.SetTrigger(takeDamageParam);
        }
        else if (BattleSystem.Instance.currentState == BattleSystem.BattleState.EnemyTurn)
        {
            if (enemyAnimator != null) enemyAnimator.SetTrigger(attackParam);
            if (playerAnimator != null) playerAnimator.SetTrigger(takeDamageParam);
        }
    }

    void OnBattleEnd()
    {
        if (BattleSystem.Instance.player.health > 0)
        {
            if (playerAnimator != null) playerAnimator.SetTrigger(victoryParam);
            if (enemyAnimator != null) enemyAnimator.SetTrigger(defeatParam);
        }
        else
        {
            if (playerAnimator != null) playerAnimator.SetTrigger(defeatParam);
            if (enemyAnimator != null) enemyAnimator.SetTrigger(victoryParam);
        }
    }

    public void PlayChargeAnimation()
    {
        if (enemyAnimator != null) enemyAnimator.SetTrigger(chargeParam);
    }
}