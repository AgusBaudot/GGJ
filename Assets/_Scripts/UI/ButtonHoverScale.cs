using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;

/// <summary>
/// Scales a button smoothly when the mouse hovers over it using DOTween.
/// Attachable to any UI button for a polished hover effect.
/// </summary>
public class ButtonHoverScale : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Scale Settings")]
    [Tooltip("The scale multiplier when hovered (e.g., 1.1 = 110% of original size)")]
    [SerializeField] private float hoverScale = 1.1f;
    
    [Tooltip("Duration of the scale animation in seconds")]
    [SerializeField] private float scaleDuration = 0.2f;
    
    [Tooltip("Ease type for the animation (default: OutBack for bouncy effect)")]
    [SerializeField] private Ease easeType = Ease.OutBack;

    private Vector3 originalScale;
    private Tween currentTween;

    private void Awake()
    {
        // Store the original scale
        originalScale = transform.localScale;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        // Kill any existing tween to prevent conflicts
        currentTween?.Kill();
        
        // Scale up to hover size
        currentTween = transform.DOScale(originalScale * hoverScale, scaleDuration)
            .SetEase(easeType)
            .SetUpdate(true); // Works even if Time.timeScale = 0 (paused game)
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // Kill any existing tween to prevent conflicts
        currentTween?.Kill();
        
        // Scale back to original size
        currentTween = transform.DOScale(originalScale, scaleDuration)
            .SetEase(easeType)
            .SetUpdate(true);
    }

    private void OnDisable()
    {
        // Clean up tween when disabled
        currentTween?.Kill();
        transform.localScale = originalScale;
    }

    private void OnDestroy()
    {
        // Clean up tween when destroyed
        currentTween?.Kill();
    }
}