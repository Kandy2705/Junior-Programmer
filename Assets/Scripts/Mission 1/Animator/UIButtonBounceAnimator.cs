using PrimeTween;
using UnityEngine;

/// <summary>
/// Adds a looping idle bounce animation to a UI button or any RectTransform.
/// Uses unscaled time so it can animate while gameplay is paused.
/// </summary>
[DisallowMultipleComponent]
public class UIButtonBounceAnimator : MonoBehaviour
{
    [Header("Bounce Settings")]
    [SerializeField] private bool playOnEnable = true;

    [Tooltip("Scale lớn nhất khi UI nhúng nhảy.")]
    [SerializeField] private float bounceScale = 1.08f;

    [Tooltip("Thời gian scale lên.")]
    [SerializeField] private float scaleUpDuration = 0.35f;

    [Tooltip("Thời gian scale xuống.")]
    [SerializeField] private float scaleDownDuration = 0.35f;

    [Tooltip("Delay giữa mỗi nhịp nhúng nhảy.")]
    [SerializeField] private float delayBetweenBounces = 0.15f;

    [Tooltip("Bật để animation vẫn chạy khi Time.timeScale = 0.")]
    [SerializeField] private bool useUnscaledTime = true;

    private Vector3 _originalScale;
    private Sequence _bounceSequence;

    private void Awake()
    {
        _originalScale = transform.localScale;
    }

    private void OnEnable()
    {
        if (playOnEnable)
        {
            PlayBounce();
        }
    }

    private void OnDisable()
    {
        StopBounce();
    }

    public void PlayBounce()
    {
        StopBounce();

        transform.localScale = _originalScale;

        _bounceSequence = Sequence.Create(cycles: -1, useUnscaledTime: useUnscaledTime)
            .Chain(Tween.Scale(
                transform,
                _originalScale * bounceScale,
                scaleUpDuration,
                Ease.OutSine
            ))
            .Chain(Tween.Scale(
                transform,
                _originalScale,
                scaleDownDuration,
                Ease.InOutSine
            ))
            .ChainDelay(delayBetweenBounces);
    }

    public void StopBounce()
    {
        if (_bounceSequence.isAlive)
        {
            _bounceSequence.Stop();
        }

        transform.localScale = _originalScale;
    }
}