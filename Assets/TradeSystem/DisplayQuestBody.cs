using TMPro;
using UnityEngine;

public class DisplayQuestBody : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _personNameText;
    [SerializeField] private TextMeshProUGUI _mainText;
    [SerializeField] private TextMeshProUGUI _deadlineText;
    [SerializeField] private TextMeshProUGUI _rewardText;

    public QuestTaskBody Quest;

    public void Display() 
    {
        _personNameText.text = Quest.PersonName;
        _mainText.text = Quest.Text;
        _deadlineText.text = Quest.Deadline.ToString();
        _rewardText.text = Quest.Reward.ToString();
    }

    public void AcceptMe() 
    {
        QuestSystem.Instance.AcceptTask(Quest);
        Destroy(gameObject);
    }

    public void RejectMe()
    {
        QuestSystem.Instance.RejectTask();
        Destroy(gameObject);
    }
}
