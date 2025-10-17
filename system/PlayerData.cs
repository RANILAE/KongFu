using UnityEngine;
using System.Collections.Generic;

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
    public int yangCriticalCounter;
    public int yinCriticalCounter;

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
    }

    public void IncrementYinCriticalCounter()
    {
        yinCriticalCounter++;
    }

    public void ResetCriticalCounters()
    {
        yangCriticalCounter = 0;
        yinCriticalCounter = 0;
    }
}