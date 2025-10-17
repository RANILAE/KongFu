using UnityEngine;

public class YinYangSystem : MonoBehaviour
{
    private BattleConfig config;
    private int balanceHealCooldown = 0;
    private bool ultimateQiUsed = false;

    public void Initialize(BattleConfig config)
    {
        this.config = config;
        balanceHealCooldown = 0;
        ultimateQiUsed = false;
        Debug.Log("YinYangSystem initialized. Balance heal cooldown set to: " + balanceHealCooldown);
    }

    public void ApplyEffects(float yangPoints, float yinPoints)
    {
        float diff = yangPoints - yinPoints;
        float absDiff = Mathf.Abs(diff);

        // Ӧ�õ��������
        ApplyFinalAttributes(yangPoints, yinPoints, diff);

        // Ӧ��Ч�����ڲ�ֵ
        if (absDiff < 1f)
        {
            ApplyBalanceEffect(yangPoints, yinPoints);
        }
        else if (diff >= 1f && diff <= 2.5f)
        {
            ApplyCriticalYangEffect(yangPoints, yinPoints);
        }
        else if (diff <= -1f && diff >= -2.5f)
        {
            ApplyCriticalYinEffect(yangPoints, yinPoints);
        }
        else if (diff > 2.5f && diff < 5f)
        {
            ApplyYangProsperityEffect(yangPoints, yinPoints);
        }
        else if (diff < -2.5f && diff > -5f)
        {
            ApplyYinProsperityEffect(yangPoints, yinPoints);
        }
        else if (diff >= 5f && diff <= 7f)
        {
            ApplyExtremeYangEffect(yangPoints, yinPoints);
        }
        else if (diff <= -5f && diff >= -7f)
        {
            ApplyExtremeYinEffect(yangPoints, yinPoints);
        }
        else if (absDiff > 7f && absDiff <= 10f && !ultimateQiUsed)
        {
            ApplyUltimateQiEffect(yangPoints, yinPoints);
        }

        // ����UI��ʾ
        BattleSystem.Instance.uiManager.UpdateYinYangState(diff, GetCurrentStateName(diff));
    }

    private void ApplyFinalAttributes(float yangPoints, float yinPoints, float diff)
    {
        Vector2 multipliers = Vector2.one;
        string stateName = GetCurrentStateName(diff);

        // ����״̬ѡ���� (��ȫ����ͼƬ����)
        switch (stateName)
        {
            case "Balance":
                multipliers = new Vector2(1.25f, 1.25f);
                break;
            case "Critical Yang":
                multipliers = new Vector2(1.75f, 1.25f);
                break;
            case "Critical Yin":
                multipliers = new Vector2(1.25f, 1.75f);
                break;
            case "Yang Prosperity":
                multipliers = new Vector2(2.75f, 1.25f);
                break;
            case "Yin Prosperity":
                multipliers = new Vector2(1.0f, 2.5f);
                break;
            case "Extreme Yang":
                multipliers = new Vector2(4.5f, 0.5f);
                break;
            case "Extreme Yin":
                multipliers = new Vector2(1.0f, 3.0f);
                break;
            case "Ultimate Qi":
                multipliers = new Vector2(7.0f, 7.0f);
                break;
        }

        // ������������ (ʹ��float)
        float finalAttack = yangPoints * multipliers.x;
        float finalDefense = yinPoints * multipliers.y;

        // Ӧ�ø����
        BattleSystem.Instance.playerManager.ApplyAttributes(finalAttack, finalDefense);
    }

    public string GetCurrentStateName(float diff)
    {
        float absDiff = Mathf.Abs(diff);

        if (absDiff < 1f) return "Balance";
        if (diff >= 1f && diff <= 2.5f) return "Critical Yang";
        if (diff <= -1f && diff >= -2.5f) return "Critical Yin";
        if (diff > 2.5f && diff < 5f) return "Yang Prosperity";
        if (diff < -2.5f && diff > -5f) return "Yin Prosperity";
        if (diff >= 5f && diff <= 7f) return "Extreme Yang";
        if (diff <= -5f && diff >= -7f) return "Extreme Yin";
        if (absDiff > 7f && absDiff <= 10f) return "Ultimate Qi";

        return "Unknown State";
    }

