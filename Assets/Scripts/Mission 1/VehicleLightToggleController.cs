using UnityEngine;
using UnityEngine.InputSystem;

public class VehicleLightToggleController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerController playerController;

    [SerializeField] private Renderer lampRenderer;

    [Header("Reverse Detection")]
    [SerializeField] private float reverseSpeedThreshold = -0.5f;

    private Material _lampMaterial;

    private void Awake()
    {
        if (playerController == null)
        {
            playerController = GetComponent<PlayerController>();
        }

        if (lampRenderer != null)
        {
            _lampMaterial = lampRenderer.material;
        }

        SetEmission(false);
    }

    private void Update()
    {
        bool shouldEnableEmission = IsBraking() || IsReversing();
        SetEmission(shouldEnableEmission);
    }

    private bool IsBraking()
    {
        return Keyboard.current != null && Keyboard.current.spaceKey.isPressed;
    }

    private bool IsReversing()
    {
        bool isPressingReverse =
            Keyboard.current != null &&
            (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed);

        bool isMovingBackward =
            playerController != null &&
            playerController.CurrentSpeedKmh < reverseSpeedThreshold;

        return isPressingReverse || isMovingBackward;
    }

    private void SetEmission(bool isEnabled)
    {
        if (_lampMaterial == null)
        {
            return;
        }

        if (isEnabled)
        {
            _lampMaterial.EnableKeyword("_EMISSION");
            return;
        }

        _lampMaterial.DisableKeyword("_EMISSION");
    }
}