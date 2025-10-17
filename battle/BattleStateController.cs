using UnityEngine;

public class BattleStateController : MonoBehaviour
{
    public enum BattleState { PlayerTurn, EnemyTurn, BattleEnd }
    public BattleState currentState { get; private set; }

    private PlayerData player;
    private EnemyData enemy;
    private YinYangSystem yinYangSystem;

    public void Initialize(PlayerData playerData, EnemyData enemyData, YinYangSystem yinYangSys)
    {
        player = playerData;
        enemy = enemyData;
        yinYangSystem = yinYangSys;
    }

    public void ChangeState(BattleState newState)
    {
        currentState = newState;
    }
}