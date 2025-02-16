using UnityEngine;
using System.Collections.Generic;
using System;

public class EnemyBattleState : EnemyState
{
    private Player _currentOpponent;
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

        _currentOpponent = enemy.Opponents[0].GetComponent<Player>();

        Debug.Log(enemy.name + " is attacking " + _currentOpponent.name + ".");

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
        if (!enemy.SeeOpponents() || _currentOpponent.CurrentHealth == 0)
            enemy.StateMachine.ChangeState(enemy.IdleState);
    }

    private void Attack()
    {
        switch (_battleState)
        {
            case BattleState.main:
                if (!OpponentInRange())
                {
                    // chase opponent
                    Chase();
                }
                else
                {
                    // always face opponent
                    enemy.FaceOpponent(_currentOpponent);

                    // main combo attack
                    ComboAttack();
                }
            break;
            case BattleState.ability1:
                // abilities/arts ie. dash or jump
            break;
        }
    }

    private bool OpponentInRange()
    {
        return Vector3.Distance(_currentOpponent.transform.position, enemy.transform.position) <= enemy.AttackRadius;
    }

    private void Chase()
    {
        enemy.MoveEnemy(_currentOpponent.transform.position - enemy.transform.position);
    }

    private void ComboAttack()
    {
        if (Time.time > _attackTime + enemy.AttackDelay)
        {
            enemy.Attack(_currentOpponent);
            _attackTime = Time.time;
        }
    }

}