    private void ApplyBalanceEffect(float yangPoints, float yinPoints)
    {
        BattleSystem.Instance.uiManager.UpdateBattleLog("Balance state activated!");

        // �������Ƿ���Ҫ��Ѫ��������Ѫ״̬��
        PlayerManager player = BattleSystem.Instance.playerManager;
        if (player.Health < player.MaxHealth)
        {
            Debug.Log($"Player health: {player.Health}/{player.MaxHealth}, CD: {balanceHealCooldown}");

            // �����ȴʱ��
            if (balanceHealCooldown <= 0)
            {
                // ��һָ�������ָ��������ֵ���������������ֵ��
                float healAmount = Mathf.Min(config.balanceHealAmount, player.MaxHealth - player.Health);
                if (healAmount > 0)
                {
                    BattleSystem.Instance.playerManager.Heal(healAmount);
                    BattleSystem.Instance.uiManager.UpdateBattleLog($"Player healed {healAmount:F0} HP!");

                    // ������ȴʱ�䣨ʹ�������е�CDֵ��
                    balanceHealCooldown = config.balanceHealCooldown;
                    Debug.Log($"Balance heal triggered. Setting cooldown to: {balanceHealCooldown}");
                }
                else
                {
                    BattleSystem.Instance.uiManager.UpdateBattleLog("Player is at full health, no healing needed!");
                }
            }
            else
            {
                BattleSystem.Instance.uiManager.UpdateBattleLog($"Balance heal effect on cooldown! ({balanceHealCooldown} turns left)");
            }
        }
        else
        {
            BattleSystem.Instance.uiManager.UpdateBattleLog("Player is at full health, balance heal skipped!");
        }
    }

    private void ApplyCriticalYangEffect(float yangPoints, float yinPoints)
    {
        BattleSystem.Instance.uiManager.UpdateBattleLog("Critical Yang state activated!");

        // ���з�ʩ������͸BUFF
        BattleSystem.Instance.effectManager.AddYangPenetrationStack();
        BattleSystem.Instance.uiManager.UpdateBattleLog("Enemy gained Yang Penetration BUFF!");
    }

    private void ApplyCriticalYinEffect(float yangPoints, float yinPoints)
    {
        BattleSystem.Instance.uiManager.UpdateBattleLog("Critical Yin state activated!");

        // ���з�ʩ��������BUFF
        BattleSystem.Instance.effectManager.AddYinCoverStack();
        BattleSystem.Instance.uiManager.UpdateBattleLog("Enemy gained Yin Cover BUFF!");
    }

    private void ApplyYangProsperityEffect(float yangPoints, float yinPoints)
    {
        BattleSystem.Instance.uiManager.UpdateBattleLog("Yang Prosperity state activated!");

        // ���������DOT�˺���������/2��
        float dotDamage = yangPoints / 2f;
        BattleSystem.Instance.effectManager.AddEnemyDotEffect(dotDamage, 2);
        BattleSystem.Instance.uiManager.UpdateBattleLog($"Enemy takes continuous damage: {dotDamage:F1}/turn, lasts 2 turns");
    }

    private void ApplyYinProsperityEffect(float yangPoints, float yinPoints)
    {
        BattleSystem.Instance.uiManager.UpdateBattleLog("Yin Prosperity state activated!");

        // �����Ч��
        BattleSystem.Instance.playerManager.ActivateCounterStrike(1.0f);
        BattleSystem.Instance.uiManager.UpdateBattleLog("Player gained Counter Strike effect!");
    }

