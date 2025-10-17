using UnityEngine;
using System.Collections;

public class BattleSystem : MonoBehaviour
{
    // 单例模式，确保跨关卡不销毁
    public static BattleSystem Instance { get; private set; }

    [Header("Core Systems")]
    public PlayerManager playerManager; // 玩家管理器，保持引用
    public EnemyManager enemyManager;
    public YinYangSystem yinYangSystem;
    public UIManager uiManager;
    public EffectManager effectManager;
    public WheelSystem wheelSystem;
    public WheelController wheelController;
    public IconManager iconManager;
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
            // 确保这个游戏对象在加载新场景时不会被销毁
            DontDestroyOnLoad(gameObject);
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
        if (iconManager == null) iconManager = FindObjectOfType<IconManager>();
        if (retainedPointsSystem == null) retainedPointsSystem = FindObjectOfType<RetainedPointsSystem>();
        if (audioManager == null) audioManager = FindObjectOfType<AudioManager>();
        if (animationManager == null) animationManager = FindObjectOfType<AnimationManager>();
        if (counterStrikeSystem == null) counterStrikeSystem = FindObjectOfType<CounterStrikeSystem>();

        // 初始化所有系统
        if (playerManager != null) playerManager.Initialize(config, iconManager);
        if (enemyManager != null) enemyManager.Initialize(config);
        if (effectManager != null) effectManager.Initialize(iconManager);
        if (yinYangSystem != null) yinYangSystem.Initialize(config);
        if (uiManager != null) uiManager.Initialize();
        if (wheelSystem != null) wheelSystem.Initialize(config.maxPoints);
        if (wheelController != null) wheelController.Initialize(config.maxPoints);
        if (retainedPointsSystem != null) retainedPointsSystem.Initialize(playerManager, wheelSystem);
        if (audioManager != null) audioManager.Initialize();
        if (animationManager != null) animationManager.Initialize();

        // 初始化CounterStrikeSystem
        if (counterStrikeSystem != null)
        {
            counterStrikeSystem.Initialize(playerManager, enemyManager, effectManager, iconManager);
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

        StartPlayerSetup();
        isBattleInitialized = true;
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
            playerManager.ResetForNewTurn();
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

        // 显示敌人意图
        if (enemyManager != null && effectManager != null && playerManager != null)
        {
            EnemyManager.EnemyAction nextAction = enemyManager.ChooseAction(currentRound, playerManager.DamageTakenLastTurn);
            effectManager.ShowEnemyIntent(nextAction);
        }

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
        StartCoroutine(StartPlayerSetupAfterDelay(1.5f));
    }

    private IEnumerator StartPlayerSetupAfterDelay(float delay)
    {
        Debug.Log($"Starting player setup after {delay} seconds delay");
        yield return new WaitForSeconds(delay);

        // 敌人回合结束后，显示玩家回合提示（使用配置中的持续时间）
        if (uiManager != null)
        {
            uiManager.ShowTurnIndicator("Player Turn", config.turnIndicatorDuration);
        }

        yield return new WaitForSeconds(config.turnIndicatorDuration); // 等待提示显示完毕

        StartPlayerSetup();
    }

    private void EndBattle(bool playerWins)
    {
        Debug.Log($"Battle ended. Player wins: {playerWins}");
        currentState = BattleState.BattleEnd;

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

    // 修改：添加叠层检查逻辑
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
        Debug.Log($"Yang points: {yangPoints}, Yin points: {yinPoints}");

        // 检查是否尝试使用极端技能但叠层不足
        float diff = yangPoints - yinPoints;
        float absDiff = Mathf.Abs(diff);

        // 检查极端阳状态（5-7点差值）
        if (diff >= 5f && diff <= 7f)
        {
            // 极端阳需要至少3层阳穿透叠层
            int yangStacks = effectManager.GetYangPenetrationStacks();
            if (yangStacks < 3)
            {
                // 显示叠层不足提示
                if (uiManager != null)
                {
                    uiManager.ShowStackInsufficientPanel("Insufficient Yang Penetration stacks!\nNeed at least 3 stacks to use Extreme Yang.");
                    uiManager.UpdateBattleLog("Cannot use Extreme Yang: Insufficient Yang Penetration stacks (need 3)!");
                }
                return; // 不继续执行
            }
        }
        // 检查极端阴状态（-5到-7点差值）
        else if (diff <= -5f && diff >= -7f)
        {
            // 极端阴需要至少3层阴覆盖叠层
            int yinStacks = effectManager.GetYinCoverStacks();
            if (yinStacks < 3)
            {
                // 显示叠层不足提示
                if (uiManager != null)
                {
                    uiManager.ShowStackInsufficientPanel("Insufficient Yin Cover stacks!\nNeed at least 3 stacks to use Extreme Yin.");
                    uiManager.UpdateBattleLog("Cannot use Extreme Yin: Insufficient Yin Cover stacks (need 3)!");
                }
                return; // 不继续执行
            }
        }

        if (wheelSystem != null)
        {
            wheelSystem.HideWheelUI();
        }

        currentState = BattleState.PlayerTurn;
        Debug.Log("State changed to PlayerTurn");

        // 应用阴阳点数效果
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
        StartCoroutine(StartEnemyTurnAfterDelay(1.5f));
    }

