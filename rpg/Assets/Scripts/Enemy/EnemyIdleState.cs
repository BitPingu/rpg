using UnityEngine;

public class EnemyIdleState : EnemyState
{
    private float _idleTime;
    private float _moveTime;
    private Vector3 _currentDirection;

    enum IdleState
    {
        idle,
        moving
    }
    private IdleState _idleState;


    // pass in any parameters you need in the constructors
    public EnemyIdleState(Enemy enemy, EnemyStateMachine enemyStateMachine) : base(enemy, enemyStateMachine)
    {
        // this.enemy = enemy;
    }

    // code that runs when we first enter the state
    public override void EnterState()
    {
        base.EnterState();

        Debug.Log(enemy.name + " is idle.");

        // enemy.Weapon.SetActive(false);

        _idleTime = enemy.IdleTime;
        _moveTime = enemy.MoveTime;

        _idleState = IdleState.idle;

        enemy.Movement = Vector3.zero;
    }

    // code that runs when we exit the state
    public override void ExitState()
    {
        base.ExitState();
    }

    public override void FrameUpdate()
    {
        base.FrameUpdate();

        // Debug.Log(enemy.name + " idling...");

        // allow idling
        Idle();

        // chase player
        if (enemy.CheckPlayerWithinRange(enemy.SightRadius))
            enemy.StateMachine.ChangeState(enemy.BattleState);
    }

    private void Idle()
    {
        switch (_idleState)
        {
            case IdleState.idle:
                if (_idleTime > 0)
                {
                    _idleTime -= Time.deltaTime;
                }
                else
                {
                    _moveTime = enemy.MoveTime;
                    _currentDirection = GetRandomDirection();
                    _idleState = IdleState.moving;
                    // Debug.Log(enemy.name + " is moving.");
                }
            break;
            case IdleState.moving:
                if (_moveTime > 0)
                {
                    enemy.MoveEnemy(_currentDirection);
                    _moveTime -= Time.deltaTime;
                }
                else
                {
                    _idleTime = enemy.IdleTime;
                    enemy.Movement = Vector3.zero;
                    _idleState = IdleState.idle;
                    // Debug.Log(enemy.name + " is not moving.");
                }
            break;
        }
    }

    private Vector3 GetRandomDirection()
    {
        return new Vector3(Random.Range(-1f, 1f), 0f, Random.Range(-1f, 1f));
    }

}

