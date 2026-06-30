using PrimeTween;
using UnityEngine;

[DisallowMultipleComponent]
public class UIButtonBounceAnimator : MonoBehaviour
{
    [Header("Scale Settings")]
    [SerializeField] private bool playOnEnable = true;

    [SerializeField] private float targetScale = 1.08f;

    [SerializeField] private float scaleUpDuration = 0.35f;

    [SerializeField] private bool useUnscaledTime = true;

    private Vector3 _originalScale;
    private Sequence _scaleSequence;

    private void Awake()
    {
        _originalScale = transform.localScale;
    }

    private void OnEnable()
    {
        if (playOnEnable)
        {
            PlayScaleUpOnce();
        }
    }

    private void OnDisable()
    {
        StopAnimation();
    }

    public void PlayScaleUpOnce()
    {
        StopAnimation();

        transform.localScale = _originalScale;

        _scaleSequence = Sequence.Create(cycles: 1, useUnscaledTime: useUnscaledTime)
            .Chain(Tween.Scale(
                transform,
                _originalScale * targetScale,
                scaleUpDuration,
                Ease.OutBack
            ));
    }

    public void StopAnimation()
    {
        if (_scaleSequence.isAlive)
        {
            _scaleSequence.Stop();
        }

        transform.localScale = _originalScale;
    }
}