using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class PlayerBattleState : PlayerState
{
    private CharacterController _controller;
    private Enemy _currentOpponent;

    public bool IsBlocking;
    public bool IsAttacking;
    private float _clickTime;

    private TextMeshProUGUI _levelText;

    public TextMeshProUGUI HealthText;
    public HealthBar HBar;

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
        GameObject playerUI = GameObject.Find("Player UI");
        HBar = playerUI.transform.Find("HealthBar").GetComponent<HealthBar>();

        HBar.SetMaxHealth(player.MaxHealth);
        HBar.gameObject.SetActive(false);

        _levelText = HBar.transform.Find("Level").GetComponent<TextMeshProUGUI>();
        _levelText.text = player.Level.ToString();

        HealthText = HBar.transform.Find("Health").GetComponent<TextMeshProUGUI>();
        HealthText.text = player.MaxHealth.ToString();

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

        IsBlocking = false;
        IsAttacking = false;
        _clickTime = 0f;

        // slow player
        player.MoveSpeed = player.MaxSpeed*.8f;

        // show ui
        HBar.gameObject.SetActive(true);
        _abilityIcon1.gameObject.SetActive(true);
        _abilityIcon2.gameObject.SetActive(true);
        _abilityIcon3.gameObject.SetActive(true);
    }

    // code that runs when we exit the state
    public override void ExitState()
    {
        base.ExitState();

        if (IsBlocking)
            player.Weapon.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);

        player.Anim.SetLayerWeight(1, 0);

        if (player.TargetingOpponent)
            player.TargetingOpponent = false;

        AbilityVisual.weight = 0f;

        // restore speed
        player.MoveSpeed = player.MaxSpeed;

        // hide ui
        HBar.gameObject.SetActive(false);
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
            TargetOpponent();

        // always face targeted opponent
        if (player.TargetingOpponent && OpponentInRange())
        {
            player.FaceOpponent(_currentOpponent);
        }
        if (player.TargetingOpponent && !OpponentInRange())
        {
            // out of range
            Debug.Log("Enemy out of range.");
            player.TargetingOpponent = false;
        }
        if (player.TargetingOpponent && OpponentInRange() && _currentOpponent.CurrentHealth == 0f)
        {
            // enemy dies
            player.TargetingOpponent = false;
        }

        // allow sword stance
        if (!IsAttacking)
            Stance();

        // allow main combo attack
        if (!IsBlocking)
            ComboAttack();

        // allow abilities/arts
        if (!IsBlocking)
            Ability();

        // exit battle
        if (player.Input.Q && _abilityState != AbilityState.active)
            player.StateMachine.ChangeState(player.IdleState);

        // player dies
        if (player.CurrentHealth <= 0f)
            player.StateMachine.ChangeState(player.DeadState);
    }

    private void TargetOpponent()
    {
        if (player.TargetingOpponent)
        {
            // exit targeting mode
            Debug.Log("Exiting targeting mode.");
            player.TargetingOpponent = false;
            return;
        }

        // Detect opponents
        // TODO: add target ui on enemy
        if (player.SeeOpponents())
        {
            // get opponent
            // Find shortest distance one
            _currentOpponent = player.Opponents[0].GetComponent<Enemy>();

            if (_currentOpponent.CurrentHealth == 0f)
            {
                Debug.Log("No enemies nearby.");
                return;
            }

            // set opponent as target
            player.TargetingOpponent = true;
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
            if (!IsBlocking)
            {
                player.Weapon.transform.localRotation = Quaternion.Euler(60f, 0f, 0f);
                player.Defence *= 1.5f;
            }
            IsBlocking = true;
        }
        else
        {
            if (IsBlocking)
            {
                player.Weapon.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
                player.Defence /= 1.5f;
            }
            IsBlocking = false;
        }
    }

    private void ComboAttack()
    {
        if (player.Input.LeftClickHold)
        {
            _clickTime += Time.deltaTime * player.AttackSpeed;
            IsAttacking = true;
        }
        else
        {
            _clickTime = 0f;
            IsAttacking = false;
        }

        if (_clickTime > 2.1f)
        {
            _clickTime = 0f;
        }

        if (_clickTime >= 1.7f && _clickTime < 2.1f) {
            // spin boost
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

