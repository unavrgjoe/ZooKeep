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

    [SerializeField] int playerSpeed;
    [SerializeField] int playerTier;

    public void Start()
    {
        if (movement == null) movement = GetComponent<Movement2D>();
        if (entity == null) entity = GetComponent<Entity>();
        entity.tier = playerTier;
        movement.speed = playerSpeed;
        if (!playerInput) playerInput = GetComponent<PlayerInput>();
        playerInput.currentActionMap.Enable();
        _move = playerInput.currentActionMap.FindAction("Move");
        _move.Enable();
    }

    void FixedUpdate()
    {
        _movement = _move.ReadValue<Vector2>();
        movement.SetMove(_movement);
    }



    // private void OnEnable()
    // {
    //     var map = playerInput.currentActionMap;
    //     if (map.enabled == false) map.Enable();


    //     _move.Enable();
    // }

    // private void OnDisable()
    // {
    //     if (_move != null)
    //     {
    //         _move.Disable();
    //         _move = null;
    //     }
    // }

}