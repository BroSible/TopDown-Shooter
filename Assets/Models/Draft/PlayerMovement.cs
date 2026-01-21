using UnityEngine;
using Cinemachine;

public class PlayerMovement : MonoBehaviour
{
    #region Movement Settings
    [Header("Movement Settings")]
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float sprintSpeed = 8f;
    [SerializeField] private float acceleration = 10f;
    [SerializeField] private float deceleration = 20f; 
    #endregion

    #region Gravity Settings
    [Header("Gravity Settings")]
    [SerializeField] private float gravity = -9.81f;
    [SerializeField] private float groundCheckDistance = 0.2f;
    [SerializeField] private LayerMask groundMask;
    #endregion

    #region Camera Settings
    [Header("Camera Settings")]
    [SerializeField] private Transform followTransform;
    [SerializeField] private float rotationPower = 3f;
    [SerializeField] private float minVerticalAngle = 40f;
    [SerializeField] private float maxVerticalAngle = 340f;
    #endregion

    #region Control Lock
    [Header("Control Lock")]
    public bool isControlLocked = false;
    public bool lockCameraRotation = true;
    #endregion

    #region Components
    private CharacterController controller;
    private Animator animator;
    private Vector3 velocity;
    private bool isGrounded;
    private Vector3 currentMovement;
    #endregion

    void Start()
    {
        controller = GetComponent<CharacterController>();
        
        if (controller == null)
        {
            Debug.LogError("Контроллер не найден, добавь его на " + gameObject.name);
        }

        animator = GetComponentInChildren<Animator>();
        
        if (animator == null)
        {
            Debug.LogWarning("Аниматор не найден на " + gameObject.name);
        }

        if (followTransform == null)
        {
            Debug.LogWarning("Follow Transform не назначен! Создай пустой объект как child игрока.");
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
        HandleGravity();

        if (!isControlLocked)
        {
            HandleCameraRotation();
            HandleMovement();
            UpdateAnimations();
        }
        else
        {
            StopMovement();
        }
    }

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

    private void HandleCameraRotation()
    {
        if (followTransform == null) return;

        float mouseX = Input.GetAxis("Mouse X");
        transform.rotation *= Quaternion.AngleAxis(mouseX * rotationPower, Vector3.up);

        float mouseY = Input.GetAxis("Mouse Y");
        followTransform.rotation *= Quaternion.AngleAxis(mouseY * rotationPower, Vector3.right);

        var angles = followTransform.localEulerAngles;
        angles.z = 0; 

        var angle = followTransform.localEulerAngles.x;

        if (angle > 180f && angle < maxVerticalAngle)
        {
            angles.x = maxVerticalAngle;
        }
        else if (angle < 180f && angle > minVerticalAngle)
        {
            angles.x = minVerticalAngle;
        }

        followTransform.localEulerAngles = new Vector3(angles.x, 0, 0);
    }

    private void HandleMovement()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        Vector3 playerForward = transform.forward;
        Vector3 playerRight = transform.right;

        playerForward.y = 0;
        playerRight.y = 0;
        playerForward.Normalize();
        playerRight.Normalize();

        Vector3 desiredMoveDirection = playerForward * vertical + playerRight * horizontal;

        bool isSprinting = Input.GetKey(KeyCode.LeftShift);
        float currentSpeed = isSprinting ? sprintSpeed : walkSpeed;

        Vector3 targetMovement = desiredMoveDirection.normalized * currentSpeed;

        float lerpSpeed = desiredMoveDirection.magnitude > 0.1f ? acceleration : deceleration;

        currentMovement = Vector3.Lerp(currentMovement, targetMovement, lerpSpeed * Time.deltaTime);
        controller.Move(currentMovement * Time.deltaTime);
    }

    private void StopMovement()
    {
        currentMovement = Vector3.Lerp(currentMovement, Vector3.zero, deceleration * Time.deltaTime);
        controller.Move(currentMovement * Time.deltaTime);

        // ИСПРАВЛЕНО: Безопасное обновление анимаций
        if (animator != null)
        {
            // Проверяем существование каждого параметра перед установкой
            if (HasParameter(animator, "Speed"))
                animator.SetFloat("Speed", 0f);
            
            if (HasParameter(animator, "IsMoving"))
                animator.SetBool("IsMoving", false);
            
            if (HasParameter(animator, "IsSprinting"))
                animator.SetBool("IsSprinting", false);
            
            if (HasParameter(animator, "Horizontal"))
                animator.SetFloat("Horizontal", 0f);
            
            if (HasParameter(animator, "Vertical"))
                animator.SetFloat("Vertical", 0f);
        }
    }

    private void HandleGravity()
    {
        if (!isGrounded)
        {
            velocity.y += gravity * Time.deltaTime;
        }

        controller.Move(velocity * Time.deltaTime);
    }

    private void UpdateAnimations()
    {
        if (animator == null) return;

        Vector3 moveDirection = currentMovement;
        moveDirection.y = 0;
        float speed = moveDirection.magnitude;

        bool isMoving = speed > 0.1f;
        bool isSprinting = Input.GetKey(KeyCode.LeftShift) && isMoving;

        // ИСПРАВЛЕНО: Безопасная установка параметров
        if (HasParameter(animator, "IsMoving"))
            animator.SetBool("IsMoving", isMoving);
        
        if (HasParameter(animator, "IsSprinting"))
            animator.SetBool("IsSprinting", isSprinting);
        
        if (HasParameter(animator, "Speed"))
        {
            if (isSprinting)
                animator.SetFloat("Speed", 2f); 
            else if (isMoving)
                animator.SetFloat("Speed", 1f); 
            else
                animator.SetFloat("Speed", 0f);
        }
        
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        
        if (HasParameter(animator, "Horizontal"))
            animator.SetFloat("Horizontal", horizontal);
        
        if (HasParameter(animator, "Vertical"))
            animator.SetFloat("Vertical", vertical);
    }

    // НОВЫЙ МЕТОД: Проверка существования параметра в Animator
    private bool HasParameter(Animator anim, string paramName)
    {
        foreach (AnimatorControllerParameter param in anim.parameters)
        {
            if (param.name == paramName)
                return true;
        }
        return false;
    }

    public void LockControls(bool lockCamera = true)
    {
        isControlLocked = true;
        lockCameraRotation = lockCamera;
        Debug.Log("Управление игроком заблокировано");
    }

    public void UnlockControls()
    {
        isControlLocked = false;
        Debug.Log("Управление игроком разблокировано");
    }

    public bool IsControlLocked()
    {
        return isControlLocked;
    }

    public void ForceStop()
    {
        currentMovement = Vector3.zero;
        velocity.y = -2f;
    }

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
}