    private void ApplyExtremeYangEffect(float yangPoints, float yinPoints)
    {
        BattleSystem.Instance.uiManager.UpdateBattleLog("Extreme Yang state activated!");

        // ��������
        int yangStacks = BattleSystem.Instance.effectManager.GetYangPenetrationStacks();
        if (yangStacks >= 3)
        {
            // ��������˺�������*2��
            float extraDamage = yangStacks * 2;
            float totalDamage = BattleSystem.Instance.playerManager.Attack + extraDamage;

            // �Ե�������˺�
            BattleSystem.Instance.enemyManager.TakeDamage(totalDamage);
            BattleSystem.Instance.uiManager.UpdateBattleLog($"Dealt Extreme Yang damage: {totalDamage:F1}!");

            // �����»غϹ�������
            BattleSystem.Instance.playerManager.NextTurnAttackDebuff = true;
            BattleSystem.Instance.uiManager.UpdateBattleLog("Next turn attack reduced by half!");

            // ���õ���
            BattleSystem.Instance.effectManager.ResetYangPenetrationStacks();
        }
        else
        {
            BattleSystem.Instance.uiManager.UpdateBattleLog("Insufficient Yang Penetration stacks (need 3)!");
        }
    }

    private void ApplyExtremeYinEffect(float yangPoints, float yinPoints)
    {
        BattleSystem.Instance.uiManager.UpdateBattleLog("Extreme Yin state activated!");

        // ��������
        int yinStacks = BattleSystem.Instance.effectManager.GetYinCoverStacks();
        if (yinStacks >= 3)
        {
            // ������ǿ����1.5����
            BattleSystem.Instance.playerManager.ActivateCounterStrike(1.5f);
            BattleSystem.Instance.uiManager.UpdateBattleLog("Player gained enhanced Counter Strike (1.5x)!");

            // ���ٵ��˹�����������*2��
            float attackReduction = yinStacks * 2;
            BattleSystem.Instance.effectManager.ApplyEnemyAttackDebuff(attackReduction, 2);
            BattleSystem.Instance.uiManager.UpdateBattleLog($"Enemy attack reduced by {attackReduction:F1}, lasts 2 turns!");

            // �����»غϷ�������
            BattleSystem.Instance.playerManager.NextTurnDefenseDebuff = true;
            BattleSystem.Instance.uiManager.UpdateBattleLog("Next turn defense reduced by half!");

            // ���õ���
            BattleSystem.Instance.effectManager.ResetYinCoverStacks();
        }
        else
        {
            BattleSystem.Instance.uiManager.UpdateBattleLog("Insufficient Yin Cover stacks (need 3)!");
        }
    }

    private void ApplyUltimateQiEffect(float yangPoints, float yinPoints)
    {
        if (ultimateQiUsed)
        {
            BattleSystem.Instance.uiManager.UpdateBattleLog("Ultimate Qi can only be used once per battle!");
            return;
        }

        BattleSystem.Instance.uiManager.UpdateBattleLog("Ultimate Qi state activated!");

        // �����������ֵΪ1
        BattleSystem.Instance.playerManager.SetHealth(1);
        BattleSystem.Instance.uiManager.UpdateBattleLog("Player health set to 1!");

        // �����ʹ��
        ultimateQiUsed = true;
    }

    // ÿ�غϽ���ʱ����
    public void ProcessCooldowns()
    {
        if (balanceHealCooldown > 0)
        {
            balanceHealCooldown--;
            Debug.Log($"YinYangSystem - Balance heal cooldown decreased to: {balanceHealCooldown}");
        }
    }

    // ��������ȡ��ǰCDֵ�����ڵ��ԣ�
    public int GetBalanceHealCooldown()
    {
        return balanceHealCooldown;
    }

    // ����������CD�����ڵ��ԣ�
    public void ResetBalanceHealCooldown()
    {
        balanceHealCooldown = 0;
        Debug.Log("YinYangSystem - Balance heal cooldown reset to 0");
    }
}