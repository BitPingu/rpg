using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class PlayerBattleState : PlayerState
{
    private CharacterController _controller;
    private Enemy _currentOpponent;

    private bool _stance;
    private bool _attacking;
    private float _clickTime;

    public TextMeshProUGUI LevelText;

    public TextMeshProUGUI HealthText;
    public Slider HealthBar;

    private IAbility _ability;
    private float _abilityCooldownTime;
    private float _abilityActiveTime;
    enum AbilityState
    {
        ready,
        active,
        cooldown
    }
    private AbilityState _abilityState;

    private Slider _abilityIcon1;
    private Slider _abilityIcon2;
    private Slider _abilityIcon3;
    
    public Volume AbilityVisual;


    // pass in any parameters you need in the constructors
    public PlayerBattleState(Player player, PlayerStateMachine playerStateMachine) : base(player, playerStateMachine)
    {
        // this.player = player;
        _controller = player.GetComponent<CharacterController>();

        // setup health bar
        LevelText = GameObject.Find("Level").GetComponent<TextMeshProUGUI>();
        LevelText.text = player.Level.ToString();

        HealthText = GameObject.Find("Health").GetComponent<TextMeshProUGUI>();
        HealthText.text = player.MaxHealth.ToString();

        HealthBar = GameObject.Find("HealthBar").GetComponent<Slider>();
        HealthBar.maxValue = player.MaxHealth;
        HealthBar.value = player.MaxHealth;
        HealthBar.gameObject.SetActive(false);

        // setup abilities
        _ability = player.Abilities[0];
        _abilityState = AbilityState.ready;

        _abilityIcon1 = GameObject.Find("Dash").GetComponent<Slider>();
        _abilityIcon1.maxValue = _ability.cooldownTime;
        _abilityIcon1.value = _abilityIcon1.minValue;
        _abilityIcon1.gameObject.SetActive(false);

        _abilityIcon2 = GameObject.Find("Buff").GetComponent<Slider>();
        _abilityIcon2.maxValue = _ability.cooldownTime;
        _abilityIcon2.value = _abilityIcon2.minValue;
        _abilityIcon2.gameObject.SetActive(false);

        _abilityIcon3 = GameObject.Find("Ultimate").GetComponent<Slider>();
        _abilityIcon3.maxValue = _ability.cooldownTime;
        _abilityIcon3.value = _abilityIcon3.minValue;
        _abilityIcon3.gameObject.SetActive(false);

        AbilityVisual = GameObject.Find("AbilityVolume").GetComponent<Volume>();
    }

    // code that runs when we first enter the state
    public override void EnterState()
    {
        base.EnterState();
        
        Debug.Log(player.name + " is attacking.");

        // hold weapon
        player.Weapon.transform.SetParent(GameObject.Find("weapon_r").transform);
        player.Weapon.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
        player.Weapon.transform.localPosition = new Vector3(0f, 0f, 0.007f);
        
        player.Anim.SetLayerWeight(1, 1);

        _stance = false;
        _attacking = false;
        _clickTime = 0f;

        // slow player
        player.MoveSpeed = player.MaxSpeed*.8f;

        // show ui
        HealthBar.gameObject.SetActive(true);
        _abilityIcon1.gameObject.SetActive(true);
        _abilityIcon2.gameObject.SetActive(true);
        _abilityIcon3.gameObject.SetActive(true);
    }

    // code that runs when we exit the state
    public override void ExitState()
    {
        base.ExitState();

        player.Anim.SetLayerWeight(1, 0);

        if (player.TargetingEnemy)
            player.TargetingEnemy = false;

        AbilityVisual.weight = 0f;

        // restore speed
        player.MoveSpeed = player.MaxSpeed;

        // hide ui
        HealthBar.gameObject.SetActive(false);
        _abilityIcon1.gameObject.SetActive(false);
        _abilityIcon2.gameObject.SetActive(false);
        _abilityIcon3.gameObject.SetActive(false);
    }

    public override void FrameUpdate()
    {
        base.FrameUpdate();

        // Debug.Log(player.name + " attacking...");

        // TODO: add special attacks, abilities, dodge roll, sprinting, jumping
        // TODO: right click should be for charge attack or secondary
        // TODO: overdrive mechanic?
        // TODO: knockback/recoil? (stronger on spin attack)
        // TODO: for blocking, chance to parry? perfect block on right timing? or chance?
        // TODO: make parrying an unlockable ability?

        // TODO: for main combo: 1-3 hits for level 1, continue holding left click to charge spin attack (hit 4) unlock on higher level
        // TODO: animation canceling mechanic (hold left click to perform 3 hits, then lift and click again)
        // TODO: right click hold should be for blocking enemy attack - deal less damage/increase defense, and knockback

        // allow targeting
        if (player.Input.E)
            TargetEnemy();

        // always face targeted opponent
        if (player.TargetingEnemy && OpponentInRange() && player.CurrentHealth > 0)
        {
            player.FaceOpponent(_currentOpponent);
        }
        else if (player.TargetingEnemy && !OpponentInRange() && player.CurrentHealth > 0)
        {
            // out of range
            Debug.Log("Enemy out of range.");
            player.TargetingEnemy = false;
        }

        // allow sword stance
        if (!_attacking)
            Stance();

        // allow main combo attack
        if (!_stance)
            ComboAttack();

        // allow abilities/arts
        if (!_stance)
            Ability();

        // exit battle
        if (player.Input.Q && _abilityState != AbilityState.active)
            player.StateMachine.ChangeState(player.IdleState);

        // player dies
        if (player.CurrentHealth <= 0f)
            player.StateMachine.ChangeState(player.DeadState);
    }

    private void TargetEnemy()
    {
        if (player.TargetingEnemy)
        {
            // exit targeting mode
            Debug.Log("Exiting targeting mode.");
            player.TargetingEnemy = false;
            return;
        }

        // Detect opponents
        // TODO: add target ui on enemy
        if (player.SeeOpponents())
        {
            // get opponent
            // Find shortest distance one
            _currentOpponent = player.Opponents[0].GetComponent<Enemy>();

            // set opponent as target
            player.TargetingEnemy = true;
            Debug.Log("Targeting " + _currentOpponent.name + ".");
        }
        else
        {
            Debug.Log("No enemies nearby.");
        }
    }

    private bool OpponentInRange()
    {
        return Vector3.Distance(_currentOpponent.transform.position, player.transform.position) <= player.SightRadius;
    }

    private void Stance()
    {
        // lock rotation for aim
        if (player.Input.RightClickHold)
        {
            // defend with weapon
            if (!_stance)
            {
                player.Weapon.transform.eulerAngles = new Vector3(
                    player.Weapon.transform.eulerAngles.x + 60f,
                    player.Weapon.transform.eulerAngles.y,
                    player.Weapon.transform.eulerAngles.z
                );
            }
            _stance = true;
        }
        else
        {
            if (_stance)
            {
                player.Weapon.transform.eulerAngles = new Vector3(
                    player.Weapon.transform.eulerAngles.x - 60f,
                    player.Weapon.transform.eulerAngles.y,
                    player.Weapon.transform.eulerAngles.z
                );
            }
            _stance = false;
        }
    }

    private void ComboAttack()
    {
        if (player.Input.LeftClickHold)
        {
            _clickTime += Time.deltaTime * player.AttackSpeed;
            _attacking = true;
        }
        else
        {
            _clickTime = 0f;
            _attacking = false;
        }

        if (_clickTime > 2.1f)
        {
            _clickTime = 0f;
        }

        if (_clickTime >= 1.7f && _clickTime < 2.1f) {
            // spin boost
            if (player.CurrentHealth > 0f)
                _controller.Move(player.Movement * Time.deltaTime);
        }

        // Animate attack
        player.Anim.SetFloat("Attack", _clickTime);

        // Animate attack speed
        player.Anim.SetFloat("Attack Speed", player.AttackSpeed);
    }

    public void ResetCombo()
    {
        _clickTime = -.5f;
    }

    private void Ability()
    {
        // TODO: make this function inheritable for player and enemies?
        switch (_abilityState)
        {
            case AbilityState.ready:
                if (player.Input.One)
                {
                    _ability.Activate(player.gameObject);
                    _abilityState = AbilityState.active;
                    _abilityActiveTime = _ability.activeTime;
                    // update ui
                    _abilityIcon1.value = _abilityIcon1.maxValue;
                }
            break;
            case AbilityState.active:
                if (_abilityActiveTime > 0)
                {
                    _abilityActiveTime -= Time.deltaTime;
                }
                else
                {
                    _ability.BeginCooldown(player.gameObject);
                    _abilityState = AbilityState.cooldown;
                    _abilityCooldownTime = _ability.cooldownTime;
                }
            break;
            case AbilityState.cooldown:
                if (_abilityCooldownTime > 0)
                {
                    _abilityCooldownTime -= Time.deltaTime;
                    // update ui
                    _abilityIcon1.value = _abilityCooldownTime;
                }
                else
                {
                    _abilityState = AbilityState.ready;
                    Debug.Log(_ability.name + " is ready.");
                }
            break;
        }
    }

}

