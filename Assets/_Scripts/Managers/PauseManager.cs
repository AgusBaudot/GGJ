using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// Central manager for gameplay time and player freeze states:
/// - Hit stop: brief time freeze on impact.
/// - Time freeze: optional slow-mo or full freeze.
/// - Pause menu: full pause (timeScale 0); wire your UI open/close to SetPauseMenuActive.
/// - Player freeze: disables player input and holds velocity at zero (timed or manual re-enable).
/// </summary>
public class PauseManager : MonoBehaviour
{
    public static PauseManager Instance { get; private set; }

    [Header("Hit stop")]
    [SerializeField] [Min(0f)] private float _defaultHitStopDuration = 0.06f;

    [Header("Time freeze")]
    [SerializeField] [Range(0f, 1f)] private float _defaultTimeFreezeScale = 0f;

    private bool _pauseMenuActive;
    private bool _hitStopActive;
    private float _timeFreezeScale = -1f; // -1 = not active
    private bool _playerFrozen;
    private float _playerFreezeEndUnscaledTime = -1f;

    /// <summary> True when the player should have input disabled and position frozen. </summary>
    public bool IsPlayerFrozen => _playerFrozen;

    /// <summary> True when the game is fully paused (e.g. pause menu open). </summary>
    public bool IsPaused => _pauseMenuActive;

    public event Action PauseMenuOpened;
    public event Action PauseMenuClosed;
    public event Action PlayerFreezeStarted;
    public event Action PlayerFreezeEnded;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Update()
    {
        if (_playerFreezeEndUnscaledTime >= 0f && Time.unscaledTime >= _playerFreezeEndUnscaledTime)
        {
            _playerFreezeEndUnscaledTime = -1f;
            UnfreezePlayer();
        }

        ApplyTimeScale();
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    private void ApplyTimeScale()
    {
        if (_pauseMenuActive)
            Time.timeScale = 0f;
        else if (_hitStopActive)
            Time.timeScale = 0f;
        else if (_timeFreezeScale >= 0f)
            Time.timeScale = _timeFreezeScale;
        else
            Time.timeScale = 1f;

        if (Time.timeScale == 0) Debug.LogWarning("TimeScale is set to 0, some UI animations may not work");
    }

    #region Pause menu

    /// <summary> Call when opening the pause menu. Sets time scale to 0. </summary>
    public void SetPauseMenuActive(bool active)
    {
        if (_pauseMenuActive == active) return;
        _pauseMenuActive = active;
        if (active)
            PauseMenuOpened?.Invoke();
        else
            PauseMenuClosed?.Invoke();
    }

    /// <summary> Toggle pause menu state. Returns new state (true = paused). </summary>
    public bool TogglePauseMenu()
    {
        SetPauseMenuActive(!_pauseMenuActive);
        return _pauseMenuActive;
    }

    #endregion

    #region Hit stop

    /// <summary> Freezes time for the given duration (uses unscaled time). Uses default duration if &lt;= 0. </summary>
    public void RequestHitStop(float duration = -1f)
    {
        if (duration <= 0f) duration = _defaultHitStopDuration;
        StopAllCoroutines();
        StartCoroutine(HitStopRoutine(duration));
    }

    private IEnumerator HitStopRoutine(float duration)
    {
        _hitStopActive = true;
        yield return new WaitForSecondsRealtime(duration);
        _hitStopActive = false;
    }

    #endregion

    #region Time freeze

    /// <summary> Sets a global time scale (e.g. 0 = frozen, 0.5 = half speed). Pass -1 to clear. </summary>
    public void SetTimeFreeze(float scale)
    {
        _timeFreezeScale = scale < 0f ? -1f : Mathf.Clamp01(scale);
    }

    /// <summary> Applies time freeze for a duration (unscaled time), then restores. </summary>
    public void TimeFreezeFor(float duration, float scale = -1f)
    {
        if (scale < 0f) scale = _defaultTimeFreezeScale;
        StopAllCoroutines();
        StartCoroutine(TimeFreezeRoutine(duration, scale));
    }

    private IEnumerator TimeFreezeRoutine(float duration, float scale)
    {
        SetTimeFreeze(scale);
        yield return new WaitForSecondsRealtime(duration);
        SetTimeFreeze(-1f);
    }

    #endregion

    #region Player freeze (input disabled + position held)

    /// <summary> Freezes the player (no input, velocity zero). Re-enable with UnfreezePlayer. </summary>
    public void FreezePlayer()
    {
        if (_playerFrozen) return;
        _playerFrozen = true;
        _playerFreezeEndUnscaledTime = -1f;
        PlayerFreezeStarted?.Invoke();
    }

    /// <summary> Unfreezes the player. Safe to call even if not frozen. </summary>
    public void UnfreezePlayer()
    {
        if (!_playerFrozen) return;
        _playerFrozen = false;
        _playerFreezeEndUnscaledTime = -1f;
        PlayerFreezeEnded?.Invoke();
    }

    /// <summary> Freezes the player for the given duration (unscaled time), then unfreezes. </summary>
    public void FreezePlayerFor(float duration)
    {
        if (duration <= 0f)
        {
            UnfreezePlayer();
            return;
        }
        _playerFrozen = true;
        _playerFreezeEndUnscaledTime = Time.unscaledTime + duration;
        PlayerFreezeStarted?.Invoke();
    }

    #endregion
}
