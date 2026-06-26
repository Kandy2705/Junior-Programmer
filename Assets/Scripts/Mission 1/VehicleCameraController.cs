using System.Collections.Generic;
using PrimeTween;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

[DisallowMultipleComponent]
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
    [SerializeField] private bool returnBehindVehicleWhenMouseReleased = false;
    [SerializeField] private float returnBehindSpeed = 180f;

    [Header("Transition")]
    [SerializeField] private float switchDuration = 0.35f;

    public bool IsInputEnabled { get; set; } = true;

    private readonly List<RaycastResult> _uiRaycastResults = new List<RaycastResult>();

    private PointerEventData _pointerEventData;
    private bool _isFirstPerson;
    private float _orbitYawOffset;
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
        _orbitYawOffset = 0f;

        ApplyCameraPositionInstant();
    }

    private void LateUpdate()
    {
        if (target == null)
        {
            return;
        }

        if (IsInputEnabled)
        {
            HandleInput();
        }

        FollowTarget();
    }

    private void HandleInput()
    {
        if (Keyboard.current != null && Keyboard.current.cKey.wasPressedThisFrame)
        {
            SwitchView();
        }

        HandleMouseLook();
    }

    private void HandleMouseLook()
    {
        if (Mouse.current == null)
        {
            return;
        }

        if (IsPointerOverInteractiveUi())
        {
            return;
        }

        bool isHoldingLeftMouse = Mouse.current.leftButton.isPressed;

        if (isHoldingLeftMouse)
        {
            Vector2 mouseDelta = Mouse.current.delta.ReadValue();

            _orbitYawOffset += mouseDelta.x * mouseSensitivity;
            _pitch -= mouseDelta.y * mouseSensitivity;
            _pitch = Mathf.Clamp(_pitch, minPitch, maxPitch);

            return;
        }

        if (returnBehindVehicleWhenMouseReleased)
        {
            _orbitYawOffset = Mathf.MoveTowards(
                _orbitYawOffset,
                0f,
                returnBehindSpeed * Time.deltaTime
            );
        }
    }

    private bool IsPointerOverInteractiveUi()
    {
        if (EventSystem.current == null || Mouse.current == null)
        {
            return false;
        }

        _pointerEventData ??= new PointerEventData(EventSystem.current);
        _pointerEventData.position = Mouse.current.position.ReadValue();

        _uiRaycastResults.Clear();
        EventSystem.current.RaycastAll(_pointerEventData, _uiRaycastResults);

        for (int i = 0; i < _uiRaycastResults.Count; i++)
        {
            GameObject hitObject = _uiRaycastResults[i].gameObject;

            if (hitObject == null)
            {
                continue;
            }

            if (hitObject.GetComponentInParent<Selectable>() != null)
            {
                return true;
            }
        }

        return false;
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
        float finalYaw = target.eulerAngles.y + _orbitYawOffset;
        Quaternion lookRotation = Quaternion.Euler(_pitch, finalYaw, 0f);

        Vector3 targetPosition = target.position + lookRotation * _currentOffset;

        transform.position = targetPosition;

        if (_isFirstPerson)
        {
            transform.rotation = lookRotation;
            return;
        }

        Vector3 lookPoint = target.position + Vector3.up * 1.2f;
        transform.rotation = Quaternion.LookRotation(lookPoint - transform.position, Vector3.up);
    }

    private void ApplyCameraPositionInstant()
    {
        float finalYaw = target.eulerAngles.y + _orbitYawOffset;
        Quaternion lookRotation = Quaternion.Euler(_pitch, finalYaw, 0f);

        transform.position = target.position + lookRotation * _currentOffset;

        Vector3 lookPoint = target.position + Vector3.up * 1.2f;
        transform.rotation = Quaternion.LookRotation(lookPoint - transform.position, Vector3.up);
    }
}