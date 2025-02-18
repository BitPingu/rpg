using UnityEngine;

public class PlayerIdleState : PlayerState
{
    // pass in any parameters you need in the constructors
    public PlayerIdleState(Player player, PlayerStateMachine playerStateMachine) : base(player, playerStateMachine)
    {
        // this.player = player;
    }

    // code that runs when we first enter the state
    public override void EnterState()
    {
        base.EnterState();

        // Debug.Log(player.name + " is idle.");

        // put weapon on back
        player.Weapon.transform.SetParent(player.transform.Find("root/pelvis/spine_01/spine_02/spine_03/BackpackBone").transform);
        player.Weapon.transform.localRotation = Quaternion.Euler(-90f, 0f, 42.983f);
        player.Weapon.transform.localPosition = new Vector3(0.456f, -0.051f, 0.241f);
    }

    // code that runs when we exit the state
    public override void ExitState()
    {
        base.ExitState();
    }

    public override void FrameUpdate()
    {
        base.FrameUpdate();

        // Debug.Log(player.name + " idling...");

        // TODO: allow interaction with world ie. chests, items, dialogue
        // TODO: cannot do this in battle state


        // enter battle
        if (player.Input.Q)
            player.StateMachine.ChangeState(player.BattleState);

        // player dies
        if (player.CurrentHealth <= 0f)
            player.StateMachine.ChangeState(player.DeadState);
    }

}

