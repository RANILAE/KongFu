using UnityEngine;

public class EnemyData : MonoBehaviour
{
    [Header("Core Attributes")]
    public int health;
    public int maxHealth;

    [Header("Battle Attributes")]
    public int baseAttack;
    public int currentAttack;
    public int defense; // Added defense attribute
    public int turnCount = 0;

    [Header("Buff Stacks")]
    public int yangPenetrateStacks;
    public int yinCoverStacks;

    public void ResetEnemy(BattleConfig config)
    {
        maxHealth = config.enemyBaseHealth;
        health = maxHealth;
        baseAttack = config.enemyBaseAttack;
        currentAttack = baseAttack;
        defense = 0; // Initialize defense
        turnCount = 0;
        yangPenetrateStacks = 0;
        yinCoverStacks = 0;
    }

    public void ApplyYangPenetrate(int amount = 1)
    {
        yangPenetrateStacks += amount;
    }

    public void ApplyYinCover(int amount = 1)
    {
        yinCoverStacks += amount;
    }
}