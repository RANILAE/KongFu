using UnityEngine;
using UnityEngine.UI; // ȷ�������������ռ�

public class YinYangSystem : MonoBehaviour
{
    private BattleConfig config;
    private int balanceHealCooldown = 0;
    private bool ultimateQiUsed = false;

    // ����������׷�١��ٽ������͡��ٽ�����״̬�������Ĵ���
    private int criticalYangTriggerCount = 0;
    private int criticalYinTriggerCount = 0;

    // �������������Ƿ��ѽ��������������͡�������������
    private bool isExtremeYangUnlocked = false;
    private bool isExtremeYinUnlocked = false;

    public void Initialize(BattleConfig config)
    {
        this.config = config;
        balanceHealCooldown = 0;
        ultimateQiUsed = false;
        // ��ʼ���������ͽ���״̬
        criticalYangTriggerCount = 0;
        criticalYinTriggerCount = 0;
        isExtremeYangUnlocked = false;
        isExtremeYinUnlocked = false;
        Debug.Log("YinYangSystem initialized. Balance heal cooldown set to: " + balanceHealCooldown);
    }

    public void ApplyEffects(float yangPoints, float yinPoints)
    {
        float diff = yangPoints - yinPoints;
        float absDiff = Mathf.Abs(diff);

        // Ӧ�õ��������
        ApplyFinalAttributes(yangPoints, yinPoints, diff);

        // Ӧ��Ч�����ڲ�ֵ - �ϸ����¹���
        if (absDiff < 1f)
        {
            ApplyBalanceEffect(yangPoints, yinPoints);
        }
        else if (diff >= 1f && diff <= 2.5f)
        {
            // ���롰�ٽ�����״̬
            criticalYangTriggerCount++;
            CheckAndUnlockExtremeYang();
            ApplyCriticalYangEffect(yangPoints, yinPoints);
        }
        else if (diff <= -1f && diff >= -2.5f)
        {
            // ���롰�ٽ�����״̬
            criticalYinTriggerCount++;
            CheckAndUnlockExtremeYin();
            ApplyCriticalYinEffect(yangPoints, yinPoints);
        }
        else if (diff > 2.5f && diff < 5f)
        {
            ApplyYangProsperityEffect(yangPoints, yinPoints); // DOT
        }
        else if (diff < -2.5f && diff > -5f)
        {
            ApplyYinProsperityEffect(yangPoints, yinPoints); // ����
        }
        else if (diff >= 5f && diff <= 7f)
        {
            // ����Ƿ��ѽ�������������
            if (isExtremeYangUnlocked)
            {
                ApplyExtremeYangEffect(yangPoints, yinPoints); // �˺���Debuff
            }
            else
            {
                BattleSystem.Instance.uiManager.UpdateBattleLog("Extreme Yang state activated! (Requires 3 Critical Yang triggers to unlock full effect)");
                BattleSystem.Instance.uiManager.ShowStackInsufficientPanel($"Cannot use Extreme Yang yet! Requires 3 Critical Yang uses. Current: {criticalYangTriggerCount}/3.");
            }
        }
        else if (diff <= -5f && diff >= -7f)
        {
            // ����Ƿ��ѽ�������������
            if (isExtremeYinUnlocked)
            {
                ApplyExtremeYinEffect(yangPoints, yinPoints); // ����, Debuff, Debuff
            }
            else
            {
                BattleSystem.Instance.uiManager.UpdateBattleLog("Extreme Yin state activated! (Requires 3 Critical Yin triggers to unlock full effect)");
                BattleSystem.Instance.uiManager.ShowStackInsufficientPanel($"Cannot use Extreme Yin yet! Requires 3 Critical Yin uses. Current: {criticalYinTriggerCount}/3.");
            }
        }
        else if (absDiff > 7f && absDiff <= 10f && !ultimateQiUsed)
        {
            ApplyUltimateQiEffect(yangPoints, yinPoints); // Ѫ��, ����, ����
        }

        // ����UI��ʾ
        BattleSystem.Instance.uiManager.UpdateYinYangState(diff, GetCurrentStateName(diff));
    }

