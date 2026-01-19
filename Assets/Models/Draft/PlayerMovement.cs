using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    #region Movement Settings
    [Header("Movement Settings")]
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float sprintSpeed = 8f;
    [SerializeField] private float rotationSpeed = 10f;
    [SerializeField] private float acceleration = 10f;
    [SerializeField] private float deceleration = 20f; 
    #endregion

    #region Gravity Settings
    [Header("Gravity Settings")]
    [SerializeField] private float gravity = -9.81f;
    [SerializeField] private float groundCheckDistance = 0.2f;
    [SerializeField] private LayerMask groundMask;
    #endregion

    #region Components
    private CharacterController controller;
    private Animator animator;
    private Vector3 velocity;
    private bool isGrounded;
    private Vector3 currentMovement;
    #endregion

    #region Unity Lifecycle
    void Start()
    {
        controller = GetComponent<CharacterController>();
        
        if (controller == null)
        {
            Debug.LogError("Контроллер не найден долбоёб добавь его" + gameObject.name);
        }

        animator = GetComponentInChildren<Animator>();
        
        if (animator == null)
        {
            Debug.LogWarning("Ты еблан? Где аниматор?");
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        if (Input.GetMouseButtonDown(0) && Cursor.lockState == CursorLockMode.None)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        HandleGroundCheck();
        HandleMovement();
        HandleGravity();
        UpdateAnimations();
    }
    #endregion

    #region Ground Check
    private void HandleGroundCheck()
    {
        isGrounded = Physics.CheckSphere(
            transform.position - new Vector3(0, controller.height / 2, 0), 
            groundCheckDistance, 
            groundMask
        );

        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f; 
        }
    }
    #endregion

    #region Movement
    private void HandleMovement()
    {
        float horizontal = Input.GetAxisRaw("Horizontal"); // A/D
        float vertical = Input.GetAxisRaw("Vertical");     // W/S

        Vector3 cameraForward = Camera.main.transform.forward;
        Vector3 cameraRight = Camera.main.transform.right;

        cameraForward.y = 0;
        cameraRight.y = 0;
        cameraForward.Normalize();
        cameraRight.Normalize();

        Vector3 desiredMoveDirection = cameraForward * vertical + cameraRight * horizontal;

        bool isSprinting = Input.GetKey(KeyCode.LeftShift);
        float currentSpeed = isSprinting ? sprintSpeed : walkSpeed;

        Vector3 targetMovement = desiredMoveDirection.normalized * currentSpeed;

        float lerpSpeed;
        if (desiredMoveDirection.magnitude > 0.1f)
        {
            lerpSpeed = acceleration;  // Разгон
        }
        else
        {
            lerpSpeed = deceleration;  // Торможение
        }

        currentMovement = Vector3.Lerp(currentMovement, targetMovement, lerpSpeed * Time.deltaTime);
        controller.Move(currentMovement * Time.deltaTime);

        if (desiredMoveDirection.magnitude > 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(desiredMoveDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }
    #endregion

    #region Gravity
    private void HandleGravity()
    {
        if (!isGrounded)
        {
            velocity.y += gravity * Time.deltaTime;
        }

        controller.Move(velocity * Time.deltaTime);
    }
    #endregion

    #region Animation Updates
    private void UpdateAnimations()
    {
        if (animator == null) return;

        Vector3 moveDirection = currentMovement;
        moveDirection.y = 0;
        float speed = moveDirection.magnitude;

        if (Input.GetKey(KeyCode.LeftShift) && speed > 0.1f)
        {
            animator.SetFloat("Speed", 1f);
        }
        else if (speed > 0.1f)
        {
            animator.SetFloat("Speed", 0.5f);
        }
        else
        {
            animator.SetFloat("Speed", 0f);
        }
    }
    #endregion

    #region Debug
    private void OnDrawGizmosSelected()
    {
        if (controller != null)
        {
            Gizmos.color = isGrounded ? Color.green : Color.red;
            Gizmos.DrawWireSphere(
                transform.position - new Vector3(0, controller.height / 2, 0),
                groundCheckDistance
            );
        }
    }
    #endregion
}