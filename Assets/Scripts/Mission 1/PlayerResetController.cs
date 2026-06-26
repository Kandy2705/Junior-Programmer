using UnityEngine;

[DisallowMultipleComponent]
public class PlayerResetController : MonoBehaviour
{
    [Header("Start Point")]
    [Tooltip("Vị trí xuất phát của player. Nếu để trống, vị trí hiện tại khi Start sẽ được dùng.")]
    [SerializeField] private Transform startPoint;

    [Header("Physics")]
    [SerializeField] private Rigidbody playerRigidbody;

    private PlayerController _playerController;

    private Vector3 _fallbackStartPosition;
    private Quaternion _fallbackStartRotation;

    private void Awake()
    {
        if (playerRigidbody == null)
        {
            playerRigidbody = GetComponent<Rigidbody>();
        }

        _playerController = GetComponent<PlayerController>();
        _fallbackStartPosition = transform.position;
        _fallbackStartRotation = transform.rotation;
    }

    public void ResetToStart()
    {
        Vector3 targetPosition = startPoint != null
            ? startPoint.position
            : _fallbackStartPosition;

        Quaternion targetRotation = startPoint != null
            ? startPoint.rotation
            : _fallbackStartRotation;

        if (_playerController != null)
        {
            _playerController.ResetState();
        }

        if (playerRigidbody != null)
        {
            playerRigidbody.linearVelocity = Vector3.zero;
            playerRigidbody.angularVelocity = Vector3.zero;

            playerRigidbody.position = targetPosition;
            playerRigidbody.rotation = targetRotation;

            playerRigidbody.Sleep();
            playerRigidbody.WakeUp();

            return;
        }

        transform.SetPositionAndRotation(targetPosition, targetRotation);
    }
}