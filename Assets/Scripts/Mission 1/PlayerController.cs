using System;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Controls a car-like vehicle with acceleration, braking, speed-based steering,
/// wheel visualization, RPM, and a simple automatic gearbox.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    [Header("Speed")]
    [SerializeField] private float maxSpeedKmh = 180f;

    [Tooltip("Lực cản khi thả ga, đơn vị km/h mỗi giây.")]
    [SerializeField] private float coastDragKmhPerSecond = 8f;

    [Tooltip("Lực phanh khi nhấn Space, đơn vị km/h mỗi giây.")]
    [SerializeField] private float brakeKmhPerSecond = 45f;

    [Header("Engine And Gearbox")]
    [SerializeField] private float maxRpm = 8000f;
    [SerializeField] private float shiftUpRpm = 7500f;
    [SerializeField] private float shiftDownRpm = 3000f;

    [Tooltip("Trễ nhỏ để xe không bị nhảy số liên tục.")]
    [SerializeField] private float shiftCooldown = 0.35f;

    [Header("Steering")]
    [SerializeField] private float steeringAngleAtZeroSpeed = 45f;
    [SerializeField] private float steeringAngleAtMaxSpeed = 10f;

    [Tooltip("Thời gian để bánh lái đi từ 0 độ tới góc lái tối đa.")]
    [SerializeField] private float steeringFullLockTime = 1.5f;

    [Tooltip("Độ nhạy xoay thân xe theo góc lái.")]
    [SerializeField] private float turnResponse = 1.8f;

    [Header("Wheel Visuals")]
    [SerializeField] private Transform frontLeftWheel;
    [SerializeField] private Transform frontRightWheel;
    [SerializeField] private Transform rearLeftWheel;
    [SerializeField] private Transform rearRightWheel;

    [Tooltip("Bán kính bánh xe dùng để tính tốc độ quay visual.")]
    [SerializeField] private float wheelRadius = 0.35f;

    private readonly float[] _gearMaxSpeedsKmh = { 50f, 90f, 140f, 180f };
    private readonly float[] _gearAccelerationKmhPerSecond =
    {
        50f / 3f,
        40f / 4f,
        50f / 8f,
        40f / 15f
    };

    private Rigidbody _rigidbody;

    private int _currentGear = 1;
    private float _currentRpm;
    private float _currentSpeedKmh;
    private float _currentSteeringAngle;
    private float _wheelSpinAngle;
    private float _lastShiftTime;

    private Quaternion _frontLeftInitialRotation;
    private Quaternion _frontRightInitialRotation;
    private Quaternion _rearLeftInitialRotation;
    private Quaternion _rearRightInitialRotation;

    public int CurrentGear => _currentGear;
    public float CurrentRpm => _currentRpm;
    public float CurrentSpeedKmh => _currentSpeedKmh;
    public float CurrentSteeringAngle => _currentSteeringAngle;

    public event Action<int> GearChanged;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();

        _frontLeftInitialRotation = frontLeftWheel != null ? frontLeftWheel.localRotation : Quaternion.identity;
        _frontRightInitialRotation = frontRightWheel != null ? frontRightWheel.localRotation : Quaternion.identity;
        _rearLeftInitialRotation = rearLeftWheel != null ? rearLeftWheel.localRotation : Quaternion.identity;
        _rearRightInitialRotation = rearRightWheel != null ? rearRightWheel.localRotation : Quaternion.identity;
    }

    private void FixedUpdate()
    {
        float throttleInput = GetThrottleInput();
        float steeringInput = GetSteeringInput();
        bool isBraking = IsBraking();

        UpdateEngineAndGearbox();
        UpdateSpeed(throttleInput, isBraking);
        UpdateSteering(steeringInput);
        MoveVehicle();
        UpdateWheelVisuals();
        StabilizeVehicle();
    }

    private float GetThrottleInput()
    {
        if (Keyboard.current == null)
        {
            return 0f;
        }

        float input = 0f;

        if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed)
        {
            input += 1f;
        }

        if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed)
        {
            input -= 1f;
        }

        return Mathf.Clamp(input, -1f, 1f);
    }

    private float GetSteeringInput()
    {
        if (Keyboard.current == null)
        {
            return 0f;
        }

        float input = 0f;

        if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed)
        {
            input += 1f;
        }

        if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed)
        {
            input -= 1f;
        }

        return Mathf.Clamp(input, -1f, 1f);
    }

    private bool IsBraking()
    {
        return Keyboard.current != null && Keyboard.current.spaceKey.isPressed;
    }

    private void UpdateEngineAndGearbox()
    {
        float currentGearMaxSpeed = _gearMaxSpeedsKmh[_currentGear - 1];

        _currentRpm = Mathf.Abs(_currentSpeedKmh) / currentGearMaxSpeed * maxRpm;
        _currentRpm = Mathf.Clamp(_currentRpm, 0f, maxRpm);

        if (Time.time - _lastShiftTime < shiftCooldown)
        {
            return;
        }

        if (_currentRpm > shiftUpRpm && _currentGear < _gearMaxSpeedsKmh.Length)
        {
            ShiftToGear(_currentGear + 1);
            return;
        }

        if (_currentRpm < shiftDownRpm && _currentGear > 1)
        {
            ShiftToGear(_currentGear - 1);
        }
    }

    private void RecalculateRpmAfterShift()
    {
        float currentGearMaxSpeed = _gearMaxSpeedsKmh[_currentGear - 1];

        _currentRpm = Mathf.Abs(_currentSpeedKmh) / currentGearMaxSpeed * maxRpm;
        _currentRpm = Mathf.Clamp(_currentRpm, 0f, maxRpm);
    }

    private void UpdateSpeed(float throttleInput, bool isBraking)
    {
        if (throttleInput > 0f)
        {
            int gearIndex = _currentGear - 1;
            float acceleration = _gearAccelerationKmhPerSecond[gearIndex];

            _currentSpeedKmh += acceleration * throttleInput * Time.fixedDeltaTime;
        }
        else if (throttleInput < 0f)
        {
            _currentSpeedKmh += 12f * throttleInput * Time.fixedDeltaTime;
        }
        else
        {
            _currentSpeedKmh = Mathf.MoveTowards(
                _currentSpeedKmh,
                0f,
                coastDragKmhPerSecond * Time.fixedDeltaTime
            );
        }

        if (isBraking)
        {
            _currentSpeedKmh = Mathf.MoveTowards(
                _currentSpeedKmh,
                0f,
                brakeKmhPerSecond * Time.fixedDeltaTime
            );
        }

        _currentSpeedKmh = Mathf.Clamp(_currentSpeedKmh, -maxSpeedKmh * 0.35f, maxSpeedKmh);
    }

    private void UpdateSteering(float steeringInput)
    {
        float speed01 = Mathf.InverseLerp(0f, maxSpeedKmh, Mathf.Abs(_currentSpeedKmh));

        float maxSteeringAngle = Mathf.Lerp(
            steeringAngleAtZeroSpeed,
            steeringAngleAtMaxSpeed,
            speed01
        );

        float targetSteeringAngle = steeringInput * maxSteeringAngle;
        float steeringSpeed = steeringAngleAtZeroSpeed / steeringFullLockTime;

        _currentSteeringAngle = Mathf.MoveTowards(
            _currentSteeringAngle,
            targetSteeringAngle,
            steeringSpeed * Time.fixedDeltaTime
        );
    }

    private void MoveVehicle()
    {
        float speedMetersPerSecond = _currentSpeedKmh / 3.6f;

        Vector3 movement = transform.forward * speedMetersPerSecond * Time.fixedDeltaTime;
        _rigidbody.MovePosition(_rigidbody.position + movement);

        float turnAmount = _currentSteeringAngle * turnResponse * Mathf.Abs(speedMetersPerSecond) * Time.fixedDeltaTime;

        if (_currentSpeedKmh < 0f)
        {
            turnAmount *= -1f;
        }

        Quaternion turnRotation = Quaternion.Euler(0f, turnAmount, 0f);
        _rigidbody.MoveRotation(_rigidbody.rotation * turnRotation);
    }

    private void UpdateWheelVisuals()
    {
        float speedMetersPerSecond = _currentSpeedKmh / 3.6f;
        float circumference = 2f * Mathf.PI * wheelRadius;

        if (circumference > 0.01f)
        {
            float spinDegreesPerSecond = speedMetersPerSecond / circumference * 360f;
            _wheelSpinAngle += spinDegreesPerSecond * Time.fixedDeltaTime;
        }

        Quaternion steeringRotation = Quaternion.Euler(0f, _currentSteeringAngle, 0f);
        Quaternion spinRotation = Quaternion.Euler(_wheelSpinAngle, 0f, 0f);

        if (frontLeftWheel != null)
        {
            frontLeftWheel.localRotation = _frontLeftInitialRotation * steeringRotation * spinRotation;
        }

        if (frontRightWheel != null)
        {
            frontRightWheel.localRotation = _frontRightInitialRotation * steeringRotation * spinRotation;
        }

        if (rearLeftWheel != null)
        {
            rearLeftWheel.localRotation = _rearLeftInitialRotation * spinRotation;
        }

        if (rearRightWheel != null)
        {
            rearRightWheel.localRotation = _rearRightInitialRotation * spinRotation;
        }
    }

    private void ShiftToGear(int newGear)
    {
        if (newGear == _currentGear)
        {
            return;
        }

        _currentGear = newGear;
        _lastShiftTime = Time.time;

        RecalculateRpmAfterShift();

        GearChanged?.Invoke(_currentGear);
    }
    private void StabilizeVehicle()
    {
        Vector3 angularVelocity = _rigidbody.angularVelocity;
        angularVelocity.x = 0f;
        angularVelocity.z = 0f;
        _rigidbody.angularVelocity = angularVelocity;

        Vector3 eulerAngles = _rigidbody.rotation.eulerAngles;
        Quaternion stableRotation = Quaternion.Euler(0f, eulerAngles.y, 0f);
        _rigidbody.MoveRotation(stableRotation);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (IsWallLikeCollision(collision))
        {
            _currentSpeedKmh *= 0.8f;
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        if (!IsWallLikeCollision(collision))
        {
            return;
        }

        _currentSpeedKmh = Mathf.MoveTowards(
            _currentSpeedKmh,
            0f,
            brakeKmhPerSecond * Time.fixedDeltaTime
        );
    }

    private bool IsWallLikeCollision(Collision collision)
    {
        foreach (ContactPoint contact in collision.contacts)
        {
            if (contact.normal.y < 0.5f)
            {
                return true;
            }
        }

        return false;
    }
}