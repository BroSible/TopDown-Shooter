using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.VFX;

public class ArtilleryStrike : MonoBehaviour
{
    [Header("UI References")]
    public RectTransform mapPanel;
    public RawImage mapImage;
    public Image targetCursor;
    public Canvas artilleryCanvas;
    
    [Header("Map Icons")]
    public GameObject playerIconPrefab;
    public GameObject enemyIconPrefab;
    public float mapUpdateInterval = 0.1f;
    
    [Header("Map Settings")]
    public Vector2 mapInitialSize = new Vector2(200f, 200f);
    public Vector2 mapExpandedSize = new Vector2(600f, 600f);
    public Vector2 mapInitialPosition = new Vector2(-100f, -100f);
    public Vector2 mapExpandedPosition = new Vector2(-300f, -300f);
    public float mapScaleSpeed = 5f;
    
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
    
    [Header("Ship Settings")]
    public GameObject shipPrefab;
    public float shipSpeed = 50f;
    public float shipHeight = 50f;
    public float shipApproachDistance = 100f;
    
    [Header("Bomb Settings")]
    public float bombFallSpeed = 20f;
    public float bombExplosionDelay = 0.1f;
    public bool useBombPhysics = false; // Отключаем физику по умолчанию для точности
    
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
    
    [Header("World Bounds")]
    public Transform worldCenter;
    public float worldSize = 100f;
    
    [Header("Debug")]
    public bool showDebugInfo = true;
    public bool drawGizmos = true;
    
    public bool isSelectingTarget = false;
    private bool isStrikeInProgress = false;
    private Vector3 selectedWorldPosition;
    private Camera mainCamera;
    private bool isMapExpanding = false;
    private CursorLockMode previousCursorMode;
    private GameObject currentStrikeMarker;
    
    private GameObject playerIcon;
    private List<GameObject> enemyIcons = new List<GameObject>();
    private float mapUpdateTimer = 0f;
    
