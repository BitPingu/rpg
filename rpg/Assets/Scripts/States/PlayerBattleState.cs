using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.UI;

public class PlayerBattleState : PlayerState
{
    private CharacterController _controller;
    private CinemachineCamera _cam;

    private float _clickTime;

    private IAbility _ability;
    private float _abilityCooldownTime;
    float _abilityActiveTime;
    enum AbilityState
    {
        ready,
        active,
        cooldown
    }
    private AbilityState _abilityState = AbilityState.ready;

    private Slider _slider;


    // pass in any parameters you need in the constructors
    public PlayerBattleState(Player player, PlayerStateMachine playerStateMachine) : base(player, playerStateMachine)
    {
        // this.player = player;
        _controller = player.GetComponent<CharacterController>();
        _cam = player.GetComponentInChildren<CinemachineCamera>();
        
        _ability = player.Ability1;
        _slider = player.Slid;
        _slider.maxValue = _ability.cooldownTime;
        _slider.value = _slider.minValue;

        _slider.gameObject.SetActive(false);
    }

    // code that runs when we first enter the state
    public override void EnterState()
    {
        base.EnterState();
        
        Debug.Log(player.name + " is attacking.");

        player.Weapon.SetActive(true);
        player.WeaponBack.SetActive(false);
        
        player.Anim.SetLayerWeight(1, 1);
    
        _clickTime = 0f;

        // slow player
        player.MoveSpeed = player.MaxSpeed*.8f;

        // show ui
        _slider.gameObject.SetActive(true);
    }

    // code that runs when we exit the state
    public override void ExitState()
    {
        base.ExitState();

        player.Anim.SetLayerWeight(1, 0);

        // restore speed
        player.MoveSpeed = player.MaxSpeed;

        // hide ui
        _slider.gameObject.SetActive(false);
    }

    public override void FrameUpdate()
    {
        base.FrameUpdate();

        // Debug.Log(player.name + " attacking...");

        // TODO: add special attacks, abilities, dodge roll, sprinting, jumping
        // TODO: right click should be for charge attack or secondary
        // TODO: overdrive mechanic?

        // lock rotation for aim
        if (player.Input.RightClickHold)
            player.transform.rotation = Quaternion.Euler(0f, _cam.transform.eulerAngles.y, 0f);

        // allow main combo attack
        ComboAttack();

        // allow abilities/arts
        Ability();

        // exit battle
        if (!player.Input.LeftClickHold && player.Input.RightClick)
            player.StateMachine.ChangeState(player.IdleState);
    }

    private void ComboAttack()
    {
        if (player.Input.LeftClickHold)
            _clickTime += Time.deltaTime * player.AttackSpeed;
        else
            _clickTime = 0f;

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

    private void Ability()
    {
        switch (_abilityState)
        {
            case AbilityState.ready:
                if (player.Input.One)
                {
                    _ability.Activate(player.gameObject);
                    _abilityState = AbilityState.active;
                    _abilityActiveTime = _ability.activeTime;
                    // update ui
                    _slider.value = _slider.maxValue;
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
                    _slider.value = _abilityCooldownTime;
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

