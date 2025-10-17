using System.Collections.Generic;
using UnityEngine;

public class PlayerData : MonoBehaviour
{
    [Header("Core Attributes")]
    public int health;
    public int maxHealth;

    [Header("Battle Attributes")]
    public int attack;
    public int defense;
    public int qiPoints;

    [Header("Critical Counters")]
    public int yangCriticalCounter; // ÁÙ½çÑô¼ÆÊýÆ÷
    public int yinCriticalCounter;  // ÁÙ½çÒõ¼ÆÊýÆ÷

    [Header("Extreme Stacks")]
    public int extremeYangStack = 0; // ¼«¶ËÑôµþ²ã
    public int extremeYinStack = 0;  // ¼«¶ËÒõµþ²ã

    [Header("Effects")]
    public bool counterStrikeActive;
    public float counterStrikeMultiplier = 1.0f;
    public bool nextTurnAttackDebuff;
    public bool nextTurnDefenseDebuff;
    public List<DotEffect> activeDots = new List<DotEffect>();

    [System.Serializable]
    public struct DotEffect
    {
        public int damage;
        public int duration;
    }

    public void ResetPlayer(BattleConfig config)
    {
        maxHealth = config.playerBaseHealth;
        health = maxHealth;
        attack = 0;
        defense = 0;
        qiPoints = config.playerBaseQiPoints;
        counterStrikeActive = false;
        nextTurnAttackDebuff = false;
        nextTurnDefenseDebuff = false;
        counterStrikeMultiplier = 1.0f;
        yangCriticalCounter = 0;
        yinCriticalCounter = 0;
        extremeYangStack = 0;
        extremeYinStack = 0;
        activeDots.Clear();
    }

    public void ResetForNewTurn(BattleConfig config)
    {
        attack = 0;
        defense = 0;
        counterStrikeActive = false;
        counterStrikeMultiplier = 1.0f;
        qiPoints = config.playerBaseQiPoints;
    }

    public void ActivateCounterStrike(float multiplier = 1.0f)
    {
        counterStrikeActive = true;
        counterStrikeMultiplier = multiplier;
    }

    public void IncrementYangCriticalCounter()
    {
        yangCriticalCounter++;
        extremeYangStack++;
    }

    public void IncrementYinCriticalCounter()
    {
        yinCriticalCounter++;
        extremeYinStack++;
    }

    public void ResetCriticalCounters()
    {
        yangCriticalCounter = 0;
        yinCriticalCounter = 0;
    }

    public void ResetExtremeYangStack()
    {
        extremeYangStack = 0;
    }

    public void ResetExtremeYinStack()
    {
        extremeYinStack = 0;
    }
}