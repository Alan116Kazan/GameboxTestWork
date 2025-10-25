using System.Collections;
using UnityEngine;

/// <summary>
/// Менеджер респавна мишеней. Должен быть прикреплён к объекту, который остаётся в сцене.
/// Можно вызвать ScheduleRespawn(position, rotation, delay) для респавна конкретно в этой точке.
/// </summary>
[DisallowMultipleComponent]
public class TargetSpawner : MonoBehaviour
{
    #region Serialized Fields

    [Header("Default prefab (optional)")]
    [Tooltip("Если не указывать в вызове ScheduleRespawn, будет использован этот prefab.")]
    [SerializeField] private GameObject prefabToSpawn;

    #endregion

    #region Public Methods

    /// <summary>
    /// Сразу создать экземпляр prefab в указанной позиции/ротации.
    /// </summary>
    /// <param name="position">Позиция спавна.</param>
    /// <param name="rotation">Ротация спавна.</param>
    /// <param name="parent">Опциональный родитель.</param>
    /// <returns>Созданный объект, либо null при ошибке.</returns>
    public GameObject SpawnAt(Vector3 position, Quaternion rotation, Transform parent = null)
    {
        if (prefabToSpawn == null)
        {
            Debug.LogError("TargetSpawner.SpawnAt: prefabToSpawn is null.");
            return null;
        }

        return Instantiate(prefabToSpawn, position, rotation, parent);
    }

    /// <summary>
    /// Запланировать респавн через delay секунд. Запускает корутину на этом объекте.
    /// Возвращает Coroutine (можно отменить через StopCoroutine).
    /// </summary>
    /// <param name="position">Позиция спавна.</param>
    /// <param name="rotation">Ротация спавна.</param>
    /// <param name="delay">Задержка перед спавном.</param>
    /// <param name="parent">Опциональный родитель.</param>
    /// <param name="prefabOverride">Опциональный префаб вместо prefabToSpawn.</param>
    /// <returns>Coroutine, который выполняет респавн.</returns>
    public Coroutine ScheduleRespawn(Vector3 position, Quaternion rotation, float delay, Transform parent = null, GameObject prefabOverride = null)
    {
        var prefab = prefabOverride ?? prefabToSpawn;
        if (prefab == null)
        {
            Debug.LogError("TargetSpawner.ScheduleRespawn: prefab is null.");
            return null;
        }

        return StartCoroutine(RespawnCoroutine(prefab, position, rotation, delay, parent));
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Корутину, которая ждёт delay секунд и создаёт экземпляр prefab.
    /// </summary>
    private IEnumerator RespawnCoroutine(GameObject prefab, Vector3 position, Quaternion rotation, float delay, Transform parent)
    {
        yield return new WaitForSeconds(delay);
        Instantiate(prefab, position, rotation, parent);
    }

    #endregion
}
