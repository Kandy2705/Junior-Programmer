using PrimeTween;
using TMPro;
using UnityEngine;

/// <summary>
/// Displays vehicle speed, gear, and RPM on the gameplay HUD.
/// Plays a small PrimeTween animation when the vehicle changes gear.
/// </summary>
public class VehicleHudController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerController playerController;
    [SerializeField] private TMP_Text speedText;
    [SerializeField] private TMP_Text gearText;
    [SerializeField] private TMP_Text rpmText;

    [Header("Gear Pop Animation")]
    [SerializeField] private float gearPopScale = 1.25f;
    [SerializeField] private float gearPopDuration = 0.12f;

    private Vector3 _gearTextOriginalScale;

    private void Awake()
    {
        if (gearText != null)
        {
            _gearTextOriginalScale = gearText.transform.localScale;
        }
    }

    private void OnEnable()
    {
        if (playerController != null)
        {
            playerController.GearChanged += HandleGearChanged;
        }
    }

    private void OnDisable()
    {
        if (playerController != null)
        {
            playerController.GearChanged -= HandleGearChanged;
        }
    }

    private void Start()
    {
        RefreshHud();
    }

    private void Update()
    {
        RefreshHud();
    }

    private void RefreshHud()
    {
        if (playerController == null)
        {
            return;
        }

        if (speedText != null)
        {
            speedText.text = $"Speed: {Mathf.Abs(playerController.CurrentSpeedKmh):0} km/h";
        }

        if (gearText != null)
        {
            gearText.text = $"Gear: {playerController.CurrentGear}";
        }

        if (rpmText != null)
        {
            rpmText.text = $"RPM: {playerController.CurrentRpm:0}";
        }
    }

    private void HandleGearChanged(int newGear)
    {
        PlayGearPopAnimation();
    }

    private void PlayGearPopAnimation()
    {
        if (gearText == null)
        {
            return;
        }

        Tween.StopAll(gearText.transform);
        gearText.transform.localScale = _gearTextOriginalScale;

        Sequence.Create()
            .Chain(Tween.Scale(
                gearText.transform,
                endValue: _gearTextOriginalScale * gearPopScale,
                duration: gearPopDuration,
                ease: Ease.OutBack
            ))
            .Chain(Tween.Scale(
                gearText.transform,
                endValue: _gearTextOriginalScale,
                duration: gearPopDuration,
                ease: Ease.InOutSine
            ));
    }
}