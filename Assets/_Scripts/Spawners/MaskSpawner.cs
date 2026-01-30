using UnityEngine;

public class MaskSpawner : MonoBehaviour
{
    [SerializeField] private MaskPickup _maskPrefab;
    
    public void SpawnPickupMask(MaskData data, Vector2 pos)
    {
        MaskPickup mask = Instantiate(_maskPrefab, pos, Quaternion.identity);
        mask.Init(data);
    }
}