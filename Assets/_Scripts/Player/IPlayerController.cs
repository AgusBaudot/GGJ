using System;
using UnityEngine;

public interface IPlayerController
{
    event Action<bool, float> GroundedChanged;
    event Action Jumped;
    event Action Dashed;
    event Action TeleportStarted;
    event Action TeleportEnded;
    event Action<AttackType> Attacked;
    Vector2 FrameInput { get; }
}
