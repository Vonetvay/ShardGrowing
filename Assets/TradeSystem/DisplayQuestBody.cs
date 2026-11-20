using TMPro;
using UnityEngine;

public class DisplayQuestBody : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _personNameText;
    [SerializeField] private TextMeshProUGUI _mainText;
    [SerializeField] private TextMeshProUGUI _deadlineText;
    [SerializeField] private TextMeshProUGUI _rewardText;
    [SerializeField] private TextMeshProUGUI _sizeText;
    [SerializeField] private TextMeshProUGUI _typeText;

    public Quest Quest;

    public void Display() 
    {
        _personNameText.text = Quest.personName;
        _mainText.text = Quest.body.Text;
        _deadlineText.text = Quest.deadline.ToString();
        _rewardText.text = Quest.reward.ToString();
        _sizeText.text = Quest.size.ToString();
        _typeText.text = Quest.type.ToString();
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
