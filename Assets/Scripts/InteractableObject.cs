using UnityEngine;
using UnityEngine.InputSystem;

public class InteractableObject : MonoBehaviour
{
    protected static readonly float HOVER_DISTANCE_LIMIT = 16f;

    protected Transform _playerTransform;
    protected InputSystem_Actions _controls;
    protected SpriteRenderer _spriteRenderer;
    protected MaterialPropertyBlock _propBlock;
    protected static readonly int OutlineWidth = Shader.PropertyToID("_OutlineWidth");

    protected bool _isHovered;
    protected bool _isSelected;

    protected bool _onCooldown;

    protected Texture2D _defaultCursor;
    protected Texture2D _hoverCursor;

    protected void Awake()
    {
        _controls = new();
    }

    protected void OnEnable()
    {
        _controls.Enable();
        _controls.Player.Interact.performed += OnInteract;
    }

    protected void OnDisable()
    {
        _controls.Disable();
        _controls.Player.Interact.performed -= OnInteract;
    }

    protected virtual void Start()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _propBlock = new();
        _isHovered = false;
        _onCooldown = false;
        _defaultCursor = Resources.Load<Texture2D>("Sprites/cursor");
        _hoverCursor = Resources.Load<Texture2D>("Sprites/cursor_hover");
        _playerTransform = GameObject.FindWithTag("Player").transform;
    }

    protected void OnMouseEnter()
    {
        _isHovered = true;
    }

    protected void OnMouseExit()
    {
        _isHovered = false;
    }

    protected virtual void OnInteract(InputAction.CallbackContext context)
    {
        return;
    }

    protected virtual void Update()
    {

        float distance = Vector2.Distance(_playerTransform.position, transform.position);
        if (distance <= HOVER_DISTANCE_LIMIT && _isHovered && !_onCooldown)
        {
            if (!_isSelected)
            {
                _spriteRenderer.GetPropertyBlock(_propBlock);
                _propBlock.SetFloat(OutlineWidth, 0.5f);
                _spriteRenderer.SetPropertyBlock(_propBlock);
                Cursor.SetCursor(_hoverCursor, new Vector2(7, 0), CursorMode.Auto);
            }
            _isSelected = true;
        }
        else
        {
            if (_isSelected)
            {
                _spriteRenderer.GetPropertyBlock(_propBlock);
                _propBlock.SetFloat(OutlineWidth, 0f);
                _spriteRenderer.SetPropertyBlock(_propBlock);
                Cursor.SetCursor(_defaultCursor, Vector2.zero, CursorMode.Auto);
            }
            _isSelected = false;
        }
    }
}
