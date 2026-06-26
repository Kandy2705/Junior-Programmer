using System;
using PrimeTween;
using UnityEngine;

/// <summary>
/// Plays popup open/close scale animations for inner popup panels.
/// Useful for Pause, Result, Settings panels.
/// </summary>
[DisallowMultipleComponent]
public class UIPopOnEnableAnimator : MonoBehaviour
{
    [Header("Open Pop Settings")]
    [SerializeField] private float startScale = 0.85f;
    [SerializeField] private float overshootScale = 1.08f;
    [SerializeField] private float endScale = 1f;
    [SerializeField] private float popUpDuration = 0.16f;
    [SerializeField] private float settleDuration = 0.08f;

    [Header("Close Pop Settings")]
    [SerializeField] private float closeScale = 0.85f;
    [SerializeField] private float closeDuration = 0.12f;

    [Header("Time")]
    [Tooltip("Bật để animation vẫn chạy khi Time.timeScale = 0.")]
    [SerializeField] private bool useUnscaledTime = true;

    private Vector3 _originalScale;
    private Sequence _activeSequence;

    private void Awake()
    {
        _originalScale = transform.localScale;
    }

    private void OnEnable()
    {
        PlayOpen();
    }

    private void OnDisable()
    {
        StopActiveAnimation();
        transform.localScale = _originalScale;
    }

    public void PlayOpen()
    {
        StopActiveAnimation();

        transform.localScale = _originalScale * startScale;

        _activeSequence = Sequence.Create(useUnscaledTime: useUnscaledTime)
            .Chain(Tween.Scale(
                transform,
                _originalScale * overshootScale,
                popUpDuration,
                Ease.OutBack
            ))
            .Chain(Tween.Scale(
                transform,
                _originalScale * endScale,
                settleDuration,
                Ease.InOutSine
            ));
    }

    public void PlayClose(Action onComplete)
    {
        StopActiveAnimation();

        _activeSequence = Sequence.Create(useUnscaledTime: useUnscaledTime)
            .Chain(Tween.Scale(
                transform,
                _originalScale * closeScale,
                closeDuration,
                Ease.InBack
            ))
            .OnComplete(() =>
            {
                transform.localScale = _originalScale;
                onComplete?.Invoke();
            });
    }

    private void StopActiveAnimation()
    {
        if (_activeSequence.isAlive)
        {
            _activeSequence.Stop();
        }
    }
}