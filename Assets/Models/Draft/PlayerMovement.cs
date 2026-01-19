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
    [SerializeField] private Transform followTransform; // Объект, за которым следует камера
    [SerializeField] private float rotationPower = 3f; // Чувствительность мыши
    [SerializeField] private float minVerticalAngle = 40f; // Минимальный угол камеры (вниз)
    [SerializeField] private float maxVerticalAngle = 340f; // Максимальный угол камеры (вверх)
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
        HandleCameraRotation();
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

    #region Camera Rotation
    private void HandleCameraRotation()
    {
        if (followTransform == null) return;

        // Горизонтальное вращение (вокруг оси Y) - вращаем ИГРОКА
        float mouseX = Input.GetAxis("Mouse X");
        transform.rotation *= Quaternion.AngleAxis(mouseX * rotationPower, Vector3.up);

        // Вертикальное вращение (вокруг оси X) - вращаем только followTransform
        float mouseY = Input.GetAxis("Mouse Y");
        followTransform.rotation *= Quaternion.AngleAxis(mouseY * rotationPower, Vector3.right);

        // Получаем текущие углы
        var angles = followTransform.localEulerAngles;
        angles.z = 0; // Убираем вращение по Z

        var angle = followTransform.localEulerAngles.x;

        // Ограничиваем вертикальное вращение
        if (angle > 180f && angle < maxVerticalAngle)
        {
            angles.x = maxVerticalAngle;
        }
        else if (angle < 180f && angle > minVerticalAngle)
        {
            angles.x = minVerticalAngle;
        }

        // Применяем только X вращение, Y и Z обнуляем
        followTransform.localEulerAngles = new Vector3(angles.x, 0, 0);
    }
    #endregion

    #region Movement
    private void HandleMovement()
    {
        float horizontal = Input.GetAxisRaw("Horizontal"); // A/D
        float vertical = Input.GetAxisRaw("Vertical");     // W/S

        // Берём направление от самого игрока (он уже поворачивается мышью)
        Vector3 playerForward = transform.forward;
        Vector3 playerRight = transform.right;

        playerForward.y = 0;
        playerRight.y = 0;
        playerForward.Normalize();
        playerRight.Normalize();

        // Движение относительно направления игрока
        Vector3 desiredMoveDirection = playerForward * vertical + playerRight * horizontal;

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

        // УБРАЛИ АВТОМАТИЧЕСКИЙ ПОВОРОТ - игрок поворачивается только мышью
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