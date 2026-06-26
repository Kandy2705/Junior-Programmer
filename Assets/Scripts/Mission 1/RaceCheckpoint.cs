using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Collider))]
public class RaceCheckpoint : MonoBehaviour
{
    private RaceManager _raceManager;

    private void Start()
    {
        _raceManager = FindFirstObjectByType<RaceManager>();

        if (_raceManager != null)
        {
            _raceManager.RegisterCheckpoint(this);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_raceManager == null)
        {
            return;
        }

        if (other.GetComponentInParent<PlayerController>() == null)
        {
            return;
        }

        _raceManager.NotifyCheckpointPassed(this);
    }
}
