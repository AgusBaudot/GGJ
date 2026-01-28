using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private Enemy _enemyPrefab;
    [SerializeField] private MaskManager _maskManager;
    [SerializeField] private MaskSpawner _maskSpawner;
    [SerializeField] private EnemyData _data;
    
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Mouse2))
        {
            Enemy enemy = Instantiate(_enemyPrefab);
            enemy.Init(_maskManager, _maskSpawner, _data);
        }
    }
}
