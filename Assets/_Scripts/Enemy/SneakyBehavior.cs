using System;
using UnityEngine;

public class SneakyBehavior : MonoBehaviour, IEnemyBehavior
{
    private Enemy _enemy;
    
    public void Initialize(Enemy enemy)
    {
        _enemy = enemy;
        Debug.LogWarning("Sneaky behavior initialized - not yet implemented");
    }

    public void UpdateBehavior()
    {
        throw new NotImplementedException();
    }

    public void FixedUpdateBehavior()
    {
        throw new NotImplementedException();
    }

    public void OnStunned()
    {
        throw new NotImplementedException();
    }

    public void OnStunEnded()
    {
        throw new NotImplementedException();
    }
}
