using UnityEngine;
using TMPro;

public class ReadDistance : MonoBehaviour
{
    
    private void OnEnable()
    {
        GetComponent<TextMeshProUGUI>().text = StatsManager.Instance.DistanceTraveled.ToString() + " m";
    }
}