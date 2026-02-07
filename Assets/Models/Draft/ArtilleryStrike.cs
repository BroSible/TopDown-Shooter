using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.VFX;

public class ArtilleryStrike : MonoBehaviour
{
    [Header("Minimap Camera")]
    public Camera minimapCamera;
    public RawImage minimapDisplay;
    public RenderTexture minimapRenderTexture;
    public Image targetCursor;
    public Canvas artilleryCanvas;
    
    [Header("Minimap Animation Settings")]
    public RectTransform minimapPanel;
    [Tooltip("Начальный размер минимапы")]
    public Vector2 minimapClosedSize = new Vector2(200f, 200f);
    [Tooltip("Развёрнутый размер минимапы")]
    public Vector2 minimapOpenSize = new Vector2(600f, 600f);
    [Tooltip("Начальная позиция минимапы")]
    public Vector2 minimapClosedPosition = new Vector2(-100f, -100f);
    [Tooltip("Развёрнутая позиция минимапы")]
    public Vector2 minimapOpenPosition = new Vector2(0f, 0f);
    [Tooltip("Скорость анимации (1-20, чем больше - тем быстрее)")]
    [Range(1f, 20f)]
    public float minimapAnimationSpeed = 8f;
    [Tooltip("Тип интерполяции")]
    public AnimationType animationType = AnimationType.Lerp;
    
    public enum AnimationType
    {
        Lerp,           
        Smoothstep,     
        EaseInOut,      
        EaseOut         
    }
    
    [Header("Strike Settings")]
    public VisualEffect strikeVFX;
    public GameObject strikeVFXPrefab;
    public GameObject bombPrefab;
    public GameObject strikeMarkerPrefab;
    public float strikeDelay = 2f;
    public float strikeRadius = 10f;
    public float strikeDamage = 300f;
    public LayerMask enemyLayer;
    public int availableStrikes = 3;
    
    [Header("Bomb Settings")]
    public float bombFallHeight = 100f;
    public float bombFallSpeed = 20f;
    public float bombExplosionDelay = 0.1f;
    
    [Header("Animation")]
    public Animator playerAnimator;
    public string callStrikeAnimation = "CallStrike";
    public string idleStrikeAnimation = "Idle_Strike";
    public string idleAnimation = "Idle";
    
    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip callStrikeSound;
    public AudioClip confirmSound;
    public AudioClip incomingSound;
    public AudioClip bombWhistleSound;
    public AudioClip explosionSound;
    
    [Header("Player References")]
    public PlayerMovement playerMovement;
    public WeaponManager weaponManager;
    public Transform playerTransform;
    
    [Header("Debug")]
    public bool showDebugInfo = true;
    public bool drawGizmos = true;
    
    public bool isSelectingTarget { get; private set; } = false;
    public bool isStrikeInProgress { get; private set; } = false;
    private Vector3 selectedWorldPosition;
    private Camera mainCamera;
    private CursorLockMode previousCursorMode;
    private GameObject currentStrikeMarker;
    private Coroutine animationCoroutine;
    
    void Start()
    {
        mainCamera = Camera.main;
        
        if (playerTransform == null)
        {
            playerTransform = transform;
            Debug.LogWarning("[Artillery] Player Transform не назначен, использую transform скрипта");
        }
        
        SetupMinimapCamera();
        
        if (artilleryCanvas != null)
        {
            artilleryCanvas.gameObject.SetActive(false);
        }
        
        if (minimapPanel != null)
        {
            minimapPanel.sizeDelta = minimapClosedSize;
            minimapPanel.anchoredPosition = minimapClosedPosition;
        }
        
        if (targetCursor != null)
        {
            targetCursor.gameObject.SetActive(false);
        }

        if (showDebugInfo)
        {
            Debug.Log($"[Artillery] Инициализация. Доступно ударов: {availableStrikes}");
        }
    }

