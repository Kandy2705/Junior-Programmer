using System;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class WheelColliderCarController : MonoBehaviour
{
    [Header("Wheel Colliders")]
    [SerializeField] private WheelCollider frontLeftWheelCollider;
    [SerializeField] private WheelCollider frontRightWheelCollider;
    [SerializeField] private WheelCollider rearLeftWheelCollider;
    [SerializeField] private WheelCollider rearRightWheelCollider;

    [Header("Wheel Visuals")]
    [SerializeField] private Transform frontLeftWheelVisual;
    [SerializeField] private Transform frontRightWheelVisual;
    [SerializeField] private Transform rearLeftWheelVisual;
    [SerializeField] private Transform rearRightWheelVisual;

    [Header("Motor")]
    [SerializeField] private float maxMotorTorque = 1800f;
    [SerializeField] private float reverseMotorTorque = 700f;
    [SerializeField] private float maxSpeedKmh = 180f;

    [Header("Engine And Gearbox")]
    [SerializeField] private float maxRpm = 8000f;
    [SerializeField] private float shiftUpRpm = 7500f;
    [SerializeField] private float shiftDownRpm = 3000f;
    [SerializeField] private float shiftCooldown = 0.35f;

    [SerializeField] private float[] gearTorqueMultipliers = { 1.6f, 1.15f, 0.85f, 0.6f };

    [Header("Brake")]
    [SerializeField] private float brakeTorque = 3500f;
    [SerializeField] private float rollingBrakeTorque = 120f;

    [Header("Steering")]
    [SerializeField] private float steeringAngleAtZeroSpeed = 35f;
    [SerializeField] private float steeringAngleAtMaxSpeed = 8f;
    [SerializeField] private float steeringFullLockTime = 1.5f;

    [Header("Stability")]
    [SerializeField] private Vector3 centerOfMassOffset = new Vector3(0f, -0.45f, 0f);

    [Header("Runtime Debug")]
    [SerializeField] private float currentSpeedKmh;
    [SerializeField] private float currentSignedSpeedKmh;
    [SerializeField] private float currentAccelerationKmhPerSecond;
    [SerializeField] private float currentRpm;
    [SerializeField] private int currentGear = 1;
    [SerializeField] private float currentSteeringAngle;
    [SerializeField] private float currentThrottleInput;
    [SerializeField] private bool isBrakingNow;
    [SerializeField] private bool isReversingNow;

    private readonly float[] _gearMaxSpeedsKmh = { 50f, 90f, 140f, 180f };

    private Rigidbody _rigidbody;
    private float _lastShiftTime;
    private float _previousSpeedKmh;

    public event Action<int> GearChanged;

    public float CurrentSpeedKmh => currentSpeedKmh;
    public float CurrentSignedSpeedKmh => currentSignedSpeedKmh;
    public float CurrentAccelerationKmhPerSecond => currentAccelerationKmhPerSecond;
    public float CurrentRpm => currentRpm;
    public int CurrentGear => currentGear;
    public float CurrentSteeringAngle => currentSteeringAngle;
    public bool IsBrakingNow => isBrakingNow;
    public bool IsReversingNow => isReversingNow;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _rigidbody.centerOfMass += centerOfMassOffset;
    }

    private void FixedUpdate()
    {
        UpdateSpeed();
        UpdateEngineAndGearbox();

        float throttleInput = GetThrottleInput();
        float steeringInput = GetSteeringInput();
        bool isBraking = IsBrakePressed();

        currentThrottleInput = throttleInput;
        isBrakingNow = isBraking;
        isReversingNow = throttleInput < -0.1f || currentSignedSpeedKmh < -0.5f;

        UpdateSteering(steeringInput);
        ApplyMotor(throttleInput, isBraking);
        ApplyBrake(throttleInput, isBraking);
    }

    private void LateUpdate()
    {
        UpdateWheelVisuals();
    }

    private void UpdateSpeed()
    {
        float forwardVelocity = Vector3.Dot(_rigidbody.linearVelocity, transform.forward);

        currentSignedSpeedKmh = forwardVelocity * 3.6f;
        currentSpeedKmh = Mathf.Abs(currentSignedSpeedKmh);

        currentAccelerationKmhPerSecond =
            (currentSpeedKmh - _previousSpeedKmh) / Time.fixedDeltaTime;

        _previousSpeedKmh = currentSpeedKmh;
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

    private bool IsBrakePressed()
    {
        return Keyboard.current != null && Keyboard.current.spaceKey.isPressed;
    }

    private void UpdateEngineAndGearbox()
    {
        float currentGearMaxSpeed = _gearMaxSpeedsKmh[currentGear - 1];

        currentRpm = currentSpeedKmh / currentGearMaxSpeed * maxRpm;
        currentRpm = Mathf.Clamp(currentRpm, 0f, maxRpm);

        if (Time.time - _lastShiftTime < shiftCooldown)
        {
            return;
        }

        if (currentRpm > shiftUpRpm && currentGear < _gearMaxSpeedsKmh.Length)
        {
            ShiftToGear(currentGear + 1);
            return;
        }

        if (currentRpm < shiftDownRpm && currentGear > 1)
        {
            ShiftToGear(currentGear - 1);
        }
    }

    private void ShiftToGear(int newGear)
    {
        if (newGear == currentGear)
        {
            return;
        }

        currentGear = newGear;
        _lastShiftTime = Time.time;

        RecalculateRpmAfterShift();

        GearChanged?.Invoke(currentGear);
    }

    private void RecalculateRpmAfterShift()
    {
        float currentGearMaxSpeed = _gearMaxSpeedsKmh[currentGear - 1];

        currentRpm = currentSpeedKmh / currentGearMaxSpeed * maxRpm;
        currentRpm = Mathf.Clamp(currentRpm, 0f, maxRpm);
    }

    private void UpdateSteering(float steeringInput)
    {
        float speed01 = Mathf.InverseLerp(0f, maxSpeedKmh, currentSpeedKmh);

        float maxSteeringAngle = Mathf.Lerp(
            steeringAngleAtZeroSpeed,
            steeringAngleAtMaxSpeed,
            speed01
        );

        float targetSteeringAngle = steeringInput * maxSteeringAngle;
        float steeringSpeed = steeringAngleAtZeroSpeed / steeringFullLockTime;

        currentSteeringAngle = Mathf.MoveTowards(
            currentSteeringAngle,
            targetSteeringAngle,
            steeringSpeed * Time.fixedDeltaTime
        );

        frontLeftWheelCollider.steerAngle = currentSteeringAngle;
        frontRightWheelCollider.steerAngle = currentSteeringAngle;
    }

    private void ApplyMotor(float throttleInput, bool isBraking)
    {
        float motorTorque = 0f;

        if (!isBraking && currentSpeedKmh < maxSpeedKmh)
        {
            if (throttleInput > 0f)
            {
                float gearMultiplier = gearTorqueMultipliers[currentGear - 1];
                motorTorque = throttleInput * maxMotorTorque * gearMultiplier;
            }
            else if (throttleInput < 0f)
            {
                motorTorque = throttleInput * reverseMotorTorque;
            }
        }

        rearLeftWheelCollider.motorTorque = motorTorque;
        rearRightWheelCollider.motorTorque = motorTorque;

        frontLeftWheelCollider.motorTorque = 0f;
        frontRightWheelCollider.motorTorque = 0f;
    }

    private void ApplyBrake(float throttleInput, bool isBraking)
    {
        float targetBrakeTorque = 0f;

        if (isBraking)
        {
            targetBrakeTorque = brakeTorque;
        }
        else if (Mathf.Approximately(throttleInput, 0f))
        {
            targetBrakeTorque = rollingBrakeTorque;
        }

        frontLeftWheelCollider.brakeTorque = targetBrakeTorque;
        frontRightWheelCollider.brakeTorque = targetBrakeTorque;
        rearLeftWheelCollider.brakeTorque = targetBrakeTorque;
        rearRightWheelCollider.brakeTorque = targetBrakeTorque;
    }

    private void UpdateWheelVisuals()
    {
        UpdateWheelVisual(frontLeftWheelCollider, frontLeftWheelVisual);
        UpdateWheelVisual(frontRightWheelCollider, frontRightWheelVisual);
        UpdateWheelVisual(rearLeftWheelCollider, rearLeftWheelVisual);
        UpdateWheelVisual(rearRightWheelCollider, rearRightWheelVisual);
    }

    private void UpdateWheelVisual(WheelCollider wheelCollider, Transform wheelVisual)
    {
        if (wheelCollider == null || wheelVisual == null)
        {
            return;
        }

        wheelCollider.GetWorldPose(out Vector3 position, out Quaternion rotation);

        wheelVisual.position = position;
        wheelVisual.rotation = rotation;
    }
}