using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class Enemy : MonoBehaviour
{
    [field: SerializeField] public int Level { get; set; } = 1;
    [field: SerializeField] public float MaxHealth { get; set; } = 20.0f;
    [field: SerializeField] public float CurrentHealth { get; set; }
    [field: SerializeField] public float MaxSpeed { get; set; } = 3.0f;
    public float MoveSpeed { get; set; }
    [field: SerializeField] public float Strength { get; set; } = 10.0f;
    [field: SerializeField] public float Defence { get; set; } = 5.0f;
    [field: SerializeField] public float MaxAttackSpeed { get; set; } = 1.0f;
    public float AttackSpeed { get; set; }

    [field: SerializeField] public float AttackDelay { get; set; } = 2.0f;

    // [field: SerializeField] public GameObject Weapon { get; set; }
    [field: SerializeField] public List<IAbility> Abilities { get; set; }

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
    public Rigidbody RB { get; set; }
    public Animator Anim { get; set; }

    public Collider[] Opponents { get; set; }

    // state machine vars
    public EnemyStateMachine StateMachine { get; set; }
    public EnemyIdleState IdleState { get; set; }
    public EnemyBattleState BattleState { get; set; }
    public EnemyDeadState DeadState { get; set; }

    private void Awake()
    {
        StateMachine = new EnemyStateMachine();

        // create state instances
        IdleState = new EnemyIdleState(this, StateMachine);
        BattleState = new EnemyBattleState(this, StateMachine);
        DeadState = new EnemyDeadState(this, StateMachine);
    }

    private void Start()
    {
        // set enemy parameters
        CurrentHealth = MaxHealth;
        MoveSpeed = MaxSpeed;
        AttackSpeed = MaxAttackSpeed;

        // get enemy components
        Controller = GetComponent<CharacterController>();
        RB = GetComponent<Rigidbody>();
        Anim = GetComponent<Animator>();

        // start in idle state
        StateMachine.Initialize(IdleState);
    }

    private void Update()
    {
        StateMachine.CurrentEnemyState.FrameUpdate();
    }

    public void MoveEnemy(Vector3 inputVector)
    {
        // Get direction from input
        inputVector = inputVector.normalized;

        Vector3 MoveDir = Vector3.zero;

        // Check if moving in any direction
        if (inputVector.magnitude >= 0.1f)
        {
            // Calculate angle
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

        // Animate movement
        Anim.SetFloat("Movement", Movement.magnitude/MoveSpeed);
    }

    public void FaceOpponent(Player opponent)
    {
        // Get direction towards opponent
        Vector3 direction = opponent.transform.position - transform.position;

		// Calculate angle
		float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;

		// Smooth rotation
		float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref _turnSmoothVelocity, TurnSmoothTime);

		// Set rotation
		transform.rotation = Quaternion.Euler(0f, angle, 0f);
    }

    public void Damage(float damageAmount)
    {
        CurrentHealth -= damageAmount;

        if (CurrentHealth <= 0f)
        {
            CurrentHealth = 0f;
        }

        // adjust health bar
        BattleState.HealthText.text = CurrentHealth.ToString();
        BattleState.HBar.SetHealth(CurrentHealth);

        // change color
        if (GetComponentsInChildren<Renderer>()[0].material.color != Color.blue)
            StartCoroutine(GetHitColor());

        // Animate attacked
        if (CurrentHealth > 0)
            StartCoroutine(GetHit());
    }

    IEnumerator GetHitColor()
    {
        List<Color> defaultColors = new List<Color>();
        foreach (Renderer r in GetComponentsInChildren<Renderer>())
        {
            defaultColors.Add(r.material.color);
            // r.material.color = new Color(1f, 0.30196078f, 0.30196078f);
            r.material.color = Color.blue;
        }

        yield return new WaitForSeconds(.2f);

        int i=0;
        foreach (Renderer r in GetComponentsInChildren<Renderer>())
        {
            r.material.color = defaultColors[i];
            i++;
        }
    }

    IEnumerator GetHit()
    {
        Anim.SetLayerWeight(2, 1);

        if (Anim.GetBool("Attacked"))
            Anim.SetBool("Attacked", false);

        Anim.SetBool("Attacked", true);

        yield return new WaitForSeconds(.6f);

        Anim.SetBool("Attacked", false);

        Anim.SetLayerWeight(2, 0);
    }

    public void Die()
    {
        StartCoroutine(TimedDeactivation());
    }

    IEnumerator TimedDeactivation()
    {
        yield return new WaitForSeconds(4f);
        gameObject.SetActive(false);
    }

    public bool SeeOpponents()
    {
        // TODO: include FOV?
        // Detect opponents
        Opponents = Physics.OverlapSphere(transform.position, SightRadius, LayerMask.GetMask("Party"));
        return Opponents.Length > 0;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position, SightRadius);
    }

    public void Attack(Player opponent)
    {
        StartCoroutine(AttackRaycast(opponent));
    }

    IEnumerator AttackRaycast(Player opponent)
    {
        // Animate attack
        Anim.SetTrigger("Attack");

        yield return new WaitForSeconds(.2f);

        float damage = Strength - opponent.Defence;
        damage = Mathf.Floor(damage);
        opponent.Damage(damage);
        Debug.Log(name + " deals " + damage + " damage to " + opponent.name + ".");

        if (opponent.CurrentHealth <= 0f)
            yield break;

        // Animate attack
        Anim.SetTrigger("Attack");
    }

}

