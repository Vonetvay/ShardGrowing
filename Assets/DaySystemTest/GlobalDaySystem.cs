using System.Collections;
using TMPro;
using UnityEngine;

public class GlobalDaySystem : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _dayCounter;
    private int dayNumber = 0;

    public void ToNext() 
    {
        dayNumber++;
        _dayCounter.text = dayNumber.ToString();
        StartCoroutine(StartNewDay());
    }

    public IEnumerator StartNewDay() 
    {
        CrystalNew[] allCrystals = Object.FindObjectsByType<CrystalNew>(FindObjectsSortMode.None);

        foreach (var item in allCrystals)
        {
            item.TryGrowthUp();
        }

        yield return null;
    }
}
