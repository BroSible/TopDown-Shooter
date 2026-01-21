using UnityEngine;

public class Minimap : MonoBehaviour
{
    [Header("Target Settings")]
    public Transform target; // Игрок или цель для слежения
    public Vector3 offset = new Vector3(0f, 50f, 0f); // Высота камеры над целью
    
    [Header("Follow Settings")]
    [Tooltip("Скорость следования камеры за целью")]
    [Range(0.1f, 20f)]
    public float smoothSpeed = 10f;
    
    [Tooltip("Следовать ли по оси X")]
    public bool followX = true;
    
    [Tooltip("Следовать ли по оси Z")]
    public bool followZ = true;
    
    [Tooltip("Фиксированная высота камеры")]
    public bool fixedHeight = true;
    
    [Header("Rotation Settings")]
    [Tooltip("Вращается ли камера вместе с игроком (обычно false для минимапы)")]
    public bool rotateWithTarget = false;
    
    [Tooltip("Фиксированное вращение камеры (90 градусов для вида сверху)")]
    public Vector3 fixedRotation = new Vector3(90f, 0f, 0f);
    
    [Header("Bounds Settings")]
    [Tooltip("Ограничивать ли движение камеры")]
    public bool useBounds = false;
    
    [Tooltip("Минимальные координаты (X, Y, Z)")]
    public Vector3 minBounds = new Vector3(-100f, 50f, -100f);
    
    [Tooltip("Максимальные координаты (X, Y, Z)")]
    public Vector3 maxBounds = new Vector3(100f, 50f, 100f);
    
    [Header("Zoom Settings")]
    [Tooltip("Можно ли изменять зум минимапы")]
    public bool allowZoom = false;
    
    [Tooltip("Текущий размер ортографической камеры")]
    [Range(10f, 200f)]
    public float orthographicSize = 50f;
    
    [Tooltip("Минимальный зум")]
    public float minZoom = 20f;
    
    [Tooltip("Максимальный зум")]
    public float maxZoom = 100f;
    
    [Tooltip("Скорость изменения зума")]
    public float zoomSpeed = 10f;
    
    private Camera minimapCamera;
    private Vector3 currentVelocity;
    private float targetOrthographicSize;
    
    void Start()
    {
        minimapCamera = GetComponent<Camera>();
        
        // Если камера не найдена, пробуем получить у дочерних объектов
        if (minimapCamera == null)
        {
            minimapCamera = GetComponentInChildren<Camera>();
        }
        
        if (minimapCamera == null)
        {
            Debug.LogError("MinimapCameraFollow: Камера не найдена на этом объекте!");
            enabled = false;
            return;
        }
        
        // Настраиваем камеру как ортографическую (обычно для минимапы)
        minimapCamera.orthographic = true;
        targetOrthographicSize = orthographicSize;
        
        // Устанавливаем фиксированное вращение
        if (!rotateWithTarget)
        {
            transform.rotation = Quaternion.Euler(fixedRotation);
        }
        
        // Начальная позиция камеры
        if (target != null)
        {
            Vector3 startPosition = target.position + offset;
            if (fixedHeight)
            {
                startPosition.y = offset.y;
            }
            transform.position = startPosition;
        }
        
        // Настраиваем параметры камеры
        minimapCamera.orthographicSize = orthographicSize;
        
        Debug.Log("MinimapCameraFollow: Инициализирован");
    }
    
    void Update()
    {
        if (target == null)
        {
            // Пытаемся найти игрока если цель не назначена
            FindPlayerTarget();
            return;
        }
        
        // Обработка зума
        HandleZoom();
    }
    
    void LateUpdate()
    {
        if (target == null) return;
        
        // Следование за целью
        FollowTarget();
        
        // Ограничение границ
        if (useBounds)
        {
            ClampPosition();
        }
    }
    
