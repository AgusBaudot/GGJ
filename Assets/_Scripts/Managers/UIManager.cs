using TMPro;
using UnityEngine;

/// <summary>
/// UI Manager will be in charge of every UI and communicate with other managers.
/// </summary>

public class UIManager : MonoBehaviour
{
    [SerializeField] private MaskManager _maskManager;
    [SerializeField] private TextMeshProUGUI _text;

    private void Start()
    {
        _maskManager.OnMaskEquipped += MaskEquipped;
        _maskManager.OnMaskBroken += MaskBroken;
        _maskManager.OnPlayerDied += PlayerDied;
        
        _text.text = "Maskless";
    }

    private void MaskEquipped(MaskData data)
    {
        _text.text = data.name;
    }

    private void MaskBroken()
    {
        _text.text = "Maskless";
    }

    private void PlayerDied()
    {
        _text.text = "Player is dead";
    }

    private void OnDestroy()
    {
        _maskManager.OnMaskEquipped -= MaskEquipped;
        _maskManager.OnMaskBroken -= MaskBroken;
        _maskManager.OnPlayerDied -= PlayerDied;
    }
}
