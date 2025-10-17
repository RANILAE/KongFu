using UnityEngine;

[CreateAssetMenu(fileName = "BattleConfig", menuName = "Battle/Battle Config")]
public class BattleConfig : ScriptableObject
{
    [Header("Player Base Attributes")]
    public int playerBaseHealth = 100;

    [Header("Enemy Base Attributes")]
    public int enemyBaseHealth = 40;
    public int enemyBaseAttack = 7;

    [Header("Specific Enemy Configurations")]
    [Tooltip("Default Enemy Configuration")]
    public EnemyConfig defaultEnemy = new EnemyConfig
    {
        health = 40,
        attack = 7,
        defense = 0,
        enemyId = 0
    };

    [Tooltip("Second Enemy Configuration")]
    public EnemyConfig secondEnemy = new EnemyConfig
    {
        health = 100,
        attack = 6,
        defense = 2,
        enemyId = 1
    };

    [Tooltip("Third Enemy Configuration")]
    public EnemyConfig thirdEnemy = new EnemyConfig
    {
        health = 150,
        attack = 9,
        defense = 2,
        enemyId = 2
    };

    [System.Serializable]
    public class EnemyConfig
    {
        public int health;
        public int attack;
        public int defense;
        public int enemyId; // 敌人ID，用于PersistentBattleData中调用
    }

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
    public Vector2 extremeYangMultipliers = new Vector2(3.0f, 1.0f);

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

    // 新增：可配置的工具提示文本
    [Header("Tooltip Text Configuration")]
    [Tooltip("Yang Stack tooltip title")]
    public string yangStackTooltipTitle = "Yang Penetration";
    [Tooltip("Yang Stack tooltip description")]
    public string yangStackTooltipDescription = "Increases damage on attack. Stacks: {0}";

    [Tooltip("Yin Stack tooltip title")]
    public string yinStackTooltipTitle = "Yin Cover";
    [Tooltip("Yin Stack tooltip description")]
    public string yinStackTooltipDescription = "Reduces incoming damage. Stacks: {0}";

    [Tooltip("Counter Strike tooltip title")]
    public string counterStrikeTooltipTitle = "Counter Strike";
    [Tooltip("Counter Strike tooltip description")]
    public string counterStrikeTooltipDescription = "Reflects damage from enemy attacks. Active{0}";

    [Tooltip("Player DOT tooltip title")]
    public string playerDotTooltipTitle = "Player DOT";
    [Tooltip("Player DOT tooltip description")]
    public string playerDotTooltipDescription = "Player takes {0:F1} damage over time. Duration: {1} turns";

    [Tooltip("Attack Debuff tooltip title")]
    public string attackDebuffTooltipTitle = "Attack Debuff";
    [Tooltip("Attack Debuff tooltip description")]
    public string attackDebuffTooltipDescription = "Player attack reduced by {0:F1}{1}";

    [Tooltip("Defense Debuff tooltip title")]
    public string defenseDebuffTooltipTitle = "Defense Debuff";
    [Tooltip("Defense Debuff tooltip description")]
    public string defenseDebuffTooltipDescription = "Player defense reduced by {0:F1}{1}";

    [Tooltip("Balance Heal CD tooltip title")]
    public string balanceHealCDTooltipTitle = "Balance Heal CD";
    [Tooltip("Balance Heal CD tooltip description")]
    public string balanceHealCDTooltipDescription = "Balance state healing on cooldown. Duration: {0} turns";

    [Tooltip("Attack Intent tooltip title")]
    public string attackIntentTooltipTitle = "Attack Intent";
    [Tooltip("Attack Intent tooltip description")]
    public string attackIntentTooltipDescription = "Enemy intends to attack next turn.";

    [Tooltip("Defend Intent tooltip title")]
    public string defendIntentTooltipTitle = "Defend Intent";
    [Tooltip("Defend Intent tooltip description")]
    public string defendIntentTooltipDescription = "Enemy intends to defend next turn.";

    [Tooltip("Charge Intent tooltip title")]
    public string chargeIntentTooltipTitle = "Charge Intent";
    [Tooltip("Charge Intent tooltip description")]
    public string chargeIntentTooltipDescription = "Enemy intends to charge attack next turn.";

    [Tooltip("Enemy DOT tooltip title")]
    public string enemyDotTooltipTitle = "Enemy DOT";
    [Tooltip("Enemy DOT tooltip description")]
    public string enemyDotTooltipDescription = "Enemy takes {0:F1} damage over time. Duration: {1} turns";

    [Tooltip("Yang Penetration tooltip title")]
    public string yangPenetrationTooltipTitle = "Yang Penetration";
    [Tooltip("Yang Penetration tooltip description")]
    public string yangPenetrationTooltipDescription = "Yang penetration effect applied to enemy. Value: {0:F1}{1}";

    [Tooltip("Yin Cover tooltip title")]
    public string yinCoverTooltipTitle = "Yin Cover";
    [Tooltip("Yin Cover tooltip description")]
    public string yinCoverTooltipDescription = "Yin cover effect applied to enemy. Value: {0:F1}{1}";

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