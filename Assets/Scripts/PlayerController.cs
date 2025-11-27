using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

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

    [Header("Camera Look")]
    [SerializeField] private Transform cameraPivot;

    [Header("Camera Look Sensitivity")]
    [SerializeField] private float horizontalSensitivity = 1.5f;
    [SerializeField] private float verticalSensitivity = 0.7f;

    [SerializeField] private float minPitch = -45f;
    [SerializeField] private float maxPitch = 70f;

    private float pitch = 0f;

    private CharacterController controller;
    private PlayerInput playerInput;
    private Transform cam;
    private InputActionMap playerMap;

    private InputAction moveAction;
    private InputAction lookAction;
    private InputAction shootAction;
    private InputAction reloadAction;
    private InputAction crouchAction;

    private InputAction respawnAction;
    private InputAction restartAction;

    private bool isCrouching = false;
    private float originalSpeed;

    private Vector3 startPosition;
    private Quaternion startRotation;

    private PlayerStats stats;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        playerInput = GetComponent<PlayerInput>();
        cam = Camera.main.transform;

        stats = GetComponent<PlayerStats>();
        stats.Init(() => { ToggleInput(false); });

        InputActionAsset inputs = playerInput.actions;
        playerMap = inputs.FindActionMap("Player");

        moveAction = playerInput.actions["Move"];
        lookAction = playerInput.actions["Look"];
        shootAction = playerInput.actions["Shoot"];
        reloadAction = playerInput.actions["Reload"];
        crouchAction = playerInput.actions["Crouch"];

        respawnAction = playerInput.actions["Respawn"];
        restartAction = playerInput.actions["RestartScene"];

        originalSpeed = moveSpeed;

        startPosition = transform.position;
        startRotation = transform.rotation;
    }

    private void OnEnable()
    {
        shootAction.performed += _ => gun.Shoot();
        reloadAction.performed += _ => gun.Reload();
        crouchAction.performed += _ => ToggleCrouch();

        respawnAction.performed += _ => RespawnPlayer();
        restartAction.performed += _ => RestartScene();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void OnDisable()
    {
        shootAction.performed -= _ => gun.Shoot();
        reloadAction.performed -= _ => gun.Reload();
        crouchAction.performed -= _ => ToggleCrouch();

        respawnAction.performed -= _ => RespawnPlayer();
        restartAction.performed -= _ => RestartScene();

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

        float yaw = look.x * horizontalSensitivity;
        transform.Rotate(Vector3.up * yaw);

        pitch -= look.y * verticalSensitivity;
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

        cameraPivot.localRotation = Quaternion.Euler(pitch, 0, 0);
    }

    private void HandleMovement()
    {
        Vector2 input = moveAction.ReadValue<Vector2>();

        Vector3 forward = transform.forward * input.y;
        Vector3 right = transform.right * input.x;
        Vector3 move = forward + right;

        if (move.sqrMagnitude > 1f)
            move.Normalize();

        controller.Move(move * moveSpeed * Time.deltaTime);

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

    private void RespawnPlayer()
    {
        Debug.Log("Player respawn!");

        // resetear transform
        controller.enabled = false;
        transform.position = startPosition;
        transform.rotation = startRotation;
        controller.enabled = true;

        // resetear movimiento
        velocity = Vector3.zero;
        pitch = 0f;
        cameraPivot.localRotation = Quaternion.identity;

        // resetear stats
        stats.ResetStats();

        // resetear crouch
        isCrouching = false;
        controller.height = normalHeight;
        moveSpeed = originalSpeed;

        ToggleInput(true);
    }

    private void RestartScene()
    {
        Debug.Log("Restarting scene...");
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void ToggleInput(bool status)
    {
        if (status)
        {
            playerMap.Enable();
        }
        else
        {
            playerMap.Disable();
            playerMap.FindAction("Respawn").Enable();
            playerMap.FindAction("RestartScene").Enable();
        }
    }
}
