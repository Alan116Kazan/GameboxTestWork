using UnityEngine;

/// <summary>
/// ������ ��������� ������:
/// - ������������� �� ������� ������ ����� EquipmentManager.OnWeaponChanged
/// - ������������� ������� OnHitRegistered
/// - �������������� RaycastHit � ������ ���� (Target ��� ������ �������)
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

        // ���� ������ ��� ����������� � ������������� �� ��� ���������
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
        // ������� �������� � ����������� ������ � ������������� �� �����
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
    /// ������������ ���������: ���� Target �� ���������� ��� � ���������
    /// </summary>
    private void HandleWeaponHit(RaycastHit hit)
    {
        if (hit.collider == null) return;

        // ������ Target
        var target = hit.collider.GetComponent<Target>();
        if (target != null)
        {
            target.RegisterHit(hit);
            return;
        }

        // Target � ���������
        var targetInParent = hit.collider.GetComponentInParent<Target>();
        if (targetInParent != null)
        {
            targetInParent.RegisterHit(hit);
            return;
        }

        // ����� ����� ������������ ��������� ��� ������ ������ (Damageable, EnemyHealth � �.�.)
    }

    #endregion
}
