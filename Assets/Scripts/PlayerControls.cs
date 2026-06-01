using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerControls : MonoBehaviour
{
    [SerializeField]
    private float _moveSpeed = 1;

    private InputSystem_Actions _controls;
    private Animator _playerAnimator;
    private bool _isWalking;
    public bool IsWalking
    {
        get { return _isWalking; }
        set
        {
            if (value == _isWalking)
            {
                return;
            }
            _isWalking = value;
            _playerAnimator.SetBool("IsWalking", value);
        }
    }

    private void Awake()
    {
        _controls = new();
        IsWalking = false;
    }

    private void Start()
    {
        _playerAnimator = GetComponentInChildren<Animator>();
    }

    private void OnEnable()
    {
       _controls.Player.Enable();
    }
    private void OnDisable()
    {
        _controls.Player.Disable();
    }

    private void FixedUpdate()
    {
        Vector2 moveInput = _controls.Player.Move.ReadValue<Vector2>();
        moveInput = Vector2Int.RoundToInt(moveInput);
        Vector3 qMoveInput = new Vector3(moveInput.x, moveInput.y, 0) * _moveSpeed * Time.fixedDeltaTime;
        IsWalking = qMoveInput != Vector3.zero;
        transform.position += qMoveInput;

    }
}
