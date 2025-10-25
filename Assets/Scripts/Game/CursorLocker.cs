using UnityEngine;
using System;

/// <summary>
/// ���������� �������� ��������� �������.
/// �������� �� ����������/������������� � �������/����������� �������.
/// ������������ ��� singleton: CursorLocker.Instance.
/// </summary>
[DisallowMultipleComponent]
public class CursorLocker : MonoBehaviour
{
    #region Singleton
    /// <summary>
    /// ������������ ��������� ��������� �������.
    /// ��������� ���������� � ����������� �� ������ �����.
    /// </summary>
    public static CursorLocker Instance { get; private set; }
    #endregion

    #region Inspector Fields
    [Header("��������� ��� ������")]
    [Tooltip("����������� ������ ��� ������")]
    [SerializeField] private bool lockOnStart = true;

    [Tooltip("�������� ������ ��� ������")]
    [SerializeField] private bool hideOnStart = true;
    #endregion

    #region Properties
    /// <summary>
    /// ������ � ��������� ���������� ������� ��� ������.
    /// ����� �������� � ��������.
    /// </summary>
    public bool LockOnStart
    {
        get => lockOnStart;
        set => lockOnStart = value;
    }

    /// <summary>
    /// ������ � ��������� ������� ������� ��� ������.
    /// ����� �������� � ��������.
    /// </summary>
    public bool HideOnStart
    {
        get => hideOnStart;
        set => hideOnStart = value;
    }
    #endregion

    #region Events
    /// <summary>
    /// ������� ���������� ��� ����� ��������� �������.
    /// bool ������ ��������: ������������ �� ������
    /// bool ������ ��������: ����� �� ������
    /// </summary>
    public event Action<bool, bool> OnCursorStateChanged;
    #endregion

    #region Unity Callbacks
    private void Awake()
    {
        // ���������� �������� Singleton
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); // ���� ��� ���� ���������, ���������� ����������� ������
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject); // ��������� ������ ����� �������
    }

    private void Start()
    {
        // ��������� ��������� ��������� �������
        Apply(lockOnStart, hideOnStart);
    }
    #endregion

    #region Public Methods
    /// <summary>
    /// ��������� ����� ��������� �������.
    /// </summary>
    /// <param name="shouldLock">����������� ������ (true) ��� ��� (false)</param>
    /// <param name="shouldHide">�������� ������ (true) ��� ���������� (false)</param>
    public void Apply(bool shouldLock, bool shouldHide)
    {
        Cursor.lockState = shouldLock ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !shouldHide;

        // ���������� ����������� � ����� ���������
        OnCursorStateChanged?.Invoke(shouldLock, shouldHide);
    }

    /// <summary>
    /// �������������� ������ � �������� ���.
    /// </summary>
    public void UnlockCursor() => Apply(false, false);

    /// <summary>
    /// ������������� ������ � ������ ���.
    /// </summary>
    public void LockCursor() => Apply(true, true);
    #endregion
}
