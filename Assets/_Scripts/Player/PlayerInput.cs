using UnityEngine;

/// <summary>
/// Handles all player input gathering and processing
/// </summary>
public class PlayerInput : MonoBehaviour
{
    [SerializeField] private PlayerBaseStats _stats;

    public FrameInput CurrentInput { get; private set; }
    public int FacingDirection { get; private set; } = 1;

    private void Update()
    {
        GatherInput();
    }

    private void GatherInput()
    {
        // 1. Create a local variable
        var newInput = new FrameInput
        {
            JumpDown = Input.GetButtonDown("Jump") || Input.GetKeyDown(KeyCode.C),
            JumpHeld = Input.GetButton("Jump") || Input.GetKey(KeyCode.C),
            Move = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")),
            GrabDown = Input.GetButtonDown("Grab") || Input.GetKeyDown(KeyCode.E),
            PrimaryDown = Input.GetButtonDown("Primary") || Input.GetKeyDown(KeyCode.J),
            SecondaryDown = Input.GetButtonDown("Secondary") || Input.GetKeyDown(KeyCode.K)
        };

        // 2. Modify the local variable
        if (_stats.SnapInput)
        {
            newInput.Move.x = Mathf.Abs(newInput.Move.x) < _stats.HorizontalDeadZoneThreshold
                ? 0
                : Mathf.Sign(newInput.Move.x);
            newInput.Move.y = Mathf.Abs(newInput.Move.y) < _stats.VerticalDeadZoneThreshold
                ? 0
                : Mathf.Sign(newInput.Move.y);
        }

        // 3. Use the local variable for logic checks
        if (newInput.Move.x != 0)
            FacingDirection = newInput.Move.x < 0 ? -1 : 1;

        // 4. Finally, save the result to the property
        CurrentInput = newInput;
    }
}
