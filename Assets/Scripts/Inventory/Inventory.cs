using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Менеджер инвентаря.
/// Важное отличие: слоты фиксируются по длине maxSlots.
/// Пустые слоты представлены InventorySlot с Item == null,
/// что упрощает взаимодействие с UI (индексы всегда стабильны).
/// </summary>
[DisallowMultipleComponent]
public class Inventory : MonoBehaviour
{
    #region Inspector Fields

    [Header("Inventory Settings")]
    [Tooltip("Максимальное количество слотов в инвентаре")]
    [SerializeField, Min(1)] private int maxSlots = 12;

    [Tooltip("Список слотов. Пустые слоты содержат InventorySlot с Item == null")]
    [SerializeField] private List<InventorySlot> slots = new List<InventorySlot>();

    #endregion

    #region Private Fields

    /// <summary>
    /// Словарь для быстрого подсчёта количества предметов: itemId -> total в инвентаре
    /// </summary>
    private readonly Dictionary<string, int> totals = new(StringComparer.Ordinal);

    #endregion

    #region Events

    /// <summary>
    /// Вызывается при изменении инвентаря (добавление/удаление предметов)
    /// </summary>
    public event Action OnInventoryChanged;

    #endregion

    #region Properties

    /// <summary>Максимальное количество слотов</summary>
    public int MaxSlots => maxSlots;

    /// <summary>Список слотов (только для чтения)</summary>
    public IReadOnlyList<InventorySlot> Slots => slots;

    #endregion

    #region Unity Callbacks

    private void Awake()
    {
        EnsureSlotCount();
        RebuildTotals();
    }

    private void OnValidate()
    {
        if (maxSlots < 1) maxSlots = 1;
        EnsureSlotCount();
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Обеспечивает правильное количество слотов (maxSlots).
    /// Пустые слоты создаются автоматически.
    /// </summary>
    private void EnsureSlotCount()
    {
        if (slots == null) slots = new List<InventorySlot>();

        while (slots.Count < maxSlots)
            slots.Add(new InventorySlot());

        while (slots.Count > maxSlots)
            slots.RemoveAt(slots.Count - 1);
    }

    /// <summary>
    /// Пересчитывает totals по текущим слотам.
    /// </summary>
    private void RebuildTotals()
    {
        totals.Clear();
        foreach (var slot in slots)
        {
            if (slot == null || slot.Item == null) continue;
            totals.TryGetValue(slot.Item.Id, out int cur);
            totals[slot.Item.Id] = cur + slot.Quantity;
        }
    }

    /// <summary>Вызов события OnInventoryChanged.</summary>
    private void FireInventoryChanged() => OnInventoryChanged?.Invoke();

    #endregion

    #region Public Methods

    /// <summary>
    /// Попытка добавить предмет. Возвращает остаток, который не удалось добавить.
    /// </summary>
    public int TryAddItemReturnLeftover(InventoryItemSO item, int amount = 1)
    {
        if (item == null || amount <= 0) return amount;

        int remaining = amount;
        bool changed = false;
        string id = item.Id;

        // 1) Если stackable — заполнить существующие стеки
        if (item.IsStackable)
        {
            foreach (var slot in slots)
            {
                if (remaining <= 0) break;
                if (slot.IsEmpty() || slot.Item.Id != id) continue;

                int added = slot.Add(remaining, item.MaxStack);
                remaining -= added;
                if (added > 0) changed = true;
            }
        }

        // 2) Занять пустые слоты
        foreach (var slot in slots)
        {
            if (remaining <= 0) break;
            if (!slot.IsEmpty()) continue;

            if (item.IsStackable)
            {
                int create = Mathf.Min(item.MaxStack, remaining);
                slot.Set(item, create);
                remaining -= create;
                changed = true;
            }
            else
            {
                slot.Set(item, 1);
                remaining -= 1;
                changed = true;
            }
        }

        // Обновление totals и вызов события
        if (changed)
        {
            totals.TryGetValue(id, out int cur);
            totals[id] = cur + (amount - remaining);
            FireInventoryChanged();
        }

        return remaining;
    }

    /// <summary>
    /// Добавление предмета с булевым результатом (успешно добавлено всё или нет)
    /// </summary>
    public bool AddItem(InventoryItemSO item, int amount = 1)
    {
        return TryAddItemReturnLeftover(item, amount) == 0;
    }

    /// <summary>
    /// Удаляет указанное количество предметов по itemId. 
    /// Возвращает true, если удалено нужное количество.
    /// </summary>
    public bool RemoveItem(string itemId, int amount = 1)
    {
        if (string.IsNullOrEmpty(itemId) || amount <= 0) return false;
        if (!totals.TryGetValue(itemId, out int total) || total < amount) return false;

        int needed = amount;
        for (int i = slots.Count - 1; i >= 0 && needed > 0; i--)
        {
            var slot = slots[i];
            if (slot.IsEmpty() || slot.Item.Id != itemId) continue;

            int removed = slot.Remove(needed);
            needed -= removed;
        }

        int newTotal = total - amount;
        if (newTotal <= 0) totals.Remove(itemId);
        else totals[itemId] = newTotal;

        FireInventoryChanged();
        return true;
    }

    /// <summary>Удаляет до amount из слота index. Возвращает фактически удалённое количество.</summary>
    public int RemoveFromSlotAt(int index, int amount = 1)
    {
        if (index < 0 || index >= slots.Count) return 0;

        var slot = slots[index];
        if (slot.IsEmpty()) return 0;

        int removed = slot.Remove(amount);
        if (removed > 0)
        {
            RebuildTotals(); // безопасный способ обновить totals
            FireInventoryChanged();
        }
        return removed;
    }

    /// <summary>Очищает слот index, возвращает количество удалённых предметов.</summary>
    public int ClearSlotAt(int index)
    {
        if (index < 0 || index >= slots.Count) return 0;

        var slot = slots[index];
        if (slot.IsEmpty()) return 0;

        int removed = slot.Quantity;
        slot.Clear();
        RebuildTotals();
        FireInventoryChanged();
        return removed;
    }

    /// <summary>Проверяет наличие предмета в количестве amount.</summary>
    public bool HasItem(string itemId, int amount = 1)
    {
        if (string.IsNullOrEmpty(itemId)) return false;
        totals.TryGetValue(itemId, out int v);
        return v >= amount;
    }

    /// <summary>Количество слотов в инвентаре</summary>
    public int GetSlotCount() => slots.Count;

    /// <summary>Возвращает слот по индексу</summary>
    public InventorySlot GetSlotAt(int index)
    {
        if (index < 0 || index >= slots.Count) return null;
        return slots[index];
    }

    /// <summary>Общее количество предметов itemId в инвентаре</summary>
    public int GetTotalQuantity(string itemId)
    {
        if (string.IsNullOrEmpty(itemId)) return 0;
        totals.TryGetValue(itemId, out int v);
        return v;
    }

    /// <summary>Удобный вызов добавления патронов с возвратом остатка</summary>
    public int TryAddAmmoReturnLeftover(InventoryItemSO ammoSO, int amount)
    {
        return TryAddItemReturnLeftover(ammoSO, amount);
    }

    #endregion
}
