using UnityEngine;

/// <summary>
/// ScriptableObject для предмета инвентаря.
/// Реализует интерфейс IInventoryItem.
/// Позволяет создавать предметы с уникальным идентификатором, иконкой, префабом и настройками стека.
/// </summary>
[CreateAssetMenu(menuName = "Inventory/Item", fileName = "NewItem")]
public class InventoryItemSO : ScriptableObject, IInventoryItem
{
    #region Identity

    [Header("Identity")]
    [Tooltip("Уникальный идентификатор предмета (например, 'pistol_ammo').")]
    [SerializeField] private string id = "new_item";

    [Tooltip("Отображаемое имя предмета для UI.")]
    [SerializeField] private string displayName = "New Item";

    #endregion

    #region Visuals

    [Header("Visuals")]
    [Tooltip("Иконка предмета для UI.")]
    [SerializeField] private Sprite icon;

    [Tooltip("Префаб предмета для спавна в мире.")]
    [SerializeField] private GameObject prefab;

    #endregion

    #region Stack Settings

    [Header("Stack Settings")]
    [Tooltip("Можно ли складывать предметы в стек.")]
    [SerializeField] private bool isStackable = false;

    [Tooltip("Максимальный размер стека предмета.")]
    [SerializeField, Min(1)] private int maxStack = 1;

    #endregion

    #region IInventoryItem Implementation

    public string Id => id;
    public string DisplayName => displayName;
    public Sprite Icon => icon;
    public GameObject Prefab => prefab;
    public bool IsStackable => isStackable;
    public int MaxStack => maxStack;

    #endregion

    #region Validation

    /// <summary>
    /// Проверка корректности полей при изменении в инспекторе.
    /// </summary>
    private void OnValidate()
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            Debug.LogWarning($"InventoryItemSO '{name}' имеет пустой Id. Задайте уникальный Id.");
            id = name.ToLower().Replace(" ", "_");
        }

        if (maxStack < 1)
            maxStack = 1;
    }

    #endregion
}
