using UnityEngine;
using UnityEngine.InputSystem;

public class FollowCursorCamera : MonoBehaviour
{
    [SerializeField] private Canvas myCanvas;
    private RectTransform rectTransform;

    private Mouse mouse;

    void Start()
    {
        mouse = Mouse.current;
        rectTransform = GetComponent<RectTransform>();
        if (myCanvas == null) myCanvas = GetComponentInParent<Canvas>();
    }

    void Update()
    {
        if (mouse == null || myCanvas == null) return;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            myCanvas.transform as RectTransform,
            mouse.position.ReadValue(),
            myCanvas.worldCamera,
            out Vector2 localPoint
        );

        rectTransform.localPosition = localPoint;
    }
}
