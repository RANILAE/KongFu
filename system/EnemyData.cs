using System.Collections.Generic;
using UnityEngine;

public class EnemyData : MonoBehaviour
{
    [Header("Core Attributes")]
    public int health;
    public int maxHealth;
    public int baseAttack;
    public int currentAttack;
    public int defense; // ������������
    public int turnCount;

    [Header("Effects")]
    public int yangPenetrationStacks; // ����͸����
    public int yinCoverStacks;        // �����ǵ���
    public List<DotEffect> activeDots = new List<DotEffect>();

    [System.Serializable]
    public struct DotEffect
    {
        public int damage;
        public int duration;
    }

    public void ResetEnemy(BattleConfig config)
    {
        maxHealth = config.enemyBaseHealth;
        health = maxHealth;
        baseAttack = config.enemyBaseAttack;
        currentAttack = baseAttack;
        defense = 0; // ��ʼ������ֵΪ0
        turnCount = 0;
        yangPenetrationStacks = 0;
        yinCoverStacks = 0;
        activeDots.Clear();
    }

    public void AddYangPenetrationStack()
    {
        yangPenetrationStacks++;
    }

    public void AddYinCoverStack()
    {
        yinCoverStacks++;
    }

    public void ResetYinCoverStacks()
    {
        yinCoverStacks = 0;
    }
}