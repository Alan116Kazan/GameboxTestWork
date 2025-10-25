using UnityEngine;

/// <summary>
/// ������ �������� � ����.
/// ������������ ���� ��������� (amount).
/// </summary>
[RequireComponent(typeof(Collider))]
public class ItemPickup : MonoBehaviour
{
    [Header("Pickup Data")]
    [Tooltip("������� ��� ������� (ScriptableObject).")]
    public InventoryItemSO item;

    [Tooltip("���������� ���������, ������� ��� ���� ������.")]
    public int amount = 1;

    private Collider cachedCollider;

    #region Unity Methods

    private void Awake()
    {
        // �������� ��������� � ������ ��� ���������
        cachedCollider = GetComponent<Collider>();
        if (cachedCollider != null)
            cachedCollider.isTrigger = true;
    }

    private void Reset()
    {
        // �������������� ��������� ���������� � ����������
        var c = GetComponent<Collider>();
        if (c != null)
            c.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        // ��������� ������� ���������� Inventory � �������, ������� ����� � �������
        if (other.TryGetComponent<Inventory>(out var inv))
        {
            TryPickup(inv);
        }
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// ��������� �������� � ���������� ��� ����� ������� �������.
    /// </summary>
    /// <param name="itemSO">ScriptableObject ��������</param>
    /// <param name="count">���������� ��������� (������� 1)</param>
    public void Setup(InventoryItemSO itemSO, int count)
    {
        item = itemSO;
        amount = Mathf.Max(1, count);
        gameObject.SetActive(true);
    }

    /// <summary>
    /// �������� �������� ������� � ��������� ���������.
    /// ���� ������� �������� �� ���������� � ������ ������������.
    /// ���� �������� �������� � ����������� amount.
    /// </summary>
    /// <param name="inv">������� ���������</param>
    /// <returns>true, ���� ������ ��� ���������, false � ���� �������� ��������</returns>
    public bool TryPickup(Inventory inv)
    {
        if (inv == null || item == null || amount <= 0)
            return false;

        // �������� �������� �������� � ���������
        int leftover = inv.TryAddItemReturnLeftover(item, amount);

        if (leftover == 0)
        {
            // �� ��������� � ���������� ������ � ����
            Destroy(gameObject);
            return true;
        }
        else
        {
            // ��������� ���������� ���������� ���������
            amount = leftover;
            return false;
        }
    }

    #endregion
}
