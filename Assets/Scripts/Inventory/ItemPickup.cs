using UnityEngine;

/// <summary>
/// Подбор предмета в мире.
/// Поддерживает стек предметов (amount).
/// </summary>
[RequireComponent(typeof(Collider))]
public class ItemPickup : MonoBehaviour
{
    [Header("Pickup Data")]
    [Tooltip("Предмет для подбора (ScriptableObject).")]
    public InventoryItemSO item;

    [Tooltip("Количество предметов, которое даёт этот объект.")]
    public int amount = 1;

    private Collider cachedCollider;

    #region Unity Methods

    private void Awake()
    {
        // Кэшируем коллайдер и делаем его триггером
        cachedCollider = GetComponent<Collider>();
        if (cachedCollider != null)
            cachedCollider.isTrigger = true;
    }

    private void Reset()
    {
        // Автоматическая настройка коллайдера в инспекторе
        var c = GetComponent<Collider>();
        if (c != null)
            c.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        // Проверяем наличие компонента Inventory у объекта, который вошёл в триггер
        if (other.TryGetComponent<Inventory>(out var inv))
        {
            TryPickup(inv);
        }
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Настройка предмета и количества для этого объекта подбора.
    /// </summary>
    /// <param name="itemSO">ScriptableObject предмета</param>
    /// <param name="count">Количество предметов (минимум 1)</param>
    public void Setup(InventoryItemSO itemSO, int count)
    {
        item = itemSO;
        amount = Mathf.Max(1, count);
        gameObject.SetActive(true);
    }

    /// <summary>
    /// Пытается добавить предмет в указанный инвентарь.
    /// Если удалось добавить всё количество — объект уничтожается.
    /// Если остались предметы — обновляется amount.
    /// </summary>
    /// <param name="inv">Целевой инвентарь</param>
    /// <returns>true, если объект был уничтожен, false — если остались предметы</returns>
    public bool TryPickup(Inventory inv)
    {
        if (inv == null || item == null || amount <= 0)
            return false;

        // Пытаемся добавить предметы в инвентарь
        int leftover = inv.TryAddItemReturnLeftover(item, amount);

        if (leftover == 0)
        {
            // Всё добавлено — уничтожаем объект в мире
            Destroy(gameObject);
            return true;
        }
        else
        {
            // Обновляем оставшееся количество предметов
            amount = leftover;
            return false;
        }
    }

    #endregion
}
