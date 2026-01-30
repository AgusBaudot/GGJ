using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private Enemy _enemyPrefab;
    [SerializeField] private MaskManager _maskManager;
    [SerializeField] private MaskSpawner _maskSpawner;
    [SerializeField] private EnemyData[] _data;
    
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            Enemy enemy = Instantiate(_enemyPrefab);
            enemy.Init(_maskManager, _maskSpawner, _data[0]);
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            Enemy enemy = Instantiate(_enemyPrefab);
            enemy.Init(_maskManager, _maskSpawner, _data[1]);
        }

        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            Enemy enemy = Instantiate(_enemyPrefab);
            enemy.Init(_maskManager, _maskSpawner, _data[2]);
        }

        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            Enemy enemy = Instantiate(_enemyPrefab);
            enemy.Init(_maskManager, _maskSpawner, _data[3]);
        }
    }
}
