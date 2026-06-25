using PrimeTween;
using TMPro;
using UnityEngine;

public class WheelColliderVehicleHudController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private WheelColliderCarController carController;

    [Header("Text UI")]
    [SerializeField] private TMP_Text speedText;
    [SerializeField] private TMP_Text gearText;
    [SerializeField] private TMP_Text rpmText;
    [SerializeField] private TMP_Text steeringText;
    [SerializeField] private TMP_Text accelerationText;

    [Header("Gear Animation")]
    [SerializeField] private float gearPopScale = 1.25f;
    [SerializeField] private float gearPopDuration = 0.12f;

    private Vector3 _gearOriginalScale;

    private void Awake()
    {
        if (gearText != null)
        {
            _gearOriginalScale = gearText.transform.localScale;
        }
    }

    private void OnEnable()
    {
        if (carController != null)
        {
            carController.GearChanged += HandleGearChanged;
        }
    }

    private void OnDisable()
    {
        if (carController != null)
        {
            carController.GearChanged -= HandleGearChanged;
        }
    }

    private void Update()
    {
        if (carController == null)
        {
            return;
        }

        UpdateSpeedText();
        UpdateGearText();
        UpdateRpmText();
        UpdateSteeringText();
        UpdateAccelerationText();
    }

    private void UpdateSpeedText()
    {
        if (speedText == null)
        {
            return;
        }

        speedText.text = $"Speed: {carController.CurrentSpeedKmh:0} km/h";
    }

    private void UpdateGearText()
    {
        if (gearText == null)
        {
            return;
        }

        gearText.text = $"Gear: {carController.CurrentGear}";
    }

    private void UpdateRpmText()
    {
        if (rpmText == null)
        {
            return;
        }

        rpmText.text = $"RPM: {carController.CurrentRpm:0}";
    }

    private void UpdateSteeringText()
    {
        if (steeringText == null)
        {
            return;
        }

        steeringText.text = $"Steering: {carController.CurrentSteeringAngle:0.0}°";
    }

    private void UpdateAccelerationText()
    {
        if (accelerationText == null)
        {
            return;
        }

        accelerationText.text =
            $"Accel: {carController.CurrentAccelerationKmhPerSecond:0.0} km/h/s";
    }

    private void HandleGearChanged(int newGear)
    {
        if (gearText == null)
        {
            return;
        }

        Tween.StopAll(gearText.transform);

        Sequence.Create()
            .Chain(Tween.Scale(
                gearText.transform,
                _gearOriginalScale * gearPopScale,
                gearPopDuration,
                Ease.OutBack
            ))
            .Chain(Tween.Scale(
                gearText.transform,
                _gearOriginalScale,
                gearPopDuration,
                Ease.InOutSine
            ));
    }
}