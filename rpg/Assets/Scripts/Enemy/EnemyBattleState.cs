using TMPro;
using UnityEngine;

public class EnemyBattleState : EnemyState
{
    public Player CurrentOpponent;
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

    public LungeAbility Lunge;
    private float _abilityCooldownTime;
    private float _abilityActiveTime;
    enum AbilityState
    {
        ready,
        active,
        cooldown
    }
    private AbilityState _abilityState;


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

        // setup abilities
        if (enemy.Abilities.Count > 0)
        {
            Lunge = (LungeAbility)enemy.Abilities[0];
            _abilityState = AbilityState.ready;
        }
    }

    // code that runs when we first enter the state
    public override void EnterState()
    {
        base.EnterState();

        CurrentOpponent = enemy.Opponents[0].GetComponent<Player>();

        // Debug.Log(enemy.name + " is attacking " + CurrentOpponent.name + ".");

        // enemy.Weapon.SetActive(true);

        enemy.Anim.SetLayerWeight(1, 1);

        // enemy run
        enemy.MoveSpeed = enemy.MaxSpeed*1.5f;

        // extend sight
        enemy.SightRadius *= 2f;

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

        // restore sight
        enemy.SightRadius /= 2f;

        // hide ui
        HBar.gameObject.SetActive(false);
    }

    public override void FrameUpdate()
    {
        base.FrameUpdate();

        // Debug.Log(enemy.name + " attacking...");

        // TODO: add aggro system for targeting different party members?

        // allow attacking
        Attack();

        // exit battle
        if (!enemy.SeeOpponents() || CurrentOpponent.CurrentHealth == 0)
            enemy.StateMachine.ChangeState(enemy.IdleState);

        // enemy dies
        if (enemy.CurrentHealth <= 0f)
            enemy.StateMachine.ChangeState(enemy.DeadState);
    }

    private void Attack()
    {
        if (_abilityState != AbilityState.active)
        {
            if (!OpponentInRange())
            {
                // chase opponent
                Chase();
            }
            else
            {
                // always face opponent
                enemy.FaceOpponent(CurrentOpponent);

                // main combo attack
                ComboAttack();
            }
        }

        // allow abilities/arts
        Ability();
    }

    public bool OpponentInRange()
    {
        return Vector3.Distance(CurrentOpponent.transform.position, enemy.transform.position) <= enemy.AttackRadius;
    }

    private void Chase()
    {
        enemy.MoveEnemy(CurrentOpponent.transform.position - enemy.transform.position);
    }

    private void ComboAttack()
    {
        if (Time.time > _attackTime + enemy.AttackDelay)
        {
            enemy.Attack(CurrentOpponent);
            _attackTime = Time.time;
        }
    }

    private void Ability()
    {
        // TODO: make this function inheritable for player and enemies?
        switch (_abilityState)
        {
            case AbilityState.ready:
                if (Lunge.Condition(enemy.gameObject))
                {
                    Lunge.Activate(enemy.gameObject);
                    _abilityState = AbilityState.active;
                    _abilityActiveTime = Lunge.activeTime;
                }
            break;
            case AbilityState.active:
                if (_abilityActiveTime > 0)
                {
                    if (Lunge.IsActive) {
                        enemy.MoveEnemy(Lunge.TargetPos);
                    }
                    _abilityActiveTime -= Time.deltaTime;
                }
                else
                {
                    Lunge.BeginCooldown(enemy.gameObject);
                    _abilityState = AbilityState.cooldown;
                    _abilityCooldownTime = Lunge.cooldownTime;
                }
            break;
            case AbilityState.cooldown:
                if (_abilityCooldownTime > 0)
                {
                    _abilityCooldownTime -= Time.deltaTime;
                }
                else
                {
                    _abilityState = AbilityState.ready;
                    // Debug.Log(Lunge.name + " is ready.");
                }
            break;
        }
    }

}

