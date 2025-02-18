using UnityEngine;

public class CompanionIdleState : CompanionState
{
    // pass in any parameters you need in the constructors
    public CompanionIdleState(Companion companion, CompanionStateMachine companionStateMachine) : base(companion, companionStateMachine)
    {
        // this.companion = companion;
    }

    // code that runs when we first enter the state
    public override void EnterState()
    {
        base.EnterState();

        // Debug.Log(companion.name + " is idle.");

        // put weapon on back
        companion.Weapon.transform.SetParent(companion.transform.Find("root/pelvis/spine_01/spine_02/spine_03/BackpackBone").transform);
        companion.Weapon.transform.localRotation = Quaternion.Euler(-90f, 0f, 42.983f);
        companion.Weapon.transform.localPosition = new Vector3(0.045f,-0.05f, -0.2f);
    }

    // code that runs when we exit the state
    public override void ExitState()
    {
        base.ExitState();
    }

    public override void FrameUpdate()
    {
        base.FrameUpdate();

        // Debug.Log(companion.name + " idling...");

        // follow player
        companion.FollowPlayer();

        // enter battle
        if (companion.Player.Input.Q)
            companion.StateMachine.ChangeState(companion.BattleState);

        // companion dies
        if (companion.CurrentHealth <= 0f)
            companion.StateMachine.ChangeState(companion.DeadState);
    }

}

