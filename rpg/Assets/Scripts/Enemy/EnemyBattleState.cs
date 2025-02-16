using UnityEngine;
using System.Collections.Generic;

public class EnemyBattleState : EnemyState
{
    private float _attackTime;

    enum BattleState
    {
        main,
        ability1,
    }
    private BattleState _battleState;


    // pass in any parameters you need in the constructors
    public EnemyBattleState(Enemy enemy, EnemyStateMachine enemyStateMachine) : base(enemy, enemyStateMachine)
    {
        // this.enemy = enemy;
    }

    // code that runs when we first enter the state
    public override void EnterState()
    {
        base.EnterState();

        Debug.Log(enemy.name + " is attacking.");

        // enemy.Weapon.SetActive(true);

        // enemy run
        enemy.MoveSpeed = enemy.MaxSpeed*1.5f;

        enemy.Anim.SetLayerWeight(1, 1);

    }

    // code that runs when we exit the state
    public override void ExitState()
    {
        base.ExitState();

        // restore speed
        enemy.MoveSpeed = enemy.MaxSpeed;

        enemy.Anim.SetLayerWeight(1, 0);
    
    }

    public override void FrameUpdate()
    {
        base.FrameUpdate();

        // Debug.Log(enemy.name + " attacking...");

        // TODO: add jump/dash attack for slime

        // allow attacking
        Attack();

        // exit battle
        if (!enemy.CheckPlayerWithinRange(enemy.SightRadius))
            enemy.StateMachine.ChangeState(enemy.IdleState);
    }

    private void Attack()
    {
        switch (_battleState)
        {
            case BattleState.main:
                if (!enemy.CheckPlayerWithinRange(enemy.AttackRadius))
                {
                    // chase player
                    Chase();
                }
                else
                {
                    // main combo attack
                    ComboAttack();
                }
            break;
            case BattleState.ability1:
                // abilities/arts ie. dash or jump
            break;
        }
    }

    private void Chase()
    {
        enemy.MoveEnemy(enemy.Player.transform.position - enemy.transform.position);
    }

    private void ComboAttack()
    {
        if (Time.time > _attackTime + enemy.AttackDelay)
        {
            enemy.Attack();
            _attackTime = Time.time;
        }
    }

}

