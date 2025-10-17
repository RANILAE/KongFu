using UnityEngine;

public class YinYangSystem : MonoBehaviour
{
    private string currentStateName = "Default";
    private int balanceHealCooldown = 0;

    public void ApplyBattleEffects(PlayerData player, EnemyData enemy, BattleConfig config)
    {
        // ���ñ��غ�״̬Ч��
        player.counterStrikeActive = false;
        player.nextTurnAttackDebuff = false;
        player.nextTurnDefenseDebuff = false;
        player.counterStrikeMultiplier = 1.0f;

        int yinPoints = player.defense;
        int yangPoints = player.attack;
        int diff = yangPoints - yinPoints; // ������ - ������

        int originalYin = yinPoints;
        int originalYang = yangPoints;

        // 1. ������״̬ (��-�� �� 5)
        if (diff >= 5)
        {
            // ��鼫���������Ƿ�����
            if (player.extremeYangStack >= 3)
            {
                currentStateName = "Extreme Yang";

                // ���㹥������ֵ
                player.attack = Mathf.FloorToInt(config.extremeYangAttackMultiplier * originalYang);
                player.defense = Mathf.FloorToInt(config.extremeYangDefenseMultiplier * originalYin);

                // ���õ���
                player.ResetExtremeYangStack();
                BattleEventSystem.OnBattleLog.Invoke("Extreme Yang activated!");
            }
            else
            {
                // ���㲻���޷�����
                currentStateName = "Extreme Yang (Locked)";
                player.attack = originalYang;
                player.defense = originalYin;
                BattleEventSystem.OnBattleLog.Invoke($"Extreme Yang locked! Requires 3 critical yang stacks (current: {player.extremeYangStack}/3)");
            }
            return;
        }

        // 2. ������״̬ (��-�� �� 5)
        if (diff <= -5)
        {
            // ��鼫���������Ƿ�����
            if (player.extremeYinStack >= 3)
            {
                currentStateName = "Extreme Yin";

                // ���㹥������ֵ
                player.attack = originalYang;
                player.defense = Mathf.FloorToInt(config.extremeYinDefenseMultiplier * originalYin);

                // ���õ���
                player.ResetExtremeYinStack();
                BattleEventSystem.OnBattleLog.Invoke("Extreme Yin activated!");

                // �޸���ȷ�������Ч��
                player.ActivateCounterStrike(1.5f); // ������ʹ��1.5������
                BattleEventSystem.OnBattleLog.Invoke("Extreme Yin activated! Counterstrike effect applied");
            }
            else
            {
                // ���㲻���޷�����
                currentStateName = "Extreme Yin (Locked)";
                player.attack = originalYang;
                player.defense = originalYin;
                BattleEventSystem.OnBattleLog.Invoke($"Extreme Yin locked! Requires 3 critical yin stacks (current: {player.extremeYinStack}/3)");
            }
            return;
        }

        // 3. ��ʢ״̬ (��-�� = 3 or 4)
        if (diff >= 3)
        {
            currentStateName = "Yang Sheng";

            // ���㹥������ֵ
            player.attack = Mathf.FloorToInt(config.yangShengAttackMultiplier * originalYang);
            player.defense = Mathf.FloorToInt(config.yangShengDefenseMultiplier * originalYin);

            // ���DOTЧ��
            int dotDamage = Mathf.FloorToInt(originalYang / 2f);
            if (dotDamage > 0)
            {
                player.activeDots.Add(new PlayerData.DotEffect
                {
                    damage = dotDamage,
                    duration = 2
                });
                BattleEventSystem.OnBattleLog.Invoke($"Yang Sheng DOT applied: {dotDamage} damage per turn for 2 turns");
            }
            return;
        }

        // 4. ��ʢ״̬ (��-�� = 3 or 4)
        if (diff <= -3)
        {
            currentStateName = "Yin Sheng";

            // ���㹥������ֵ
            player.attack = originalYang;
            player.defense = Mathf.FloorToInt(config.yinShengDefenseMultiplier * originalYin);

            // �����Ч��
            player.ActivateCounterStrike(1.0f);
            BattleEventSystem.OnBattleLog.Invoke("Yin Sheng activated! Counterstrike effect applied");
            return;
        }

        // 5. �ٽ���״̬ (��-�� = 2)
        if (diff == 2)
        {
            currentStateName = "Critical Yang";

            // ���㹥������ֵ
            player.attack = Mathf.FloorToInt(config.criticalYangAttackMultiplier * originalYang);
            player.defense = Mathf.FloorToInt(config.criticalYangDefenseMultiplier * originalYin);

            // �����ٽ����������ͼ���������
            player.IncrementYangCriticalCounter();
            BattleEventSystem.OnBattleLog.Invoke($"Critical Yang applied! (Yang Critical Stacks: {player.yangCriticalCounter}, Extreme Yang Stacks: {player.extremeYangStack}/3)");
            return;
        }

        // 6. �ٽ���״̬ (��-�� = 2)
        if (diff == -2)
        {
            currentStateName = "Critical Yin";

            // ���㹥������ֵ
            player.attack = Mathf.FloorToInt(config.criticalYinAttackMultiplier * originalYang);
            player.defense = Mathf.FloorToInt(config.criticalYinDefenseMultiplier * originalYin);

            // �����ٽ����������ͼ���������
            player.IncrementYinCriticalCounter();
            BattleEventSystem.OnBattleLog.Invoke($"Critical Yin applied! (Yin Critical Stacks: {player.yinCriticalCounter}, Extreme Yin Stacks: {player.extremeYinStack}/3)");

            // �����Ч��
            player.ActivateCounterStrike(1.0f);
            return;
        }

        // 7. ƽ��״̬ (��-���ľ���ֵ = 0 or 1)
        if (Mathf.Abs(diff) <= 1)
        {
            currentStateName = "Balance";

            // ���㹥������ֵ
            player.attack = Mathf.FloorToInt(config.balanceMultiplier * originalYang);
            player.defense = Mathf.FloorToInt(config.balanceMultiplier * originalYin);

            // ����ָ�Ч��
            if (balanceHealCooldown == 0)
            {
                player.health = Mathf.Min(player.health + 5, player.maxHealth);
                BattleEventSystem.OnBattleLog.Invoke("Balance state healed 5 HP!");
                balanceHealCooldown = 2;
            }
            else
            {
                balanceHealCooldown--;
                BattleEventSystem.OnBattleLog.Invoke($"Balance state heal on cooldown. {balanceHealCooldown} turns left");
            }
            return;
        }

        // Ĭ��״̬
        currentStateName = "Default";
        player.attack = originalYang;
        player.defense = originalYin;
        BattleEventSystem.OnBattleLog.Invoke("No special effect from Yin-Yang energies");
    }

    public void UpdateBalanceCooldown()
    {
        if (balanceHealCooldown > 0)
        {
            balanceHealCooldown--;
        }
    }

    public string GetCurrentStateName()
    {
        return currentStateName;
    }
}