    void Start()
    {
        mainCamera = Camera.main;
        
        if (playerTransform == null)
        {
            playerTransform = transform;
            Debug.LogWarning("[Artillery] Player Transform не назначен, использую transform скрипта");
        }
        
        if (artilleryCanvas != null)
        {
            artilleryCanvas.gameObject.SetActive(false);
        }
        
        if (mapPanel != null)
        {
            mapPanel.sizeDelta = mapInitialSize;
            mapPanel.anchoredPosition = mapInitialPosition;
        }
        
        if (targetCursor != null)
        {
            targetCursor.gameObject.SetActive(false);
        }

        if (showDebugInfo)
        {
            Debug.Log($"[Artillery] Инициализация. Доступно ударов: {availableStrikes}");
            Debug.Log($"[Artillery] World Center: {(worldCenter != null ? worldCenter.position.ToString() : "NULL")}");
            Debug.Log($"[Artillery] World Size: {worldSize}");
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
            UpdateMapIcons();
            
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
        Cursor.lockState = CursorLockMode.None;
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
        
        StartCoroutine(AnimateMapExpand(true));
        
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
        
        CreateMapIcons();
        
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

    IEnumerator AnimateMapExpand(bool expand)
    {
        isMapExpanding = true;
        
        Vector2 targetSize = expand ? mapExpandedSize : mapInitialSize;
        Vector2 targetPosition = expand ? mapExpandedPosition : mapInitialPosition;
        
        while (Vector2.Distance(mapPanel.sizeDelta, targetSize) > 1f)
        {
            mapPanel.sizeDelta = Vector2.Lerp(mapPanel.sizeDelta, targetSize, Time.deltaTime * mapScaleSpeed);
            mapPanel.anchoredPosition = Vector2.Lerp(mapPanel.anchoredPosition, targetPosition, Time.deltaTime * mapScaleSpeed);
            yield return null;
        }
        
        mapPanel.sizeDelta = targetSize;
        mapPanel.anchoredPosition = targetPosition;
        
        isMapExpanding = false;
    }

    void UpdateTargetSelection()
    {
        if (isMapExpanding) return;
        
        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            mapPanel, 
            Input.mousePosition, 
            artilleryCanvas.worldCamera, 
            out localPoint
        );
        
        Vector2 clampedPoint = new Vector2(
            Mathf.Clamp(localPoint.x, -mapPanel.sizeDelta.x / 2, mapPanel.sizeDelta.x / 2),
            Mathf.Clamp(localPoint.y, -mapPanel.sizeDelta.y / 2, mapPanel.sizeDelta.y / 2)
        );
        
        if (targetCursor != null)
        {
            targetCursor.rectTransform.anchoredPosition = clampedPoint;
        }
        
        selectedWorldPosition = MapToWorldPosition(clampedPoint);
        
        UpdateStrikeMarker(selectedWorldPosition);
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

    Vector3 MapToWorldPosition(Vector2 mapPosition)
    {
        // УПРОЩЕННОЕ преобразование: -1 до 1 напрямую в мировые координаты
        float normalizedX = mapPosition.x / (mapPanel.sizeDelta.x / 2);
        float normalizedY = mapPosition.y / (mapPanel.sizeDelta.y / 2);
        
        Vector3 centerPos = worldCenter != null ? worldCenter.position : Vector3.zero;
        
        Vector3 worldPos = new Vector3(
            centerPos.x + (normalizedX * worldSize / 2),
            0,
            centerPos.z + (normalizedY * worldSize / 2)
        );
        
        if (showDebugInfo)
        {
            Debug.Log($"[Artillery Map→World] Map({mapPosition.x:F1}, {mapPosition.y:F1}) → Norm({normalizedX:F2}, {normalizedY:F2}) → World{worldPos}");
        }
        
        return worldPos;
    }

    Vector2 WorldToMapPosition(Vector3 worldPosition)
    {
        Vector3 centerPos = worldCenter != null ? worldCenter.position : Vector3.zero;
        
        // Смещение относительно центра мира
        float offsetX = worldPosition.x - centerPos.x;
        float offsetZ = worldPosition.z - centerPos.z;
        
        // Нормализация: -1 до 1
        float normalizedX = offsetX / (worldSize / 2);
        float normalizedY = offsetZ / (worldSize / 2);
        
        // Ограничиваем диапазон
        normalizedX = Mathf.Clamp(normalizedX, -1f, 1f);
        normalizedY = Mathf.Clamp(normalizedY, -1f, 1f);
        
        // Преобразуем в координаты карты
        Vector2 mapPos = new Vector2(
            normalizedX * (mapPanel.sizeDelta.x / 2),
            normalizedY * (mapPanel.sizeDelta.y / 2)
        );
        
        return mapPos;
    }

    void CreateMapIcons()
    {
        // Создаем иконку игрока
        if (playerTransform != null)
        {
            if (playerIconPrefab != null)
            {
                playerIcon = Instantiate(playerIconPrefab, mapPanel);
            }
            else
            {
                playerIcon = new GameObject("PlayerIcon");
                playerIcon.transform.SetParent(mapPanel);
                Image img = playerIcon.AddComponent<Image>();
                img.color = Color.green;
                RectTransform rt = playerIcon.GetComponent<RectTransform>();
                rt.sizeDelta = new Vector2(20, 20);
            }
            
            if (showDebugInfo) Debug.Log("[Artillery] Иконка игрока создана");
        }
    }

    void UpdateMapIcons()
    {
        mapUpdateTimer += Time.deltaTime;
        if (mapUpdateTimer < mapUpdateInterval) return;
        mapUpdateTimer = 0f;
        
        // Обновляем позицию иконки игрока
        if (playerIcon != null && playerTransform != null)
        {
            Vector2 mapPos = WorldToMapPosition(playerTransform.position);
            RectTransform rt = playerIcon.GetComponent<RectTransform>();
            if (rt != null)
            {
                rt.anchoredPosition = mapPos;
            }
        }
        
        // Обновляем иконки врагов
        UpdateEnemyIcons();
    }

    void UpdateEnemyIcons()
    {
        // Удаляем старые иконки
        foreach (GameObject icon in enemyIcons)
        {
            if (icon != null) Destroy(icon);
        }
        enemyIcons.Clear();
        
        // ИСПРАВЛЕНО: Используем FindObjectsOfType для поиска всех BaseEnemy
        BaseEnemy[] allEnemies = FindObjectsOfType<BaseEnemy>();
        
        if (showDebugInfo)
        {
            Debug.Log($"[Artillery] Найдено врагов через FindObjectsOfType: {allEnemies.Length}");
        }
        
        foreach (BaseEnemy enemy in allEnemies)
        {
            if (enemy == null || enemy.gameObject == null) continue;
            
            // Проверяем активность объекта вместо _isDead
            if (!enemy.gameObject.activeInHierarchy) continue;
            
            Vector2 mapPos = WorldToMapPosition(enemy.transform.position);
            
            // Проверяем что враг в пределах карты
            if (Mathf.Abs(mapPos.x) <= mapPanel.sizeDelta.x / 2 && 
                Mathf.Abs(mapPos.y) <= mapPanel.sizeDelta.y / 2)
            {
                GameObject enemyIcon;
                
                if (enemyIconPrefab != null)
                {
                    enemyIcon = Instantiate(enemyIconPrefab, mapPanel);
                }
                else
                {
                    enemyIcon = new GameObject("EnemyIcon");
                    enemyIcon.transform.SetParent(mapPanel);
                    Image img = enemyIcon.AddComponent<Image>();
                    img.color = Color.red;
                    RectTransform rt = enemyIcon.GetComponent<RectTransform>();
                    rt.sizeDelta = new Vector2(15, 15);
                }
                
                RectTransform iconRT = enemyIcon.GetComponent<RectTransform>();
                if (iconRT != null)
                {
                    iconRT.anchoredPosition = mapPos;
                }
                
                enemyIcons.Add(enemyIcon);
            }
        }
        
        if (showDebugInfo)
        {
            Debug.Log($"[Artillery] Иконок врагов на карте: {enemyIcons.Count}");
        }
    }

    void ClearMapIcons()
    {
        if (playerIcon != null)
        {
            Destroy(playerIcon);
            playerIcon = null;
        }
        
        foreach (GameObject icon in enemyIcons)
        {
            if (icon != null) Destroy(icon);
        }
        enemyIcons.Clear();
    }

    void ConfirmStrike()
    {
        if (isMapExpanding) return;
        
        isSelectingTarget = false;
        availableStrikes--;
        
        if (showDebugInfo)
        {
            Debug.Log($"[Artillery] ========== УДАР ПОДТВЕРЖДЁН ==========");
            Debug.Log($"[Artillery] Целевая позиция: {selectedWorldPosition}");
            Debug.Log($"[Artillery] Оставшиеся удары: {availableStrikes}");
        }
        
        if (audioSource != null && confirmSound != null)
        {
            audioSource.PlayOneShot(confirmSound);
        }
        
        StartCoroutine(AnimateMapExpand(false));
        
        if (targetCursor != null)
        {
            targetCursor.gameObject.SetActive(false);
        }
        
        if (currentStrikeMarker != null)
        {
            Destroy(currentStrikeMarker);
        }
        
        ClearMapIcons();
        
        StartCoroutine(ReturnControlToPlayer());
        StartCoroutine(ExecuteStrike(selectedWorldPosition));
    }

    void CancelStrikeSelection()
    {
        isSelectingTarget = false;
        
        StartCoroutine(AnimateMapExpand(false));
        
        if (targetCursor != null)
        {
            targetCursor.gameObject.SetActive(false);
        }
        
        if (currentStrikeMarker != null)
        {
            Destroy(currentStrikeMarker);
        }
        
        ClearMapIcons();
        
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
            Debug.Log($"[Artillery] ФИНАЛЬНАЯ цель: {targetPosition}");
        }
        
        if (audioSource != null && incomingSound != null)
        {
            audioSource.PlayOneShot(incomingSound);
        }
        
        // УПРОЩЕНО: Корабль летит справа налево (или слева направо)
        Vector3 approachDirection = Vector3.right;
        Vector3 shipStartPos = targetPosition - (approachDirection * shipApproachDistance) + Vector3.up * shipHeight;
        Vector3 shipEndPos = targetPosition + (approachDirection * shipApproachDistance) + Vector3.up * shipHeight;
        
        if (showDebugInfo)
        {
            Debug.Log($"[Artillery] Корабль летит от {shipStartPos} до {shipEndPos}");
            Debug.Log($"[Artillery] Расстояние полёта: {Vector3.Distance(shipStartPos, shipEndPos):F1}м");
        }
        
        GameObject ship = null;
        GameObject shipSpawnPoint = null;
        GameObject bombSpawnPoint = null;
        
        if (shipPrefab != null)
        {
            Quaternion shipRotation = Quaternion.LookRotation(approachDirection);
            ship = Instantiate(shipPrefab, shipStartPos, shipRotation);
            
            // ДОБАВЛЕНО: Создаем spawn points на корабле автоматически
            shipSpawnPoint = new GameObject("ShipSpawnPoint");
            shipSpawnPoint.transform.SetParent(ship.transform);
            shipSpawnPoint.transform.localPosition = Vector3.zero;
            
            bombSpawnPoint = new GameObject("BombSpawnPoint");
            bombSpawnPoint.transform.SetParent(ship.transform);
            bombSpawnPoint.transform.localPosition = new Vector3(0, -2f, 0); // Чуть ниже корабля
            
            if (showDebugInfo) Debug.Log($"[Artillery] Корабль создан с spawn points");
        }
        else
        {
            Debug.LogError("[Artillery] Ship Prefab не назначен!");
        }
        
        float travelTime = (shipApproachDistance * 2) / shipSpeed;
        float elapsedTime = 0f;
        bool bombDropped = false;
        
        while (elapsedTime < travelTime)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / travelTime;
            
            if (ship != null)
            {
                ship.transform.position = Vector3.Lerp(shipStartPos, shipEndPos, t);
            }
            
            // ИСПРАВЛЕНО: Сбрасываем бомбу точно когда корабль над целью
            if (t >= 0.5f && !bombDropped && bombSpawnPoint != null)
            {
                bombDropped = true;
                if (showDebugInfo)
                {
                    Debug.Log($"[Artillery] СБРОС БОМБЫ!");
                    Debug.Log($"[Artillery] Позиция корабля: {ship.transform.position}");
                    Debug.Log($"[Artillery] Позиция spawn point: {bombSpawnPoint.transform.position}");
                    Debug.Log($"[Artillery] Цель на земле: {targetPosition}");
                }
                StartCoroutine(DropBomb(targetPosition, bombSpawnPoint));
            }
            
            yield return null;
        }
        
        if (showDebugInfo) Debug.Log("[Artillery] Корабль завершил полёт");
        
        if (ship != null)
        {
            Destroy(ship, 2f);
        }
        
        isStrikeInProgress = false;
        if (showDebugInfo) Debug.Log($"[Artillery] ===== КОНЕЦ УДАРА =====");
    }