    void SetupMinimapCamera()
    {
        if (minimapCamera == null)
        {
            GameObject minimapCamObj = GameObject.FindGameObjectWithTag("MinimapCamera");
            if (minimapCamObj != null)
            {
                minimapCamera = minimapCamObj.GetComponent<Camera>();
            }
        }
        
        if (minimapCamera == null)
        {
            GameObject camObj = new GameObject("Minimap Camera");
            minimapCamera = camObj.AddComponent<Camera>();
            minimapCamera.orthographic = true;
            minimapCamera.orthographicSize = 50f;
            minimapCamera.cullingMask = LayerMask.GetMask("Default", "Enemies", "Ground");
            
            if (playerTransform != null)
            {
                minimapCamera.transform.position = playerTransform.position + Vector3.up * 100f;
            }
            else
            {
                minimapCamera.transform.position = Vector3.up * 100f;
            }
            
            minimapCamera.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
            minimapCamera.depth = -1;
            minimapCamera.gameObject.tag = "MinimapCamera";
        }
        
        if (minimapRenderTexture == null)
        {
            minimapRenderTexture = new RenderTexture(512, 512, 16);
            minimapRenderTexture.name = "MinimapRT";
        }
        
        minimapCamera.targetTexture = minimapRenderTexture;
        minimapCamera.enabled = false; 
        
        if (minimapDisplay != null)
        {
            minimapDisplay.texture = minimapRenderTexture;
        }
        
        if (showDebugInfo)
        {
            Debug.Log("[Artillery] Minimap Camera настроена");
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.V))
        {
            if (isSelectingTarget)
            {
                CancelStrikeSelection();
            }
            else if (!isStrikeInProgress && availableStrikes > 0)
            {
                StartStrikeSelection();
            }
            else if (availableStrikes <= 0)
            {
                Debug.Log("[Artillery] Нет доступных ударов!");
            }
        }
        
