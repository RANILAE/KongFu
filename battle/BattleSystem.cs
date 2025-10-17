using UnityEngine;
using System.Collections;
using System; // 添加这个 using 语句

public class BattleSystem : MonoBehaviour
{
    // 单例模式（移除跨场景不销毁）
    public static BattleSystem Instance { get; private set; }

    [Header("Core Systems")]
    public PlayerManager playerManager; // 玩家管理器，保持引用
    public EnemyManager enemyManager;
    public YinYangSystem yinYangSystem;
    public UIManager uiManager;
    public EffectManager effectManager;
    public WheelSystem wheelSystem;
    public WheelController wheelController;
    // 移除了 public IconManager iconManager;
    public RetainedPointsSystem retainedPointsSystem;
    public AudioManager audioManager;
    public AnimationManager animationManager;
    public CounterStrikeSystem counterStrikeSystem;

    [Header("Battle Config")]
    public BattleConfig config;

    [Header("Battle State")]
    public BattleState currentState;
    public enum BattleState { PlayerSetup, PlayerTurn, EnemyTurn, BattleEnd }

    private int currentRound = 1;
    private bool isBattleInitialized = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            // 移除了 DontDestroyOnLoad，这样在切换场景时会自动销毁该对象
        }
        else
        {
            // 如果已经存在一个实例，则销毁当前这个
            Destroy(gameObject);
        }
    }

    void Start()
    {
        if (config == null)
        {
            Debug.LogError("BattleConfig not assigned! Please assign a BattleConfig asset.");
            return;
        }

        // 在Start中调用初始化，确保所有依赖项都已加载
        InitializeBattle();
    }

    public void InitializeBattle()
    {
        if (isBattleInitialized) return;

        Debug.Log("Initializing battle");

        // 确保所有核心组件都已初始化
        if (playerManager == null) playerManager = FindObjectOfType<PlayerManager>();
        if (enemyManager == null) enemyManager = FindObjectOfType<EnemyManager>();
        if (yinYangSystem == null) yinYangSystem = FindObjectOfType<YinYangSystem>();
        if (uiManager == null) uiManager = FindObjectOfType<UIManager>();
        if (effectManager == null) effectManager = FindObjectOfType<EffectManager>();
        if (wheelSystem == null) wheelSystem = FindObjectOfType<WheelSystem>();
        if (wheelController == null) wheelController = FindObjectOfType<WheelController>();
        // 移除了 if (iconManager == null) iconManager = FindObjectOfType<IconManager>();
        if (retainedPointsSystem == null) retainedPointsSystem = FindObjectOfType<RetainedPointsSystem>();
        if (audioManager == null) audioManager = FindObjectOfType<AudioManager>();
        if (animationManager == null) animationManager = FindObjectOfType<AnimationManager>();
        if (counterStrikeSystem == null) counterStrikeSystem = FindObjectOfType<CounterStrikeSystem>();

        // 初始化所有系统
        if (playerManager != null)
        {
            // PlayerManager会自动应用PersistentBattleData中的生命值加成
            // 移除了 iconManager 参数
            playerManager.Initialize(config);
        }

        if (enemyManager != null)
        {
            enemyManager.Initialize(config);
            // 应用跨关卡的敌人类型
            if (PersistentBattleData.Instance != null)
            {
                int enemyId = PersistentBattleData.Instance.GetNextEnemyId();
                switch (enemyId)
                {
                    case 1: // 第二个敌人
                        enemyManager.SetEnemyType(EnemyManager.EnemyType.Second);
                        break;
                    case 2: // 第三个敌人
                        enemyManager.SetEnemyType(EnemyManager.EnemyType.Third);
                        break;
                    case 0: // 默认敌人
                    default:
                        enemyManager.SetEnemyType(EnemyManager.EnemyType.Default);
                        break;
                }
            }
            // 重新初始化敌人属性（确保使用正确的配置）
            enemyManager.Initialize(config);
        }

        // 移除了 iconManager 参数
        if (effectManager != null) effectManager.Initialize();

        if (yinYangSystem != null) yinYangSystem.Initialize(config);
        if (uiManager != null) uiManager.Initialize();
        if (wheelSystem != null) wheelSystem.Initialize(config.maxPoints);
        if (wheelController != null) wheelController.Initialize(config.maxPoints);
        if (retainedPointsSystem != null) retainedPointsSystem.Initialize(playerManager, wheelSystem);
        if (audioManager != null) audioManager.Initialize();
        if (animationManager != null) animationManager.Initialize();

        // 初始化CounterStrikeSystem，移除了 iconManager 参数
        if (counterStrikeSystem != null)
        {
            // 注意：CounterStrikeSystem.Initialize 签名也需要修改
            counterStrikeSystem.Initialize(playerManager, enemyManager, effectManager);
        }

        if (uiManager != null && playerManager != null && enemyManager != null)
        {
            uiManager.UpdateRoundDisplay(currentRound);
            uiManager.UpdatePlayerStatus(playerManager.Health, playerManager.MaxHealth, playerManager.Attack, playerManager.Defense);
            uiManager.UpdateEnemyStatus(enemyManager.Health, enemyManager.MaxHealth);
        }

        // 播放背景音乐
        if (config.backgroundMusic != null && audioManager != null)
        {
            audioManager.PlayBackgroundMusic(config.backgroundMusic);
        }

        // ========== 关键修改：在初始化时预测并显示第一回合的敌人意图 ==========
        if (enemyManager != null && effectManager != null && playerManager != null)
        {
            // 预测敌人在第一回合的行动意图 (currentRound 初始为 1)
            EnemyManager.EnemyAction firstAction = enemyManager.ChooseAction(currentRound, playerManager.DamageTakenLastTurn); // 玩家初始 DamageTakenLastTurn 为 0
            // 调用 effectManager 的方法来显示意图
            effectManager.ShowEnemyIntent(firstAction);
            Debug.Log($"[Enemy Intent Initialized] For round {currentRound}, enemy will: {firstAction.type}");
        }
        // ========== 修改结束 ==========

        StartPlayerSetup();
        isBattleInitialized = true;
    }

    // 新增：重新开始战斗方法（修复BGM重叠问题）
    public void RestartBattle()
    {
        Debug.Log("Restarting battle...");

        // 重置战斗状态
        currentState = BattleState.PlayerSetup;
        currentRound = 1;
        isBattleInitialized = false;

        // 重置所有系统
        if (playerManager != null)
        {
            // PlayerManager会自动应用PersistentBattleData中的生命值加成
            // 移除了 iconManager 参数
            playerManager.Initialize(config);
        }

        if (enemyManager != null)
        {
            enemyManager.Initialize(config);
            // 应用跨关卡的敌人类型
            if (PersistentBattleData.Instance != null)
            {
                int enemyId = PersistentBattleData.Instance.GetNextEnemyId();
                switch (enemyId)
                {
                    case 1: // 第二个敌人
                        enemyManager.SetEnemyType(EnemyManager.EnemyType.Second);
                        break;
                    case 2: // 第三个敌人
                        enemyManager.SetEnemyType(EnemyManager.EnemyType.Third);
                        break;
                    case 0: // 默认敌人
                    default:
                        enemyManager.SetEnemyType(EnemyManager.EnemyType.Default);
                        break;
                }
            }
            // 重新初始化敌人属性（确保使用正确的配置）
            enemyManager.Initialize(config);
        }

        if (wheelSystem != null)
        {
            wheelSystem.ResetBaseMaxPoints();
            wheelSystem.ResetRetainedPoints();
        }

        if (retainedPointsSystem != null)
        {
            retainedPointsSystem.ResetCalculatedRetainedPoints();
        }

        if (effectManager != null)
        {
            // 清除所有效果
        }

        // 简化BGM处理：重新播放BGM
        if (audioManager != null)
        {
            if (config.backgroundMusic != null)
            {
                audioManager.PlayBackgroundMusic(config.backgroundMusic);
            }
        }

        // 更新UI
        if (uiManager != null)
        {
            uiManager.UpdateRoundDisplay(currentRound);
            if (playerManager != null)
            {
                uiManager.UpdatePlayerStatus(playerManager.Health, playerManager.MaxHealth, playerManager.Attack, playerManager.Defense);
            }
            if (enemyManager != null)
            {
                uiManager.UpdateEnemyStatus(enemyManager.Health, enemyManager.MaxHealth);
            }
            uiManager.ClearBattleLog();
        }

        // ========== 关键修改：在重新开始时预测并显示第一回合的敌人意图 ==========
        if (enemyManager != null && effectManager != null && playerManager != null)
        {
            // 预测敌人在第一回合的行动意图 (currentRound 初始为 1)
            EnemyManager.EnemyAction firstAction = enemyManager.ChooseAction(currentRound, playerManager.DamageTakenLastTurn); // 玩家初始 DamageTakenLastTurn 为 0
            // 调用 effectManager 的方法来显示意图
            effectManager.ShowEnemyIntent(firstAction);
            Debug.Log($"[Enemy Intent Restarted] For round {currentRound}, enemy will: {firstAction.type}");
        }
        // ========== 修改结束 ==========

        // 重新初始化战斗
        StartPlayerSetup();
    }

    public void StartPlayerSetup()
    {
        currentState = BattleState.PlayerSetup;
        Debug.Log("Player Setup Phase");

        // 重置轮盘保留点数（每回合开始时）
        if (wheelSystem != null)
        {
            wheelSystem.ResetRetainedPoints();
        }

        // 应用保留点数（在回合开始时应用上一回合计算的保留点数）
        if (retainedPointsSystem != null)
        {
            retainedPointsSystem.ApplyRetainedPointsToWheel();
        }

        // 重置玩家攻击防御属性
        if (playerManager != null)
        {
            playerManager.ResetForNewTurn(); // 这会重置 DamageTakenLastTurn
        }

        // 显示轮盘和UI
        if (wheelSystem != null)
        {
            wheelSystem.ShowWheelUI();
            wheelSystem.ResetPoints();
        }

        if (wheelController != null)
        {
            wheelController.ResetSliders();
            // 更新滑块最大值以反映当前的保留点数
            wheelController.UpdateSliderMaxValues();
        }

        // ========== 关键修改：在每回合开始时预测并显示下一回合的敌人意图 ==========
        // 注意：这里预测的是 currentRound 回合的意图，因为 currentRound 在敌人回合结束后才递增
        if (enemyManager != null && effectManager != null && playerManager != null)
        {
            // 预测敌人在下一回合（currentRound）的行动意图
            EnemyManager.EnemyAction nextAction = enemyManager.ChooseAction(currentRound, playerManager.DamageTakenLastTurn);
            // 调用 effectManager 的方法来显示意图
            effectManager.ShowEnemyIntent(nextAction);
            Debug.Log($"[Enemy Intent Updated] For upcoming round ({currentRound}), enemy will: {nextAction.type}");
        }
        // ========== 修改结束 ==========

        if (uiManager != null)
        {
            uiManager.UpdateBattleLog("Choose your Yin-Yang points for the round.");
            uiManager.ShowYinYangSetup();
            // 显示玩家回合提示（使用配置中的持续时间）
            uiManager.ShowTurnIndicator("Player Turn", config.turnIndicatorDuration);
        }
    }

    private IEnumerator StartEnemyTurnAfterDelay(float delay)
    {
        Debug.Log($"Starting enemy turn after {delay} seconds delay");
        yield return new WaitForSeconds(delay);
        StartCoroutine(StartEnemyTurn());
    }

    public IEnumerator StartEnemyTurn()
    {
        Debug.Log("===== ENEMY TURN STARTED =====");
        currentState = BattleState.EnemyTurn;
        Debug.Log("Enemy Turn");

        // 显示敌人回合提示（使用配置中的持续时间）
        if (uiManager != null)
        {
            uiManager.ShowTurnIndicator("Enemy Turn", config.turnIndicatorDuration);
        }

        // 等待提示显示完毕后再开始敌人行动
        yield return new WaitForSeconds(config.turnIndicatorDuration);

        // 处理DOT和Debuff效果
        if (effectManager != null)
        {
            Debug.Log("Processing all effects");
            effectManager.ProcessAllEffects();
        }

        // 检查玩家是否死亡
        if (playerManager != null && playerManager.Health <= 0)
        {
            Debug.Log("Player died during effect processing");
            EndBattle(false);
            yield break;
        }

        // 敌人回合逻辑
        if (enemyManager != null && playerManager != null)
        {
            Debug.Log("Enemy choosing action");
            EnemyManager.EnemyAction action = enemyManager.ChooseAction(currentRound, playerManager.DamageTakenLastTurn);

            switch (action.type)
            {
                case EnemyManager.EnemyAction.Type.Attack:
                    Debug.Log("Enemy chose to attack");
                    yield return StartCoroutine(HandleEnemyAttackSequence());
                    break;
                case EnemyManager.EnemyAction.Type.Defend:
                    Debug.Log("Enemy chose to defend");
                    yield return StartCoroutine(HandleEnemyDefendSequence());
                    break;
                case EnemyManager.EnemyAction.Type.Charge:
                    Debug.Log("Enemy chose to charge");
                    yield return StartCoroutine(HandleEnemyChargeSequence());
                    break;
            }
            // ========== 新增：执行动作后更新状态计数器 ==========
            enemyManager.ExecuteAction(action.type);
            // ========== 新增结束 ==========
        }

        // 处理反震
        if (counterStrikeSystem != null)
        {
            Debug.Log("Handling counter strike");
            counterStrikeSystem.HandleCounterStrike();
        }

        if (animationManager != null)
        {
            Debug.Log("Playing counter strike effect");
            animationManager.PlayCounterStrikeEffect();
        }

        if (playerManager != null && playerManager.Health <= 0)
        {
            Debug.Log("Player died after enemy actions");
            EndBattle(false);
            yield break;
        }

        currentRound++;
        Debug.Log($"Round incremented to {currentRound}");

        if (uiManager != null)
        {
            uiManager.UpdateRoundDisplay(currentRound);
        }

        Debug.Log("Starting player setup after delay");
        // 显示玩家回合提示
        if (uiManager != null)
        {
            uiManager.ShowTurnIndicator("Player Turn", config.turnIndicatorDuration);
        }
        // 等待提示显示完毕
        yield return new WaitForSeconds(config.turnIndicatorDuration);

        // ========== 移除：之前在StartPlayerSetup之前更新意图的代码 ==========
        // 因为意图更新逻辑已移到StartPlayerSetup内部，这里不再需要。
        // StartPlayerSetup 会在被调用时自动更新意图。
        // ========== 移除结束 ==========

        StartPlayerSetup();
    }

    private void EndBattle(bool playerWins)
    {
        Debug.Log($"Battle ended. Player wins: {playerWins}");
        currentState = BattleState.BattleEnd;

        // 如果玩家胜利，准备下一关的数据
        if (playerWins)
        {
            // 确保PersistentBattleData存在
            if (PersistentBattleData.Instance == null)
            {
                GameObject persistentObj = new GameObject("PersistentBattleData");
                persistentObj.AddComponent<PersistentBattleData>();
                DontDestroyOnLoad(persistentObj);
            }

            // 设置下一关的敌人类型（这里可以根据当前敌人类型来决定下一关敌人）
            if (enemyManager != null)
            {
                switch (enemyManager.GetCurrentEnemyType())
                {
                    case EnemyManager.EnemyType.Default:
                        PersistentBattleData.Instance.SetNextEnemyId(1); // 下一关是第二个敌人
                        break;
                    case EnemyManager.EnemyType.Second:
                        PersistentBattleData.Instance.SetNextEnemyId(2); // 下一关是第三个敌人
                        break;
                    case EnemyManager.EnemyType.Third:
                        PersistentBattleData.Instance.SetNextEnemyId(0); // 下一关回到默认敌人
                        break;
                }
            }
        }

        // 调用新方法来显示结束面板
        if (uiManager != null)
        {
            if (playerWins)
            {
                uiManager.ShowWinPanel();
            }
            else
            {
                uiManager.ShowLosePanel();
            }
        }

        // 播放结束音效
        if (audioManager != null)
        {
            audioManager.StopBackgroundMusic();

            if (playerWins && config.victorySound != null)
            {
                Debug.Log("Playing victory sound");
                audioManager.PlaySoundEffect(config.victorySound);
            }
            else if (!playerWins && config.defeatSound != null)
            {
                Debug.Log("Playing defeat sound");
                audioManager.PlaySoundEffect(config.defeatSound);
            }
        }

        // 重置轮盘
        if (wheelSystem != null)
        {
            wheelSystem.ResetRetainedPoints();
            wheelSystem.ResetBaseMaxPoints(); // 战斗结束时重置基础点数
        }
    }

    /// <summary>
    /// 计算阴阳点数差值
    /// </summary>
    /// <param name="yangPoints">阳点数</param>
    /// <param name="yinPoints">阴点数</param>
    /// <returns>阳点数 - 阴点数</returns>
    private float CalculateYangYinDifference(float yangPoints, float yinPoints)
    {
        return yangPoints - yinPoints;
    }

    /// <summary>
    /// 检查是否处于极端阳状态范围 (5 <= diff <= 7)
    /// </summary>
    private bool IsInExtremeYangRange(float diff)
    {
        return diff >= 5f && diff <= 7f;
    }

    /// <summary>
    /// 检查是否处于极端阴状态范围 (-7 <= diff <= -5)
    /// </summary>
    private bool IsInExtremeYinRange(float diff)
    {
        return diff <= -5f && diff >= -7f;
    }

    public void OnEndButtonClick()
    {
        Debug.Log("End button clicked");

        if (currentState != BattleState.PlayerSetup)
        {
            Debug.LogWarning("End button clicked but not in PlayerSetup state");
            return;
        }

        if (wheelSystem == null)
        {
            Debug.LogError("WheelSystem is null");
            return;
        }

        float yangPoints = wheelSystem.CurrentYangPoints;
        float yinPoints = wheelSystem.CurrentYinPoints;
        float diff = CalculateYangYinDifference(yangPoints, yinPoints);
        Debug.Log($"Yang points: {yangPoints}, Yin points: {yinPoints}, Diff: {diff}");

        if (wheelSystem != null)
        {
            wheelSystem.HideWheelUI();
        }

        currentState = BattleState.PlayerTurn;
        Debug.Log("State changed to PlayerTurn");

        // 新增：在应用效果前进行极端状态前置条件检查
        if (yinYangSystem != null)
        {
            // 检查是否处于极端阳范围但前置条件不满足
            if (IsInExtremeYangRange(diff) && yinYangSystem.GetCriticalYangTriggerCount() < 3)
            {
                Debug.Log("Attempted Extreme Yang but insufficient Critical Yang uses.");
                // 显示UI提示面板，阻止后续逻辑
                if (uiManager != null)
                {
                    uiManager.ShowStackInsufficientPanel($"Cannot use Extreme Yang yet! Requires 3 Critical Yang uses. Current: {yinYangSystem.GetCriticalYangTriggerCount()}/3. Please adjust your points.");
                }
                // 重置状态，因为操作被取消
                currentState = BattleState.PlayerSetup;
                // 重新显示轮盘UI
                if (wheelSystem != null)
                {
                    wheelSystem.ShowWheelUI();
                }
                // 重新显示点数设置UI
                if (uiManager != null)
                {
                    uiManager.ShowYinYangSetup();
                }
                return; // 阻止后续所有逻辑执行
            }
            // 检查是否处于极端阴范围但前置条件不满足
            else if (IsInExtremeYinRange(diff) && yinYangSystem.GetCriticalYinTriggerCount() < 3)
            {
                Debug.Log("Attempted Extreme Yin but insufficient Critical Yin uses.");
                // 显示UI提示面板，阻止后续逻辑
                if (uiManager != null)
                {
                    // 修复：调用正确的UI方法和YinYangSystem方法
                    uiManager.ShowStackInsufficientPanel($"Cannot use Extreme Yin yet! Requires 3 Critical Yin uses. Current: {yinYangSystem.GetCriticalYinTriggerCount()}/3. Please adjust your points.");
                }
                // 重置状态，因为操作被取消
                currentState = BattleState.PlayerSetup;
                // 重新显示轮盘UI
                if (wheelSystem != null)
                {
                    wheelSystem.ShowWheelUI();
                }
                // 重新显示点数设置UI
                if (uiManager != null)
                {
                    uiManager.ShowYinYangSetup();
                }
                return; // 阻止后续所有逻辑执行
            }
        }

        // 应用阴阳点数效果 (只有在检查通过后才会执行)
        if (yinYangSystem != null)
        {
            Debug.Log("Applying yin-yang effects");
            yinYangSystem.ApplyEffects(yangPoints, yinPoints);
        }

        // 更新UI
        if (uiManager != null && playerManager != null)
        {
            uiManager.UpdatePlayerAttackDefense(playerManager.Attack, playerManager.Defense);
        }

        // 计算当前回合的保留点数（在玩家回合结束时）
        if (retainedPointsSystem != null)
        {
            Debug.Log("Calculating current retained points");
            retainedPointsSystem.CalculateCurrentRetainedPoints();
        }

        // 启动玩家攻击序列协程
        StartCoroutine(HandlePlayerAttackSequence());

        // 延迟开始敌人回合
        Debug.Log("Starting enemy turn after delay");
        StartCoroutine(StartEnemyTurnAfterDelay(config.enemyTurnDelay)); // 从配置读取延迟时间
    }

    private IEnumerator HandlePlayerAttackSequence()
    {
        Coroutine playerAttackSoundCoroutine = null;
        Coroutine enemyHitSoundCoroutine = null;

        // 1. 播放玩家攻击动画
        if (animationManager != null)
        {
            Debug.Log("Playing player attack animation");
            animationManager.PlayPlayerAttack();

            // 在动画开始后立即启动音效延迟协程
            playerAttackSoundCoroutine = StartCoroutine(PlayPlayerAttackSoundAfterDelay());

            // 等待动画完成
            while (animationManager.IsPlayerAnimating())
            {
                yield return null;
            }
        }

        // 等待音效协程完成（如果它还没完成的话）
        if (playerAttackSoundCoroutine != null)
        {
            yield return playerAttackSoundCoroutine;
        }

        // 3. 计算并应用玩家伤害
        if (playerManager != null && enemyManager != null)
        {
            float damage = playerManager.CalculateDamage(enemyManager.CurrentDefense);
            Debug.Log($"Player calculated damage: {damage}");

            // 敌人受到伤害
            if (enemyManager != null)
            {
                Debug.Log("Enemy taking damage");
                enemyManager.TakeDamage(damage);
            }

            // 更新敌人状态UI
            if (uiManager != null)
            {
                uiManager.UpdateEnemyStatus(enemyManager.Health, enemyManager.MaxHealth);
            }

            // 检查敌人是否死亡
            if (enemyManager.Health <= 0)
            {
                Debug.Log("Enemy died. Player wins!");
                EndBattle(true);
                yield break;
            }
        }

        // 4. 等待配置的时间后播放敌人受击动画
        yield return new WaitForSeconds(config.playerToEnemyHitDelay);
        if (animationManager != null)
        {
            Debug.Log("Playing enemy hit animation");
            animationManager.PlayEnemyHit();

            // 在动画开始后立即启动音效延迟协程
            enemyHitSoundCoroutine = StartCoroutine(PlayEnemyHitSoundAfterDelay());

            // 等待动画完成
            while (animationManager.IsEnemyAnimating())
            {
                yield return null;
            }
        }

        // 等待音效协程完成（如果它还没完成的话）
        if (enemyHitSoundCoroutine != null)
        {
            yield return enemyHitSoundCoroutine;
        }

        // 6. 计算保留点数
        if (retainedPointsSystem != null)
        {
            Debug.Log("Calculating retained points");
            retainedPointsSystem.CalculateCurrentRetainedPoints();
        }
    }

    private IEnumerator HandleEnemyAttackSequence()
    {
        Coroutine enemyAttackSoundCoroutine = null;
        Coroutine playerHitSoundCoroutine = null;

        // 1. 播放敌人攻击动画
        if (animationManager != null)
        {
            Debug.Log("Playing enemy attack animation");
            animationManager.PlayEnemyAttack();

            // 在动画开始后立即启动音效延迟协程
            enemyAttackSoundCoroutine = StartCoroutine(PlayEnemyAttackSoundAfterDelay());

            // 等待动画完成
            while (animationManager.IsEnemyAnimating())
            {
                yield return null;
            }
        }

        // 等待音效协程完成（如果它还没完成的话）
        if (enemyAttackSoundCoroutine != null)
        {
            yield return enemyAttackSoundCoroutine;
        }

        // 3. 等待配置的时间后播放玩家受击动画
        yield return new WaitForSeconds(config.enemyToPlayerHitDelay);
        if (animationManager != null)
        {
            Debug.Log("Playing player hit animation");
            animationManager.PlayPlayerHit();

            // 在动画开始后立即启动音效延迟协程
            playerHitSoundCoroutine = StartCoroutine(PlayPlayerHitSoundAfterDelay());

            // 等待动画完成
            while (animationManager.IsPlayerAnimating())
            {
                yield return null;
            }
        }

        // 等待音效协程完成（如果它还没完成的话）
        if (playerHitSoundCoroutine != null)
        {
            yield return playerHitSoundCoroutine;
        }

        // 5. 计算并应用敌人伤害
        if (playerManager != null && enemyManager != null)
        {
            float damage = enemyManager.CalculateDamage(playerManager.Defense);
            playerManager.TakeDamage(damage);
            uiManager.UpdatePlayerStatus(playerManager.Health, playerManager.MaxHealth, playerManager.Attack, playerManager.Defense);
            if (playerManager.Health <= 0)
            {
                EndBattle(false);
                yield break;
            }
        }
    }

    private IEnumerator HandleEnemyDefendSequence()
    {
        Coroutine enemySkillSoundCoroutine = null;

        // 播放敌人技能动画（防御）
        if (animationManager != null)
        {
            Debug.Log("Playing enemy defend skill animation");
            animationManager.PlayEnemySkill();

            // 在动画开始后立即启动音效延迟协程
            enemySkillSoundCoroutine = StartCoroutine(PlayEnemySkillSoundAfterDelay());

            // 等待动画完成
            while (animationManager.IsEnemyAnimating())
            {
                yield return null;
            }
        }

        // 等待音效协程完成（如果它还没完成的话）
        if (enemySkillSoundCoroutine != null)
        {
            yield return enemySkillSoundCoroutine;
        }

        if (enemyManager != null)
        {
            Debug.Log("Enemy enabling defense");
            enemyManager.EnableDefense();
        }

        if (uiManager != null)
        {
            uiManager.UpdateBattleLog("Enemy defends, reducing incoming damage.");
        }

        yield return null;
    }

    private IEnumerator HandleEnemyChargeSequence()
    {
        Coroutine enemySkillSoundCoroutine = null;

        // 播放敌人技能动画（蓄力）
        if (animationManager != null)
        {
            Debug.Log("Playing enemy charge skill animation");
            animationManager.PlayEnemySkill();

            // 在动画开始后立即启动音效延迟协程
            enemySkillSoundCoroutine = StartCoroutine(PlayEnemySkillSoundAfterDelay());

            // 等待动画完成
            while (animationManager.IsEnemyAnimating())
            {
                yield return null;
            }
        }

        // 等待音效协程完成（如果它还没完成的话）
        if (enemySkillSoundCoroutine != null)
        {
            yield return enemySkillSoundCoroutine;
        }

        if (enemyManager != null)
        {
            Debug.Log("Enemy charging attack");
            enemyManager.ChargeAttack();
        }

        if (uiManager != null)
        {
            uiManager.UpdateBattleLog("Enemy charges, permanently increasing its attack!");
        }

        yield return null;
    }

    // 新增：用于在延迟后播放玩家攻击音效的辅助协程
    private IEnumerator PlayPlayerAttackSoundAfterDelay()
    {
        yield return new WaitForSeconds(config.playerAttackSoundDelay);
        if (config.playerAttackSound != null && audioManager != null)
        {
            Debug.Log("Playing player attack sound after delay");
            audioManager.PlaySoundEffect(config.playerAttackSound);
        }
    }

    // 新增：用于在延迟后播放玩家受击音效的辅助协程
    private IEnumerator PlayPlayerHitSoundAfterDelay()
    {
        yield return new WaitForSeconds(config.playerHitSoundDelay);
        if (config.playerHitSound != null && audioManager != null)
        {
            Debug.Log("Playing player hit sound after delay");
            audioManager.PlaySoundEffect(config.playerHitSound);
        }
    }

    // 新增：用于在延迟后播放敌人攻击音效的辅助协程
    private IEnumerator PlayEnemyAttackSoundAfterDelay()
    {
        yield return new WaitForSeconds(config.enemyAttackSoundDelay);
        if (config.enemyAttackSound != null && audioManager != null)
        {
            Debug.Log("Playing enemy attack sound after delay");
            audioManager.PlaySoundEffect(config.enemyAttackSound);
        }
    }

    // 新增：用于在延迟后播放敌人受击音效的辅助协程
    private IEnumerator PlayEnemyHitSoundAfterDelay()
    {
        yield return new WaitForSeconds(config.enemyHitSoundDelay);
        if (config.enemyHitSound != null && audioManager != null)
        {
            Debug.Log("Playing enemy hit sound after delay");
            audioManager.PlaySoundEffect(config.enemyHitSound);
        }
    }

    // 新增：用于在延迟后播放敌人技能音效的辅助协程
    private IEnumerator PlayEnemySkillSoundAfterDelay()
    {
        yield return new WaitForSeconds(config.enemySkillSoundDelay);
        if (config.enemySkillSound != null && audioManager != null)
        {
            Debug.Log("Playing enemy skill sound after delay");
            audioManager.PlaySoundEffect(config.enemySkillSound);
        }
    }

    void Update()
    {
        // Debugging section is removed
    }
}