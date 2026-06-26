using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Collider))]
public class RaceFinishLine : MonoBehaviour
{
    private RaceManager _raceManager;

    private void Start()
    {
        _raceManager = FindFirstObjectByType<RaceManager>();
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

        _raceManager.NotifyFinishLineCrossed();
    }
}
