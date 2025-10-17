using UnityEngine;

[CreateAssetMenu(fileName = "BattleConfig", menuName = "Battle/BattleConfig")]
public class BattleConfig : ScriptableObject
{
    [Header("Player Settings")]
    public int playerBaseHealth = 40;
    public int playerBaseQiPoints = 7;

    [Header("Enemy Settings")]
    public int enemyBaseHealth = 40;
    public int enemyBaseAttack = 7;

    [Header("YinYang Settings")]
    // Yang Sheng coefficients
    public float yangShengAttackMultiplier = 2.75f;
    public float yangShengDefenseMultiplier = 1.25f;

    // Yin Sheng coefficients
    public float yinShengDefenseMultiplier = 2.5f;

    // Balance coefficient
    public float balanceMultiplier = 1.25f;

    // Critical Yang coefficients
    public float criticalYangAttackMultiplier = 1.75f;
    public float criticalYangDefenseMultiplier = 1.25f;

    // Critical Yin coefficients
    public float criticalYinAttackMultiplier = 1.25f;
    public float criticalYinDefenseMultiplier = 1.75f;

    // Extreme Yang coefficients
    public float extremeYangAttackMultiplier = 4.5f;
    public float extremeYangDefenseMultiplier = 0.5f;

    // Extreme Yin coefficients
    public float extremeYinDefenseMultiplier = 3.0f;

    [Header("Critical Stack Settings")]
    public int criticalStacksRequired = 3;

    [Header("Battle Settings")]
    public int maxLogLines = 10; // 最大日志行数
    public int maxLogEntriesPerTurn = 5; // 每回合最大日志条目数

    [Header("Animation Settings")]
    public float attackAnimationDuration = 0.5f;
    public float damageAnimationDuration = 0.3f;

    [Header("Audio Settings")]
    public AudioClip attackSound;
    public AudioClip damageSound;
    public AudioClip chargeSound;
    public AudioClip defenseSound;
    public AudioClip victorySound;
    public AudioClip defeatSound;

    [Header("Icon Settings")]
    public Sprite yangStackIcon;           // 阳叠层图标
    public Sprite yinStackIcon;            // 阴叠层图标
    public Sprite extremeYangDebuffIcon;   // 极端阳debuff图标（攻击下降）
    public Sprite extremeYinDebuffIcon;    // 极端阴debuff图标（防御下降）
    public Sprite playerDotIcon;           // 玩家受到的DOT伤害图标
    public Sprite enemyDotIcon;            // 敌人受到的DOT伤害图标
    public Sprite counterStrikeIcon;       // 反震效果图标
    public Sprite yangPenetrationIcon;     // 新增：阳穿透效果图标
    public Sprite yinCoverIcon;            // 新增：阴覆盖效果图标
}