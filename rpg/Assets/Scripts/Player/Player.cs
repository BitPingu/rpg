using UnityEngine;
using Unity.Cinemachine;
using System.Collections.Generic;
using System.Collections;

public class Player : MonoBehaviour
{
    [field: SerializeField] public int Level { get; set; } = 1;
    [field: SerializeField] public float MaxHealth { get; set; } = 100f;
    [field: SerializeField] public float CurrentHealth { get; set; }
    [field: SerializeField] public float MaxSpeed { get; set; } = 3.0f;
    public float MoveSpeed { get; set; }
    [field: SerializeField] public float Strength { get; set; } = 10.0f;
    [field: SerializeField] public float Defence { get; set; } = 5.0f;
    [field: SerializeField] public float MaxAttackSpeed { get; set; } = 1.0f;
    public float AttackSpeed { get; set; }

    [field: SerializeField] public float SightRadius { get; set; } = 7.0f;
    [field: SerializeField] public bool TargetingOpponent { get; set; }

    [field: SerializeField] private GameObject _sword;
    [field: SerializeField] public GameObject Weapon { get; set; }
    [field: SerializeField] public List<IAbility> Abilities { get; set; }

    public PlayerController Input { get; set; }
    private Vector3 _currentInputVector;
    public float SmoothInputSpeed { get; set; } = 0.1f;
    private Vector3 _smoothInputVelocity;
    public float TurnSmoothTime { get; set; } = 0.1f;
    private float _turnSmoothVelocity;
    public Vector3 Movement { get; set; }

    public CharacterController Controller { get; set; }
    public Animator Anim { get; set; }

    public Collider[] Opponents { get; set; }

    // state machine vars
    public PlayerStateMachine StateMachine { get; set; }
    public PlayerIdleState IdleState { get; set; }
    public PlayerBattleState BattleState { get; set; }
    public PlayerDeadState DeadState { get; set; }

    private void Awake()
    {
        StateMachine = new PlayerStateMachine();

        // create state instances
        IdleState = new PlayerIdleState(this, StateMachine);
        BattleState = new PlayerBattleState(this, StateMachine);
        DeadState = new PlayerDeadState(this, StateMachine);
    }

    private void Start()
    {
        // set player parameters
        CurrentHealth = MaxHealth;
        MoveSpeed = MaxSpeed;
        AttackSpeed = MaxAttackSpeed;

        // get player components
        Input = GetComponent<PlayerController>();
        Controller = GetComponent<CharacterController>();
        Anim = GetComponent<Animator>();

        // get weapon
        Weapon = Instantiate(_sword, transform);

        // start in idle state
        StateMachine.Initialize(IdleState);
    }

    private void Update()
    {
        StateMachine.CurrentPlayerState.FrameUpdate();
        // allow player movement all states if alive
        if (CurrentHealth > 0)
            MovePlayer();
    }

    private void MovePlayer()
    {
        // TODO: make this function inheritable for player and enemies?
        // Get direction from input
        Vector3 inputVector = new Vector3(Input.HorizontalInput, 0f, Input.VerticalInput).normalized;

        Vector3 MoveDir = Vector3.zero;

        // Check if moving in any direction
        if (inputVector.magnitude >= 0.1f)
        {
            // Calculate angle with cam
            Transform cam = GetComponentInChildren<CinemachineCamera>().transform;
            float targetAngle = Mathf.Atan2(inputVector.x, inputVector.z) * Mathf.Rad2Deg + cam.eulerAngles.y;

            // Rotate if not targeting opponent
            if (!TargetingOpponent)
            {
                // Smooth rotation
                float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref _turnSmoothVelocity, TurnSmoothTime);

                // Set rotation
                transform.rotation = Quaternion.Euler(0f, angle, 0f);
            }

            // Move to direction
            MoveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
        }

        // Smooth movement
        _currentInputVector = Vector3.SmoothDamp(_currentInputVector, MoveDir, ref _smoothInputVelocity, SmoothInputSpeed);

        // Set movement
        Movement = new Vector3(_currentInputVector.x, 0f, _currentInputVector.z);

        // Speed multiplier
        Movement *= MoveSpeed;

        // Move player
        Controller.Move(Movement * Time.deltaTime);

        // Animate movement
        Anim.SetFloat("Movement", Movement.magnitude/MoveSpeed);
    }

    public void FaceOpponent(Enemy opponent)
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
        if (!BattleState.HBar.gameObject.activeSelf)
            StartCoroutine(ShowHealthBar());
        BattleState.HBar.SetHealth(CurrentHealth);

        // change color
        StartCoroutine(GetHitColor());

        if (!BattleState.IsBlocking && CurrentHealth > 0)
        {
            // reset combo attack (chance?)
            BattleState.ResetCombo();

            // Animate attacked
            StartCoroutine(GetHit());
        }
    }

    IEnumerator ShowHealthBar()
    {
        BattleState.HBar.gameObject.SetActive(true);
        yield return new WaitForSeconds(1f);
        if (StateMachine.CurrentPlayerState != BattleState)
            BattleState.HBar.gameObject.SetActive(false);
    }

    IEnumerator GetHitColor()
    {
        List<Color> defaultColors = new List<Color>();
        foreach (Renderer r in GetComponentsInChildren<Renderer>())
        {
            defaultColors.Add(r.material.color);
            r.material.color = new Color(1f, 0.30196078f, 0.30196078f);
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
        Anim.SetBool("Attacked", true);

        yield return new WaitForSeconds(.6f);

        Anim.SetBool("Attacked", false);
        Anim.SetLayerWeight(2, 0);
    }

    public bool SeeOpponents()
    {
        // TODO: include FOV?
        // Detect opponents
        Opponents = Physics.OverlapSphere(transform.position, SightRadius, LayerMask.GetMask("Enemy"));
        return Opponents.Length > 0;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position, SightRadius);
    }




}

