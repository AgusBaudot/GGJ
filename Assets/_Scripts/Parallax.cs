using UnityEngine;
using UnityEngine.UI;

public class Parallax : MonoBehaviour
{
    [Header("Layers & images")]
    [SerializeField] private int[] _layers;

    Material mat;
    float distance;

    [Range(0f, 0.5f)]
    public float speed = 0.2f;


    void Start()
    {
        mat = GetComponent<Renderer>().material;
    }

    void Update()
    {
        distance += Time.deltaTime * speed;
        mat.SetTextureOffset("_MainTex", Vector2.right * distance);
    }
}