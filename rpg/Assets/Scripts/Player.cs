using Unity.Cinemachine;
using UnityEngine;

public class Player : MonoBehaviour
{
    [field: SerializeField] public float MaxHealth { get; set; } = 100f;
    public float CurrentHealth { get; set; }
    [field: SerializeField] public float MaxSpeed { get; set; } = 3.0f;
    public float MoveSpeed { get; set; }

    [field: SerializeField] public GameObject Weapon { get; set; }
    [field: SerializeField] public GameObject WeaponBack { get; set; }

    public PlayerController Input { get; set; }
    public float TurnSmoothTime { get; set; } = 0.1f;
    private float _turnSmoothVelocity;
    public Vector3 MoveDir { get; set; }

    public bool IsAttacking { get; set; }

    public CharacterController Controller { get; set; }
    public Animator Anim { get; set; }

    // state machine vars
    public PlayerStateMachine StateMachine { get; set; }
    public PlayerIdleState IdleState { get; set; }
    public PlayerBattleState BattleState { get; set; }

    private void Awake()
    {
        StateMachine = new PlayerStateMachine();

        // create state instances
        IdleState = new PlayerIdleState(this, StateMachine);
        BattleState = new PlayerBattleState(this, StateMachine);
    }

    private void Start()
    {
        // set health and speed
        CurrentHealth = MaxHealth;
        MoveSpeed = MaxSpeed;

        // get player components
        Input = GetComponent<PlayerController>();
        Controller = GetComponent<CharacterController>();
        Anim = GetComponent<Animator>();

        // start in idle state
        StateMachine.Initialize(IdleState);
    }

    private void Update()
    {
        StateMachine.CurrentPlayerState.FrameUpdate();
        // allow player movement
        MovePlayer();
        // check if attacking
        IsAttacking = Input.LeftClick;
    }

    public void MovePlayer()
    {
        // Get direction from input
        Vector3 direction = new Vector3(Input.HorizontalInput, 0f, Input.VerticalInput).normalized;

        // Check if moving in any direction
        if (direction.magnitude >= 0.1f)
        {
            // Calculate angle
            Transform cam = GetComponentInChildren<CinemachineCamera>().transform;
            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + cam.eulerAngles.y;

            // Smooth rotation
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref _turnSmoothVelocity, TurnSmoothTime);

            // Set rotation
            if (!Input.LeftClick || (Input.LeftClick && !Input.RightClick))
                transform.rotation = Quaternion.Euler(0f, angle, 0f);

            // Move to direction
            MoveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
            Controller.Move(MoveDir.normalized * MoveSpeed * Time.deltaTime);
        }

        // Animate
        Anim.SetFloat("Speed", direction.magnitude);
    }

    public void Damage(float damageAmount)
    {
        CurrentHealth -= damageAmount;

        if (CurrentHealth <= 0f)
        {
            Debug.Log(name + " dies.");
        }
    }

}

