using System;
using UnityEngine;

/// <summary>
/// ���� ������ ���������.
/// ���� ����� ���� ������ (Item == null) ��� ��������� ������� � ����������� �����������.
/// ��� ��������� ����� �������� ������� OnSlotChanged ��� UI ��� ������ ����.
/// </summary>
[Serializable]
public class InventorySlot
{
    [Header("Slot Data")]
    [SerializeField] private InventoryItemSO item;  // ������� � ����� (null, ���� ������)
    [SerializeField] private int quantity;          // ���������� ��������� � �����

    /// <summary>������� � ������. Null, ���� ���� ������.</summary>
    public InventoryItemSO Item => item;

    /// <summary>���������� ��������� � �����.</summary>
    public int Quantity => quantity;

    /// <summary>������� ���������� ��� ��������� ����������� �����.</summary>
    public event Action<InventorySlot> OnSlotChanged;

    #region Constructors

    public InventorySlot()
    {
        item = null;
        quantity = 0;
    }

    public InventorySlot(InventoryItemSO item, int qty)
    {
        this.item = item;
        this.quantity = Mathf.Max(0, qty);
    }

    #endregion

    #region Slot Operations

    /// <summary>���������, ������ �� ����.</summary>
    public bool IsEmpty() => item == null || quantity <= 0;

    /// <summary>������������� ������� � ����������. ���� qty = 0, ���� ���������.</summary>
    public void Set(InventoryItemSO newItem, int qty)
    {
        item = newItem;
        quantity = Mathf.Max(0, qty);

        if (quantity == 0)
            item = null;

        OnSlotChanged?.Invoke(this);
    }

    /// <summary>
    /// ��������� �������� � ����.
    /// ���������� ���������� ����������� ����������.
    /// </summary>
    public int Add(int amount, int maxStack)
    {
        if (amount <= 0 || item == null || !item.IsStackable)
            return 0;

        int space = maxStack - quantity;
        int toAdd = Mathf.Min(space, amount);
        quantity += toAdd;

        OnSlotChanged?.Invoke(this);
        return toAdd;
    }

    /// <summary>
    /// ������� ��������� ���������� ��������� �� �����.
    /// ���������� ���������� �������� ����������.
    /// ���� ���������� ������ �� 0, ���� ���������.
    /// </summary>
    public int Remove(int amount)
    {
        if (amount <= 0 || item == null || quantity <= 0)
            return 0;

        int removed = Mathf.Min(amount, quantity);
        quantity -= removed;

        if (quantity <= 0)
            ClearInternal();

        OnSlotChanged?.Invoke(this);
        return removed;
    }

    /// <summary>��������� ������� ����.</summary>
    public void Clear()
    {
        ClearInternal();
        OnSlotChanged?.Invoke(this);
    }

    #endregion

    #region Private Helpers

    /// <summary>���������� ������� ��� ������ �������.</summary>
    private void ClearInternal()
    {
        item = null;
        quantity = 0;
    }

    #endregion
}
