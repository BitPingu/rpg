using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Unity.Cinemachine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    public CharacterController controller;
    public Transform cam;
    public Animator animator;
    private CinemachineOrbitalFollow orbit;
    private CinemachineRotationComposer composer;


    public float maxSpeed = 3.0F;
    public float speed = 0;
    public float turnSmoothTime = 0.1F;
    public float turnSmoothVelocity;
    public float zoomSensitivity = .1f;

    public Vector3 moveDir;
    private bool recenterCooldown = false;

    void Start()
    {
        orbit = GetComponentInChildren<CinemachineOrbitalFollow>();
        composer = GetComponentInChildren<CinemachineRotationComposer>();
        speed = maxSpeed;

        // cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        // Reset cam
        if (Input.GetKeyDown(KeyCode.E) && !recenterCooldown && 
            (!Input.GetMouseButton(0) || (Input.GetMouseButton(0) && !Input.GetMouseButton(1))) &&
                !orbit.HorizontalAxis.Recentering.Enabled && !orbit.VerticalAxis.Recentering.Enabled)
            StartCoroutine(ResetCam());
        
        // Zoom
        if (Input.GetAxis("Mouse ScrollWheel") != 0)
        {
            float cameraDistance = Input.GetAxis("Mouse ScrollWheel") * zoomSensitivity;
            if (orbit.RadialAxis.Value - cameraDistance >= orbit.RadialAxis.Range.x && 
                    orbit.RadialAxis.Value - cameraDistance <= orbit.RadialAxis.Range.y)
                orbit.RadialAxis.Value -= cameraDistance;
            // if (composer.TargetOffset.y - cameraDistance >= 0f && 
            //         composer.TargetOffset.y - cameraDistance <= 1f)
            //     composer.TargetOffset.y -= cameraDistance;
        }

        // Get horizontal and vertical input
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        // Get direction from input
        Vector3 direction = new Vector3(horizontal, 0f, vertical).normalized;

        // Check if moving in any direction
        if (direction.magnitude >= 0.1f)
        {
            // Calculate angle
            float targetAngle;
            if (!recenterCooldown)
                targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + cam.eulerAngles.y;
            else
                targetAngle = Mathf.Atan2(transform.forward.x, transform.forward.z) * Mathf.Rad2Deg;

            // Smooth rotation
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);

            // Set rotation
            if (!Input.GetMouseButton(0) || (Input.GetMouseButton(0) && !Input.GetMouseButton(1)))
                transform.rotation = Quaternion.Euler(0f, angle, 0f);

            // Move to direction
            moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
            controller.Move(moveDir.normalized * speed * Time.deltaTime);
        }
        else
        {
            moveDir = Vector3.zero;
        }

        // Animate
        animator.SetFloat("Speed", direction.magnitude);
    }

    IEnumerator ResetCam()
    {
        // reset cam orbits
        recenterCooldown = true;
        orbit.HorizontalAxis.Recentering.Enabled = true;
        orbit.VerticalAxis.Recentering.Enabled = true;
        orbit.RadialAxis.Recentering.Enabled = true;
        composer.TargetOffset.y = 1.3f;
        yield return new WaitForSeconds(.2f);
        orbit.HorizontalAxis.Recentering.Enabled = false;
        orbit.VerticalAxis.Recentering.Enabled = false;
        orbit.RadialAxis.Recentering.Enabled = false;

        // if (orbit.HorizontalAxis.Value == orbit.HorizontalAxis.Center && orbit.VerticalAxis.Value == orbit.VerticalAxis.Center &&
        //     orbit.RadialAxis.Value == orbit.RadialAxis.Center && composer.TargetOffset.y == .3f)
        recenterCooldown = false;
    }
}


