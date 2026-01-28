using System;

/// <summary>
/// References its MaskData and tracks current durability.
/// </summary>

public class MaskInstance
{
    public event Action OnBreak;
    
    public MaskData Data { get; }

    private int _currentHP;
    private bool _isBroken;
    
    public MaskInstance(MaskData data)
    {
        Data = data;
        _currentHP = data.Hp;
    }

    public void TakeDamage(int amount)
    {
        _currentHP -= amount;
        if (_currentHP <= 0 && !_isBroken)
        {
            OnBreak?.Invoke();
            _isBroken = true;
        }
    }
}