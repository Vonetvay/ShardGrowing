using System.Collections;
using UnityEngine;

public class GlobalDaySystem : MonoBehaviour
{
    private int dayNumber = 0;

    public void ToNext() 
    {
        dayNumber++;
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
