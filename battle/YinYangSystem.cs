using UnityEngine;

public class YinYangSystem : MonoBehaviour
{
    private string currentStateName = "Default";
    private int balanceHealCooldown = 0;

    public void ApplyBattleEffects(PlayerData player, EnemyData enemy, BattleConfig config)
    {
        // Reset combat effects
        player.counterStrikeActive = false;
        player.counterStrikeMultiplier = 1.0f;

        int yinPoints = player.defense;
        int yangPoints = player.attack;
        int diff = yangPoints - yinPoints;

        int originalYin = yinPoints;
        int originalYang = yangPoints;

        // 1. Extreme Yang (Yang - Yin >= 5)
        if (diff >= 5)
        {
            currentStateName = "Extreme Yang";

            // Calculate combat stats
            player.attack = Mathf.FloorToInt(config.extremeYangAttackMultiplier * originalYang);
            player.defense = Mathf.FloorToInt(config.extremeYangDefenseMultiplier * originalYin);

            // Check critical yang counter
            if (player.yangCriticalCounter >= config.criticalStacksRequired)
            {
                // Explode yang penetrate stacks
                int yangDamage = enemy.yangPenetrateStacks * originalYang;
                enemy.health = Mathf.Max(0, enemy.health - yangDamage);
                BattleEventSystem.OnBattleLog.Invoke($"Exploded Yang Penetration! Dealt {yangDamage} damage");

                // Set next turn debuff
                player.nextTurnAttackDebuff = true;
                BattleEventSystem.OnBattleLog.Invoke("Extreme Yang activated! Player attack halved next turn");

                // Reset counters
                player.ResetCriticalCounters();
                enemy.yangPenetrateStacks = 0;
            }
            else
            {
                // Not enough critical stacks
                int required = config.criticalStacksRequired - player.yangCriticalCounter;
                BattleEventSystem.OnBattleLog.Invoke($"Needs {required} more Critical Yang usages for explosion");
                currentStateName = "Extreme Yang (Inactive)";
            }
            return;
        }

        // 2. Extreme Yin (Yin - Yang >= 5)
        if (diff <= -5)
        {
            currentStateName = "Extreme Yin";

            // Calculate combat stats
            player.attack = originalYang;
            player.defense = Mathf.FloorToInt(config.extremeYinDefenseMultiplier * originalYin);

            // Activate counter strike
            player.ActivateCounterStrike(1.5f);

            // Check critical yin counter
            if (player.yinCriticalCounter >= config.criticalStacksRequired)
            {
                // Explode yin cover stacks
                int attackReduction = enemy.yinCoverStacks * 2;
                enemy.baseAttack = Mathf.Max(0, enemy.baseAttack - attackReduction);
                enemy.currentAttack = enemy.baseAttack;
                BattleEventSystem.OnBattleLog.Invoke($"Exploded Yin Cover! Enemy ATK reduced by {attackReduction}");

                // Set next turn debuff
                player.nextTurnDefenseDebuff = true;
                BattleEventSystem.OnBattleLog.Invoke("Extreme Yin activated! Player defense halved next turn");

                // Reset counters
                player.ResetCriticalCounters();
                enemy.yinCoverStacks = 0;
            }
            else
            {
                // Not enough critical stacks
                int required = config.criticalStacksRequired - player.yinCriticalCounter;
                BattleEventSystem.OnBattleLog.Invoke($"Needs {required} more Critical Yin usages for explosion");
                currentStateName = "Extreme Yin (Inactive)";
            }
            return;
        }

        // 3. Yang Sheng (Yang - Yin = 3 or 4)
        if (diff >= 3 && diff <= 4)
        {
            currentStateName = "Yang Sheng";

            // Calculate combat stats
            player.attack = Mathf.FloorToInt(config.yangShengAttackMultiplier * originalYang);
            player.defense = Mathf.FloorToInt(config.yangShengDefenseMultiplier * originalYin);

            // Apply DOT
            int dotDamage = Mathf.FloorToInt(originalYang / 2f);
            if (dotDamage > 0)
            {
                player.activeDots.Add(new PlayerData.DotEffect
                {
                    damage = dotDamage,
                    duration = 2
                });
                BattleEventSystem.OnBattleLog.Invoke($"Applied Yang Sheng DOT: {dotDamage} damage per turn for 2 turns");
            }
            return;
        }

        // 4. Yin Sheng (Yin - Yang = 3 or 4)
        if (diff <= -3 && diff >= -4)
        {
            currentStateName = "Yin Sheng";

            // Calculate combat stats
            player.attack = originalYang;
            player.defense = Mathf.FloorToInt(config.yinShengDefenseMultiplier * originalYin);

            // Activate counter strike
            player.ActivateCounterStrike(1.0f);
            BattleEventSystem.OnBattleLog.Invoke("Yin Sheng activated! Counterstrike effect applied");
            return;
        }

        // 5. Critical Yang (Yang - Yin = 2)
        if (diff == 2)
        {
            currentStateName = "Critical Yang";

            // Calculate combat stats
            player.attack = Mathf.FloorToInt(config.criticalYangAttackMultiplier * originalYang);
            player.defense = Mathf.FloorToInt(config.criticalYangDefenseMultiplier * originalYin);

            // Apply buffs and increment counter
            enemy.yangPenetrateStacks++;
            player.IncrementYangCriticalCounter();
            BattleEventSystem.OnBattleLog.Invoke($"Applied Yang Penetration! (Stacks: {enemy.yangPenetrateStacks}, Counter: {player.yangCriticalCounter}/{config.criticalStacksRequired})");
            return;
        }

        // 6. Critical Yin (Yin - Yang = 2)
        if (diff == -2)
        {
            currentStateName = "Critical Yin";

            // Calculate combat stats
            player.attack = Mathf.FloorToInt(config.criticalYinAttackMultiplier * originalYang);
            player.defense = Mathf.FloorToInt(config.criticalYinDefenseMultiplier * originalYin);

            // Apply buffs and increment counter
            enemy.yinCoverStacks++;
            player.IncrementYinCriticalCounter();
            BattleEventSystem.OnBattleLog.Invoke($"Applied Yin Cover! (Stacks: {enemy.yinCoverStacks}, Counter: {player.yinCriticalCounter}/{config.criticalStacksRequired})");

            // Activate counter strike
            player.ActivateCounterStrike(1.0f);
            return;
        }

        // 7. Balance (|Yang - Yin| <= 1)
        if (Mathf.Abs(diff) <= 1)
        {
            currentStateName = "Balance";

            // Calculate combat stats
            player.attack = Mathf.FloorToInt(config.balanceMultiplier * originalYang);
            player.defense = Mathf.FloorToInt(config.balanceMultiplier * originalYin);

            // Handle healing (every 2 turns)
            if (balanceHealCooldown == 0)
            {
                player.health = Mathf.Min(player.health + 5, player.maxHealth);
                BattleEventSystem.OnBattleLog.Invoke("Balance healed 5 HP!");
                balanceHealCooldown = 2;
            }
            else
            {
                balanceHealCooldown--;
                BattleEventSystem.OnBattleLog.Invoke($"Balance heal cooldown: {balanceHealCooldown} turns left");
            }
            return;
        }

        // Default state
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