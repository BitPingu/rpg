using UnityEngine;

public class EnemyChaseState : EnemyState
{
    // pass in any parameters you need in the constructors
    public EnemyChaseState(Enemy enemy, EnemyStateMachine enemyStateMachine) : base(enemy, enemyStateMachine)
    {
        // this.enemy = enemy;
    }

    // code that runs when we first enter the state
    public override void EnterState()
    {
        base.EnterState();

        Debug.Log(enemy.name + " is chasing.");

        // enemy.Weapon.SetActive(false);
        // enemy.WeaponBack.SetActive(true);

        // enemy run
        enemy.MoveSpeed = enemy.MaxSpeed*1.5f;
    }

    // code that runs when we exit the state
    public override void ExitState()
    {
        base.ExitState();

        // restore speed
        enemy.MoveSpeed = enemy.MaxSpeed;
    }

    public override void FrameUpdate()
    {
        base.FrameUpdate();

        // Debug.Log(enemy.name + " chasing...");

        // allow chasing
        Chase();

        // stop chasing
        if (!enemy.CheckPlayerWithinRange(enemy.SightRadius))
            enemy.StateMachine.ChangeState(enemy.IdleState);

        // enter battle
        if (enemy.CheckPlayerWithinRange(enemy.AttackRadius))
            enemy.StateMachine.ChangeState(enemy.AttackState);
    }

    private void Chase()
    {
        enemy.MoveEnemy(enemy.Player.transform.position - enemy.transform.position);
    }

}

