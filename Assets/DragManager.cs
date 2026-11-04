using UnityEngine;
// Обязательно добавляем пространство имен новой системы ввода
using UnityEngine.InputSystem;

public class DragManager : MonoBehaviour
{
    [Header("Текущее состояние")]
    [SerializeField] private GameObject currentDraggedObject = null;

    private Camera mainCamera;
    private Vector2 offset;
    private bool isDragging = false;
    private LayerMask _defaultLayer;
    private bool _isOverInteractiveObject = false;

    public GameObject CurrentDraggedObject => currentDraggedObject;

    private void Awake()
    {
        _defaultLayer = gameObject.layer;
    }

    private void Start()
    {
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogError("На сцене не найдена главная камера с тегом MainCamera!");
        }
    }

    private void Update()
    {
        if (mainCamera == null) return;

        // Проверяем, подключено ли устройство указателя (мышь или тач)
        if (Pointer.current == null) return;

        // Получаем позицию указателя на экране и состояние нажатия
        Mouse currentMouse = Mouse.current;
        Vector2 screenPosition = currentMouse.position.ReadValue();

        // 1. Начало клика / касания
        if (!isDragging && currentMouse.press.wasPressedThisFrame)
        {
            TryStartDrag(screenPosition);
        }
        else if (isDragging && currentMouse.press.wasReleasedThisFrame)
        {
            EndDrag();
        }
        // 2. Процесс перетаскивания (удержание кнопки)
        if (isDragging)
        {
            DragObject(screenPosition);
        }

        if (isDragging && currentMouse.rightButton.isPressed) 
        {
            CheckHover(screenPosition);
        }

        CheckObjectWithInfo(screenPosition);
    }

    private void CheckObjectWithInfo(Vector2 screenPosition) 
    {
        // Переводим экранные координаты в мировые 2D-координаты 
        Vector2 mouseWorldPos = mainCamera.ScreenToWorldPoint(screenPosition);

        // Делаем Raycast в точку клика
        RaycastHit2D hit = Physics2D.Raycast(mouseWorldPos, Vector2.zero);

        if (hit.collider != null)
        {
            ObjectWithInfo infoComponent = hit.collider.GetComponent<ObjectWithInfo>();

            if (infoComponent != null)
            {
                InfoBox.SetInfoBox?.Invoke(infoComponent.Info.ToArray());
                _isOverInteractiveObject = true;
            }
            else
            {
                if (_isOverInteractiveObject) InfoBox.RemoveInfoBox?.Invoke();
                _isOverInteractiveObject = false;
            }
        }
        else 
        {
            if (_isOverInteractiveObject) InfoBox.RemoveInfoBox?.Invoke();
            _isOverInteractiveObject = false;
        }
    }

    private void TryStartDrag(Vector2 screenPosition)
    {
        // Переводим экранные координаты в мировые 2D-координаты 
        Vector2 mouseWorldPos = mainCamera.ScreenToWorldPoint(screenPosition);

        // Делаем Raycast в точку клика
        RaycastHit2D hit = Physics2D.Raycast(mouseWorldPos, Vector2.zero);

        if (hit.collider != null)
        {
            Draggable draggableComponent = hit.collider.GetComponent<Draggable>();

            if (draggableComponent != null)
            {
                currentDraggedObject = draggableComponent.gameObject;
                draggableComponent.Dragged = true;
                isDragging = draggableComponent.Dragged;
                currentDraggedObject.layer = LayerMask.NameToLayer("Ignore Raycast");

                // Считаем смещение
                offset = (Vector2)currentDraggedObject.transform.position - mouseWorldPos;
            }
        }
    }

    private void CheckHover(Vector2 screenPosition) 
    {
        Vector2 mouseWorldPos = mainCamera.ScreenToWorldPoint(screenPosition);

        // Делаем Raycast в точку клика
        RaycastHit2D hit = Physics2D.Raycast(mouseWorldPos, Vector2.zero);

        if (hit.collider != null)
        {
            InteractiveObject interactiveComponent = hit.collider.GetComponent<InteractiveObject>();

            if (interactiveComponent != null)
            {
                _isOverInteractiveObject = true;
                interactiveComponent.Interact(currentDraggedObject);
            }
            else
            {
                _isOverInteractiveObject = false;
            }

        }
        else
        {
            _isOverInteractiveObject = false;
        }
    }

    private void DragObject(Vector2 screenPosition)
    {
        Vector2 mouseWorldPos = mainCamera.ScreenToWorldPoint(screenPosition);

        float currentZ = currentDraggedObject.transform.position.z;
        Vector2 newPosition = mouseWorldPos + offset;

        currentDraggedObject.transform.position = new Vector3(newPosition.x, newPosition.y, currentZ);
    }

    private void EndDrag()
    {
        currentDraggedObject.GetComponent<Draggable>().Dragged = false;
        isDragging = false;
        currentDraggedObject.layer = _defaultLayer;

        currentDraggedObject = null;
    }
}
