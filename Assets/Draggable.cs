using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Draggable : MonoBehaviour
{
    public bool Dragged;
    private Color _defaultColor;
    private int _defaultSortingOrder;

    private SpriteRenderer _spriteRenderer;
    private Rigidbody2D _rigidbody;
    private Collider2D _collider;

    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _rigidbody = GetComponent<Rigidbody2D>();
        _rigidbody.gravityScale = 0f;
        _rigidbody.linearDamping = 1f;
        _rigidbody.freezeRotation = true;
        if (_spriteRenderer != null) 
        {
            _defaultColor = _spriteRenderer.color;
            _defaultSortingOrder = _spriteRenderer.sortingOrder;
        }
    }

    private void Update()
    {
        if (_spriteRenderer != null) 
        {
            if (Dragged)
            {
                _spriteRenderer.color = _defaultColor * new Color(1f, 1f, 1f, 0.5f);
                _spriteRenderer.sortingOrder = 10;
                _rigidbody.bodyType = RigidbodyType2D.Kinematic;
                GetComponent<Collider2D>().enabled = false;
            }
            else 
            {
                _spriteRenderer.color = _defaultColor;
                _spriteRenderer.sortingOrder = _defaultSortingOrder;
                _rigidbody.bodyType = RigidbodyType2D.Dynamic;
                GetComponent<Collider2D>().enabled = true;
            }
        }
    }
}
