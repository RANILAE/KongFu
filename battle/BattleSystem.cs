using UnityEngine;
using System.Collections;
using System; // ������ using ���

public class BattleSystem : MonoBehaviour
{
    // ����ģʽ���Ƴ��糡�������٣�
    public static BattleSystem Instance { get; private set; }

    [Header("Core Systems")]
    public PlayerManager playerManager; // ��ҹ���������������
    public EnemyManager enemyManager;
    public YinYangSystem yinYangSystem;
    public UIManager uiManager;
    public EffectManager effectManager;
    public WheelSystem wheelSystem;
    public WheelController wheelController;
    // �Ƴ��� public IconManager iconManager;
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
            // �Ƴ��� DontDestroyOnLoad���������л�����ʱ���Զ����ٸö���
        }
        else
        {
            // ����Ѿ�����һ��ʵ���������ٵ�ǰ���
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

        // ��Start�е��ó�ʼ����ȷ������������Ѽ���
        InitializeBattle();
    }

    public void InitializeBattle()
    {
        if (isBattleInitialized) return;

        Debug.Log("Initializing battle");

        // ȷ�����к���������ѳ�ʼ��
        if (playerManager == null) playerManager = FindObjectOfType<PlayerManager>();
        if (enemyManager == null) enemyManager = FindObjectOfType<EnemyManager>();
        if (yinYangSystem == null) yinYangSystem = FindObjectOfType<YinYangSystem>();
        if (uiManager == null) uiManager = FindObjectOfType<UIManager>();
        if (effectManager == null) effectManager = FindObjectOfType<EffectManager>();
        if (wheelSystem == null) wheelSystem = FindObjectOfType<WheelSystem>();
        if (wheelController == null) wheelController = FindObjectOfType<WheelController>();
        // �Ƴ��� if (iconManager == null) iconManager = FindObjectOfType<IconManager>();
        if (retainedPointsSystem == null) retainedPointsSystem = FindObjectOfType<RetainedPointsSystem>();
        if (audioManager == null) audioManager = FindObjectOfType<AudioManager>();
        if (animationManager == null) animationManager = FindObjectOfType<AnimationManager>();
        if (counterStrikeSystem == null) counterStrikeSystem = FindObjectOfType<CounterStrikeSystem>();

        // ��ʼ������ϵͳ
        if (playerManager != null)
        {
            // PlayerManager���Զ�Ӧ��PersistentBattleData�е�����ֵ�ӳ�
            // �Ƴ��� iconManager ����
            playerManager.Initialize(config);
        }

        if (enemyManager != null)
        {
            enemyManager.Initialize(config);
            // Ӧ�ÿ�ؿ��ĵ�������
            if (PersistentBattleData.Instance != null)
            {
                int enemyId = PersistentBattleData.Instance.GetNextEnemyId();
                switch (enemyId)
                {
                    case 1: // �ڶ�������
                        enemyManager.SetEnemyType(EnemyManager.EnemyType.Second);
                        break;
                    case 2: // ����������
                        enemyManager.SetEnemyType(EnemyManager.EnemyType.Third);
                        break;
                    case 0: // Ĭ�ϵ���
                    default:
                        enemyManager.SetEnemyType(EnemyManager.EnemyType.Default);
                        break;
                }
            }
            // ���³�ʼ���������ԣ�ȷ��ʹ����ȷ�����ã�
            enemyManager.Initialize(config);
        }

        // �Ƴ��� iconManager ����
        if (effectManager != null) effectManager.Initialize();

        if (yinYangSystem != null) yinYangSystem.Initialize(config);
        if (uiManager != null) uiManager.Initialize();
        if (wheelSystem != null) wheelSystem.Initialize(config.maxPoints);
        if (wheelController != null) wheelController.Initialize(config.maxPoints);
        if (retainedPointsSystem != null) retainedPointsSystem.Initialize(playerManager, wheelSystem);
        if (audioManager != null) audioManager.Initialize();
        if (animationManager != null) animationManager.Initialize();

        // ��ʼ��CounterStrikeSystem���Ƴ��� iconManager ����
        if (counterStrikeSystem != null)
        {
            // ע�⣺CounterStrikeSystem.Initialize ǩ��Ҳ��Ҫ�޸�
            counterStrikeSystem.Initialize(playerManager, enemyManager, effectManager);
        }

        if (uiManager != null && playerManager != null && enemyManager != null)
        {
            uiManager.UpdateRoundDisplay(currentRound);
            uiManager.UpdatePlayerStatus(playerManager.Health, playerManager.MaxHealth, playerManager.Attack, playerManager.Defense);
            uiManager.UpdateEnemyStatus(enemyManager.Health, enemyManager.MaxHealth);
        }

        // ���ű�������
        if (config.backgroundMusic != null && audioManager != null)
        {
            audioManager.PlayBackgroundMusic(config.backgroundMusic);
        }

        // ========== �ؼ��޸ģ��ڳ�ʼ��ʱԤ�Ⲣ��ʾ��һ�غϵĵ�����ͼ ==========
        if (enemyManager != null && effectManager != null && playerManager != null)
        {
            // Ԥ������ڵ�һ�غϵ��ж���ͼ (currentRound ��ʼΪ 1)
            EnemyManager.EnemyAction firstAction = enemyManager.ChooseAction(currentRound, playerManager.DamageTakenLastTurn); // ��ҳ�ʼ DamageTakenLastTurn Ϊ 0
            // ���� effectManager �ķ�������ʾ��ͼ
            effectManager.ShowEnemyIntent(firstAction);
            Debug.Log($"[Enemy Intent Initialized] For round {currentRound}, enemy will: {firstAction.type}");
        }
        // ========== �޸Ľ��� ==========

        StartPlayerSetup();
        isBattleInitialized = true;
    }

    // ���������¿�ʼս���������޸�BGM�ص����⣩
    public void RestartBattle()
    {
        Debug.Log("Restarting battle...");

        // ����ս��״̬
        currentState = BattleState.PlayerSetup;
        currentRound = 1;
        isBattleInitialized = false;

        // ��������ϵͳ
        if (playerManager != null)
        {
            // PlayerManager���Զ�Ӧ��PersistentBattleData�е�����ֵ�ӳ�
            // �Ƴ��� iconManager ����
            playerManager.Initialize(config);
        }

        if (enemyManager != null)
        {
            enemyManager.Initialize(config);
            // Ӧ�ÿ�ؿ��ĵ�������
            if (PersistentBattleData.Instance != null)
            {
                int enemyId = PersistentBattleData.Instance.GetNextEnemyId();
                switch (enemyId)
                {
                    case 1: // �ڶ�������
                        enemyManager.SetEnemyType(EnemyManager.EnemyType.Second);
                        break;
                    case 2: // ����������
                        enemyManager.SetEnemyType(EnemyManager.EnemyType.Third);
                        break;
                    case 0: // Ĭ�ϵ���
                    default:
                        enemyManager.SetEnemyType(EnemyManager.EnemyType.Default);
                        break;
                }
            }
            // ���³�ʼ���������ԣ�ȷ��ʹ����ȷ�����ã�
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
            // �������Ч��
        }

        // ��BGM�������²���BGM
        if (audioManager != null)
        {
            if (config.backgroundMusic != null)
            {
                audioManager.PlayBackgroundMusic(config.backgroundMusic);
            }
        }

        // ����UI
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

        // ========== �ؼ��޸ģ������¿�ʼʱԤ�Ⲣ��ʾ��һ�غϵĵ�����ͼ ==========
        if (enemyManager != null && effectManager != null && playerManager != null)
        {
            // Ԥ������ڵ�һ�غϵ��ж���ͼ (currentRound ��ʼΪ 1)
            EnemyManager.EnemyAction firstAction = enemyManager.ChooseAction(currentRound, playerManager.DamageTakenLastTurn); // ��ҳ�ʼ DamageTakenLastTurn Ϊ 0
            // ���� effectManager �ķ�������ʾ��ͼ
            effectManager.ShowEnemyIntent(firstAction);
            Debug.Log($"[Enemy Intent Restarted] For round {currentRound}, enemy will: {firstAction.type}");
        }
        // ========== �޸Ľ��� ==========

        // ���³�ʼ��ս��
        StartPlayerSetup();
    }

    public void StartPlayerSetup()
    {
        currentState = BattleState.PlayerSetup;
        Debug.Log("Player Setup Phase");

        // �������̱���������ÿ�غϿ�ʼʱ��
        if (wheelSystem != null)
        {
            wheelSystem.ResetRetainedPoints();
        }

        // Ӧ�ñ����������ڻغϿ�ʼʱӦ����һ�غϼ���ı���������
        if (retainedPointsSystem != null)
        {
            retainedPointsSystem.ApplyRetainedPointsToWheel();
        }

        // ������ҹ�����������
        if (playerManager != null)
        {
            playerManager.ResetForNewTurn(); // ������� DamageTakenLastTurn
        }

        // ��ʾ���̺�UI
        if (wheelSystem != null)
        {
            wheelSystem.ShowWheelUI();
            wheelSystem.ResetPoints();
        }

        if (wheelController != null)
        {
            wheelController.ResetSliders();
            // ���»������ֵ�Է�ӳ��ǰ�ı�������
            wheelController.UpdateSliderMaxValues();
        }

        // ========== �ؼ��޸ģ���ÿ�غϿ�ʼʱԤ�Ⲣ��ʾ��һ�غϵĵ�����ͼ ==========
        // ע�⣺����Ԥ����� currentRound �غϵ���ͼ����Ϊ currentRound �ڵ��˻غϽ�����ŵ���
        if (enemyManager != null && effectManager != null && playerManager != null)
        {
            // Ԥ���������һ�غϣ�currentRound�����ж���ͼ
            EnemyManager.EnemyAction nextAction = enemyManager.ChooseAction(currentRound, playerManager.DamageTakenLastTurn);
            // ���� effectManager �ķ�������ʾ��ͼ
            effectManager.ShowEnemyIntent(nextAction);
            Debug.Log($"[Enemy Intent Updated] For upcoming round ({currentRound}), enemy will: {nextAction.type}");
        }
        // ========== �޸Ľ��� ==========

        if (uiManager != null)
        {
            uiManager.UpdateBattleLog("Choose your Yin-Yang points for the round.");
            uiManager.ShowYinYangSetup();
            // ��ʾ��һغ���ʾ��ʹ�������еĳ���ʱ�䣩
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

        // ��ʾ���˻غ���ʾ��ʹ�������еĳ���ʱ�䣩
        if (uiManager != null)
        {
            uiManager.ShowTurnIndicator("Enemy Turn", config.turnIndicatorDuration);
        }

        // �ȴ���ʾ��ʾ��Ϻ��ٿ�ʼ�����ж�
        yield return new WaitForSeconds(config.turnIndicatorDuration);

        // ����DOT��DebuffЧ��
        if (effectManager != null)
        {
            Debug.Log("Processing all effects");
            effectManager.ProcessAllEffects();
        }

        // �������Ƿ�����
        if (playerManager != null && playerManager.Health <= 0)
        {
            Debug.Log("Player died during effect processing");
            EndBattle(false);
            yield break;
        }

        // ���˻غ��߼�
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
            // ========== ������ִ�ж��������״̬������ ==========
            enemyManager.ExecuteAction(action.type);
            // ========== �������� ==========
        }

        // ������
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
        // ��ʾ��һغ���ʾ
        if (uiManager != null)
        {
            uiManager.ShowTurnIndicator("Player Turn", config.turnIndicatorDuration);
        }
        // �ȴ���ʾ��ʾ���
        yield return new WaitForSeconds(config.turnIndicatorDuration);

        // ========== �Ƴ���֮ǰ��StartPlayerSetup֮ǰ������ͼ�Ĵ��� ==========
        // ��Ϊ��ͼ�����߼����Ƶ�StartPlayerSetup�ڲ������ﲻ����Ҫ��
        // StartPlayerSetup ���ڱ�����ʱ�Զ�������ͼ��
        // ========== �Ƴ����� ==========

        StartPlayerSetup();
    }

    private void EndBattle(bool playerWins)
    {
        Debug.Log($"Battle ended. Player wins: {playerWins}");
        currentState = BattleState.BattleEnd;

        // ������ʤ����׼����һ�ص�����
        if (playerWins)
        {
            // ȷ��PersistentBattleData����
            if (PersistentBattleData.Instance == null)
            {
                GameObject persistentObj = new GameObject("PersistentBattleData");
                persistentObj.AddComponent<PersistentBattleData>();
                DontDestroyOnLoad(persistentObj);
            }

            // ������һ�صĵ������ͣ�������Ը��ݵ�ǰ����������������һ�ص��ˣ�
            if (enemyManager != null)
            {
                switch (enemyManager.GetCurrentEnemyType())
                {
                    case EnemyManager.EnemyType.Default:
                        PersistentBattleData.Instance.SetNextEnemyId(1); // ��һ���ǵڶ�������
                        break;
                    case EnemyManager.EnemyType.Second:
                        PersistentBattleData.Instance.SetNextEnemyId(2); // ��һ���ǵ���������
                        break;
                    case EnemyManager.EnemyType.Third:
                        PersistentBattleData.Instance.SetNextEnemyId(0); // ��һ�ػص�Ĭ�ϵ���
                        break;
                }
            }
        }

        // �����·�������ʾ�������
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

        // ���Ž�����Ч
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

        // ��������
        if (wheelSystem != null)
        {
            wheelSystem.ResetRetainedPoints();
            wheelSystem.ResetBaseMaxPoints(); // ս������ʱ���û�������
        }
    }

    /// <summary>
    /// ��������������ֵ
    /// </summary>
    /// <param name="yangPoints">������</param>
    /// <param name="yinPoints">������</param>
    /// <returns>������ - ������</returns>
    private float CalculateYangYinDifference(float yangPoints, float yinPoints)
    {
        return yangPoints - yinPoints;
    }

    /// <summary>
    /// ����Ƿ��ڼ�����״̬��Χ (5 <= diff <= 7)
    /// </summary>
    private bool IsInExtremeYangRange(float diff)
    {
        return diff >= 5f && diff <= 7f;
    }

    /// <summary>
    /// ����Ƿ��ڼ�����״̬��Χ (-7 <= diff <= -5)
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

        // ��������Ӧ��Ч��ǰ���м���״̬ǰ���������
        if (yinYangSystem != null)
        {
            // ����Ƿ��ڼ�������Χ��ǰ������������
            if (IsInExtremeYangRange(diff) && yinYangSystem.GetCriticalYangTriggerCount() < 3)
            {
                Debug.Log("Attempted Extreme Yang but insufficient Critical Yang uses.");
                // ��ʾUI��ʾ��壬��ֹ�����߼�
                if (uiManager != null)
                {
                    uiManager.ShowStackInsufficientPanel($"Cannot use Extreme Yang yet! Requires 3 Critical Yang uses. Current: {yinYangSystem.GetCriticalYangTriggerCount()}/3. Please adjust your points.");
                }
                // ����״̬����Ϊ������ȡ��
                currentState = BattleState.PlayerSetup;
                // ������ʾ����UI
                if (wheelSystem != null)
                {
                    wheelSystem.ShowWheelUI();
                }
                // ������ʾ��������UI
                if (uiManager != null)
                {
                    uiManager.ShowYinYangSetup();
                }
                return; // ��ֹ���������߼�ִ��
            }
            // ����Ƿ��ڼ�������Χ��ǰ������������
            else if (IsInExtremeYinRange(diff) && yinYangSystem.GetCriticalYinTriggerCount() < 3)
            {
                Debug.Log("Attempted Extreme Yin but insufficient Critical Yin uses.");
                // ��ʾUI��ʾ��壬��ֹ�����߼�
                if (uiManager != null)
                {
                    // �޸���������ȷ��UI������YinYangSystem����
                    uiManager.ShowStackInsufficientPanel($"Cannot use Extreme Yin yet! Requires 3 Critical Yin uses. Current: {yinYangSystem.GetCriticalYinTriggerCount()}/3. Please adjust your points.");
                }
                // ����״̬����Ϊ������ȡ��
                currentState = BattleState.PlayerSetup;
                // ������ʾ����UI
                if (wheelSystem != null)
                {
                    wheelSystem.ShowWheelUI();
                }
                // ������ʾ��������UI
                if (uiManager != null)
                {
                    uiManager.ShowYinYangSetup();
                }
                return; // ��ֹ���������߼�ִ��
            }
        }

        // Ӧ����������Ч�� (ֻ���ڼ��ͨ����Ż�ִ��)
        if (yinYangSystem != null)
        {
            Debug.Log("Applying yin-yang effects");
            yinYangSystem.ApplyEffects(yangPoints, yinPoints);
        }

        // ����UI
        if (uiManager != null && playerManager != null)
        {
            uiManager.UpdatePlayerAttackDefense(playerManager.Attack, playerManager.Defense);
        }

        // ���㵱ǰ�غϵı�������������һغϽ���ʱ��
        if (retainedPointsSystem != null)
        {
            Debug.Log("Calculating current retained points");
            retainedPointsSystem.CalculateCurrentRetainedPoints();
        }

        // ������ҹ�������Э��
        StartCoroutine(HandlePlayerAttackSequence());

        // �ӳٿ�ʼ���˻غ�
        Debug.Log("Starting enemy turn after delay");
        StartCoroutine(StartEnemyTurnAfterDelay(config.enemyTurnDelay)); // �����ö�ȡ�ӳ�ʱ��
    }

    private IEnumerator HandlePlayerAttackSequence()
    {
        Coroutine playerAttackSoundCoroutine = null;
        Coroutine enemyHitSoundCoroutine = null;

        // 1. ������ҹ�������
        if (animationManager != null)
        {
            Debug.Log("Playing player attack animation");
            animationManager.PlayPlayerAttack();

            // �ڶ�����ʼ������������Ч�ӳ�Э��
            playerAttackSoundCoroutine = StartCoroutine(PlayPlayerAttackSoundAfterDelay());

            // �ȴ��������
            while (animationManager.IsPlayerAnimating())
            {
                yield return null;
            }
        }

        // �ȴ���ЧЭ����ɣ��������û��ɵĻ���
        if (playerAttackSoundCoroutine != null)
        {
            yield return playerAttackSoundCoroutine;
        }

        // 3. ���㲢Ӧ������˺�
        if (playerManager != null && enemyManager != null)
        {
            float damage = playerManager.CalculateDamage(enemyManager.CurrentDefense);
            Debug.Log($"Player calculated damage: {damage}");

            // �����ܵ��˺�
            if (enemyManager != null)
            {
                Debug.Log("Enemy taking damage");
                enemyManager.TakeDamage(damage);
            }

            // ���µ���״̬UI
            if (uiManager != null)
            {
                uiManager.UpdateEnemyStatus(enemyManager.Health, enemyManager.MaxHealth);
            }

            // �������Ƿ�����
            if (enemyManager.Health <= 0)
            {
                Debug.Log("Enemy died. Player wins!");
                EndBattle(true);
                yield break;
            }
        }

        // 4. �ȴ����õ�ʱ��󲥷ŵ����ܻ�����
        yield return new WaitForSeconds(config.playerToEnemyHitDelay);
        if (animationManager != null)
        {
            Debug.Log("Playing enemy hit animation");
            animationManager.PlayEnemyHit();

            // �ڶ�����ʼ������������Ч�ӳ�Э��
            enemyHitSoundCoroutine = StartCoroutine(PlayEnemyHitSoundAfterDelay());

            // �ȴ��������
            while (animationManager.IsEnemyAnimating())
            {
                yield return null;
            }
        }

        // �ȴ���ЧЭ����ɣ��������û��ɵĻ���
        if (enemyHitSoundCoroutine != null)
        {
            yield return enemyHitSoundCoroutine;
        }

        // 6. ���㱣������
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

        // 1. ���ŵ��˹�������
        if (animationManager != null)
        {
            Debug.Log("Playing enemy attack animation");
            animationManager.PlayEnemyAttack();

            // �ڶ�����ʼ������������Ч�ӳ�Э��
            enemyAttackSoundCoroutine = StartCoroutine(PlayEnemyAttackSoundAfterDelay());

            // �ȴ��������
            while (animationManager.IsEnemyAnimating())
            {
                yield return null;
            }
        }

        // �ȴ���ЧЭ����ɣ��������û��ɵĻ���
        if (enemyAttackSoundCoroutine != null)
        {
            yield return enemyAttackSoundCoroutine;
        }

        // 3. �ȴ����õ�ʱ��󲥷�����ܻ�����
        yield return new WaitForSeconds(config.enemyToPlayerHitDelay);
        if (animationManager != null)
        {
            Debug.Log("Playing player hit animation");
            animationManager.PlayPlayerHit();

            // �ڶ�����ʼ������������Ч�ӳ�Э��
            playerHitSoundCoroutine = StartCoroutine(PlayPlayerHitSoundAfterDelay());

            // �ȴ��������
            while (animationManager.IsPlayerAnimating())
            {
                yield return null;
            }
        }

        // �ȴ���ЧЭ����ɣ��������û��ɵĻ���
        if (playerHitSoundCoroutine != null)
        {
            yield return playerHitSoundCoroutine;
        }

        // 5. ���㲢Ӧ�õ����˺�
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

        // ���ŵ��˼��ܶ�����������
        if (animationManager != null)
        {
            Debug.Log("Playing enemy defend skill animation");
            animationManager.PlayEnemySkill();

            // �ڶ�����ʼ������������Ч�ӳ�Э��
            enemySkillSoundCoroutine = StartCoroutine(PlayEnemySkillSoundAfterDelay());

            // �ȴ��������
            while (animationManager.IsEnemyAnimating())
            {
                yield return null;
            }
        }

        // �ȴ���ЧЭ����ɣ��������û��ɵĻ���
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

        // ���ŵ��˼��ܶ�����������
        if (animationManager != null)
        {
            Debug.Log("Playing enemy charge skill animation");
            animationManager.PlayEnemySkill();

            // �ڶ�����ʼ������������Ч�ӳ�Э��
            enemySkillSoundCoroutine = StartCoroutine(PlayEnemySkillSoundAfterDelay());

            // �ȴ��������
            while (animationManager.IsEnemyAnimating())
            {
                yield return null;
            }
        }

        // �ȴ���ЧЭ����ɣ��������û��ɵĻ���
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

    // �������������ӳٺ󲥷���ҹ�����Ч�ĸ���Э��
    private IEnumerator PlayPlayerAttackSoundAfterDelay()
    {
        yield return new WaitForSeconds(config.playerAttackSoundDelay);
        if (config.playerAttackSound != null && audioManager != null)
        {
            Debug.Log("Playing player attack sound after delay");
            audioManager.PlaySoundEffect(config.playerAttackSound);
        }
    }

    // �������������ӳٺ󲥷�����ܻ���Ч�ĸ���Э��
    private IEnumerator PlayPlayerHitSoundAfterDelay()
    {
        yield return new WaitForSeconds(config.playerHitSoundDelay);
        if (config.playerHitSound != null && audioManager != null)
        {
            Debug.Log("Playing player hit sound after delay");
            audioManager.PlaySoundEffect(config.playerHitSound);
        }
    }

    // �������������ӳٺ󲥷ŵ��˹�����Ч�ĸ���Э��
    private IEnumerator PlayEnemyAttackSoundAfterDelay()
    {
        yield return new WaitForSeconds(config.enemyAttackSoundDelay);
        if (config.enemyAttackSound != null && audioManager != null)
        {
            Debug.Log("Playing enemy attack sound after delay");
            audioManager.PlaySoundEffect(config.enemyAttackSound);
        }
    }

    // �������������ӳٺ󲥷ŵ����ܻ���Ч�ĸ���Э��
    private IEnumerator PlayEnemyHitSoundAfterDelay()
    {
        yield return new WaitForSeconds(config.enemyHitSoundDelay);
        if (config.enemyHitSound != null && audioManager != null)
        {
            Debug.Log("Playing enemy hit sound after delay");
            audioManager.PlaySoundEffect(config.enemyHitSound);
        }
    }

    // �������������ӳٺ󲥷ŵ��˼�����Ч�ĸ���Э��
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