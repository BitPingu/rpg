using UnityEngine;

public class EnemyAttackState : EnemyState
{
    // pass in any parameters you need in the constructors
    public EnemyAttackState(Enemy enemy, EnemyStateMachine enemyStateMachine) : base(enemy, enemyStateMachine)
    {
        // this.enemy = enemy;
    }

    // code that runs when we first enter the state
    public override void EnterState()
    {
        base.EnterState();

        Debug.Log(enemy.name + " is attacking.");

        enemy.Anim.SetLayerWeight(1, 1);

    }

    // code that runs when we exit the state
    public override void ExitState()
    {
        base.ExitState();

        enemy.Anim.SetLayerWeight(1, 0);
    
    }

    public override void FrameUpdate()
    {
        base.FrameUpdate();

        // Debug.Log(enemy.name + " attacking...");

        // allow main combo attack
        ComboAttack();

        // exit battle
        if (!enemy.CheckPlayerWithinRange(enemy.AttackRadius))
            enemy.StateMachine.ChangeState(enemy.ChaseState);
    }

    private void ComboAttack()
    {
        // Animate attack
        enemy.Anim.SetFloat("Attack", 1);

        // Animate attack speed
        enemy.Anim.SetFloat("Attack Speed", enemy.AttackSpeed);
    }

}

