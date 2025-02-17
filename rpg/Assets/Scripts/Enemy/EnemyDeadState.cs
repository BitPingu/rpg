using UnityEngine;

public class EnemyDeadState : EnemyState
{
    // pass in any parameters you need in the constructors
    public EnemyDeadState(Enemy enemy, EnemyStateMachine enemyStateMachine) : base(enemy, enemyStateMachine)
    {
        // this.enemy = enemy;
    }

    // code that runs when we first enter the state
    public override void EnterState()
    {
        base.EnterState();

        Debug.Log(enemy.name + " is dead.");

        Die();
    }

    // code that runs when we exit the state
    public override void ExitState()
    {
        base.ExitState();
    }

    public override void FrameUpdate()
    {
        base.FrameUpdate();
    }

    private void Die()
    {
        enemy.Movement = Vector3.zero;
        enemy.Anim.SetFloat("Movement", enemy.Movement.magnitude);

        enemy.Controller.enabled = false;

        if (enemy.Anim.GetLayerWeight(1) == 1)
            enemy.Anim.SetLayerWeight(1, 0);

        // Animate death
        enemy.Anim.SetTrigger("Die");

        // timed deactivation
        enemy.Die();
    }

}

