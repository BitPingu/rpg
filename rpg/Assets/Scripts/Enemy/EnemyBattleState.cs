using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EnemyBattleState : EnemyState
{
    private Player _currentOpponent;
    private float _attackTime;

    public TextMeshProUGUI _nameText;
    public TextMeshProUGUI _levelText;

    public TextMeshProUGUI HealthText;
    public HealthBar HBar;

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

        // setup health bar
        GameObject enemyUI = GameObject.Find("Enemy UI");
        enemyUI.transform.SetParent(enemy.transform);
        HBar = enemyUI.transform.Find("HealthBar").GetComponent<HealthBar>();

        HBar.SetMaxHealth(enemy.MaxHealth);
        HBar.gameObject.SetActive(false);

        _nameText = HBar.transform.Find("Name").GetComponent<TextMeshProUGUI>();
        _nameText.text = enemy.name.ToString();

        _levelText = HBar.transform.Find("Level").GetComponent<TextMeshProUGUI>();
        _levelText.text = enemy.Level.ToString();

        HealthText = HBar.transform.Find("Health").GetComponent<TextMeshProUGUI>();
        HealthText.text = enemy.MaxHealth.ToString();
    }

    // code that runs when we first enter the state
    public override void EnterState()
    {
        base.EnterState();

        _currentOpponent = enemy.Opponents[0].GetComponent<Player>();

        Debug.Log(enemy.name + " is attacking " + _currentOpponent.name + ".");

        // enemy.Weapon.SetActive(true);

        enemy.Anim.SetLayerWeight(1, 1);

        // enemy run
        enemy.MoveSpeed = enemy.MaxSpeed*1.5f;

        // show ui
        HBar.gameObject.SetActive(true);
    }

    // code that runs when we exit the state
    public override void ExitState()
    {
        base.ExitState();

        enemy.Anim.SetLayerWeight(1, 0);

        // restore speed
        enemy.MoveSpeed = enemy.MaxSpeed;

        // hide ui
        HBar.gameObject.SetActive(false);
    }

    public override void FrameUpdate()
    {
        base.FrameUpdate();

        // Debug.Log(enemy.name + " attacking...");

        // TODO: add jump/dash attack for slime
        // TODO: add aggro system for targeting different party members?

        // allow attacking
        Attack();

        // exit battle
        if (!enemy.SeeOpponents() || _currentOpponent.CurrentHealth == 0)
            enemy.StateMachine.ChangeState(enemy.IdleState);

        // enemy dies
        if (enemy.CurrentHealth <= 0f)
            enemy.StateMachine.ChangeState(enemy.DeadState);
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

