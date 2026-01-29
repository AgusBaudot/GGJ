using UnityEngine;

/// <summary>
/// Dumb courier.
/// </summary>

public class MaskPickup : MonoBehaviour
{
    private MaskManager _maskManager;
    private MaskData _data;

    public void Init(MaskManager manager, MaskData data)
    {
        _maskManager = manager;
        _data = data;
        
        transform.GetChild(0).GetComponent<SpriteRenderer>().sprite = data.MaskSprite;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        if(_maskManager.AddMaskToStack(_data)) Destroy(gameObject);
    }
}