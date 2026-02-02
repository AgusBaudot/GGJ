using UnityEngine;
using TMPro;

public class ReadKillCount : MonoBehaviour
{
    
    private void OnEnable()
    {
        GetComponent<TextMeshProUGUI>().text = StatsManager.Instance.EnemiesKilled.ToString();
    }
}