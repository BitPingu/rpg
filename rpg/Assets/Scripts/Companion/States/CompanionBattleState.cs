using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class CompanionBattleState : CompanionState
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
    public CompanionBattleState(Companion companion, CompanionStateMachine companionStateMachine) : base(companion, companionStateMachine)
    {
        // this.companion = companion;
        _controller = companion.GetComponent<CharacterController>();

        // setup health bar
        // GameObject playerUI = GameObject.Find("Player UI");
        // HBar = playerUI.transform.Find("HealthBar").GetComponent<HealthBar>();

        // HBar.SetMaxHealth(companion.MaxHealth);

        // _levelText = HBar.transform.Find("Level").GetComponent<TextMeshProUGUI>();
        // _levelText.text = companion.Level.ToString();

        // HealthText = HBar.transform.Find("Health").GetComponent<TextMeshProUGUI>();
        // HealthText.text = companion.MaxHealth.ToString();

        // setup abilities
        // _ability = companion.Abilities[0];
        // _abilityState = AbilityState.ready;

        // _abilityIcon1 = GameObject.Find("Dash").GetComponent<Slider>();
        // _abilityIcon1.maxValue = _ability.cooldownTime;
        // _abilityIcon1.value = _abilityIcon1.minValue;
        // _abilityIcon1.gameObject.SetActive(false);

        // _abilityIcon2 = GameObject.Find("Buff").GetComponent<Slider>();
        // _abilityIcon2.maxValue = _ability.cooldownTime;
        // _abilityIcon2.value = _abilityIcon2.minValue;
        // _abilityIcon2.gameObject.SetActive(false);

        // _abilityIcon3 = GameObject.Find("Ultimate").GetComponent<Slider>();
        // _abilityIcon3.maxValue = _ability.cooldownTime;
        // _abilityIcon3.value = _abilityIcon3.minValue;
        // _abilityIcon3.gameObject.SetActive(false);

        // AbilityVisual = GameObject.Find("AbilityVolume").GetComponent<Volume>();
    }

    // code that runs when we first enter the state
    public override void EnterState()
    {
        base.EnterState();
        
        // Debug.Log(companion.name + " is attacking.");

        // hold weapon
        companion.Weapon.transform.SetParent(companion.transform.Find("root/pelvis/spine_01/spine_02/spine_03/clavicle_r/upperarm_r/lowerarm_r/hand_r/weapon_r").transform);
        companion.Weapon.transform.localRotation = Quaternion.Euler(180f, 0f, 0f);
        companion.Weapon.transform.localPosition = new Vector3(0f, 0f, 0.007f);
        
        companion.Anim.SetLayerWeight(1, 1);

        IsBlocking = false;
        IsAttacking = false;
        _clickTime = 0f;

        // slow companion
        companion.MoveSpeed = companion.MaxSpeed*.8f;
    }

    // code that runs when we exit the state
    public override void ExitState()
    {
        base.ExitState();

        if (IsBlocking)
            companion.Weapon.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);

        companion.Anim.SetLayerWeight(1, 0);

        if (companion.TargetingOpponent)
        {
            companion.TargetingOpponent = false;
            // unfree
            companion.DistanceFromPlayer /= 3f;
        }

        // AbilityVisual.weight = 0f;

        // restore speed
        companion.MoveSpeed = companion.MaxSpeed;
    }

    public override void FrameUpdate()
    {
        base.FrameUpdate();

        // Debug.Log(companion.name + " attacking...");

        // follow player
        companion.FollowPlayer();

        // allow targeting
        TargetOpponent();

        // always face targeted opponent
        if (companion.TargetingOpponent && OpponentInRange())
        {
            companion.FaceOpponent(_currentOpponent);
        }
        if (companion.TargetingOpponent && !OpponentInRange())
        {
            // out of range
            companion.TargetingOpponent = false;
            // unfree
            companion.DistanceFromPlayer /= 3f;
        }
        if (companion.TargetingOpponent && OpponentInRange() && _currentOpponent.CurrentHealth == 0f)
        {
            // enemy dies
            companion.TargetingOpponent = false;
            // unfree
            companion.DistanceFromPlayer /= 3f;
        }

        // allow staff stance
        if (!IsAttacking)
            Block();

        // allow main combo attack
        if (!IsBlocking)
            ComboAttack();

        // allow abilities/arts
        if (!IsBlocking)
            Ability();

        // exit battle
        if (companion.Player.Input.Q && _abilityState != AbilityState.active)
            companion.StateMachine.ChangeState(companion.IdleState);

        // companion dies
        if (companion.CurrentHealth <= 0f)
            companion.StateMachine.ChangeState(companion.DeadState);
    }

    private void TargetOpponent()
    {
        if (companion.TargetingOpponent)
        {
            return;
        }

        // Detect opponents
        // TODO: add target ui on enemy
        if (companion.SeeOpponents())
        {
            // get opponent
            // Find shortest distance one
            _currentOpponent = companion.Opponents[0].GetComponent<Enemy>();

            if (_currentOpponent.CurrentHealth == 0f)
            {
                return;
            }

            // set opponent as target
            companion.TargetingOpponent = true;
            // Debug.Log(companion.name + " is targeting " + _currentOpponent.name + ".");

            // free
            companion.DistanceFromPlayer *= 3f;
        }
        // else
        // {
        //     Debug.Log("No enemies nearby.");
        // }
    }

    private bool OpponentInRange()
    {
        return Vector3.Distance(_currentOpponent.transform.position, companion.transform.position) <= companion.SightRadius;
    }

    private void Block()
    {
        // lock rotation for aim
        // TODO: make chance to block, or if enemy gets close or if low hp
        // if (player.Input.RightClickHold)
        // {
        //     // defend with weapon
        //     if (!IsBlocking)
        //     {
        //         companion.Weapon.transform.localRotation = Quaternion.Euler(60f, 0f, 0f);
        //         companion.Defence *= 1.5f;
        //     }
        //     IsBlocking = true;
        // }
        // else
        // {
        //     if (IsBlocking)
        //     {
        //         companion.Weapon.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
        //         companion.Defence /= 1.5f;
        //     }
        //     IsBlocking = false;
        // }
    }

    private void ComboAttack()
    {
        // if (player.Input.LeftClickHold)
        // {
        //     _clickTime += Time.deltaTime * player.AttackSpeed;
        //     IsAttacking = true;
        // }
        // else
        // {
        //     _clickTime = 0f;
        //     IsAttacking = false;
        // }

        // if (_clickTime > 2.1f)
        // {
        //     _clickTime = 0f;
        // }

        // // Animate attack
        // companion.Anim.SetFloat("Attack", _clickTime);

        // // Animate attack speed
        // companion.Anim.SetFloat("Attack Speed", companion.AttackSpeed);
    }

    public void ResetCombo()
    {
        _clickTime = -.5f;
    }

    private void Ability()
    {
        // switch (_abilityState)
        // {
        //     case AbilityState.ready:
        //         if (player.Input.One)
        //         {
        //             _ability.Activate(player.gameObject);
        //             _abilityState = AbilityState.active;
        //             _abilityActiveTime = _ability.activeTime;
        //             // update ui
        //             _abilityIcon1.value = _abilityIcon1.maxValue;
        //         }
        //     break;
        //     case AbilityState.active:
        //         if (_abilityActiveTime > 0)
        //         {
        //             _abilityActiveTime -= Time.deltaTime;
        //         }
        //         else
        //         {
        //             _ability.BeginCooldown(player.gameObject);
        //             _abilityState = AbilityState.cooldown;
        //             _abilityCooldownTime = _ability.cooldownTime;
        //         }
        //     break;
        //     case AbilityState.cooldown:
        //         if (_abilityCooldownTime > 0)
        //         {
        //             _abilityCooldownTime -= Time.deltaTime;
        //             // update ui
        //             _abilityIcon1.value = _abilityCooldownTime;
        //         }
        //         else
        //         {
        //             _abilityState = AbilityState.ready;
        //             Debug.Log(_ability.name + " is ready.");
        //         }
        //     break;
        // }
    }

}

