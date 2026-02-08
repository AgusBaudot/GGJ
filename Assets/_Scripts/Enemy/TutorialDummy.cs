using System.Collections;
using UnityEngine;

public class TutorialDummy : MonoBehaviour
{
    [SerializeField] private int MaxHealth = 3;
    [SerializeField] private GameObject maskPickupPrefab;
    [SerializeField] private MaskSpawner maskSpawner;
    [SerializeField] private MaskData[] spawnableMasks;
    
    private int _currentHealth;
    private SpriteRenderer _renderer;
    private MaskData _currentMask;

    private void Awake()
    {
        _renderer = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        _currentHealth = MaxHealth;
        
        _currentMask = spawnableMasks[Random.Range(0, spawnableMasks.Length)];
    }

    public void TakeDamage(int amount)
    {
        _currentHealth -=  amount;
        
        //Visual feedback - flash white
        if (_renderer != null)
            StartCoroutine(FlashWhite());
        
        //Play hit sound?
        
        if (_currentHealth <= 0)
            Die();
    }

    private IEnumerator FlashWhite()
    {
        Color originalColor = _renderer.color;
        _renderer.color = new Color(2f, 2f, 2f); //Bright white flash
        yield return Helpers.GetWait(0.1f);
        _renderer.color = originalColor;
    }

    private void Die()
    {
        if (maskPickupPrefab != null && _currentMask != null)
        {
            maskSpawner.SpawnPickupMask(_currentMask, transform.position);
        }
        
        Destroy(gameObject);
    }
}