using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController), typeof(PlayerInput))]
public class PlayerController : MonoBehaviour
{
    [Header("Movimiento")]
    [SerializeField] private float moveSpeed = 5.0f;
    [SerializeField] private float rotationSpeed = 150f;

    [Header("Crouch")]
    [SerializeField] private float normalHeight = 2f;
    [SerializeField] private float crouchHeight = 1f;
    [SerializeField] private float crouchSpeedMultiplier = 0.25f;

    [Header("Arma")]
    [SerializeField] private Gun gun;

    [Header("Gravedad")]
    [SerializeField] private float gravity = -9.81f;
    private Vector3 velocity;


    private CharacterController controller;
    private PlayerInput playerInput;
    private Transform cam;

    private InputAction moveAction;
    private InputAction lookAction;
    private InputAction shootAction;
    private InputAction reloadAction;
    private InputAction crouchAction;

    private bool isCrouching = false;
    private float originalSpeed;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        playerInput = GetComponent<PlayerInput>();
        cam = Camera.main.transform;

        moveAction = playerInput.actions["Move"];
        lookAction = playerInput.actions["Look"];
        shootAction = playerInput.actions["Shoot"];
        reloadAction = playerInput.actions["Reload"];
        crouchAction = playerInput.actions["Crouch"];

        originalSpeed = moveSpeed;
    }

    private void OnEnable()
    {
        shootAction.performed += _ => gun.Shoot();
        reloadAction.performed += _ => gun.Reload();
        crouchAction.performed += _ => ToggleCrouch();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void OnDisable()
    {
        shootAction.performed -= _ => gun.Shoot();
        reloadAction.performed -= _ => gun.Reload();
        crouchAction.performed -= _ => ToggleCrouch();

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void Update()
    {
        HandleLook();
        HandleMovement();
    }

    private void HandleLook()
    {
        Vector2 look = lookAction.ReadValue<Vector2>();
        transform.Rotate(Vector3.up * look.x * rotationSpeed * Time.deltaTime);
    }

    private void HandleMovement()
    {
        Vector2 input = moveAction.ReadValue<Vector2>();

        Vector3 forward = transform.forward * input.y;
        Vector3 right = transform.right * input.x;
        Vector3 move = forward + right;

        if (move.sqrMagnitude > 1f)
            move.Normalize();

        // Movimiento plano
        controller.Move(move * moveSpeed * Time.deltaTime);

        // ===== GRAVEDAD =====
        if (controller.isGrounded)
            velocity.y = -1f;
        else
            velocity.y += gravity * Time.deltaTime;

        controller.Move(velocity * Time.deltaTime);
    }
    
    private void ToggleCrouch()
    {
        isCrouching = !isCrouching;

        if (isCrouching)
        {
            controller.height = crouchHeight;
            moveSpeed = originalSpeed * crouchSpeedMultiplier;
        }
        else
        {
            controller.height = normalHeight;
            moveSpeed = originalSpeed;
        }
    }
}
