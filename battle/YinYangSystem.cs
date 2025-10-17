using UnityEngine;

public class YinYangSystem : MonoBehaviour
{
    private string currentStateName = "Default";
    private int balanceHealCooldown = 0;

    public void ApplyBattleEffects(PlayerData player, EnemyData enemy, BattleConfig config)
    {
        // 重置本回合状态效果
        player.counterStrikeActive = false;
        player.nextTurnAttackDebuff = false;
        player.nextTurnDefenseDebuff = false;
        player.counterStrikeMultiplier = 1.0f;

        int yinPoints = player.defense;
        int yangPoints = player.attack;
        int diff = yangPoints - yinPoints; // 阳点数 - 阴点数

        int originalYin = yinPoints;
        int originalYang = yangPoints;

        // 1. 极端阳状态 (阳-阴 ≥ 5)
        if (diff >= 5)
        {
            // 检查极端阳叠层是否满足
            if (player.extremeYangStack >= 3)
            {
                currentStateName = "Extreme Yang";

                // 计算攻击防御值
                player.attack = Mathf.FloorToInt(config.extremeYangAttackMultiplier * originalYang);
                player.defense = Mathf.FloorToInt(config.extremeYangDefenseMultiplier * originalYin);

                // 重置叠层
                player.ResetExtremeYangStack();
                BattleEventSystem.OnBattleLog.Invoke("Extreme Yang activated!");
            }
            else
            {
                // 叠层不足无法激活
                currentStateName = "Extreme Yang (Locked)";
                player.attack = originalYang;
                player.defense = originalYin;
                BattleEventSystem.OnBattleLog.Invoke($"Extreme Yang locked! Requires 3 critical yang stacks (current: {player.extremeYangStack}/3)");
            }
            return;
        }

        // 2. 极端阴状态 (阴-阳 ≥ 5)
        if (diff <= -5)
        {
            // 检查极端阴叠层是否满足
            if (player.extremeYinStack >= 3)
            {
                currentStateName = "Extreme Yin";

                // 计算攻击防御值
                player.attack = originalYang;
                player.defense = Mathf.FloorToInt(config.extremeYinDefenseMultiplier * originalYin);

                // 重置叠层
                player.ResetExtremeYinStack();
                BattleEventSystem.OnBattleLog.Invoke("Extreme Yin activated!");

                // 修复：确保激活反震效果
                player.ActivateCounterStrike(1.5f); // 极端阴使用1.5倍反震
                BattleEventSystem.OnBattleLog.Invoke("Extreme Yin activated! Counterstrike effect applied");
            }
            else
            {
                // 叠层不足无法激活
                currentStateName = "Extreme Yin (Locked)";
                player.attack = originalYang;
                player.defense = originalYin;
                BattleEventSystem.OnBattleLog.Invoke($"Extreme Yin locked! Requires 3 critical yin stacks (current: {player.extremeYinStack}/3)");
            }
            return;
        }

        // 3. 阳盛状态 (阳-阴 = 3 or 4)
        if (diff >= 3)
        {
            currentStateName = "Yang Sheng";

            // 计算攻击防御值
            player.attack = Mathf.FloorToInt(config.yangShengAttackMultiplier * originalYang);
            player.defense = Mathf.FloorToInt(config.yangShengDefenseMultiplier * originalYin);

            // 添加DOT效果
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

        // 4. 阴盛状态 (阴-阳 = 3 or 4)
        if (diff <= -3)
        {
            currentStateName = "Yin Sheng";

            // 计算攻击防御值
            player.attack = originalYang;
            player.defense = Mathf.FloorToInt(config.yinShengDefenseMultiplier * originalYin);

            // 激活反震效果
            player.ActivateCounterStrike(1.0f);
            BattleEventSystem.OnBattleLog.Invoke("Yin Sheng activated! Counterstrike effect applied");
            return;
        }

        // 5. 临界阳状态 (阳-阴 = 2)
        if (diff == 2)
        {
            currentStateName = "Critical Yang";

            // 计算攻击防御值
            player.attack = Mathf.FloorToInt(config.criticalYangAttackMultiplier * originalYang);
            player.defense = Mathf.FloorToInt(config.criticalYangDefenseMultiplier * originalYin);

            // 增加临界阳计数器和极端阳叠层
            player.IncrementYangCriticalCounter();
            BattleEventSystem.OnBattleLog.Invoke($"Critical Yang applied! (Yang Critical Stacks: {player.yangCriticalCounter}, Extreme Yang Stacks: {player.extremeYangStack}/3)");
            return;
        }

        // 6. 临界阴状态 (阴-阳 = 2)
        if (diff == -2)
        {
            currentStateName = "Critical Yin";

            // 计算攻击防御值
            player.attack = Mathf.FloorToInt(config.criticalYinAttackMultiplier * originalYang);
            player.defense = Mathf.FloorToInt(config.criticalYinDefenseMultiplier * originalYin);

            // 增加临界阴计数器和极端阴叠层
            player.IncrementYinCriticalCounter();
            BattleEventSystem.OnBattleLog.Invoke($"Critical Yin applied! (Yin Critical Stacks: {player.yinCriticalCounter}, Extreme Yin Stacks: {player.extremeYinStack}/3)");

            // 激活反震效果
            player.ActivateCounterStrike(1.0f);
            return;
        }

        // 7. 平衡状态 (阳-阴的绝对值 = 0 or 1)
        if (Mathf.Abs(diff) <= 1)
        {
            currentStateName = "Balance";

            // 计算攻击防御值
            player.attack = Mathf.FloorToInt(config.balanceMultiplier * originalYang);
            player.defense = Mathf.FloorToInt(config.balanceMultiplier * originalYin);

            // 处理恢复效果
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

        // 默认状态
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