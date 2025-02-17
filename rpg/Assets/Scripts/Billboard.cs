using UnityEngine;
using Unity.Cinemachine;

public class Billboard : MonoBehaviour
{
    private Transform _cam;

    private void Awake()
    {
        _cam = GameObject.Find("Player").GetComponentInChildren<CinemachineCamera>().transform;
    }

    private void LateUpdate()
    {
        transform.forward = _cam.forward;
    }
}
