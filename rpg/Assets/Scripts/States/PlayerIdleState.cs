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

        // enter battle
        if (player.Input.LeftClick)
            player.StateMachine.ChangeState(player.BattleState);
    }

}

