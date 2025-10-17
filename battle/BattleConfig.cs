using UnityEngine;

[CreateAssetMenu(fileName = "BattleConfig", menuName = "Battle/Battle Config")]
public class BattleConfig : ScriptableObject
{
    [Header("Player Base Attributes")]
    public int playerBaseHealth = 100;

    [Header("Enemy Base Attributes")]
    public int enemyBaseHealth = 40;
    public int enemyBaseAttack = 7;

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
    public Vector2 yinProsperityMultipliers = new Vector2(1.0f, 2.5f);

    [Tooltip("Extreme Yang state multipliers")]
    public Vector2 extremeYangMultipliers = new Vector2(4.5f, 0.5f);

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

    [Header("Animation Configuration")]
    public AnimationClip playerAttackAnim;
    public AnimationClip playerDefendAnim;
    public AnimationClip playerHitAnim;
    public AnimationClip enemyAttackAnim;
    public AnimationClip enemyDefendAnim;
    public AnimationClip enemyChargeAnim;
    public AnimationClip enemyHitAnim;
    public AnimationClip victoryAnim;
    public AnimationClip defeatAnim;
}