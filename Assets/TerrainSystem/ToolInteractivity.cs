using UnityEngine;
using UnityEngine.InputSystem;

public class ToolInteractivity : MonoBehaviour
{
    [SerializeField] private Camera cam;
    [SerializeField] private CustomRenderTexture maskTexture; // Ссылка на FarmMask_CRT
    [SerializeField] private Material paintMaterial;         // Ссылка на M_PaintBrush
    [SerializeField] private float brushSize = 0.02f;

    void Update()
    {
        Mouse currentMouse = Mouse.current;

        if (currentMouse.leftButton.IsPressed()) // Используем GetMouseButton для возможности непрерывного рисования
        {
            Ray ray = cam.ScreenPointToRay(currentMouse.position.ReadValue());
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                // Проверяем, что попали по объекту ландшафта
                if (hit.collider != null)
                {
                    
                    // Получаем точные UV-координаты геометрии в точке удара
                    Vector2 uv = hit.textureCoord;
                    Debug.Log($"{uv.x}, {uv.y}");
                    PaintOnMask(uv);
                }
            }
        }
    }

    private void PaintOnMask(Vector2 uv)
    {
        // Передаем UV координаты клика в шейдер кисти
        paintMaterial.SetVector("_PaintPosition", new Vector4(uv.x, uv.y, 0f, 0f));
        //paintMaterial.SetFloat("_BrushSize", brushSize);

        // Заставляем CustomRenderTexture сделать один шаг симуляции (отрисовать круг)
        maskTexture.Update();
    }
}
