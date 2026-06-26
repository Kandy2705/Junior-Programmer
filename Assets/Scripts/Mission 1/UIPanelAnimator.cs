using PrimeTween;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(CanvasGroup))]
public class UIPanelAnimator : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CanvasGroup canvasGroup;

    [Header("Animation")]
    [SerializeField] private float showDuration = 0.25f;
    [SerializeField] private float hideDuration = 0.18f;
    [SerializeField] private float hiddenScale = 0.9f;
    [SerializeField] private float visibleScale = 1f;

    private RectTransform _rectTransform;
    private Sequence _activeSequence;

    private void Awake()
    {
        if (canvasGroup == null)
        {
            canvasGroup = GetComponent<CanvasGroup>();
        }

        _rectTransform = GetComponent<RectTransform>();
    }

    public void ShowInstant()
    {
        StopActiveAnimation();

        gameObject.SetActive(true);

        canvasGroup.alpha = 1f;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;

        if (_rectTransform != null)
        {
            _rectTransform.localScale = Vector3.one * visibleScale;
        }
    }

    public void HideInstant()
    {
        StopActiveAnimation();

        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        if (_rectTransform != null)
        {
            _rectTransform.localScale = Vector3.one * hiddenScale;
        }

        gameObject.SetActive(false);
    }

    public void Show()
    {
        StopActiveAnimation();

        gameObject.SetActive(true);

        canvasGroup.alpha = 0f;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;

        if (_rectTransform != null)
        {
            _rectTransform.localScale = Vector3.one * hiddenScale;
        }

        _activeSequence = Sequence.Create()
            .Group(Tween.Alpha(canvasGroup, 1f, showDuration))
            .Group(Tween.Scale(transform, Vector3.one * visibleScale, showDuration, Ease.OutBack));
    }

    public void Hide()
    {
        StopActiveAnimation();

        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        _activeSequence = Sequence.Create()
            .Group(Tween.Alpha(canvasGroup, 0f, hideDuration))
            .Group(Tween.Scale(transform, Vector3.one * hiddenScale, hideDuration, Ease.InSine))
            .OnComplete(() => gameObject.SetActive(false));
    }

    private void StopActiveAnimation()
    {
        if (_activeSequence.isAlive)
        {
            _activeSequence.Stop();
        }
    }
}