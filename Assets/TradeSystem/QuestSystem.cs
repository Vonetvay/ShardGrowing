using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class Quest 
{
    public Quest(QuestBody body,string personName, CrystalType type, CrystalSize size, int deadline, int reward, int address=0) 
    {
        this.body = body;
        this.personName = personName;
        this.type = type;
        this.size = size;
        this.deadline = deadline;
        this.reward = reward;
        this.address = address;
    }

    public QuestBody body;
    public string personName;
    public CrystalType type;
    public CrystalSize size;
    public int deadline;
    public int reward;
    public int address;
}

public class QuestSystem : MonoBehaviour, ISaveable
{
    public QuestBody[] _questBodyDatabase;
    private Dictionary<int, HashSet<Quest>> _currentTasks = new();
    private HashSet<GameObject> _questVisuals = new();
    private Dictionary<Quest, GameObject> _taskVisuals = new();

    public float Famous = 0f;

    [SerializeField] private Vector2Int _dailyTaskCountRange;
    [SerializeField] private GameObject _questVisual;
    [SerializeField] private GameObject _taskVisual;
    [SerializeField] private TextMeshProUGUI _famousCounter;
    [SerializeField] private TextMeshProUGUI _walletVisuals;
    [SerializeField] private Transform _questContentTransform;
    [SerializeField] private Transform _taskContentTransform;

    public static Action<int> SendToPerson;

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
        foreach (GameObject body in _questVisuals) 
        {
            Destroy(body);
        }
        _questVisuals = new HashSet<GameObject>();

        HashSet<Quest> todayQuests = new();

        for (int i = 0; i < UnityEngine.Random.Range(_dailyTaskCountRange.x, _dailyTaskCountRange.y + 1); i++) 
        {
            Quest task = GenerateQuest();
            
            todayQuests.Add(task);
        }

        _currentTasks.TryGetValue(GlobalDaySystem.Instance.DayNumber, out HashSet<Quest> todayTasks);
        if (todayTasks != null) 
        {
            foreach (var task in todayTasks.ToList())
            {
                UncompleteTask(task);
            }
        }

        UpdateAllTaskDeadline();
        DisplayQuests(todayQuests);
    }

    public Quest GenerateQuest() 
    {
        QuestBody source = _questBodyDatabase[UnityEngine.Random.Range(0, _questBodyDatabase.Length)];
        QuestBody questBody = Instantiate(source);
        int deadline = UnityEngine.Random.Range(questBody.DeadlineRange.x, questBody.DeadlineRange.y + 1);
        int reward = UnityEngine.Random.Range(questBody.RewardRange.x, questBody.RewardRange.y);
        CrystalType type = questBody.Types[UnityEngine.Random.Range(0, questBody.Types.Length)];
        CrystalSize size = questBody.Sizes[UnityEngine.Random.Range(0, questBody.Sizes.Length)];
        Quest quest = new(questBody, GameManager.GetRandomPersonName(), type, size, deadline, reward);
        return quest;
    }

    private void DisplayQuests(HashSet<Quest> quests) 
    {
        foreach (var quest in quests)
        {
            DisplayQuestBody body = Instantiate(_questVisual, _questContentTransform).GetComponent<DisplayQuestBody>();
            body.Quest = quest;
            body.Display();
            _questVisuals.Add(body.gameObject);
        }
    }

    private void DisplayNewTask(Quest task) 
    {
        DisplayTaskBody body = Instantiate(_taskVisual, _taskContentTransform).GetComponent<DisplayTaskBody>();
        body.Task = task;
        body.Task.address = MailSystem.RegisterNewMail(body.GetComponent<IMailComponent>());
        body.Display();
        _taskVisuals.Add(body.Task, body.gameObject);
    }

    private void DisplayAllTask() 
    {
        foreach (var task in _taskVisuals.Values)
        {
            task.GetComponent<DisplayTaskBody>().Display();
        }
    }

    private void UpdateAllTaskDeadline() 
    {
        _currentTasks = _currentTasks.ToDictionary(
            pair => pair.Key,
            pair => pair.Value.Select(q =>
            {
                q.deadline -= 1; // мен€ем копию структуры
                return q;        // возвращаем обновлЄнный Quest
            }).ToHashSet()       // если значение словар€ Ч HashSet<Quest> (или .ToList(), если List<Quest>)
        );

        DisplayAllTask();
    }

    private void RemoveTask(Quest task) 
    {
        if (!_taskVisuals.ContainsKey(task))
        {
            Debug.Log($" вест от {task.personName} не найден");
            return;
        }

        Debug.Log($" вест от {task.personName} найден");

        // ”дал€ем квест из того дн€ дедлайна, в который он был записан
        foreach (var taskSet in _currentTasks.Values)
        {
            if (taskSet.Remove(task))
            {
                break; // Ќашли и удалили, выходим из цикла
            }
        }

        Destroy(_taskVisuals[task]);
        MailSystem.RemoveMail(task.address);
        _taskVisuals.Remove(task);
    }

    public void AcceptTask(Quest task) 
    {
        int deadlineDay = GlobalDaySystem.Instance.DayNumber + task.deadline;
        if (!_currentTasks.ContainsKey(deadlineDay))
        {
            _currentTasks.Add(deadlineDay, new HashSet<Quest>());
        }

        _currentTasks[deadlineDay].Add(task);

        DisplayNewTask(task);
    }

    public void RejectTask() 
    {
        AddFamous(-1);
    }

    public void CompleteTask(Quest task) 
    {
        WalletSystem.GetReward(task.reward);
        RemoveTask(task);

        AddFamous(1);
    }

    public void UncompleteTask(Quest task) 
    {
        RemoveTask(task);

        AddFamous(-2);
    }

    private void AddFamous(int count) 
    {
        Famous += count;
        _famousCounter.text = Famous.ToString();
    }

    public struct QuestSystemData 
    {
        public float famous;
    }

    public string CaptureState()
    {
        QuestSystemData data = new()
        {
            famous = Famous
        };

        return JsonUtility.ToJson(data);
    }

    public void RestoreState(string jsonState)
    {
        var data = JsonUtility.FromJson<QuestSystemData>(jsonState);

        Famous = data.famous;
    }
}
