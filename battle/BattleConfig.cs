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
    [Tooltip("Critical stacks required for Extreme states")]
    public int criticalStacksRequired = 3;

    [Header("Battle Settings")]
    public int maxLogLines = 10;

    [Header("Animation Settings")]
    public float attackAnimationDuration = 0.5f;
    public float damageAnimationDuration = 0.3f;
}