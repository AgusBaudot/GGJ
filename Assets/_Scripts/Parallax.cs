using UnityEngine;
using UnityEngine.UI;

public class Parallax : MonoBehaviour
{
    [Header("Layers & images")]
    [SerializeField] private int[] _layers;

    private Camera _cam;
    private Vector3 _lastCameraPos;

    private void Start()
    {
        _cam = Helpers.Camera;
        _lastCameraPos = _cam.transform.position;
    }
}