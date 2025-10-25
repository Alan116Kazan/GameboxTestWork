using UnityEngine;

/// <summary>
/// Роутер попаданий оружия:
/// - Подписывается на текущее оружие через EquipmentManager.OnWeaponChanged
/// - Перехватывает событие OnHitRegistered
/// - Маршрутизирует RaycastHit к логике цели (Target или другие системы)
/// </summary>
[DisallowMultipleComponent]
public class WeaponHitRouter : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private EquipmentManager equipmentManager;

    private EquippedWeapon currentWeapon;

    #region Unity Callbacks

    private void Awake()
    {
        if (equipmentManager == null)
            equipmentManager = FindObjectOfType<EquipmentManager>();
    }

    private void OnEnable()
    {
        if (equipmentManager != null)
            equipmentManager.OnWeaponChanged += OnWeaponChanged;

        // Если оружие уже экипировано — подписываемся на его попадания
        if (equipmentManager != null && equipmentManager.CurrentWeapon != null)
            SubscribeToWeapon(equipmentManager.CurrentWeapon);
    }

    private void OnDisable()
    {
        if (equipmentManager != null)
            equipmentManager.OnWeaponChanged -= OnWeaponChanged;

        UnsubscribeFromWeapon();
    }

    #endregion

    #region Weapon Subscription

    private void OnWeaponChanged(EquippedWeapon newWeapon)
    {
        // Снимаем подписку с предыдущего оружия и подписываемся на новое
        UnsubscribeFromWeapon();
        SubscribeToWeapon(newWeapon);
    }

    private void SubscribeToWeapon(EquippedWeapon weapon)
    {
        if (weapon == null) return;
        currentWeapon = weapon;
        weapon.OnHitRegistered += HandleWeaponHit;
    }

    private void UnsubscribeFromWeapon()
    {
        if (currentWeapon != null)
        {
            currentWeapon.OnHitRegistered -= HandleWeaponHit;
            currentWeapon = null;
        }
    }

    #endregion

    #region Hit Handling

    /// <summary>
    /// Обрабатывает попадание: ищет Target на коллайдере или в родителях
    /// </summary>
    private void HandleWeaponHit(RaycastHit hit)
    {
        if (hit.collider == null) return;

        // Прямой Target
        var target = hit.collider.GetComponent<Target>();
        if (target != null)
        {
            target.RegisterHit(hit);
            return;
        }

        // Target в родителях
        var targetInParent = hit.collider.GetComponentInParent<Target>();
        if (targetInParent != null)
        {
            targetInParent.RegisterHit(hit);
            return;
        }

        // Здесь можно обрабатывать попадания для других систем (Damageable, EnemyHealth и т.д.)
    }

    #endregion
}
