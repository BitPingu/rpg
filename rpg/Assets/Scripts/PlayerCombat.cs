using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    public CharacterController controller;
    private PlayerMovement movement;
    public Animator animator;

    private float clickTime = 0f;

    void Start()
    {
        movement = GetComponent<PlayerMovement>();
    }

    void Update()
    {
        if (Input.GetMouseButton(0))
        {
            // TODO: add special attacks, abilities, dodge roll, sprinting, jumping
            // TODO: allow camera zoom using scroll wheel
            // TODO: right click should be for charge attack or secondary

            if (Input.GetMouseButton(1))
            {
                // lock rotation
                transform.rotation = Quaternion.Euler(0f, movement.cam.transform.eulerAngles.y, 0f);
            }

            movement.speed = movement.maxSpeed*.8f;

            clickTime += Time.deltaTime;

            if (clickTime > 2.1f)
            {
                clickTime = 0f;
            }

            if (clickTime >= 1.7f && clickTime < 2.1f) {
                // spin boost
                controller.Move(movement.moveDir.normalized * 5f * Time.deltaTime);
            }
        }
        else
        {
            movement.speed = movement.maxSpeed;

            clickTime = 0f;
        }

        // Animate
        animator.SetLayerWeight(1, Mathf.Ceil(clickTime));
        animator.SetFloat("Attack", clickTime);
    }
}


