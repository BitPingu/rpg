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

        Debug.Log(player.name + " is idle.");

        player.Weapon.SetActive(false);
        player.WeaponBack.SetActive(true);
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
        if (player.Input.E)
            player.StateMachine.ChangeState(player.BattleState);
    }

}

