using UnityEngine;
using System.Collections;

public class BattleSystem : MonoBehaviour
{
    // ����ģʽ��ȷ����ؿ�������
    public static BattleSystem Instance { get; private set; }

    [Header("Core Systems")]
    public PlayerManager playerManager; // ��ҹ���������������
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
            // ȷ�������Ϸ�����ڼ����³���ʱ���ᱻ����
            DontDestroyOnLoad(gameObject);
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
        if (iconManager == null) iconManager = FindObjectOfType<IconManager>();
        if (retainedPointsSystem == null) retainedPointsSystem = FindObjectOfType<RetainedPointsSystem>();
        if (audioManager == null) audioManager = FindObjectOfType<AudioManager>();
        if (animationManager == null) animationManager = FindObjectOfType<AnimationManager>();
        if (counterStrikeSystem == null) counterStrikeSystem = FindObjectOfType<CounterStrikeSystem>();

        // ��ʼ������ϵͳ
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

        // ��ʼ��CounterStrikeSystem
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

        // ���ű�������
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
            playerManager.ResetForNewTurn();
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

        // ��ʾ������ͼ
        if (enemyManager != null && effectManager != null && playerManager != null)
        {
            EnemyManager.EnemyAction nextAction = enemyManager.ChooseAction(currentRound, playerManager.DamageTakenLastTurn);
            effectManager.ShowEnemyIntent(nextAction);
        }

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
        StartCoroutine(StartPlayerSetupAfterDelay(1.5f));
    }

    private IEnumerator StartPlayerSetupAfterDelay(float delay)
    {
        Debug.Log($"Starting player setup after {delay} seconds delay");
        yield return new WaitForSeconds(delay);

        // ���˻غϽ�������ʾ��һغ���ʾ��ʹ�������еĳ���ʱ�䣩
        if (uiManager != null)
        {
            uiManager.ShowTurnIndicator("Player Turn", config.turnIndicatorDuration);
        }

        yield return new WaitForSeconds(config.turnIndicatorDuration); // �ȴ���ʾ��ʾ���

        StartPlayerSetup();
    }

    private void EndBattle(bool playerWins)
    {
        Debug.Log($"Battle ended. Player wins: {playerWins}");
        currentState = BattleState.BattleEnd;

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

    // �޸ģ���ӵ������߼�
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

        // ����Ƿ���ʹ�ü��˼��ܵ����㲻��
        float diff = yangPoints - yinPoints;
        float absDiff = Mathf.Abs(diff);

        // ��鼫����״̬��5-7���ֵ��
        if (diff >= 5f && diff <= 7f)
        {
            // ��������Ҫ����3������͸����
            int yangStacks = effectManager.GetYangPenetrationStacks();
            if (yangStacks < 3)
            {
                // ��ʾ���㲻����ʾ
                if (uiManager != null)
                {
                    uiManager.ShowStackInsufficientPanel("Insufficient Yang Penetration stacks!\nNeed at least 3 stacks to use Extreme Yang.");
                    uiManager.UpdateBattleLog("Cannot use Extreme Yang: Insufficient Yang Penetration stacks (need 3)!");
                }
                return; // ������ִ��
            }
        }
        // ��鼫����״̬��-5��-7���ֵ��
        else if (diff <= -5f && diff >= -7f)
        {
            // ��������Ҫ����3�������ǵ���
            int yinStacks = effectManager.GetYinCoverStacks();
            if (yinStacks < 3)
            {
                // ��ʾ���㲻����ʾ
                if (uiManager != null)
                {
                    uiManager.ShowStackInsufficientPanel("Insufficient Yin Cover stacks!\nNeed at least 3 stacks to use Extreme Yin.");
                    uiManager.UpdateBattleLog("Cannot use Extreme Yin: Insufficient Yin Cover stacks (need 3)!");
                }
                return; // ������ִ��
            }
        }

        if (wheelSystem != null)
        {
            wheelSystem.HideWheelUI();
        }

        currentState = BattleState.PlayerTurn;
        Debug.Log("State changed to PlayerTurn");

        // Ӧ����������Ч��
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
        StartCoroutine(StartEnemyTurnAfterDelay(1.5f));
    }

    private IEnumerator HandlePlayerAttackSequence()
    {
        // 1. ������ҹ�������
        if (animationManager != null)
        {
            Debug.Log("Playing player attack animation");
            animationManager.PlayPlayerAttack();
            // �ȴ��������
            while (animationManager.IsPlayerAnimating())
            {
                yield return null;
            }
        }

        // 2. ���Ź�����Ч
        yield return new WaitForSeconds(config.playerAttackSoundDelay);
        if (config.playerAttackSound != null && audioManager != null)
        {
            Debug.Log("Playing player attack sound");
            audioManager.PlaySoundEffect(config.playerAttackSound);
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

        // 4. ���ŵ����ܻ�����
        yield return new WaitForSeconds(config.enemyHitSoundDelay);
        if (animationManager != null)
        {
            Debug.Log("Playing enemy hit animation");
            animationManager.PlayEnemyHit();
            // �ȴ��������
            while (animationManager.IsEnemyAnimating())
            {
                yield return null;
            }
        }

        // 5. ���ŵ����ܻ���Ч
        if (config.enemyHitSound != null && audioManager != null)
        {
            Debug.Log("Playing enemy hit sound");
            audioManager.PlaySoundEffect(config.enemyHitSound);
        }
    }

    private IEnumerator HandleEnemyAttackSequence()
    {
        // 1. ���ŵ��˹�������
        if (animationManager != null)
        {
            Debug.Log("Playing enemy attack animation");
            animationManager.PlayEnemyAttack();
            // �ȴ��������
            while (animationManager.IsEnemyAnimating())
            {
                yield return null;
            }
        }

        // 2. ���ŵ��˹�����Ч
        yield return new WaitForSeconds(config.enemyAttackSoundDelay);
        if (config.enemyAttackSound != null && audioManager != null)
        {
            Debug.Log("Playing enemy attack sound");
            audioManager.PlaySoundEffect(config.enemyAttackSound);
        }

        // 3. ��������ܻ�����
        yield return new WaitForSeconds(config.playerHitSoundDelay);
        if (animationManager != null)
        {
            Debug.Log("Playing player hit animation");
            animationManager.PlayPlayerHit();
            // �ȴ��������
            while (animationManager.IsPlayerAnimating())
            {
                yield return null;
            }
        }

        // 4. ��������ܻ���Ч
        if (config.playerHitSound != null && audioManager != null)
        {
            Debug.Log("Playing player hit sound");
            audioManager.PlaySoundEffect(config.playerHitSound);
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
        // ���ŵ��˼��ܶ�����������
        if (animationManager != null)
        {
            Debug.Log("Playing enemy defend skill animation");
            animationManager.PlayEnemySkill();
            // �ȴ��������
            while (animationManager.IsEnemyAnimating())
            {
                yield return null;
            }
        }

        // ���ŵ��˼�����Ч
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
        // ���ŵ��˼��ܶ�����������
        if (animationManager != null)
        {
            Debug.Log("Playing enemy charge skill animation");
            animationManager.PlayEnemySkill();
            // �ȴ��������
            while (animationManager.IsEnemyAnimating())
            {
                yield return null;
            }
        }

        // ���ŵ��˼�����Ч
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

    // ���������¿�ʼս����������BGM���ƣ�
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
            uiManager.HideStackInsufficientPanel(); // ���ص��㲻����ʾ���
        }

        // ���³�ʼ��ս��
        StartPlayerSetup();
    }
}