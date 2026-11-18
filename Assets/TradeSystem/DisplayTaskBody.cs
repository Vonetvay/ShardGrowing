using TMPro;
using UnityEngine.UI;
using UnityEngine;

public class DisplayTaskBody : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _personNameText;
    [SerializeField] private TextMeshProUGUI _deadlineText;
    //[SerializeField] private Image _crystalTask;

    public QuestTaskBody Task;

    public void Display()
    {
        _personNameText.text = Task.PersonName;
        _deadlineText.text = Task.Deadline.ToString();
    }
}
