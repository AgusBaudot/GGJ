using System;
using UnityEngine;
using DG.Tweening;

/// <summary>
/// Dumb courier.
/// </summary>

public class MaskPickup : MonoBehaviour
{
    public event Action OnDestroy;
    
    public MaskData Data { get; private set; }
    [Header("SETTINGS")]
    [SerializeField] private float _floatDistance = 0.5f;
    [Tooltip("How long it takes to go up.")]
    [SerializeField] private float _duration = 1.0f;
    [Tooltip("Add the slight scale pulse?")]
    [SerializeField] private bool _enableBreathing;

    private int _lifeTime = 3;
    private Vector2 _startPos;

    public void Init(MaskData data)
    {
        Data = data;
        transform.GetChild(0).GetComponent<SpriteRenderer>().sprite = data.MaskSprite;
        
        _startPos = transform.position;
        StartMoving();
        
        Destroy(gameObject, _lifeTime);
    }

    private void StartMoving()
    {
        transform.DOLocalMoveY(_startPos.y + _floatDistance, _duration)
            .SetEase(Ease.InOutSine).
            SetLoops(-1,  LoopType.Yoyo);

        if (_enableBreathing)
        {
            transform.DOScale(transform.localScale * 1.05f, _duration)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo);
        }
    }

    private void OnDisable()
    {
        OnDestroy?.Invoke();
        transform.DOKill();
    }
}