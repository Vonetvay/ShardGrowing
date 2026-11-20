using TMPro;
using UnityEngine;

public class DisplayTaskBody : MonoBehaviour, IMailComponent
{
    [SerializeField] private TextMeshProUGUI _personNameText;
    [SerializeField] private TextMeshProUGUI _deadlineText;
    [SerializeField] private TextMeshProUGUI _rewardText;
    [SerializeField] private TextMeshProUGUI _sizeText;
    [SerializeField] private TextMeshProUGUI _typeText;

    public Quest Task;

    public void Display()
    {
        _personNameText.text = Task.personName;
        _deadlineText.text = Task.deadline.ToString();
        _rewardText.text = Task.reward.ToString();
        _sizeText.text = Task.size.ToString();
        _typeText.text = Task.type.ToString();
    }

    public void RequestToAltar() 
    {
        MailSystem.Request(GameManager.AltairAddress, Task.address);
    }

    private void MatchAltarItems(GameObject[] givenObjects) 
    {
        foreach (GameObject o in givenObjects)
        {
            if (o.TryGetComponent<Crystal>(out Crystal crystal))
            {
                if (crystal.Size == Task.size && crystal.Type == Task.type)
                {
                    QuestSystem.Instance.CompleteTask(Task);
                }
            }
        }
    }

    public GameObject[] TakeRequest()
    {
        throw new System.NotImplementedException();
    }

    public void GetRequest(GameObject[] objects)
    {
        Debug.Log($"Принято объектов с алтаря {objects.Length}");
        MatchAltarItems(objects);
    }
}
