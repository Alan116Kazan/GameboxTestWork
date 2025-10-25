using UnityEngine;

/// <summary>
/// Интерфейс, описывающий базовый предмет инвентаря.
/// Используется для ScriptableObject или игровых объектов.
/// </summary>
public interface IInventoryItem
{
    /// <summary>
    /// Уникальный идентификатор предмета (например, "pistol_ammo").
    /// Используется для поиска и сравнения предметов.
    /// </summary>
    string Id { get; }

    /// <summary>
    /// Отображаемое имя предмета для UI.
    /// </summary>
    string DisplayName { get; }

    /// <summary>
    /// Иконка предмета для отображения в UI.
    /// </summary>
    Sprite Icon { get; }

    /// <summary>
    /// Префаб предмета для спавна в мире.
    /// </summary>
    GameObject Prefab { get; }

    /// <summary>
    /// Можно ли складывать предметы в один стек.
    /// </summary>
    bool IsStackable { get; }

    /// <summary>
    /// Максимальное количество предметов в одном стеке.
    /// </summary>
    int MaxStack { get; }
}
