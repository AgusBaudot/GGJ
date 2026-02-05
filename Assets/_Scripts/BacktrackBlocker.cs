using UnityEngine;

public class BacktrackBlocker : MonoBehaviour
{
    [Header("SETTINGS")]
    [Tooltip("How far behind the max distance the wall stays.")]
    [SerializeField] private float _offsetFromMaxDistance = 15f; 
    
    [Header("DEPENDENCIES")]
    [SerializeField] private StatsManager _statsManager;
    
    private float _currentMaxX;

    private void Update()
    {
        // 1. Get the furthest point the player has reached
        float maxDistance = _statsManager.DistanceTraveled;

        // 2. Ensure the wall never moves backwards
        if (maxDistance > _currentMaxX)
        {
            _currentMaxX = maxDistance;
        }

        // 3. Update position
        float targetX = _currentMaxX - _offsetFromMaxDistance;
        
        transform.position = new Vector3(
            targetX, 
            transform.position.y, 
            transform.position.z
        );
    }
}