
public class CompanionStateMachine
{
    public CompanionState CurrentCompanionState { get; set; }

    // set the starting state
    public void Initialize(CompanionState startingState)
    {
        CurrentCompanionState = startingState;
        CurrentCompanionState.EnterState();
    }

    // exit this state and enter another
    public void ChangeState(CompanionState newState)
    {
        CurrentCompanionState.ExitState();
        CurrentCompanionState = newState;
        CurrentCompanionState.EnterState();
    }
}

