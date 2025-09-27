using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.InputSystem; // New Input System

[DisallowMultipleComponent]
public class PlayerController : MonoBehaviour
{

    private Vector2 _movement;
    [SerializeField] Movement2D movement;
    [SerializeField] Entity entity;
    [SerializeField] private PlayerInput playerInput;
    private InputAction _move;
    private InputAction _attack;
    private InputAction _mousePosition;
    [SerializeField] private Camera mainCamera;
    [SerializeField] int playerSpeed;
    [SerializeField] int playerTier;
    public AttackController attackController;

    [Header("Attack Settings")]
    [SerializeField] private int selectedAttackIndex = 0;

    public void Start()
    {
        if (movement == null) movement = GetComponent<Movement2D>();
        if (entity == null) entity = GetComponent<Entity>();
        if (attackController == null) attackController = GetComponent<AttackController>();
        entity.tier = playerTier;
        movement.speed = playerSpeed;
        if (mainCamera == null) mainCamera = Camera.main;

        //INPUT Actions
        if (!playerInput) playerInput = GetComponent<PlayerInput>();
        playerInput.currentActionMap.Enable();
        _move = playerInput.currentActionMap.FindAction("Move");
        _attack = playerInput.currentActionMap.FindAction("Attack");
        _mousePosition = playerInput.currentActionMap.FindAction("MousePosition");

        _move.Enable();
        _attack.Enable();
        _mousePosition.Enable();

        _attack.performed += OnAttack;
        selectedAttackIndex = 0;
        // **************************************************

    }

    void FixedUpdate()
    {
        _movement = _move.ReadValue<Vector2>();
        movement.SetMove(_movement);

        // Switch attacks with number keys //this should be replaced when item system implemented
        for (int i = 0; i < attackController.availableAttacks.Count; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
            {
                selectedAttackIndex = i;
            }
        }
    }

    void OnAttack(InputAction.CallbackContext context)
    {

        if (attackController == null || attackController.availableAttacks.Count == 0) { Debug.Log("No Attacks Available"); return; }

        // Get mouse position in world space
        Vector2 mouseScreenPos = _mousePosition.ReadValue<Vector2>();
        Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(new Vector3(mouseScreenPos.x, mouseScreenPos.y, mainCamera.nearClipPlane));
        mouseWorldPos.z = 0;  //dont need cuz 2d?? //welp apparently i do or mouse position is based on lower left corenr of screen
        Debug.Log("Attack Attempt, Mouse Position: " + mouseWorldPos);
        // Try to attack towards mouse position
        AttackSO selectedAttack = attackController.availableAttacks[selectedAttackIndex];
        attackController.TryAttack(selectedAttack, mouseWorldPos);
    }



    // private void OnEnable()
    // {
    //     var map = playerInput.currentActionMap;
    //     if (map.enabled == false) map.Enable();


    //     _move.Enable();
    // }

    private void OnDisable()
    {
        if (_move != null)
        {
            _move.Disable();
            _move = null;
        }
        _mousePosition.Disable();
        _attack.performed -= OnAttack;
    }

    void OnDrawGizmosSelected()
    {
        // Always show attack range when selected
        Gizmos.color = new Color(1, 1, 0, 0.2f);
        if (attackController?.availableAttacks?.Count > 0)
        {
            var attack = attackController.availableAttacks[0];
            if (attack != null)
            {
                Gizmos.DrawWireSphere(transform.position, attack.range);
            }
        }
    }



}