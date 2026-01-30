using UnityEngine;

public class PlayerSecondaryAbilities
{
    private Rigidbody2D _rb;
    private CapsuleCollider2D _col;
    private PlayerBaseStats _stats;
    private MaskManager _maskManager;
    private PlayerController _controller;

    public PlayerSecondaryAbilities(Rigidbody2D rb, CapsuleCollider2D col, PlayerBaseStats stats, MaskManager maanger, PlayerController controller)
    {
        _rb = rb;
        _col = col;
        _stats = stats;
        _maskManager = maanger;
        _controller = controller;
    }

    public void TryUse(SecondaryType ability)
    {
        switch (ability)
        {
            case SecondaryType.Dash:
                TryDash();
                break;
            case SecondaryType.Teleport:
                TryTeleport();
                break;
        }
    }

    private void TryDash()
    {

    }

    private void TryTeleport()
    {

    }
}