using System.Collections;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [field: SerializeField] public float MaxHealth { get; set; } = 100f;
    public float CurrentHealth { get; set; }
    [field: SerializeField] public float MaxSpeed { get; set; } = 3.0f;
    public float MoveSpeed { get; set; }
    [field: SerializeField] public float MaxAttackSpeed { get; set; } = 1.0f;
    public float AttackSpeed { get; set; }

    [field: SerializeField] public float AttackDelay { get; set; } = 2.0f;

    // [field: SerializeField] public GameObject Weapon { get; set; }
    // [field: SerializeField] public GameObject WeaponBack { get; set; }

    // [field: SerializeField] public IAbility Ability1 { get; set; }

    [field: SerializeField] public float IdleTime { get; set; } = 3.0f;
    [field: SerializeField] public float MoveTime { get; set; } = 3.0f;
    [field: SerializeField] public float SightRadius { get; set; } = 3.0f;

    [field: SerializeField] public float AttackRadius { get; set; } = 1.0f;

    private Vector3 _currentMoveVector;
    public float SmoothMoveSpeed { get; set; } = 0.1f;
    private Vector3 _smoothInputVelocity;
    public float TurnSmoothTime { get; set; } = 0.1f;
    private float _turnSmoothVelocity;
    public Vector3 Movement { get; set; }

    public CharacterController Controller { get; set; }
    public Animator Anim { get; set; }

    public GameObject Player { get; set; }

    // state machine vars
    public EnemyStateMachine StateMachine { get; set; }
    public EnemyIdleState IdleState { get; set; }
    public EnemyBattleState BattleState { get; set; }

    private void Awake()
    {
        StateMachine = new EnemyStateMachine();

        // create state instances
        IdleState = new EnemyIdleState(this, StateMachine);
        BattleState = new EnemyBattleState(this, StateMachine);
    }

    private void Start()
    {
        // set enemy parameters
        CurrentHealth = MaxHealth;
        MoveSpeed = MaxSpeed;
        AttackSpeed = MaxAttackSpeed;

        // get enemy components
        Controller = GetComponent<CharacterController>();
        Anim = GetComponent<Animator>();

        // get player
        Player = GameObject.FindWithTag("Player");

        // start in idle state
        StateMachine.Initialize(IdleState);
    }

    private void Update()
    {
        StateMachine.CurrentEnemyState.FrameUpdate();
        // Animate movement
        Anim.SetFloat("Movement", Movement.magnitude/MoveSpeed);
    }

    public void MoveEnemy(Vector3 inputVector)
    {
        // Get direction from input
        inputVector = inputVector.normalized;

        Vector3 MoveDir = Vector3.zero;

        // Check if moving in any direction
        if (inputVector.magnitude >= 0.1f)
        {
            // Calculate angle with cam
            float targetAngle = Mathf.Atan2(inputVector.x, inputVector.z) * Mathf.Rad2Deg;

            // Smooth rotation
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref _turnSmoothVelocity, TurnSmoothTime);

            // Set rotation
            transform.rotation = Quaternion.Euler(0f, angle, 0f);

            // Move to direction
            MoveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
        }

        // Smooth movement
        _currentMoveVector = Vector3.SmoothDamp(_currentMoveVector, MoveDir, ref _smoothInputVelocity, SmoothMoveSpeed);

        // Set movement
        Movement = new Vector3(_currentMoveVector.x, 0f, _currentMoveVector.z);

        // Speed multiplier
        Movement *= MoveSpeed;

        // Move enemy
        Controller.Move(Movement * Time.deltaTime);
    }

    public void Damage(float damageAmount)
    {
        CurrentHealth -= damageAmount;

        if (CurrentHealth <= 0f)
        {
            Debug.Log(name + " dies.");
        }
    }

    public bool CheckPlayerWithinRange(float range)
    {
        // TODO: include FOV?
        // Get distance from player
        return Vector3.Distance(Player.transform.position, transform.position) <= range;
    }

    private void Die()
    {

    }

    public void Attack()
    {
        StartCoroutine(AttackRaycast());

        // Animate attack
        Anim.SetTrigger("Attack");
    }

    IEnumerator AttackRaycast()
    {
        yield return new WaitForSeconds(.2f);
        Debug.Log(name + " deals damage.");

        // Animate attack
        Anim.SetTrigger("Attack");
    }

}

