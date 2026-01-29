using UnityEngine;
using TMPro;

public class DevToolsAbilities : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _jump;
    [SerializeField] private TextMeshProUGUI _dash;
    [SerializeField] private TextMeshProUGUI _teleport;

    public bool _hasJump;
    public bool _hasDash;
    public bool _hasTeleport;
    
    private void Update()
    {
        #region Inputs

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            _hasJump = !_hasJump;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            _hasDash = !_hasDash;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            _hasTeleport = !_hasTeleport;
        }

        #endregion

        #region Coloring

        _jump.color = _hasJump ? Color.white : Color.grey;
        _dash.color = _hasDash ? Color.white : Color.grey;
        _teleport.color = _hasTeleport ? Color.white : Color.grey;

        #endregion
    }
}