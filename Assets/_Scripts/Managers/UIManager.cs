using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// UI Manager will be in charge of every UI and communicate with other managers.
/// </summary>

public class UIManager : MonoBehaviour
{
    [Header("DEPENDENCIES")]
    [SerializeField] private MaskManager _maskManager;
    [SerializeField] private StatsManager _statsManager;
    [Header("TEXTS")]
    [SerializeField] private TextMeshProUGUI _text;
    [SerializeField] private TextMeshProUGUI _maxPositionText;
    [SerializeField] private TextMeshProUGUI _killCountText;
    [Header("HUD")]
    [SerializeField] private Image _healthBar;
    [SerializeField] private Sprite[] _healthBarSprites;
    [SerializeField] private GameObject _deathUI;

    private void Start()
    {
        _maskManager.OnMaskEquipped += MaskEquipped;
        _maskManager.OnMaskBroken += MaskBroken;
        _maskManager.OnPlayerDied += PlayerDied;
        _maskManager.OnDamageReceived += UpdateHealthBar;
        _statsManager.OnEnemyKilled += EnemyKilled;
        
        _text.text = "Maskless";
    }

    private void Update()
    {
        _maxPositionText.text = $"{_statsManager.DistanceTraveled}m";
    }

    private void EnemyKilled(int total)
    {
        _killCountText.text = $"Total kills: {total}";
    }

    private void MaskEquipped(MaskData data)
    {
        //Should show every mask IN ORDER, not only last one.
        _text.text = data.name;
        UpdateHealthBar();
    }

    private void MaskBroken()
    {
        _text.text = "Maskless";
        UpdateHealthBar();
    }

    private void UpdateHealthBar(int hpLeft = 0)
    {
        Debug.Log(_maskManager.CurrentHP);
        _healthBar.sprite = _healthBarSprites[_maskManager.CurrentHP];
    }

    private void PlayerDied()
    {
        _text.text = "Game over";
        _deathUI.SetActive(true);
    }

    private void OnDestroy()
    {
        _maskManager.OnMaskEquipped -= MaskEquipped;
        _maskManager.OnMaskBroken -= MaskBroken;
        _maskManager.OnPlayerDied -= PlayerDied;
    }
}
