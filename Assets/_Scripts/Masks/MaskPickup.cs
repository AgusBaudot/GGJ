using UnityEngine;

/// <summary>
/// Dumb courier.
/// </summary>

public class MaskPickup : MonoBehaviour
{
    public MaskData Data { get; private set; }

    private int _lifeTime = 3;

    public void Init(MaskData data)
    {
        Data = data;
        transform.GetChild(0).GetComponent<SpriteRenderer>().sprite = data.MaskSprite;
        
        Destroy(gameObject, _lifeTime);
    }
}