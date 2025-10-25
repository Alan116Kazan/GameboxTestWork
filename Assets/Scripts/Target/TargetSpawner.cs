using System.Collections;
using UnityEngine;

/// <summary>
/// �������� �������� �������. ������ ���� ��������� � �������, ������� ������� � �����.
/// ����� ������� ScheduleRespawn(position, rotation, delay) ��� �������� ��������� � ���� �����.
/// </summary>
[DisallowMultipleComponent]
public class TargetSpawner : MonoBehaviour
{
    #region Serialized Fields

    [Header("Default prefab (optional)")]
    [Tooltip("���� �� ��������� � ������ ScheduleRespawn, ����� ����������� ���� prefab.")]
    [SerializeField] private GameObject prefabToSpawn;

    #endregion

    #region Public Methods

    /// <summary>
    /// ����� ������� ��������� prefab � ��������� �������/�������.
    /// </summary>
    /// <param name="position">������� ������.</param>
    /// <param name="rotation">������� ������.</param>
    /// <param name="parent">������������ ��������.</param>
    /// <returns>��������� ������, ���� null ��� ������.</returns>
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
    /// ������������� ������� ����� delay ������. ��������� �������� �� ���� �������.
    /// ���������� Coroutine (����� �������� ����� StopCoroutine).
    /// </summary>
    /// <param name="position">������� ������.</param>
    /// <param name="rotation">������� ������.</param>
    /// <param name="delay">�������� ����� �������.</param>
    /// <param name="parent">������������ ��������.</param>
    /// <param name="prefabOverride">������������ ������ ������ prefabToSpawn.</param>
    /// <returns>Coroutine, ������� ��������� �������.</returns>
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
    /// ��������, ������� ��� delay ������ � ������ ��������� prefab.
    /// </summary>
    private IEnumerator RespawnCoroutine(GameObject prefab, Vector3 position, Quaternion rotation, float delay, Transform parent)
    {
        yield return new WaitForSeconds(delay);
        Instantiate(prefab, position, rotation, parent);
    }

    #endregion
}
