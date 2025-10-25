using UnityEngine;

/// <summary>
/// Префаб мишени для тренировочного/игрового полигона.
/// При попадании вызывается метод RegisterHit(RaycastHit), который учитывает урон и запускает события.
/// </summary>
[DisallowMultipleComponent]
public class Target : MonoBehaviour
{
    #region Serialized Fields

    [Header("Target settings")]
    [Tooltip("Количество попаданий, необходимых для уничтожения цели.")]
    [SerializeField, Min(1)] private int requiredHits = 5;

    [Tooltip("Задержка перед респавном цели после уничтожения.")]
    [SerializeField, Min(0f)] private float respawnDelay = 3f;

    [Header("Spawner (optional)")]
    [Tooltip("Ссылочный спawner для автоматического респавна. Если не указан — ищется автоматически.")]
    [SerializeField] private TargetSpawner spawner;

    #endregion

    #region Private Fields

    // Текущее количество попаданий по цели
    private int currentHits = 0;

    #endregion

    #region Unity Methods

    private void Awake()
    {
        // Если спawner не назначен в инспекторе, ищем его среди родителей
        if (spawner == null)
        {
            spawner = GetComponentInParent<TargetSpawner>();
        }

        // Если родительский спawner не найден — ищем в сцене
        if (spawner == null)
        {
            spawner = FindObjectOfType<TargetSpawner>();
        }
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Регистрирует попадание по цели. Вызывается роутером WeaponHitRouter.
    /// </summary>
    /// <param name="hit">Информация о попадании (RaycastHit), можно использовать для VFX/SFX).</param>
    public void RegisterHit(RaycastHit hit)
    {
        currentHits++;
        Debug.Log($"{name} registered hit {currentHits}/{requiredHits} at {hit.point}");

        // Здесь можно проигрывать эффекты попадания.

        if (currentHits >= requiredHits)
        {
            DieAndRequestRespawn();
        }
    }

    /// <summary>
    /// Сбрасывает состояние цели, полезно при использовании пулов объектов.
    /// </summary>
    public void ResetState()
    {
        currentHits = 0;
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Уничтожает цель и запрашивает её респавн через TargetSpawner.
    /// </summary>
    private void DieAndRequestRespawn()
    {
        Vector3 pos = transform.position;
        Quaternion rot = transform.rotation;
        Transform parent = transform.parent;

        if (spawner != null)
        {
            spawner.ScheduleRespawn(pos, rot, respawnDelay, parent);
        }
        else
        {
            Debug.LogWarning($"Target ({name}) has no spawner assigned/found — it will be destroyed without respawn.");
        }

        Destroy(gameObject);
    }

    #endregion
}
