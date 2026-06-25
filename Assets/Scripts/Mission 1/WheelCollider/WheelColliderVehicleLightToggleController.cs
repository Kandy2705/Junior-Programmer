using UnityEngine;

public class WheelColliderVehicleLightToggleController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private WheelColliderCarController carController;

    [SerializeField] private Renderer lampRenderer;

    private Material _lampMaterial;

    private void Awake()
    {
        if (carController == null)
        {
            carController = GetComponent<WheelColliderCarController>();
        }

        if (lampRenderer != null)
        {
            _lampMaterial = lampRenderer.material;
        }

        SetEmission(false);
    }

    private void Update()
    {
        if (carController == null)
        {
            return;
        }

        bool shouldEnableEmission = carController.IsBrakingNow || carController.IsReversingNow;
        SetEmission(shouldEnableEmission);
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