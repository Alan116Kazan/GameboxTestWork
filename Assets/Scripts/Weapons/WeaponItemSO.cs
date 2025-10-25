using UnityEngine;

/// <summary>
/// ScriptableObject для оружия.
/// Наследуется от InventoryItemSO (то есть является предметом инвентаря),
/// содержит все настройки оружия: магазин, скорость стрельбы, тип боеприпасов, автомат/полуавтомат и префаб.
/// </summary>
[CreateAssetMenu(menuName = "Inventory/WeaponItem", fileName = "NewWeapon")]
public class WeaponItemSO : InventoryItemSO
{
    [Header("Weapon Settings")]

    [Tooltip("Скорость стрельбы в секундах между выстрелами.")]
    public float fireRate = 0.1f;

    [Tooltip("Ёмкость магазина.")]
    public int magazineSize = 7;

    [Tooltip("Ссылка на предмет боеприпасов, используемых этим оружием.")]
    public InventoryItemSO ammoItemReference;

    [Tooltip("Автоматическое оружие (true) или полуавтоматическое (false).")]
    public bool isAutomatic = false;

    [Tooltip("Префаб оружия, который будет инстанциирован при экипировке.")]
    public GameObject weaponPrefab;
}
