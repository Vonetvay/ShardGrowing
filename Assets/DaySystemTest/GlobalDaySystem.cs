using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

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

    public void ReloadActiveScene() 
    {
        StartCoroutine(LoadSceneRoutine(SceneManager.GetActiveScene().buildIndex));
    }

    public IEnumerator LoadSceneRoutine(int index) 
    {
        AsyncOperation operation = SceneManager.LoadSceneAsync(index);

        yield return new WaitUntil(() => operation.isDone == true);

        SaveManager.Instance.LoadGame();

        // Ждём, пока загрузка не завершится
        /*while (!operation.isDone)
        {
            // В Unity прогресс идёт от 0 до 0.9 (последние 0.1 — активация сцены)
            float progress = Mathf.Clamp01(operation.progress / 0.9f);

            if (progressBar != null)
            {
                progressBar.value = progress;
            }

            yield return null; // Ждём следующий кадр
        }*/

        // Код здесь выполнится сразу после полного открытия сцены
        Debug.Log("Сцена загружена!");
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
