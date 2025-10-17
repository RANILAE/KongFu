using UnityEngine;

[CreateAssetMenu(fileName = "BattleConfig", menuName = "Battle/Battle Config")]
public class BattleConfig : ScriptableObject
{
    [Header("Player Base Attributes")]
    public int playerBaseHealth = 100;

    [Header("Enemy Base Attributes (Legacy/Fallback)")]
    // 为兼容旧代码添加的属性，现在从 EnemyConfig 获取默认值
    [Tooltip("Default Enemy Max Health (Fallback) - Use defaultEnemy.health instead")]
    public int enemyMaxHealth = 40;
    [Tooltip("Default Enemy Base Attack (Fallback) - Use defaultEnemy.attack instead")]
    public int enemyBaseAttack = 7;
    [Tooltip("Default Enemy Base Defense (Fallback) - Use defaultEnemy.defense instead")]
    public int enemyBaseDefense = 0;

    [Tooltip("Second Enemy Max Health (Fallback) - Use secondEnemy.health instead")]
    public int secondEnemyMaxHealth = 100;
    [Tooltip("Second Enemy Base Attack (Fallback) - Use secondEnemy.attack instead")]
    public int secondEnemyBaseAttack = 6;
    [Tooltip("Second Enemy Base Defense (Fallback) - Use secondEnemy.defense instead")]
    public int secondEnemyBaseDefense = 2;

    [Tooltip("Third Enemy Max Health (Fallback) - Use thirdEnemy.health instead")]
    public int thirdEnemyMaxHealth = 150;
    [Tooltip("Third Enemy Base Attack (Fallback) - Use thirdEnemy.attack instead")]
    public int thirdEnemyBaseAttack = 9;
    [Tooltip("Third Enemy Base Defense (Fallback) - Use thirdEnemy.defense instead")]
    public int thirdEnemyBaseDefense = 2;

    [Header("Second Enemy Special Configuration")]
    [Tooltip("Second enemy defense damage reduction percentage (0.5 = 50%)")]
    public float secondEnemyDefenseReduction = 0.5f; // 第二个敌人防御时的伤害减免百分比

    [Header("Yin-Yang Points")]
    [Tooltip("Maximum points for Yin and Yang")]
    public int maxPoints = 7; // 点数上限

    [Header("Yin-Yang State Multipliers")]
    [Tooltip("Balance state multipliers")]
    public Vector2 balanceMultipliers = new Vector2(1.25f, 1.25f);

    [Tooltip("Critical Yang state multipliers")]
    public Vector2 criticalYangMultipliers = new Vector2(1.75f, 1.25f);

    [Tooltip("Critical Yin state multipliers")]
    public Vector2 criticalYinMultipliers = new Vector2(1.25f, 1.75f);

    [Tooltip("Yang Prosperity state multipliers")]
    public Vector2 yangProsperityMultipliers = new Vector2(2.75f, 1.25f);

    [Tooltip("Yin Prosperity state multipliers")]
    public Vector2 yinProsperityMultipliers = new Vector2(1.25f, 2.5f);

    [Tooltip("Extreme Yang state multipliers")]
    public Vector2 extremeYangMultipliers = new Vector2(3.0f, 1.0f); // 注意：你之前的代码中是 (4.5, 0.5)，这里是 (3.0, 1.0)

    [Tooltip("Extreme Yin state multipliers")]
    public Vector2 extremeYinMultipliers = new Vector2(1.0f, 3.0f);

    [Tooltip("Ultimate Qi state multipliers")]
    public Vector2 ultimateQiMultipliers = new Vector2(7.0f, 7.0f);

    [Header("Balance State Healing")]
    public int balanceHealAmount = 5;
    public int balanceHealCooldown = 2;

    [Header("Counter Strike Configuration")]
    public float counterStrikeSuccessMultiplier = 1.0f;
    public float counterStrikeFailureMultiplier = 1.5f;
    public int counterStrikeFailureDotDamage = 2;
    public int counterStrikeFailureDotDuration = 2;

    // 新增：不同状态的反震持续时间配置
    [Header("Counter Strike Duration Configuration")]
    [Tooltip("Extreme Yin counter strike duration (in turns)")]
    public int extremeYinCounterStrikeDuration = 1; // 极端阴反震持续时间

    [Tooltip("Yin Prosperity counter strike duration (in turns)")]
    public int yinProsperityCounterStrikeDuration = 1; // 阴盛反震持续时间

    [Tooltip("Ultimate Qi counter strike duration (in turns)")]
    public int ultimateQiCounterStrikeDuration = 3; // 究极气反震持续时间

    [Header("Extreme State Configuration")]
    public int extremeStacksRequired = 3;
    public int extremeYangBonusPerStack = 2;
    public int extremeYinAttackReducePerStack = 2;
    public int extremeDebuffDuration = 2;

    [Header("Ultimate Qi Configuration")]
    public int ultimateQiHealthSet = 1;

    [Header("Points Retention Mechanism")]
    [Tooltip("Points retention factor")]
    public float pointsRetentionFactor = 0.5f;

    [Header("UI Configuration")]
    [Tooltip("Duration (in seconds) for turn indicator display")]
    public float turnIndicatorDuration = 2.0f;

    // 新增：延迟时间配置（从BattleSystem获取）
    [Header("Delay Configuration")]
    [Tooltip("Delay (in seconds) before starting player setup after enemy turn")]
    public float playerSetupDelay = 1.5f; // 玩家设置延迟时间

    [Tooltip("Delay (in seconds) before starting enemy turn after player action")]
    public float enemyTurnDelay = 1.5f; // 敌人回合延迟时间

    [Header("Audio Configuration")]
    public AudioClip backgroundMusic;
    public AudioClip playerAttackSound;
    public AudioClip playerHitSound;
    public AudioClip enemyAttackSound;
    public AudioClip enemyHitSound;
    public AudioClip enemySkillSound; // 新增：敌人技能音效
    public AudioClip victorySound;
    public AudioClip defeatSound;

    [Header("Audio Delay Configuration")]
    [Tooltip("Delay (in seconds) from player attack animation start to play attack sound.")]
    public float playerAttackSoundDelay = 0.2f;
    [Tooltip("Delay (in seconds) from player hit animation start to play hit sound.")]
    public float playerHitSoundDelay = 0.4f;
    [Tooltip("Delay (in seconds) from enemy attack animation start to play attack sound.")]
    public float enemyAttackSoundDelay = 0.2f;
    [Tooltip("Delay (in seconds) from enemy hit animation start to play hit sound.")]
    public float enemyHitSoundDelay = 0.4f;
    [Tooltip("Delay (in seconds) from enemy skill animation start to play skill sound.")]
    public float enemySkillSoundDelay = 0.3f;

    [Header("Animation Timing Configuration")]
    [Tooltip("Delay (in seconds) between player attack animation and enemy hit animation")]
    public float playerToEnemyHitDelay = 0.3f; // 玩家攻击到敌人受击的延迟

    [Tooltip("Delay (in seconds) between enemy attack animation and player hit animation")]
    public float enemyToPlayerHitDelay = 0.3f; // 敌人攻击到玩家受击的延迟
}