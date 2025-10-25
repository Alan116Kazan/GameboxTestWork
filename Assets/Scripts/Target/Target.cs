using UnityEngine;

/// <summary>
/// ������ ������ ��� ��������������/�������� ��������.
/// ��� ��������� ���������� ����� RegisterHit(RaycastHit), ������� ��������� ���� � ��������� �������.
/// </summary>
[DisallowMultipleComponent]
public class Target : MonoBehaviour
{
    #region Serialized Fields

    [Header("Target settings")]
    [Tooltip("���������� ���������, ����������� ��� ����������� ����.")]
    [SerializeField, Min(1)] private int requiredHits = 5;

    [Tooltip("�������� ����� ��������� ���� ����� �����������.")]
    [SerializeField, Min(0f)] private float respawnDelay = 3f;

    [Header("Spawner (optional)")]
    [Tooltip("��������� ��awner ��� ��������������� ��������. ���� �� ������ � ������ �������������.")]
    [SerializeField] private TargetSpawner spawner;

    #endregion

    #region Private Fields

    // ������� ���������� ��������� �� ����
    private int currentHits = 0;

    #endregion

    #region Unity Methods

    private void Awake()
    {
        // ���� ��awner �� �������� � ����������, ���� ��� ����� ���������
        if (spawner == null)
        {
            spawner = GetComponentInParent<TargetSpawner>();
        }

        // ���� ������������ ��awner �� ������ � ���� � �����
        if (spawner == null)
        {
            spawner = FindObjectOfType<TargetSpawner>();
        }
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// ������������ ��������� �� ����. ���������� �������� WeaponHitRouter.
    /// </summary>
    /// <param name="hit">���������� � ��������� (RaycastHit), ����� ������������ ��� VFX/SFX).</param>
    public void RegisterHit(RaycastHit hit)
    {
        currentHits++;
        Debug.Log($"{name} registered hit {currentHits}/{requiredHits} at {hit.point}");

        // ����� ����� ����������� ������� ���������.

        if (currentHits >= requiredHits)
        {
            DieAndRequestRespawn();
        }
    }

    /// <summary>
    /// ���������� ��������� ����, ������� ��� ������������� ����� ��������.
    /// </summary>
    public void ResetState()
    {
        currentHits = 0;
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// ���������� ���� � ����������� � ������� ����� TargetSpawner.
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
            Debug.LogWarning($"Target ({name}) has no spawner assigned/found � it will be destroyed without respawn.");
        }

        Destroy(gameObject);
    }

    #endregion
}
