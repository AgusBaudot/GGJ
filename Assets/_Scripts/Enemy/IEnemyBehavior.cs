/// <summary>
/// Interface for all enemy behavior types
/// </summary>

public interface IEnemyBehavior
{
    void Initialize(Enemy enemy, PlayerBaseStats stats);
    void UpdateBehavior();
    void FixedUpdateBehavior();
    void OnStunned();
    void OnStunEnded();
}