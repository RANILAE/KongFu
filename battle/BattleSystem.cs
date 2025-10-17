using UnityEngine;
using System.Collections;

public class BattleSystem : MonoBehaviour
{
    public static BattleSystem Instance { get; private set; }

    [Header("Core Systems")]
    public PlayerManager playerManager;
    public EnemyManager enemyManager;
    public YinYangSystem yinYangSystem;
    public UIManager uiManager;
    public EffectManager effectManager;
    public WheelSystem wheelSystem;
    public WheelController wheelController;
    public IconManager iconManager;
    public RetainedPointsSystem retainedPointsSystem;

    [Header("Battle Config")]
    public BattleConfig config;

    [Header("Battle State")]
    public BattleState currentState;
    public enum BattleState { PlayerSetup, PlayerTurn, EnemyTurn, BattleEnd }

    private int currentRound = 1;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        if (config == null)
        {
            Debug.LogError("BattleConfig not assigned!");
            return;
        }

        InitializeBattle();
    }

    public void InitializeBattle()
    {
        Debug.Log("Initializing battle");

        playerManager.Initialize(config, iconManager);
        enemyManager.Initialize(config);
        effectManager.Initialize(iconManager);
        yinYangSystem.Initialize(config);
        uiManager.Initialize();
        wheelSystem.Initialize(config.maxPoints);
        wheelController.Initialize(config.maxPoints);

        retainedPointsSystem = gameObject.AddComponent<RetainedPointsSystem>();
        retainedPointsSystem.Initialize(playerManager, wheelSystem);

        currentRound = 1;
        currentState = BattleState.PlayerSetup;

        uiManager.UpdateRoundDisplay(currentRound);
        uiManager.UpdatePlayerStatus(
            playerManager.Health,
            playerManager.MaxHealth,
            playerManager.Attack,
            playerManager.Defense
        );

        uiManager.UpdateEnemyStatus(enemyManager.Health, enemyManager.MaxHealth);

        wheelSystem.ShowWheelUI();

        Debug.Log("Battle initialization complete");
    }

    public void OnYinYangSet(float yangPoints, float yinPoints)
    {
        Debug.Log($"YinYang points set: Yang={yangPoints}, Yin={yinPoints}");

        if (currentState != BattleState.PlayerSetup)
        {
            Debug.LogWarning("Called YinYangSet in wrong state");
            return;
        }

        currentState = BattleState.PlayerTurn;
        wheelSystem.HideWheelUI();

        yinYangSystem.ApplyEffects(yangPoints, yinPoints);
        uiManager.UpdatePlayerAttackDefense(playerManager.Attack, playerManager.Defense);

        float damage = playerManager.CalculateDamage(enemyManager.CurrentDefense);
        enemyManager.TakeDamage(damage);
        uiManager.UpdateEnemyStatus(enemyManager.Health, enemyManager.MaxHealth);

        if (enemyManager.Health <= 0)
        {
            EndBattle(true);
            return;
        }

        retainedPointsSystem.CalculateAndStoreRetainedPoints();
        StartCoroutine(StartEnemyTurnAfterDelay(1.5f));
    }

    IEnumerator StartEnemyTurnAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        StartEnemyTurn();
    }

    void StartEnemyTurn()
    {
        currentState = BattleState.EnemyTurn;
        uiManager.UpdateBattleLog($"Enemy Turn - Round {currentRound}");

        retainedPointsSystem.ApplyStoredPoints();
        EnemyManager.EnemyAction action = enemyManager.ChooseAction(currentRound, playerManager.DamageTakenLastTurn);
        effectManager.ShowEnemyIntent(action);

        switch (action.type)
        {
            case EnemyManager.EnemyAction.Type.Attack:
                float damage = enemyManager.CalculateDamage(playerManager.Defense);
                playerManager.TakeDamage(damage);
                effectManager.HandleCounterStrike(playerManager, enemyManager);
                break;

            case EnemyManager.EnemyAction.Type.Defend:
                enemyManager.EnableDefense();
                break;

            case EnemyManager.EnemyAction.Type.Charge:
                enemyManager.ChargeAttack();
                break;
        }

        effectManager.ProcessAllEffects();
        UpdateUI();

        if (playerManager.Health <= 0)
        {
            EndBattle(false);
            return;
        }

        currentRound++;
        uiManager.UpdateRoundDisplay(currentRound);

        playerManager.ResetForNewTurn();
        wheelSystem.ResetPoints();
        wheelController.ResetSliders();
        yinYangSystem.ProcessCooldowns();
        retainedPointsSystem.ResetRetainedPoints();

        currentState = BattleState.PlayerSetup;
        wheelSystem.ShowWheelUI();
    }

    void EndBattle(bool playerWins)
    {
        Debug.Log($"Battle ended: {(playerWins ? "Victory" : "Defeat")}");

        currentState = BattleState.BattleEnd;
        string result = playerWins ? "Victory!" : "Defeat...";
        uiManager.UpdateBattleLog(result);

        wheelSystem.ResetMaxPoints(); // 使用正确的方法名
    }

    public void OnEndButtonClick()
    {
        if (currentState != BattleState.PlayerSetup) return;

        float yangPoints = wheelSystem.CurrentYangPoints;
        float yinPoints = wheelSystem.CurrentYinPoints;

        wheelSystem.HideWheelUI();
        currentState = BattleState.PlayerTurn;

        yinYangSystem.ApplyEffects(yangPoints, yinPoints);
        uiManager.UpdatePlayerAttackDefense(playerManager.Attack, playerManager.Defense);

        float damage = playerManager.CalculateDamage(enemyManager.CurrentDefense);
        enemyManager.TakeDamage(damage);
        uiManager.UpdateEnemyStatus(enemyManager.Health, enemyManager.MaxHealth);

        if (enemyManager.Health <= 0)
        {
            EndBattle(true);
            return;
        }

        retainedPointsSystem.CalculateAndStoreRetainedPoints();
        StartCoroutine(StartEnemyTurnAfterDelay(1.5f));
    }

    private void UpdateUI()
    {
        uiManager.UpdatePlayerStatus(
            playerManager.Health,
            playerManager.MaxHealth,
            playerManager.Attack,
            playerManager.Defense
        );
        uiManager.UpdateEnemyStatus(enemyManager.Health, enemyManager.MaxHealth);
        uiManager.UpdatePlayerAttackDefense(playerManager.Attack, playerManager.Defense);
    }
}