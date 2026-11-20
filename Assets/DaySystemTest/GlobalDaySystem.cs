using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class GlobalDaySystem : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _dayCounter;
    public int DayNumber { get; private set; }

    public static GlobalDaySystem Instance;

    private void Awake()
    {
        DayNumber = 0;
        if (Instance == null) 
        {
            Instance = this;
        }
    }

    private void Start()
    {
        SaveManager.Instance.LoadGame();
    }

    public void ToNext() 
    {
        DayNumber++;
        _dayCounter.text = DayNumber.ToString();
        StartCoroutine(StartNewDay());
    }

    public IEnumerator StartNewDay() 
    {
        QuestSystem.Instance.GenerateDailyQuests();

        Crystal[] allCrystals = UnityEngine.Object.FindObjectsByType<Crystal>(FindObjectsSortMode.None);

        Array.Sort(allCrystals, (a, b) => b.AnimationProgress.CompareTo(a.AnimationProgress));

        foreach (var item in allCrystals)
        {
            item.TryGrowthUp();
        }

        SaveManager.Instance.SaveGame();

        yield return null;
    }
}
