using UnityEngine;
using UnityEngine.Events;

public static class BattleEventSystem
{
    // 基础战斗事件
    public static UnityEvent OnPlayerTurnStart = new UnityEvent();
    public static UnityEvent OnEnemyTurnStart = new UnityEvent();
    public static UnityEvent OnBattleEnd = new UnityEvent();
    public static UnityEvent<int> OnDamageCalculated = new UnityEvent<int>();
    public static UnityEvent<string> OnBattleLog = new UnityEvent<string>();

    // 音效事件
    public static UnityEvent<AudioClip> OnPlaySound = new UnityEvent<AudioClip>();

    // 清除所有事件
    public static void ClearAllEvents()
    {
        OnPlayerTurnStart.RemoveAllListeners();
        OnEnemyTurnStart.RemoveAllListeners();
        OnBattleEnd.RemoveAllListeners();
        OnDamageCalculated.RemoveAllListeners();
        OnBattleLog.RemoveAllListeners();
        OnPlaySound.RemoveAllListeners();
    }
}