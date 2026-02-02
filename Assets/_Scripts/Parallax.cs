using UnityEngine;

[System.Serializable]
public class BackgroundElement
{
    public SpriteRenderer backgroundSprite;
    [Range(0f, 1f)] public float scrollSpeed;
    [HideInInspector] public Material spriteMaterial;
}

public class Parallax : MonoBehaviour
{
    private const float SCROLL_MULTIPLIER = 0.02f;

    [SerializeField] private BackgroundElement[] backgroundElements;
    private Transform cam;

    private void Start()
    {
        cam = Camera.main.transform;

        foreach (var element in backgroundElements)
        {
            element.spriteMaterial = element.backgroundSprite.material;
        }
    }

    private void Update()
    {
        float camX = cam.position.x;

        foreach (var element in backgroundElements)
        {
            element.spriteMaterial.mainTextureOffset =
                new Vector2(camX * element.scrollSpeed * SCROLL_MULTIPLIER, 0f);
        }
    }
}