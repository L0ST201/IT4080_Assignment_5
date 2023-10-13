using UnityEngine;
using Unity.Netcode;

public class Player : NetworkBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float movementSpeed = 5f;
    [SerializeField] private float slowWalkMultiplier = 0.5f;
    [SerializeField] private float mouseSensitivity = 2f;
    [SerializeField] private float jumpForce = 7f;
    [SerializeField] private float gravity = 9.81f;
    [SerializeField] private Vector3 minBoundary = new Vector3(-2, 0, -2);
    [SerializeField] private Vector3 maxBoundary = new Vector3(2, 0, 2);

    [Header("Aiming Settings")]
    [SerializeField] private float aimingFOV = 40f;
    private const float FOV_LERP_SPEED = 5f;
    private const float MOUSE_Y_MULTIPLIER = -1f;
    private const float EXTRA_HEIGHT_TEST = 0.1f;

    private float normalFOV;
    private Camera playerCamera;
    private Vector3 moveDirection = Vector3.zero;
    private CharacterController characterController;
    private float verticalLookRotation = 0f;
    private bool isReloading = false;
    private PlayerAnimationHandler playerAnimationHandler;
    private NetworkVariable<Vector3> networkedPosition = new NetworkVariable<Vector3>();
    private NetworkVariable<Quaternion> networkedRotation = new NetworkVariable<Quaternion>();

    private void Awake()
    {
        InitializeComponents();
        normalFOV = playerCamera.fieldOfView;
    }

    private void InitializeComponents()
    {
        playerCamera = transform.Find("Camera").GetComponent<Camera>();
        characterController = GetComponent<CharacterController>();
        Animator animator = GetComponent<Animator>();

        if (!playerCamera) throw new System.NullReferenceException("Player camera not found.");
        if (!characterController) throw new System.NullReferenceException("CharacterController not found on player.");
        if (!animator) throw new System.NullReferenceException("Animator not found on player.");

        playerAnimationHandler = new PlayerAnimationHandler(animator);
    }

    private void Start()
    {
        SetupCameraAndListener();
        SetupNetworkVariables();
    }

    private void SetupCameraAndListener()
    {
        playerCamera.enabled = IsOwner;
        if (playerCamera.TryGetComponent(out AudioListener audioListener))
        {
            audioListener.enabled = IsOwner;
        }
    }

    private void SetupNetworkVariables()
    {
        networkedPosition.OnValueChanged += OnPositionChanged;
        networkedRotation.OnValueChanged += OnRotationChanged;
    }

    private void Update()
    {
        if (IsOwner)
        {
            OwnerHandleInput();
            HandleMouseLook();
        }
    }

    private void OwnerHandleInput()
    {
        HandleMovementInput();
        HandleMouseInput();
        HandleActionInput();
    }

    private void HandleMovementInput()
    {
        float xMove = Input.GetAxis("Horizontal");
        float zMove = Input.GetAxis("Vertical");
        bool isShiftKeyDown = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);

        Vector3 move = transform.right * xMove + transform.forward * zMove;
        float currentSpeed = isShiftKeyDown ? movementSpeed * slowWalkMultiplier : movementSpeed;

        moveDirection.x = move.x * currentSpeed;
        moveDirection.z = move.z * currentSpeed;
        moveDirection.y -= gravity * Time.deltaTime;

        playerAnimationHandler.UpdateMovementAnimations(xMove, zMove);
    }

    private void HandleMouseInput()
    {
        if (Input.GetMouseButton(1))
        {
            playerAnimationHandler.SetAiming(true);
            playerCamera.fieldOfView = Mathf.Lerp(playerCamera.fieldOfView, aimingFOV, Time.deltaTime * FOV_LERP_SPEED);
        }
        else
        {
            playerAnimationHandler.SetAiming(false);
            playerCamera.fieldOfView = Mathf.Lerp(playerCamera.fieldOfView, normalFOV, Time.deltaTime * FOV_LERP_SPEED);
        }
    }

    private void HandleActionInput()
    {
        HandleShootingInput();
        HandleReloadingInput();
        HandleRollInput();
        HandleJumpInput();
        HandlePickupInput();

        MoveServerRpc(moveDirection * Time.deltaTime);
    }

    private void HandleShootingInput()
    {
        if (Input.GetMouseButtonDown(0) && !isReloading)
        {
            playerAnimationHandler.TriggerShootAnimation();
        }
    }

    private void HandleReloadingInput()
    {
        if (Input.GetKeyDown(KeyCode.R) && !isReloading)
        {
            isReloading = true;
            playerAnimationHandler.TriggerReloadAnimation();
        }
    }

    private void HandleRollInput()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            playerAnimationHandler.TriggerRollAnimation();
        }
    }

    private void HandleJumpInput()
    {
        if (IsPlayerGrounded() && Input.GetButtonDown("Jump"))
        {
            moveDirection.y = jumpForce;
            playerAnimationHandler.SetJumping(1.0f);
        }
        else
        {
            playerAnimationHandler.SetJumping(0.0f);
        }
    }

    private void HandlePickupInput()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            playerAnimationHandler.TriggerPickupAnimation();
        }
    }

    private void HandleMouseLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        verticalLookRotation += mouseY * MOUSE_Y_MULTIPLIER;
        verticalLookRotation = Mathf.Clamp(verticalLookRotation, -90f, 90f);

        playerCamera.transform.localRotation = Quaternion.Euler(verticalLookRotation, 0f, 0f);
        RotatePlayer(mouseX);
    }

    private void RotatePlayer(float mouseX)
    {
        Vector3 rotation = Vector3.up * mouseX;
        transform.Rotate(rotation);
        RotateServerRpc(rotation);
    }

    private void OnPositionChanged(Vector3 oldValue, Vector3 newValue)
    {
        transform.position = newValue;
    }

    private void OnRotationChanged(Quaternion oldValue, Quaternion newValue)
    {
        transform.rotation = newValue;
    }

    [ServerRpc]
    private void MoveServerRpc(Vector3 movement)
    {
        if (!IsHostPlayer())
        {
            Vector3 intendedPosition = transform.position + movement;
            intendedPosition = Vector3.Max(minBoundary, intendedPosition);
            intendedPosition = Vector3.Min(maxBoundary, intendedPosition);
            movement = intendedPosition - transform.position;
        }

        characterController.Move(movement);

        if (transform.position != networkedPosition.Value)
        {
            networkedPosition.Value = transform.position;
        }
    }

    [ServerRpc]
    private void RotateServerRpc(Vector3 rotation)
    {
        if (rotation != Vector3.zero && transform.rotation != Quaternion.Euler(rotation))
        {
            transform.Rotate(rotation);
            networkedRotation.Value = transform.rotation;
        }
    }

    private bool IsPlayerGrounded()
    {
        return Physics.SphereCast(characterController.bounds.center, characterController.radius, Vector3.down, out RaycastHit hit, characterController.bounds.extents.y + EXTRA_HEIGHT_TEST);
    }

    private bool IsHostPlayer()
    {
        return NetworkManager.Singleton != null && NetworkManager.Singleton.LocalClientId == NetworkManager.ServerClientId;
    }

    public void FootStep()
    {
        playerAnimationHandler.FootStep();
    }

    public void RollSound()
    {
        playerAnimationHandler.RollSound();
    }

    public void CantRotate()
    {
        playerAnimationHandler.CantRotate();
    }

    public void EndRoll()
    {
        playerAnimationHandler.EndRoll();
    }
}
