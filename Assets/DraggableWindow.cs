using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DraggableWindow : MonoBehaviour, IDragHandler, IPointerDownHandler
{
    [Header("Ссылки на элементы")]
    [SerializeField] private RectTransform windowRect; // Главный RectTransform окна
    [SerializeField] private GameObject contentPanel;  // Панель с контентом
    [SerializeField] private Button toggleButton;      // Кнопка свернуть/развернуть

    private Canvas canvas;
    private bool isExpanded = true;

    private void Awake()
    {
        // Находим родительский Canvas для корректного масштабирования при перетаскивании
        canvas = GetComponentInParent<Canvas>();

        if (toggleButton != null)
        {
            toggleButton.onClick.AddListener(ToggleWindow);
        }
    }

    // Выводим окно на передний план при клике
    public void OnPointerDown(PointerEventData eventData)
    {
        windowRect.SetAsLastSibling();
    }

    // Перетаскивание окна с учетом Scale Factor канваса
    public void OnDrag(PointerEventData eventData)
    {
        if (windowRect == null || canvas == null) return;
        windowRect.anchoredPosition += eventData.delta / canvas.scaleFactor;
    }

    // Сворачивание и раскрытие контента
    public void ToggleWindow()
    {
        isExpanded = !isExpanded;
        if (contentPanel != null)
        {
            contentPanel.SetActive(isExpanded);
        }
    }
}
