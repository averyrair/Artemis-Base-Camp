using UnityEngine;
using UnityEngine.InputSystem;

public class InteractableObject : MonoBehaviour
{
    protected InputSystem_Actions _controls;
    protected SpriteRenderer _spriteRenderer;
    protected MaterialPropertyBlock _propBlock;
    protected static readonly int OutlineWidth = Shader.PropertyToID("_OutlineWidth");

    protected bool _isHovered;

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
    }

    protected void OnMouseEnter()
    {
        _isHovered = true;
        _spriteRenderer.GetPropertyBlock(_propBlock);
        _propBlock.SetFloat(OutlineWidth, 0.5f);
        _spriteRenderer.SetPropertyBlock(_propBlock);
    }

    protected void OnMouseExit()
    {
        _isHovered = false;
        _spriteRenderer.GetPropertyBlock(_propBlock);
        _propBlock.SetFloat(OutlineWidth, 0f);
        _spriteRenderer.SetPropertyBlock(_propBlock);
    }

    protected virtual void OnInteract(InputAction.CallbackContext context)
    {
        return;
    }
}
