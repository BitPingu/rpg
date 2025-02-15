
public class EnemyStateMachine
{
    public EnemyState CurrentEnemyState { get; set; }

    // set the starting state
    public void Initialize(EnemyState startingState)
    {
        CurrentEnemyState = startingState;
        CurrentEnemyState.EnterState();
    }

    // exit this state and enter another
    public void ChangeState(EnemyState newState)
    {
        CurrentEnemyState.ExitState();
        CurrentEnemyState = newState;
        CurrentEnemyState.EnterState();
    }
}

