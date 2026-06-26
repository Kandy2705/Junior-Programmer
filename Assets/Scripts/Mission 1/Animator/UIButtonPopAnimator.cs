using PrimeTween;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Adds hover and click pop animations to a UI button.
/// The button stays still when idle.
/// Uses unscaled time so it still works while the game is paused.
/// </summary>
[DisallowMultipleComponent]
public class UIButtonPopAnimator : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [Header("Hover Animation")]
    [SerializeField] private float hoverDownScale = 0.9f;
    [SerializeField] private float hoverReleaseBounceScale = 1.08f;
    [SerializeField] private float hoverDownDuration = 0.12f;
    [SerializeField] private float hoverBounceUpDuration = 0.12f;
    [SerializeField] private float hoverReturnDuration = 0.08f;

    [Header("Click Animation")]
    [SerializeField] private float clickDownScale = 0.88f;
    [SerializeField] private float clickUpScale = 1.12f;
    [SerializeField] private float clickDownDuration = 0.06f;
    [SerializeField] private float clickUpDuration = 0.1f;
    [SerializeField] private float clickReturnDuration = 0.08f;

    private Vector3 _originalScale;
    private Sequence _activeSequence;

    private void Awake()
    {
        _originalScale = transform.localScale;
    }

    private void OnDisable()
    {
        StopActiveAnimation();
        transform.localScale = _originalScale;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        StopActiveAnimation();

        _activeSequence = Sequence.Create(useUnscaledTime: true)
            .Chain(Tween.Scale(
                transform,
                _originalScale * hoverDownScale,
                hoverDownDuration,
                Ease.InOutSine
            ));
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        StopActiveAnimation();

        _activeSequence = Sequence.Create(useUnscaledTime: true)
            .Chain(Tween.Scale(
                transform,
                _originalScale * hoverReleaseBounceScale,
                hoverBounceUpDuration,
                Ease.OutBack
            ))
            .Chain(Tween.Scale(
                transform,
                _originalScale,
                hoverReturnDuration,
                Ease.InOutSine
            ));
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        StopActiveAnimation();

        _activeSequence = Sequence.Create(useUnscaledTime: true)
            .Chain(Tween.Scale(
                transform,
                _originalScale * clickDownScale,
                clickDownDuration,
                Ease.InSine
            ))
            .Chain(Tween.Scale(
                transform,
                _originalScale * clickUpScale,
                clickUpDuration,
                Ease.OutBack
            ))
            .Chain(Tween.Scale(
                transform,
                _originalScale,
                clickReturnDuration,
                Ease.InOutSine
            ));
    }

    private void StopActiveAnimation()
    {
        if (_activeSequence.isAlive)
        {
            _activeSequence.Stop();
        }
    }
}