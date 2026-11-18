using System.Collections.Generic;
using TMPro;
using UnityEngine;

[CreateAssetMenu(fileName = "new QuestTask", menuName = "Custom/QuestTask")]
public class QuestTaskBody : ScriptableObject 
{
    public string PersonName;
    public string Text;
    public Vector2Int DeadlineRange;
    public Vector2 RewardRange;
    public CrystalData Request;
    public int Deadline;
    public float Reward;
    //public QuestTask Task;
}

/*public struct QuestTask 
{
    
}*/

public class QuestSystem : MonoBehaviour
{
    public QuestTaskBody[] _questDatabase;
    private Dictionary<int, HashSet<QuestTaskBody>> _currentTasks = new Dictionary<int, HashSet<QuestTaskBody>>();
    private HashSet<GameObject> _questsBodies = new HashSet<GameObject>();
    private Dictionary<QuestTaskBody, GameObject> _tasksBodies = new Dictionary<QuestTaskBody, GameObject>();

    public float Famous = 0f;

    [SerializeField] private Vector2Int _dailyTaskCountRange;
    [SerializeField] private GameObject _questVisual;
    [SerializeField] private GameObject _taskVisual;
    [SerializeField] private TextMeshProUGUI _famousCounter;
    [SerializeField] private Transform _questContentTransform;
    [SerializeField] private Transform _taskContentTransform;

    public static QuestSystem Instance;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    public void GenerateDailyQuests() 
    {
        foreach (GameObject body in _questsBodies) 
        {
            Destroy(body);
        }
        _questsBodies = new HashSet<GameObject>();

        HashSet<QuestTaskBody> todayQuests;
        if (!_currentTasks.TryGetValue(GlobalDaySystem.Instance.DayNumber, out todayQuests))
        {
            todayQuests = new HashSet<QuestTaskBody>();
        }

        for (int i = 0; i < UnityEngine.Random.Range(_dailyTaskCountRange.x, _dailyTaskCountRange.y + 1); i++) 
        {
            QuestTaskBody task = GenerateQuest();
            
            todayQuests.Add(task);
        }

        UpdateAllTaskDeadline();
        DisplayQuests(todayQuests);
    }

    public QuestTaskBody GenerateQuest() 
    {
        QuestTaskBody sourse = _questDatabase[UnityEngine.Random.Range(0, _questDatabase.Length)];
        QuestTaskBody task = Instantiate(sourse);
        task.Deadline = UnityEngine.Random.Range(task.DeadlineRange.x, task.DeadlineRange.y + 1);
        task.Reward = Mathf.Round(UnityEngine.Random.Range(task.RewardRange.x, task.RewardRange.y));
        return task;
    }

    private void DisplayQuests(HashSet<QuestTaskBody> quests) 
    {
        foreach (var quest in quests)
        {
            DisplayQuestBody body = Instantiate(_questVisual, _questContentTransform).GetComponent<DisplayQuestBody>();
            body.Quest = quest;
            body.Display();
            _questsBodies.Add(body.gameObject);
        }
    }

    private void DisplayNewTask(QuestTaskBody task) 
    {
        DisplayTaskBody body = Instantiate(_taskVisual, _taskContentTransform).GetComponent<DisplayTaskBody>();
        body.Task = task;
        body.Display();
        _tasksBodies.Add(task, body.gameObject);
    }

    private void DisplayAllTask() 
    {
        foreach (var task in _tasksBodies.Values)
        {
            task.GetComponent<DisplayTaskBody>().Display();
        }
    }

    private void UpdateAllTaskDeadline() 
    {
        foreach (var set in _currentTasks.Values)
        {
            foreach (var task in set)
            {
                task.Deadline -= 1;
            }
        }
        DisplayAllTask();
    }

    private void RemoveTask(QuestTaskBody task) 
    {
        Destroy(_tasksBodies[task]);
    }

    public void AcceptTask(QuestTaskBody task) 
    {
        int deadlineDay = GlobalDaySystem.Instance.DayNumber + task.Deadline;
        if (!_currentTasks.ContainsKey(deadlineDay))
        {
            _currentTasks.Add(deadlineDay, new HashSet<QuestTaskBody>());
        }

        _currentTasks[deadlineDay].Add(task);

        DisplayNewTask(task);
    }

    public void RejectTask() 
    {
        AddFamous(-1);
    }

    public void CompleteTask(QuestTaskBody task) 
    {
        _currentTasks[GlobalDaySystem.Instance.DayNumber].Remove(task);

        AddFamous(1);
    }

    public void UncompleteTask(QuestTaskBody task) 
    {
        _currentTasks[GlobalDaySystem.Instance.DayNumber].Remove(task);

        AddFamous(-2);
    }

    private void AddFamous(int count) 
    {
        Famous += count;
        _famousCounter.text = Famous.ToString();
    }
}