    // ��������鲢���¡�������������״̬
    private void CheckAndUnlockExtremeYang()
    {
        if (criticalYangTriggerCount >= 3 && !isExtremeYangUnlocked)
        {
            isExtremeYangUnlocked = true;
            Debug.Log("Extreme Yang ability unlocked!");
            BattleSystem.Instance.uiManager.UpdateBattleLog("Extreme Yang ability unlocked! Full effect now available.");
        }
    }

    // ��������鲢���¡�������������״̬
    private void CheckAndUnlockExtremeYin()
    {
        if (criticalYinTriggerCount >= 3 && !isExtremeYinUnlocked)
        {
            isExtremeYinUnlocked = true;
            Debug.Log("Extreme Yin ability unlocked!");
            BattleSystem.Instance.uiManager.UpdateBattleLog("Extreme Yin ability unlocked! Full effect now available.");
        }
    }

    private void ApplyFinalAttributes(float yangPoints, float yinPoints, float diff)
    {
        Vector2 multipliers = Vector2.one;
        string stateName = GetCurrentStateName(diff);

        // ����״̬ѡ���� (��ȫ����ͼƬ����)
        switch (stateName)
        {
            case "Balance":
                multipliers = config.balanceMultipliers;
                break;
            case "Critical Yang":
                multipliers = config.criticalYangMultipliers;
                break;
            case "Critical Yin":
                multipliers = config.criticalYinMultipliers;
                break;
            case "Yang Prosperity":
                multipliers = config.yangProsperityMultipliers;
                break;
            case "Yin Prosperity":
                multipliers = config.yinProsperityMultipliers;
                break;
            case "Extreme Yang":
                multipliers = config.extremeYangMultipliers; // ʹ�����õ� 4.5, 0.5
                break;
            case "Extreme Yin":
                multipliers = config.extremeYinMultipliers; // ʹ�����õ� 1.0, 3.0
                break;
            case "Ultimate Qi":
                multipliers = config.ultimateQiMultipliers;
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

        // Ч���������Ч�������ڱ��غ���Ч��
        // ����ʱ��ӦΪ1�غϣ��ڵ�ǰ�غϵĵ��˹�������Ч����һ�غϿ�ʼǰʧЧ
        // PlayerManager.ResetForNewTurn �ᴦ�� Duration--
        BattleSystem.Instance.playerManager.ActivateCounterStrike(1.0f, 1); // ����1.0������1�غ�
        BattleSystem.Instance.uiManager.UpdateBattleLog($"Player gained Counter Strike effect for this turn!");
    }


    private void ApplyExtremeYangEffect(float yangPoints, float yinPoints)
    {
        BattleSystem.Instance.uiManager.UpdateBattleLog("Extreme Yang state activated! (Full effect applied)");

        // ��������˺�������*2��
        // ע�⣺����ʹ�� EffectManager �� GetYangPenetrationStacks() ������ȡ��ǰ����
        int yangStacks = BattleSystem.Instance.effectManager.GetYangPenetrationStacks();
        if (yangStacks > 0) // ֻ�����в���ʱ��Ӧ���˺�
        {
            float extraDamage = yangStacks * config.extremeYangBonusPerStack; // ʹ�����õ�ֵ
            float totalDamage = BattleSystem.Instance.playerManager.Attack + extraDamage;

            // �Ե�������˺�
            BattleSystem.Instance.enemyManager.TakeDamage(totalDamage);
            BattleSystem.Instance.uiManager.UpdateBattleLog($"Dealt Extreme Yang damage: {totalDamage:F1}!");
        }
        else
        {
            BattleSystem.Instance.uiManager.UpdateBattleLog($"No Yang Penetration stacks, no extra damage dealt.");
        }


        // �����»غϹ������� (����һ����ǣ���PlayerManager���»غϿ�ʼʱ����)
        BattleSystem.Instance.playerManager.NextTurnAttackDebuff = true;
        BattleSystem.Instance.uiManager.UpdateBattleLog("Next turn attack will be reduced by half!");

        // ���õ���
        BattleSystem.Instance.effectManager.ResetYangPenetrationStacks();
    }

    private void ApplyExtremeYinEffect(float yangPoints, float yinPoints)
    {
        BattleSystem.Instance.uiManager.UpdateBattleLog("Extreme Yin state activated! (Full effect applied)");

        // Ч��1��������ǿ����1.5���˺��߼���CounterStrikeSystem�д�������״̬�жϣ�����ֻ�輤�
        // ����ʱ��ӦΪ1�غ�
        BattleSystem.Instance.playerManager.ActivateCounterStrike(1.5f, 1); // �������1�غ�
        BattleSystem.Instance.uiManager.UpdateBattleLog($"Player gained enhanced Counter Strike for this turn!");

        // Ч��2�����ٵ��˹�����������*2������2�غϣ�
        int yinStacks = BattleSystem.Instance.effectManager.GetYinCoverStacks();
        if (yinStacks > 0) // ֻ�����в���ʱ��Ӧ��
        {
            float attackReduction = yinStacks * config.extremeYinAttackReducePerStack;
            BattleSystem.Instance.effectManager.ApplyEnemyAttackDebuff(attackReduction, config.extremeDebuffDuration);
            BattleSystem.Instance.uiManager.UpdateBattleLog($"Enemy attack reduced by {attackReduction:F1}, lasts {config.extremeDebuffDuration} turns!");
        }
        else
        {
            BattleSystem.Instance.uiManager.UpdateBattleLog($"No Yin Cover stacks, no enemy attack reduction applied.");
        }

        // Ч��3�������»غϷ������� (����һ����ǣ���PlayerManager���»غϿ�ʼʱ����)
        BattleSystem.Instance.playerManager.NextTurnDefenseDebuff = true;
        BattleSystem.Instance.uiManager.UpdateBattleLog("Next turn defense will be reduced by half!");

        // Ч��4�����õ���
        BattleSystem.Instance.effectManager.ResetYinCoverStacks();
    }

    private void ApplyUltimateQiEffect(float yangPoints, float yinPoints)
    {
        if (ultimateQiUsed)
        {
            BattleSystem.Instance.uiManager.UpdateBattleLog("Ultimate Qi can only be used once per battle!");
            return;
        }

        BattleSystem.Instance.uiManager.UpdateBattleLog("Ultimate Qi state activated!");

        // Ч��1�������������ֵΪ1
        BattleSystem.Instance.playerManager.SetHealth(config.ultimateQiHealthSet);
        BattleSystem.Instance.uiManager.UpdateBattleLog("Player health set to 1!");

        // Ч��2�����㾿����״̬�µķ�������7*��������
        float qiDefense = 7 * yinPoints;

        // ȷ������������Ϊ15
        if (qiDefense < 15f)
        {
            qiDefense = 15f;
            BattleSystem.Instance.uiManager.UpdateBattleLog("Qi defense boosted to minimum 15!");
        }

        // ��ʱ������ҷ�����Ϊ������������ (���ֵ����PlayerManager.ResetForNewTurn�б����ã�������NextTurnDebuff)
        BattleSystem.Instance.playerManager.Defense = qiDefense;
        BattleSystem.Instance.uiManager.UpdatePlayerAttackDefense(BattleSystem.Instance.playerManager.Attack, BattleSystem.Instance.playerManager.Defense);

        // Ч��3����÷���Ч��������3�غϣ�
        BattleSystem.Instance.playerManager.ActivateCounterStrike(1.0f, config.ultimateQiCounterStrikeDuration); // ����3�غ�
        BattleSystem.Instance.uiManager.UpdateBattleLog($"Player gained Counter Strike effect for {config.ultimateQiCounterStrikeDuration} turns!");

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

    // ��������ȡ���ٽ������������������ڵ��Ի�UI��
    public int GetCriticalYangTriggerCount()
    {
        return criticalYangTriggerCount;
    }

    // ��������ȡ���ٽ������������������ڵ��Ի�UI��
    public int GetCriticalYinTriggerCount()
    {
        return criticalYinTriggerCount;
    }

    // ��������顰���������Ƿ��ѽ��������ڵ��Ի�UI��
    public bool IsExtremeYangUnlocked()
    {
        return isExtremeYangUnlocked;
    }

    // ��������顰���������Ƿ��ѽ��������ڵ��Ի�UI��
    public bool IsExtremeYinUnlocked()
    {
        return isExtremeYinUnlocked;
    }
}
