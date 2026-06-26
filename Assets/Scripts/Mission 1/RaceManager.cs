using System.Collections.Generic;
using PrimeTween;
using TMPro;
using UnityEngine;

[DisallowMultipleComponent]
public class RaceManager : MonoBehaviour
{
    [Header("Race Settings")]
    [SerializeField] private int totalLaps = 3;

    [Header("References")]
    [SerializeField] private PlayerController playerController;
    [SerializeField] private UIFlowController uiFlowController;

    [Header("HUD")]
    [SerializeField] private TMP_Text lapCounterText;
    [SerializeField] private CanvasGroup lapNotificationGroup;
    [SerializeField] private TMP_Text lapNotificationText;
    [SerializeField] private float notificationVisibleDuration = 2f;
    [SerializeField] private float notificationPopScale = 1.15f;

    [Header("Result Panel Texts")]
    [SerializeField] private TMP_Text totalTimeValueText;
    [SerializeField] private TMP_Text bestLapValueText;
    [SerializeField] private TMP_Text maxSpeedValueText;

    private readonly HashSet<RaceCheckpoint> _allCheckpoints = new();
    private readonly HashSet<RaceCheckpoint> _passedCheckpoints = new();

    private bool _isRacing;
    private int _completedLaps;
    private float _totalTime;
    private float _currentLapTime;
    private float _bestLapTime;
    private float _maxSpeedKmh;

    private Vector3 _notificationBaseScale = Vector3.one;
    private Sequence _notificationSequence;

    private void Awake()
    {
        if (lapNotificationGroup != null)
        {
            _notificationBaseScale = lapNotificationGroup.transform.localScale;
        }
    }

    public void RegisterCheckpoint(RaceCheckpoint checkpoint)
    {
        _allCheckpoints.Add(checkpoint);
    }

    public void StartRace()
    {
        ResetRace();
        _isRacing = true;
        UpdateLapCounter();
    }

    public void ResetRace()
    {
        StopNotification();

        _isRacing = false;
        _completedLaps = 0;
        _totalTime = 0f;
        _currentLapTime = 0f;
        _bestLapTime = float.MaxValue;
        _maxSpeedKmh = 0f;
        _passedCheckpoints.Clear();

        if (lapNotificationGroup != null)
        {
            lapNotificationGroup.alpha = 0f;
            lapNotificationGroup.transform.localScale = _notificationBaseScale;
            lapNotificationGroup.gameObject.SetActive(false);
        }

        UpdateLapCounter();
    }

    private void Update()
    {
        if (!_isRacing)
        {
            return;
        }

        _totalTime += Time.deltaTime;
        _currentLapTime += Time.deltaTime;

        if (playerController != null)
        {
            float speed = Mathf.Abs(playerController.CurrentSpeedKmh);

            if (speed > _maxSpeedKmh)
            {
                _maxSpeedKmh = speed;
            }
        }
    }

    public void NotifyCheckpointPassed(RaceCheckpoint checkpoint)
    {
        if (!_isRacing)
        {
            return;
        }

        _passedCheckpoints.Add(checkpoint);
    }

    public void NotifyFinishLineCrossed()
    {
        if (!_isRacing)
        {
            return;
        }

        // Phải đi qua hết tất cả checkpoint thì lần cán vạch mới được tính là 1 vòng.
        // Nhờ vậy lần cán vạch ngay lúc xuất phát không bị đếm nhầm.
        if (_allCheckpoints.Count > 0 && _passedCheckpoints.Count < _allCheckpoints.Count)
        {
            return;
        }

        CompleteLap();
    }

    private void CompleteLap()
    {
        if (_currentLapTime < _bestLapTime)
        {
            _bestLapTime = _currentLapTime;
        }

        _completedLaps++;
        _currentLapTime = 0f;
        _passedCheckpoints.Clear();

        UpdateLapCounter();

        if (_completedLaps >= totalLaps)
        {
            FinishRace();
            return;
        }

        ShowLapNotification(
            $"Hoàn thành vòng {_completedLaps}!\nCòn {totalLaps - _completedLaps} vòng nữa"
        );
    }

    private void FinishRace()
    {
        _isRacing = false;
        FillResultPanel();

        if (uiFlowController != null)
        {
            uiFlowController.ShowResult();
        }
    }

    private void UpdateLapCounter()
    {
        if (lapCounterText == null)
        {
            return;
        }

        int displayLap = Mathf.Clamp(_completedLaps + 1, 1, totalLaps);
        lapCounterText.text = $"Lap {displayLap}/{totalLaps}";
    }

    private void FillResultPanel()
    {
        if (totalTimeValueText != null)
        {
            totalTimeValueText.text = FormatTime(_totalTime);
        }

        if (bestLapValueText != null)
        {
            bestLapValueText.text = _bestLapTime >= float.MaxValue
                ? "--:--.---"
                : FormatTime(_bestLapTime);
        }

        if (maxSpeedValueText != null)
        {
            maxSpeedValueText.text = $"{_maxSpeedKmh:0} km/h";
        }
    }

    private static string FormatTime(float seconds)
    {
        if (seconds < 0f)
        {
            seconds = 0f;
        }

        int minutes = (int)(seconds / 60f);
        float remaining = seconds - minutes * 60f;

        return $"{minutes:00}:{remaining:00.000}";
    }

    private void ShowLapNotification(string message)
    {
        if (lapNotificationText != null)
        {
            lapNotificationText.text = message;
        }

        if (lapNotificationGroup == null)
        {
            return;
        }

        StopNotification();

        lapNotificationGroup.gameObject.SetActive(true);
        lapNotificationGroup.alpha = 0f;
        lapNotificationGroup.transform.localScale = _notificationBaseScale * 0.8f;

        _notificationSequence = Sequence.Create()
            .Group(Tween.Alpha(lapNotificationGroup, 1f, 0.25f))
            .Group(Tween.Scale(
                lapNotificationGroup.transform,
                _notificationBaseScale * notificationPopScale,
                0.25f,
                Ease.OutBack))
            .Chain(Tween.Scale(
                lapNotificationGroup.transform,
                _notificationBaseScale,
                0.12f))
            .ChainDelay(notificationVisibleDuration)
            .Chain(Tween.Alpha(lapNotificationGroup, 0f, 0.3f))
            .OnComplete(() => lapNotificationGroup.gameObject.SetActive(false));
    }

    private void StopNotification()
    {
        if (_notificationSequence.isAlive)
        {
            _notificationSequence.Stop();
        }
    }
}
