using UnityEngine;
using Unity.Netcode;

public class Player : NetworkBehaviour
{
    // --- Constants ---
    private const float FOV_LERP_SPEED = 5f;
    private const float MOUSE_Y_MULTIPLIER = -1f;
    private const float EXTRA_HEIGHT_TEST = 0.1f;

    // --- NetworkVariables ---
    [Header("Network Variables")]
    public NetworkVariable<Color> playerColorNetVar = new NetworkVariable<Color>();
    public NetworkVariable<int> ScoreNetVar = new NetworkVariable<int>();
    private NetworkVariable<Vector3> networkedPosition = new NetworkVariable<Vector3>();
    private NetworkVariable<Quaternion> networkedRotation = new NetworkVariable<Quaternion>();

    // --- Player Attributes ---
    [Header("Player Attributes")]
    public PlayerColorManager colorManager;
    public BulletSpawner bulletSpawner;

    [Header("Player Color")]
    [Tooltip("The player's color.")]
    public Color playerColor;

    [Header("Score Settings")]
    [Tooltip("Multiplier affecting the score.")]
    public float scoreMultiplier = 1f;

    // --- Movement ---
    [Header("Movement Settings")]
    [SerializeField]
    [Tooltip("Default movement speed of the player.")]
    private float movementSpeed = 5f;

    [SerializeField]
    [Tooltip("Multiplier for walking speed.")]
    private float slowWalkMultiplier = 0.5f;

    [SerializeField]
    [Tooltip("The force applied when the player jumps.")]
    private float jumpForce = 7f;

    [SerializeField]
    [Tooltip("Gravity affecting the player.")]
    private float gravity = 9.81f;

    // --- Aiming ---
    [Header("Aiming Settings")]
    [SerializeField]
    [Tooltip("Field of view when aiming.")]
    private float aimingFOV = 40f;

    [SerializeField]
    [Tooltip("Sensitivity of mouse movements.")]
    private float mouseSensitivity = 2f;

    // --- Private Runtime Variables ---
    [Header("Runtime Components and Variables")]
    private Camera playerCamera;
    private float normalFOV;
    private Vector3 moveDirection = Vector3.zero;
    private CharacterController characterController;
    private float verticalLookRotation = 0f;
    private bool isReloading = false;
    private PlayerAnimationHandler playerAnimationHandler;

    public void AddScore(int points)
    {
        ScoreNetVar.Value += (int)(points * scoreMultiplier);
    }

    // --- Initialization and Setup ---
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

    // --- Update and Input Handling ---
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
        // HandlePickupInput();

        MoveServerRpc(moveDirection * Time.deltaTime);
    }

    // --- Individual Action Handlers ---
   private void HandleShootingInput()
    {
        if (Input.GetMouseButtonDown(0) && !isReloading)
        {
            playerAnimationHandler.TriggerShootAnimation();
            bulletSpawner.RequestFireServerRpc();  // Request the server to spawn the bullet
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

/*     private void HandlePickupInput()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            playerAnimationHandler.TriggerPickupAnimation();

            // Check for nearby power-ups
            Collider[] hitColliders = Physics.OverlapSphere(transform.position, pickupRadius);
            foreach (var hitCollider in hitColliders)
            {
                BasePowerUp powerUp = hitCollider.GetComponent<BasePowerUp>();
                if (powerUp)
                {
                    powerUp.ServerPickUp(this);
                    break; // Assuming a player can pick up only one power-up at a time
                }
            }
        }
    } */

    // --- RPCs and their callbacks ---
    [ServerRpc]
    private void MoveServerRpc(Vector3 movement)
    {
        characterController.Move(movement);
        networkedPosition.Value = transform.position;
    }

    [ServerRpc]
    private void RotateServerRpc(Vector3 rotation)
    {
        transform.eulerAngles = rotation;
        networkedRotation.Value = transform.rotation;
    }

    private void OnPositionChanged(Vector3 oldValue, Vector3 newValue)
    {
        transform.position = newValue;
    }

    private void OnRotationChanged(Quaternion oldValue, Quaternion newValue)
    {
        transform.rotation = newValue;
    }

    // --- Public and Animation/Sound-related methods ---
   public void ChangeColor(Color newColor)
    {
        if (colorManager == null)
        {
            Debug.LogError("colorManager is not set on the player.");
            return;
        }

        playerColorNetVar.Value = newColor;
        colorManager.UpdateNetworkedColor(newColor);
    }

    public float GetMovementSpeed()
    {
        return movementSpeed;
    }

    public void SetMovementSpeed(float speed)
    {
        movementSpeed = speed;
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

    // --- Utility Methods ---
    private bool IsPlayerGrounded()
    {
        return characterController.isGrounded || Physics.Raycast(transform.position, Vector3.down, characterController.height / 2 + EXTRA_HEIGHT_TEST);
    }

    private bool IsHostPlayer()
    {
        return NetworkManager.Singleton.LocalClientId == OwnerClientId;
    }

    private void HandleMouseLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        verticalLookRotation = Mathf.Clamp(verticalLookRotation + mouseY, -90f, 90f);

        playerCamera.transform.localEulerAngles = new Vector3(verticalLookRotation, 0, 0);
        transform.Rotate(Vector3.up * mouseX);

        RotateServerRpc(transform.eulerAngles);
    }
}
