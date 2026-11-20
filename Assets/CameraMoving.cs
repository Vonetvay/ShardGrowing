using UnityEngine;
using UnityEngine.InputSystem;

public class CameraMoving : MonoBehaviour
{
    private Camera _camera;
    [SerializeField] private float _moveSpeed = 1.0f;
    [SerializeField] private Vector2 _horizontalBarrier;
    [SerializeField] private Vector2 _verticalBarrier;

    [SerializeField] private bool _gizmosDraw;

    private InputSystem_Actions _controls;
    private Vector2 _moveInput;

    private void Awake()
    {
        _controls = new InputSystem_Actions();
        _camera = Camera.main;
    }

    private void OnEnable()
    {
        _controls.Camera.Enable();
        _controls.Camera.Move.performed += OnMove;
        _controls.Camera.Move.canceled += OnMove;
    }

    private void OnDisable()
    {
        _controls.Camera.Move.performed -= OnMove;
        _controls.Camera.Move.canceled -= OnMove;
        _controls.Camera.Disable();
    }

    private void OnMove(InputAction.CallbackContext context)
    {
        _moveInput = context.ReadValue<Vector2>();
    }

    private void LateUpdate()
    {
        Vector3 direction = new(_moveInput.x, _moveInput.y, 0f);
        Vector3 position = _camera.transform.position + direction * (_moveSpeed * Time.deltaTime);

        float halfHeight = _camera.orthographicSize;
        float halfWidth = halfHeight * _camera.aspect;

        if (position.x < _horizontalBarrier.x + halfWidth) { position = new Vector3(_horizontalBarrier.x + halfWidth, position.y, position.z); }
        else if (position.x > _horizontalBarrier.y - halfWidth) { position = new Vector3(_horizontalBarrier.y - halfWidth, position.y, position.z); }
        if (position.y < _verticalBarrier.x + halfHeight) { position = new Vector3(position.x, _verticalBarrier.x + halfHeight, position.z); }
        else if (position.y > _verticalBarrier.y - halfHeight) { position = new Vector3(position.x, _verticalBarrier.y - halfHeight, position.z); }

        _camera.transform.position = position;
    }

    private void OnDrawGizmos()
    {
        if (_gizmosDraw) 
        {
            Gizmos.color = Color.green;

            Vector3 size = new(
                _horizontalBarrier.y - _horizontalBarrier.x,
                _verticalBarrier.y - _verticalBarrier.x,
                0f);
            Vector3 center = new(
                (_horizontalBarrier.x + _horizontalBarrier.y) / 2f,
                (_verticalBarrier.x + _verticalBarrier.y) / 2f,
                0f);

            Gizmos.DrawWireCube(center, size);
        }
    }
}
