using System;
using UnityEngine;

/// <summary>
/// Одна ячейка инвентаря.
/// Слот может быть пустым (Item == null) или содержать предмет с определённым количеством.
/// Все изменения слота вызывают событие OnSlotChanged для UI или логики игры.
/// </summary>
[Serializable]
public class InventorySlot
{
    [Header("Slot Data")]
    [SerializeField] private InventoryItemSO item;  // Предмет в слоте (null, если пустой)
    [SerializeField] private int quantity;          // Количество предметов в слоте

    /// <summary>Предмет в ячейке. Null, если слот пустой.</summary>
    public InventoryItemSO Item => item;

    /// <summary>Количество предметов в слоте.</summary>
    public int Quantity => quantity;

    /// <summary>Событие вызывается при изменении содержимого слота.</summary>
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

    /// <summary>Проверяет, пустой ли слот.</summary>
    public bool IsEmpty() => item == null || quantity <= 0;

    /// <summary>Устанавливает предмет и количество. Если qty = 0, слот очищается.</summary>
    public void Set(InventoryItemSO newItem, int qty)
    {
        item = newItem;
        quantity = Mathf.Max(0, qty);

        if (quantity == 0)
            item = null;

        OnSlotChanged?.Invoke(this);
    }

    /// <summary>
    /// Добавляет предметы в слот.
    /// Возвращает фактически добавленное количество.
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
    /// Удаляет указанное количество предметов из слота.
    /// Возвращает фактически удалённое количество.
    /// Если количество падает до 0, слот очищается.
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

    /// <summary>Полностью очищает слот.</summary>
    public void Clear()
    {
        ClearInternal();
        OnSlotChanged?.Invoke(this);
    }

    #endregion

    #region Private Helpers

    /// <summary>Внутренняя очистка без вызова события.</summary>
    private void ClearInternal()
    {
        item = null;
        quantity = 0;
    }

    #endregion
}