    IEnumerator DropBomb(Vector3 targetPosition, GameObject spawnPoint)
    {
        // УПРОЩЕНО: Бомба стартует из spawn point корабля
        Vector3 bombStartPos = spawnPoint != null ? spawnPoint.transform.position : targetPosition + Vector3.up * shipHeight;
        
        if (showDebugInfo)
        {
            Debug.Log($"[Artillery] --- Падение бомбы ---");
            Debug.Log($"[Artillery] Старт бомбы: {bombStartPos}");
            Debug.Log($"[Artillery] Финиш бомбы: {targetPosition}");
            Debug.Log($"[Artillery] Расстояние падения: {Vector3.Distance(bombStartPos, targetPosition):F1}м");
        }
        
        GameObject bomb = null;
        
        if (bombPrefab != null)
        {
            bomb = Instantiate(bombPrefab, bombStartPos, Quaternion.identity);
            
            if (useBombPhysics)
            {
                Rigidbody rb = bomb.GetComponent<Rigidbody>();
                if (rb == null) rb = bomb.AddComponent<Rigidbody>();
                rb.useGravity = true;
                rb.drag = 0.5f;
            }
        }
        else
        {
            bomb = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            bomb.transform.position = bombStartPos;
            bomb.transform.localScale = Vector3.one * 0.5f;
            bomb.GetComponent<Renderer>().material.color = Color.red;
        }
        
        if (audioSource != null && bombWhistleSound != null)
        {
            audioSource.PlayOneShot(bombWhistleSound);
        }
        
        // Анимированное падение (для точности)
        float fallDistance = bombStartPos.y - targetPosition.y;
        float fallTime = fallDistance / bombFallSpeed;
        float elapsed = 0f;
        
        Vector3 startPos = bombStartPos;
        
        while (elapsed < fallTime && bomb != null)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fallTime;
            
            // Прямое движение к цели
            bomb.transform.position = Vector3.Lerp(startPos, targetPosition, t);
            
            yield return null;
        }
        
        // Финальная позиция - ТОЧНО цель
        Vector3 finalImpactPos = targetPosition;
        
        if (showDebugInfo)
        {
            Debug.Log($"[Artillery] Бомба достигла цели!");
            Debug.Log($"[Artillery] Финальная позиция удара: {finalImpactPos}");
        }
        
        if (bomb != null) Destroy(bomb);
        
        yield return new WaitForSeconds(bombExplosionDelay);
        
        SpawnExplosion(finalImpactPos);
    }

    void SpawnExplosion(Vector3 position)
    {
        if (showDebugInfo) Debug.Log($"[Artillery] 💥 ВЗРЫВ на позиции: {position}");
        
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
        else
        {
            GameObject tempExplosion = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            tempExplosion.transform.position = position;
            tempExplosion.transform.localScale = Vector3.one * strikeRadius * 2;
            tempExplosion.GetComponent<Renderer>().material.color = new Color(1f, 0.5f, 0f, 0.5f);
            Destroy(tempExplosion, 1f);
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
            Gizmos.DrawLine(selectedWorldPosition, selectedWorldPosition + Vector3.up * 50);
        }
        
        if (worldCenter != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube(worldCenter.position, new Vector3(worldSize, 1, worldSize));
        }
    }
}