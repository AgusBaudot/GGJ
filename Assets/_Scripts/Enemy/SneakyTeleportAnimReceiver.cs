using UnityEngine;

/// <summary>
/// Place this on the same GameObject as the Animator (e.g. the child with EnemyAnimator).
/// Add an Animation Event at the "disappear" frame of the Teleport clip, calling OnTeleportDisappear.
/// This forwards to SneakyBehavior so it can pause the animator, teleport, and resume.
/// </summary>
public class SneakyTeleportAnimReceiver : MonoBehaviour
{
    public void OnTeleportDisappear()
    {
        var sneaky = GetComponentInParent<SneakyBehavior>();
        if (sneaky != null)
            sneaky.OnTeleportDisappear();
    }
}