        if (isSelectingTarget)
        {
            UpdateTargetSelection();
            
            if (Input.GetMouseButtonDown(0))
            {
                ConfirmStrike();
            }
            
            if (Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.Escape))
            {
                CancelStrikeSelection();
            }
        }
    }

    void StartStrikeSelection()
    {
        isSelectingTarget = true;
        
        previousCursorMode = Cursor.lockState;
        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = true;
        
        if (playerMovement != null)
        {
            playerMovement.LockControls(lockCamera: true);
        }
        
        if (weaponManager != null)
        {
            weaponManager.enabled = false;
        }
        
        if (artilleryCanvas != null)
        {
            artilleryCanvas.gameObject.SetActive(true);
        }
        
        if (minimapCamera != null)
        {
            minimapCamera.enabled = true;
        }
        
        if (animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
        }
        animationCoroutine = StartCoroutine(AnimateMinimap(true));
        
        if (playerAnimator != null)
        {
            playerAnimator.CrossFade(callStrikeAnimation, 0.2f);
            StartCoroutine(WaitForCallAnimation());
        }
        
        if (targetCursor != null)
        {
            targetCursor.gameObject.SetActive(true);
        }
        
        if (audioSource != null && callStrikeSound != null)
        {
            audioSource.PlayOneShot(callStrikeSound);
        }
        
        if (showDebugInfo) Debug.Log("[Artillery] Режим выбора цели активирован");
    }

    IEnumerator WaitForCallAnimation()
    {
        yield return new WaitForSeconds(1f);
        
        if (playerAnimator != null && isSelectingTarget)
        {
            playerAnimator.CrossFade(idleStrikeAnimation, 0.2f);
        }
    }

    IEnumerator AnimateMinimap(bool open)
    {
        if (minimapPanel == null) yield break;
        
        Vector2 targetSize = open ? minimapOpenSize : minimapClosedSize;
        Vector2 targetPosition = open ? minimapOpenPosition : minimapClosedPosition;
        Vector2 startSize = minimapPanel.sizeDelta;
        Vector2 startPosition = minimapPanel.anchoredPosition;
        
        float elapsed = 0f;
        float duration = 1f / minimapAnimationSpeed;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            
            float interpolatedT = GetInterpolation(t, animationType);
            
            minimapPanel.sizeDelta = Vector2.Lerp(startSize, targetSize, interpolatedT);
            minimapPanel.anchoredPosition = Vector2.Lerp(startPosition, targetPosition, interpolatedT);
            
            yield return null;
        }
        
        minimapPanel.sizeDelta = targetSize;
        minimapPanel.anchoredPosition = targetPosition;
    }

    float GetInterpolation(float t, AnimationType type)
    {
        switch (type)
        {
            case AnimationType.Lerp:
                return t;
            
            case AnimationType.Smoothstep:
                return t * t * (3f - 2f * t);
            
            case AnimationType.EaseInOut:
                return t < 0.5f 
                    ? 2f * t * t 
                    : 1f - Mathf.Pow(-2f * t + 2f, 2f) / 2f;
            
            case AnimationType.EaseOut:
                return 1f - (1f - t) * (1f - t);
            
            default:
                return t;
        }
    }

    void UpdateTargetSelection()
    {
        if (minimapDisplay == null || minimapPanel == null || minimapCamera == null)
        {
            if (showDebugInfo) Debug.LogWarning("[Artillery] UpdateTargetSelection: некоторые компоненты null!");
            return;
        }
        
        Vector2 localPoint;
        bool isInsideMinimap = RectTransformUtility.ScreenPointToLocalPointInRectangle(
            minimapPanel, 
            Input.mousePosition, 
            artilleryCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : artilleryCanvas.worldCamera, 
            out localPoint
        );
        
        if (showDebugInfo)
        {
            Debug.Log($"[Artillery] Mouse screen pos: {Input.mousePosition}, Local point: {localPoint}");
        }
        
        Rect rect = minimapPanel.rect;
        bool cursorInBounds = rect.Contains(localPoint);
        
        Vector2 clampedPoint = new Vector2(
            Mathf.Clamp(localPoint.x, rect.xMin, rect.xMax),
            Mathf.Clamp(localPoint.y, rect.yMin, rect.yMax)
        );
        
        if (targetCursor != null)
        {
            targetCursor.rectTransform.anchoredPosition = clampedPoint;
            
            Color cursorColor = targetCursor.color;
            cursorColor.a = cursorInBounds ? 1f : 0.5f;
            targetCursor.color = cursorColor;
        }
        
        selectedWorldPosition = MinimapToWorldPosition(clampedPoint);
        
        if (showDebugInfo)
        {
            Debug.Log($"[Artillery] ★ ОБНОВЛЕНА целевая позиция: {selectedWorldPosition}");
        }
        
        UpdateStrikeMarker(selectedWorldPosition);
    }

    Vector3 MinimapToWorldPosition(Vector2 minimapPosition)
    {
        if (minimapCamera == null || minimapPanel == null)
        {
            return playerTransform != null ? playerTransform.position : Vector3.zero;
        }
        
        Rect rect = minimapPanel.rect;
        
        float normalizedX = (minimapPosition.x - rect.xMin) / rect.width;
        float normalizedY = (minimapPosition.y - rect.yMin) / rect.height;
        
        normalizedY = 1f - normalizedY;
        
        if (showDebugInfo)
        {
            Debug.Log($"[Artillery] Minimap pos: {minimapPosition}, Normalized: ({normalizedX}, {normalizedY})");
        }
        
        Ray ray = minimapCamera.ViewportPointToRay(new Vector3(normalizedX, normalizedY, 0));
        RaycastHit hit;
        
        if (showDebugInfo)
        {
            Debug.DrawRay(ray.origin, ray.direction * 1000f, Color.yellow, 2f);
        }
        
        if (Physics.Raycast(ray, out hit, 1000f))
        {
            if (showDebugInfo)
            {
                Debug.Log($"[Artillery] ✓ Raycast попал в: {hit.point} ({hit.collider.name})");
            }
            return hit.point;
        }
        
        float orthoWidth = minimapCamera.orthographicSize * minimapCamera.aspect;
        float orthoHeight = minimapCamera.orthographicSize;
        
        Vector3 cameraPos = minimapCamera.transform.position;
        
        float worldX = cameraPos.x + (normalizedX - 0.5f) * 2f * orthoWidth;
        float worldZ = cameraPos.z - (normalizedY - 0.5f) * 2f * orthoHeight;
        
        Vector3 fallbackPos = new Vector3(worldX, 0f, worldZ);
        
        if (Physics.Raycast(new Vector3(worldX, 1000f, worldZ), Vector3.down, out hit, 2000f))
        {
            fallbackPos.y = hit.point.y;
            if (showDebugInfo)
            {
                Debug.Log($"[Artillery] ✓ Fallback нашёл землю на высоте: {hit.point.y}");
            }
        }
        else
        {
            if (showDebugInfo)
            {
                Debug.LogWarning($"[Artillery] ✗ Fallback НЕ нашёл землю!");
            }
        }
        
        if (showDebugInfo)
        {
            Debug.Log($"[Artillery] → Финальная позиция: {fallbackPos}");
        }
        
        return fallbackPos;
    }

    void UpdateStrikeMarker(Vector3 position)
    {
        if (strikeMarkerPrefab != null)
        {
            if (currentStrikeMarker == null)
            {
                currentStrikeMarker = Instantiate(strikeMarkerPrefab, position, Quaternion.identity);
            }
            else
            {
                currentStrikeMarker.transform.position = position;
            }
        }
    }

    void ConfirmStrike()
    {
        if (minimapPanel != null && minimapCamera != null)
        {
            Vector2 localPoint;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                minimapPanel, 
                Input.mousePosition, 
                artilleryCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : artilleryCanvas.worldCamera, 
                out localPoint
            );
            
            Rect rect = minimapPanel.rect;
            Vector2 clampedPoint = new Vector2(
                Mathf.Clamp(localPoint.x, rect.xMin, rect.xMax),
                Mathf.Clamp(localPoint.y, rect.yMin, rect.yMax)
            );
            
            selectedWorldPosition = MinimapToWorldPosition(clampedPoint);
        }
        
        isSelectingTarget = false;
        availableStrikes--;
        
        if (showDebugInfo)
        {
            Debug.Log($"[Artillery] ========== УДАР ПОДТВЕРЖДЁН ==========");
            Debug.Log($"[Artillery] ФИНАЛЬНАЯ целевая позиция: {selectedWorldPosition} ★★★");
            Debug.Log($"[Artillery] Оставшиеся удары: {availableStrikes}");
        }
        
        if (audioSource != null && confirmSound != null)
        {
            audioSource.PlayOneShot(confirmSound);
        }
        
        if (animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
        }
        animationCoroutine = StartCoroutine(AnimateMinimap(false));
        
        if (targetCursor != null)
        {
            targetCursor.gameObject.SetActive(false);
        }
        
        if (currentStrikeMarker != null)
        {
            Destroy(currentStrikeMarker);
        }
        
        if (minimapCamera != null)
        {
            minimapCamera.enabled = false;
        }
        
        StartCoroutine(ReturnControlToPlayer());
        StartCoroutine(ExecuteStrike(selectedWorldPosition));
    }

    void CancelStrikeSelection()
    {
        isSelectingTarget = false;
        
        if (animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
        }
        animationCoroutine = StartCoroutine(AnimateMinimap(false));
        
        if (targetCursor != null)
        {
            targetCursor.gameObject.SetActive(false);
        }
        
        if (currentStrikeMarker != null)
        {
            Destroy(currentStrikeMarker);
        }
        
        if (minimapCamera != null)
        {
            minimapCamera.enabled = false;
        }
        
        StartCoroutine(ReturnControlToPlayer());
        
        if (showDebugInfo) Debug.Log("[Artillery] Выбор цели отменён");
    }

    IEnumerator ReturnControlToPlayer()
    {
        yield return new WaitForSeconds(0.5f);
        
        if (playerAnimator != null)
        {
            playerAnimator.CrossFade(idleAnimation, 0.3f);
        }
        
        Cursor.lockState = previousCursorMode;
        Cursor.visible = (previousCursorMode == CursorLockMode.None);
        
        if (playerMovement != null)
        {
            playerMovement.UnlockControls();
        }
        
        if (weaponManager != null)
        {
            weaponManager.enabled = true;
        }
        
        if (artilleryCanvas != null)
        {
            artilleryCanvas.gameObject.SetActive(false);
        }
    }

    IEnumerator ExecuteStrike(Vector3 targetPosition)
    {
        isStrikeInProgress = true;
        
        if (showDebugInfo)
        {
            Debug.Log($"[Artillery] ===== НАЧАЛО УДАРА =====");
            Debug.Log($"[Artillery] ★★★ Цель удара В EXECUTE: {targetPosition} ★★★");
        }
        
        if (audioSource != null && incomingSound != null)
        {
            audioSource.PlayOneShot(incomingSound);
        }
        
        yield return new WaitForSeconds(strikeDelay);
        
        Vector3 dropPosition = targetPosition + Vector3.up * bombFallHeight;
        
        if (showDebugInfo)
        {
            Debug.Log($"[Artillery] Сброс бомбы с позиции: {dropPosition}");
            Debug.Log($"[Artillery] На цель: {targetPosition}");
        }
        
        StartCoroutine(DropBomb(targetPosition, dropPosition));
        
        yield return new WaitForSeconds(2f);
        
        isStrikeInProgress = false;
        if (showDebugInfo) Debug.Log($"[Artillery] ===== КОНЕЦ УДАРА =====");
    }

    IEnumerator DropBomb(Vector3 targetPosition, Vector3 dropPosition)
    {
        GameObject bomb = null;
        
        if (bombPrefab != null)
        {
            bomb = Instantiate(bombPrefab, dropPosition, Quaternion.identity);
        }
        else
        {
            bomb = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            bomb.transform.position = dropPosition;
            bomb.transform.localScale = Vector3.one * 0.5f;
            bomb.GetComponent<Renderer>().material.color = Color.red;
        }
        
        if (audioSource != null && bombWhistleSound != null)
        {
            audioSource.PlayOneShot(bombWhistleSound);
        }
        
        float fallDistance = dropPosition.y - targetPosition.y;
        float fallTime = fallDistance / bombFallSpeed;
        float elapsed = 0f;
        
        while (elapsed < fallTime && bomb != null)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fallTime;
            
            bomb.transform.position = Vector3.Lerp(dropPosition, targetPosition, t);
            
            yield return null;
        }
        
        if (showDebugInfo)
        {
            Debug.Log($"[Artillery] Бомба достигла цели: {targetPosition}");
        }
        
        if (bomb != null) Destroy(bomb);
        
        yield return new WaitForSeconds(bombExplosionDelay);
        
        SpawnExplosion(targetPosition);
    }

    void SpawnExplosion(Vector3 position)
    {
        if (showDebugInfo) Debug.Log($"[Artillery] ВЗРЫВ на позиции: {position}");
        
        if (strikeVFX != null)
        {
            strikeVFX.transform.position = position;
            strikeVFX.Play();
        }
        else if (strikeVFXPrefab != null)
        {
            GameObject vfxObject = Instantiate(strikeVFXPrefab, position, Quaternion.identity);
            
            VisualEffect vfx = vfxObject.GetComponent<VisualEffect>();
            if (vfx != null) vfx.Play();
            
            ParticleSystem ps = vfxObject.GetComponent<ParticleSystem>();
            if (ps != null) ps.Play();
            
            Destroy(vfxObject, 5f);
        }
        
        if (audioSource != null && explosionSound != null)
        {
            audioSource.PlayOneShot(explosionSound);
        }
        
        Collider[] hitEnemies = Physics.OverlapSphere(position, strikeRadius, enemyLayer);
        if (showDebugInfo) Debug.Log($"[Artillery] Врагов в радиусе поражения: {hitEnemies.Length}");
        
        foreach (Collider enemy in hitEnemies)
        {
            float distance = Vector3.Distance(position, enemy.transform.position);
            float damageMultiplier = 1f - (distance / strikeRadius);
            float finalDamage = strikeDamage * damageMultiplier;
            
            BaseEnemy baseEnemy = enemy.GetComponent<BaseEnemy>();
            if (baseEnemy != null)
            {
                baseEnemy.TakeDamage(finalDamage);
                if (showDebugInfo) Debug.Log($"[Artillery] ⚔️ Урон {finalDamage:F0} → {enemy.name}");
            }
        }
    }

    public void AddStrike(int amount = 1)
    {
        availableStrikes += amount;
        if (showDebugInfo) Debug.Log($"[Artillery] +{amount} удар(ов). Всего: {availableStrikes}");
    }

    public int GetAvailableStrikes()
    {
        return availableStrikes;
    }

    public bool CanUseWeapons()
    {
        return !isSelectingTarget && !isStrikeInProgress;
    }

    void OnDrawGizmosSelected()
    {
        if (!drawGizmos) return;
        
        if (isSelectingTarget)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(selectedWorldPosition, strikeRadius);
            
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(selectedWorldPosition, selectedWorldPosition + Vector3.up * bombFallHeight);
        }
    }
}