    private IEnumerator HandlePlayerAttackSequence()
    {
        // 1. 播放玩家攻击动画
        if (animationManager != null)
        {
            Debug.Log("Playing player attack animation");
            animationManager.PlayPlayerAttack();
            // 等待动画完成
            while (animationManager.IsPlayerAnimating())
            {
                yield return null;
            }
        }

        // 2. 播放攻击音效
        yield return new WaitForSeconds(config.playerAttackSoundDelay);
        if (config.playerAttackSound != null && audioManager != null)
        {
            Debug.Log("Playing player attack sound");
            audioManager.PlaySoundEffect(config.playerAttackSound);
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

        // 4. 播放敌人受击动画
        yield return new WaitForSeconds(config.enemyHitSoundDelay);
        if (animationManager != null)
        {
            Debug.Log("Playing enemy hit animation");
            animationManager.PlayEnemyHit();
            // 等待动画完成
            while (animationManager.IsEnemyAnimating())
            {
                yield return null;
            }
        }

        // 5. 播放敌人受击音效
        if (config.enemyHitSound != null && audioManager != null)
        {
            Debug.Log("Playing enemy hit sound");
            audioManager.PlaySoundEffect(config.enemyHitSound);
        }
    }

    private IEnumerator HandleEnemyAttackSequence()
    {
        // 1. 播放敌人攻击动画
        if (animationManager != null)
        {
            Debug.Log("Playing enemy attack animation");
            animationManager.PlayEnemyAttack();
            // 等待动画完成
            while (animationManager.IsEnemyAnimating())
            {
                yield return null;
            }
        }

        // 2. 播放敌人攻击音效
        yield return new WaitForSeconds(config.enemyAttackSoundDelay);
        if (config.enemyAttackSound != null && audioManager != null)
        {
            Debug.Log("Playing enemy attack sound");
            audioManager.PlaySoundEffect(config.enemyAttackSound);
        }

        // 3. 播放玩家受击动画
        yield return new WaitForSeconds(config.playerHitSoundDelay);
        if (animationManager != null)
        {
            Debug.Log("Playing player hit animation");
            animationManager.PlayPlayerHit();
            // 等待动画完成
            while (animationManager.IsPlayerAnimating())
            {
                yield return null;
            }
        }

        // 4. 播放玩家受击音效
        if (config.playerHitSound != null && audioManager != null)
        {
            Debug.Log("Playing player hit sound");
            audioManager.PlaySoundEffect(config.playerHitSound);
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
        // 播放敌人技能动画（防御）
        if (animationManager != null)
        {
            Debug.Log("Playing enemy defend skill animation");
            animationManager.PlayEnemySkill();
            // 等待动画完成
            while (animationManager.IsEnemyAnimating())
            {
                yield return null;
            }
        }

        // 播放敌人技能音效
        yield return new WaitForSeconds(config.enemySkillSoundDelay);
        if (config.enemySkillSound != null && audioManager != null)
        {
            Debug.Log("Playing enemy skill sound");
            audioManager.PlaySoundEffect(config.enemySkillSound);
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
        // 播放敌人技能动画（蓄力）
        if (animationManager != null)
        {
            Debug.Log("Playing enemy charge skill animation");
            animationManager.PlayEnemySkill();
            // 等待动画完成
            while (animationManager.IsEnemyAnimating())
            {
                yield return null;
            }
        }

        // 播放敌人技能音效
        yield return new WaitForSeconds(config.enemySkillSoundDelay);
        if (config.enemySkillSound != null && audioManager != null)
        {
            Debug.Log("Playing enemy skill sound");
            audioManager.PlaySoundEffect(config.enemySkillSound);
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

    void Update()
    {
        // Debugging section is removed
    }

    // 新增：重新开始战斗方法（简化BGM控制）
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
            playerManager.ResetPlayerState(config, iconManager);
        }

        if (enemyManager != null)
        {
            enemyManager.ResetEnemyState(config);
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
            uiManager.HideStackInsufficientPanel(); // 隐藏叠层不足提示面板
        }

        // 重新初始化战斗
        StartPlayerSetup();
    }
}