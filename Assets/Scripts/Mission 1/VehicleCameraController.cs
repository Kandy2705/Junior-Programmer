using PrimeTween;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Controls first-person and third-person vehicle camera views.
/// Supports mouse look and smooth PrimeTween transitions between views.
/// </summary>
public class VehicleCameraController : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform target;

    [Header("Third Person View")]
    [SerializeField] private Vector3 thirdPersonOffset = new Vector3(0f, 4f, -8f);
    [SerializeField] private float thirdPersonPitch = 18f;

    [Header("First Person View")]
    [SerializeField] private Vector3 firstPersonOffset = new Vector3(0f, 1.8f, 1.2f);
    [SerializeField] private float firstPersonPitch = 5f;

    [Header("Mouse Look")]
    [SerializeField] private float mouseSensitivity = 0.18f;
    [SerializeField] private float minPitch = -25f;
    [SerializeField] private float maxPitch = 65f;

    [Header("Transition")]
    [SerializeField] private float switchDuration = 0.35f;

    private bool _isFirstPerson;
    private float _yaw;
    private float _pitch;
    private Vector3 _currentOffset;

    private void Start()
    {
        if (target == null)
        {
            Debug.LogWarning($"{nameof(VehicleCameraController)} needs a target.");
            enabled = false;
            return;
        }

        _currentOffset = thirdPersonOffset;
        _pitch = thirdPersonPitch;
        _yaw = target.eulerAngles.y;

        ApplyCameraPositionInstant();
    }

    private void LateUpdate()
    {
        if (target == null)
        {
            return;
        }

        HandleInput();
        FollowTarget();
    }

    private void HandleInput()
    {
        if (Keyboard.current != null && Keyboard.current.cKey.wasPressedThisFrame)
        {
            SwitchView();
        }

        if (Mouse.current == null)
        {
            return;
        }

        Vector2 mouseDelta = Mouse.current.delta.ReadValue();

        _yaw += mouseDelta.x * mouseSensitivity;
        _pitch -= mouseDelta.y * mouseSensitivity;
        _pitch = Mathf.Clamp(_pitch, minPitch, maxPitch);
    }

    private void SwitchView()
    {
        _isFirstPerson = !_isFirstPerson;

        Vector3 targetOffset = _isFirstPerson ? firstPersonOffset : thirdPersonOffset;
        float targetPitch = _isFirstPerson ? firstPersonPitch : thirdPersonPitch;

        Tween.StopAll(transform);

        Tween.Custom(
            startValue: _currentOffset,
            endValue: targetOffset,
            duration: switchDuration,
            onValueChange: value => _currentOffset = value,
            ease: Ease.InOutSine
        );

        Tween.Custom(
            startValue: _pitch,
            endValue: targetPitch,
            duration: switchDuration,
            onValueChange: value => _pitch = value,
            ease: Ease.InOutSine
        );
    }

    private void FollowTarget()
    {
        Quaternion lookRotation = Quaternion.Euler(_pitch, _yaw, 0f);

        Vector3 targetPosition = target.position + lookRotation * _currentOffset;

        transform.position = targetPosition;

        if (_isFirstPerson)
        {
            transform.rotation = lookRotation;
        }
        else
        {
            Vector3 lookPoint = target.position + Vector3.up * 1.2f;
            transform.rotation = Quaternion.LookRotation(lookPoint - transform.position, Vector3.up);
        }
    }

    private void ApplyCameraPositionInstant()
    {
        Quaternion lookRotation = Quaternion.Euler(_pitch, _yaw, 0f);
        transform.position = target.position + lookRotation * _currentOffset;

        Vector3 lookPoint = target.position + Vector3.up * 1.2f;
        transform.rotation = Quaternion.LookRotation(lookPoint - transform.position, Vector3.up);
    }
}