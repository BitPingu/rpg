using UnityEngine;
using System.Collections;
using Unity.Cinemachine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public float HorizontalInput { get; set; }
    public float VerticalInput { get; set; }

    public bool LeftClick { get; set; }
    public bool LeftClickHold { get; set; }
    public bool RightClick { get; set; }
    public bool RightClickHold { get; set; }

    public bool One { get; set; }

    private CinemachineOrbitalFollow _orbit;
    private CinemachineRotationComposer _composer;
    private bool _recenterCooldown = false;

    private void Start()
    {
        _orbit = GetComponentInChildren<CinemachineOrbitalFollow>();
        _composer = GetComponentInChildren<CinemachineRotationComposer>();

        // cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        // Get horizontal and vertical input
        HorizontalInput = Input.GetAxisRaw("Horizontal");
        VerticalInput = Input.GetAxisRaw("Vertical");

        // Reset cam
        if (Input.GetKeyDown(KeyCode.E) && !_recenterCooldown && 
            (!Input.GetMouseButton(0) || (Input.GetMouseButton(0) && !Input.GetMouseButton(1))) &&
                !_orbit.HorizontalAxis.Recentering.Enabled && !_orbit.VerticalAxis.Recentering.Enabled)
            StartCoroutine(ResetCam());

        // Get mouse input
        LeftClick = Input.GetMouseButtonDown(0);
        LeftClickHold = Input.GetMouseButton(0);
        RightClick = Input.GetMouseButtonDown(1);
        RightClickHold = Input.GetMouseButton(1);

        // Get key inputs (abilities)
        One = Input.GetKeyDown(KeyCode.Alpha1);
    }

    IEnumerator ResetCam()
    {
        // reset cam orbits
        _recenterCooldown = true;
        _orbit.HorizontalAxis.Recentering.Enabled = true;
        _orbit.VerticalAxis.Recentering.Enabled = true;
        _orbit.RadialAxis.Recentering.Enabled = true;
        _composer.TargetOffset.y = 1.3f;
        yield return new WaitForSeconds(.2f);
        _orbit.HorizontalAxis.Recentering.Enabled = false;
        _orbit.VerticalAxis.Recentering.Enabled = false;
        _orbit.RadialAxis.Recentering.Enabled = false;
        _recenterCooldown = false;
    }
}