    void FollowTarget()
    {
        Vector3 targetPosition = target.position + offset;
        
        // Применяем настройки следования по осям
        Vector3 currentPosition = transform.position;
        
        if (!followX)
        {
            targetPosition.x = currentPosition.x;
        }
        
        if (!followZ)
        {
            targetPosition.z = currentPosition.z;
        }
        
        if (fixedHeight)
        {
            targetPosition.y = offset.y;
        }
        
        // Плавное перемещение
        Vector3 smoothPosition = Vector3.SmoothDamp(
            currentPosition, 
            targetPosition, 
            ref currentVelocity, 
            smoothSpeed > 0 ? 1f / smoothSpeed : 0.1f
        );
        
        transform.position = smoothPosition;
        
        // Вращение
        if (rotateWithTarget)
        {
            // Поворачиваем камеру в ту же сторону, что и игрок (но сохраняем вид сверху)
            Quaternion targetRotation = Quaternion.Euler(fixedRotation.x, target.eulerAngles.y, fixedRotation.z);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * smoothSpeed);
        }
    }
    
    void HandleZoom()
    {
        if (!allowZoom || minimapCamera == null) return;
        
        // Изменение зума с помощью колесика мыши
        float zoomInput = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(zoomInput) > 0.01f)
        {
            targetOrthographicSize -= zoomInput * zoomSpeed;
            targetOrthographicSize = Mathf.Clamp(targetOrthographicSize, minZoom, maxZoom);
        }
        
        // Плавное изменение размера камеры
        if (Mathf.Abs(minimapCamera.orthographicSize - targetOrthographicSize) > 0.1f)
        {
            minimapCamera.orthographicSize = Mathf.Lerp(
                minimapCamera.orthographicSize, 
                targetOrthographicSize, 
                Time.deltaTime * zoomSpeed
            );
        }
        else
        {
            minimapCamera.orthographicSize = targetOrthographicSize;
        }
    }
    
    void ClampPosition()
    {
        Vector3 clampedPosition = transform.position;
        
        clampedPosition.x = Mathf.Clamp(clampedPosition.x, minBounds.x, maxBounds.x);
        clampedPosition.y = Mathf.Clamp(clampedPosition.y, minBounds.y, maxBounds.y);
        clampedPosition.z = Mathf.Clamp(clampedPosition.z, minBounds.z, maxBounds.z);
        
        transform.position = clampedPosition;
    }
    
    void FindPlayerTarget()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            target = player.transform;
            Debug.Log("MinimapCameraFollow: Найден игрок как цель для слежения");
        }
    }
    
    // Метод для смены цели во время игры
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }
    
    // Метод для изменения смещения камеры
    public void SetOffset(Vector3 newOffset)
    {
        offset = newOffset;
    }
    
    // Метод для изменения высоты камеры
    public void SetHeight(float height)
    {
        offset.y = height;
        if (fixedHeight)
        {
            Vector3 newPosition = transform.position;
            newPosition.y = height;
            transform.position = newPosition;
        }
    }
    
    // Метод для изменения зума
    public void SetZoom(float zoom)
    {
        if (allowZoom && minimapCamera != null)
        {
            targetOrthographicSize = Mathf.Clamp(zoom, minZoom, maxZoom);
            minimapCamera.orthographicSize = targetOrthographicSize;
        }
    }
    
    // Метод для включения/выключения следования по осям
    public void SetFollowAxes(bool followXAxis, bool followZAxis)
    {
        followX = followXAxis;
        followZ = followZAxis;
    }
    
    // Метод для принудительного обновления позиции камеры (без плавности)
    public void SnapToTarget()
    {
        if (target == null) return;
        
        Vector3 targetPosition = target.position + offset;
        
        if (!followX) targetPosition.x = transform.position.x;
        if (!followZ) targetPosition.z = transform.position.z;
        if (fixedHeight) targetPosition.y = offset.y;
        
        transform.position = targetPosition;
    }
    
    // Метод для получения текущего зума
    public float GetCurrentZoom()
    {
        return minimapCamera != null ? minimapCamera.orthographicSize : orthographicSize;
    }
    
    #if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        // Отображение области видимости камеры
        if (minimapCamera != null && minimapCamera.orthographic)
        {
            Gizmos.color = Color.green;
            
            Vector3 cameraPos = transform.position;
            float size = minimapCamera.orthographicSize;
            
            // Рисуем квадрат области видимости
            Vector3 bottomLeft = new Vector3(cameraPos.x - size, cameraPos.y, cameraPos.z - size);
            Vector3 bottomRight = new Vector3(cameraPos.x + size, cameraPos.y, cameraPos.z - size);
            Vector3 topLeft = new Vector3(cameraPos.x - size, cameraPos.y, cameraPos.z + size);
            Vector3 topRight = new Vector3(cameraPos.x + size, cameraPos.y, cameraPos.z + size);
            
            Gizmos.DrawLine(bottomLeft, bottomRight);
            Gizmos.DrawLine(bottomRight, topRight);
            Gizmos.DrawLine(topRight, topLeft);
            Gizmos.DrawLine(topLeft, bottomLeft);
            
            // Линия от камеры к цели
            if (target != null)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(cameraPos, target.position);
            }
        }
        
        // Отображение границ
        if (useBounds)
        {
            Gizmos.color = new Color(1f, 0.5f, 0f, 0.3f); // Оранжевый с прозрачностью
            Vector3 center = (minBounds + maxBounds) / 2f;
            Vector3 size = maxBounds - minBounds;
            Gizmos.DrawWireCube(center, size);
        }
    }
    #endif
}