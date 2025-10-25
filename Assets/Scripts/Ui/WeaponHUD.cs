using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// HUD ��� ����������� �������� ������ � ���������� ��������.
/// ������������� �� ������� EquipmentManager � EquippedWeapon.
/// </summary>
[RequireComponent(typeof(Canvas))]
public class WeaponHUD : MonoBehaviour
{
    #region Serialized Fields

    [Header("References")]
    [Tooltip("�������� ���������� ������")]
    [SerializeField] private EquipmentManager equipmentManager;

    [Header("UI Elements")]
    [Tooltip("��������� ���� ��� ����� ������")]
    [SerializeField] private Text weaponNameText;

    [Tooltip("��������� ���� ��� ����������� ��������")]
    [SerializeField] private Text ammoText;

    #endregion

    #region Private Fields

    private EquippedWeapon currentWeapon;

    #endregion

    #region Unity Callbacks

    private void OnEnable()
    {
        if (equipmentManager != null)
            equipmentManager.OnWeaponChanged += OnWeaponChanged;

        // ����� ��������� HUD �� ������� ������
        if (equipmentManager != null && equipmentManager.CurrentWeapon != null)
            OnWeaponChanged(equipmentManager.CurrentWeapon);
        else
            ShowNoWeapon();
    }

    private void OnDisable()
    {
        if (equipmentManager != null)
            equipmentManager.OnWeaponChanged -= OnWeaponChanged;

        if (currentWeapon != null)
            currentWeapon.OnAmmoChanged -= OnAmmoChanged;
    }

    #endregion

    #region Event Handlers

    /// <summary>
    /// ���������� ��� ����� ������ � EquipmentManager.
    /// ������������� �� ������� ������ ������ � ��������� HUD.
    /// </summary>
    /// <param name="newWeapon">����� ������������� ������</param>
    private void OnWeaponChanged(EquippedWeapon newWeapon)
    {
        if (currentWeapon != null)
            currentWeapon.OnAmmoChanged -= OnAmmoChanged;

        currentWeapon = newWeapon;

        if (currentWeapon == null)
        {
            ShowNoWeapon();
            return;
        }

        currentWeapon.OnAmmoChanged += OnAmmoChanged;

        UpdateTexts(
            currentWeapon.CurrentMagazine,
            currentWeapon.GetTotalAmmoInInventory(),
            currentWeapon.weaponData != null ? currentWeapon.weaponData.DisplayName : "Weapon");
    }

    /// <summary>
    /// ���������� ��� ��������� �������� � ������� ������.
    /// </summary>
    private void OnAmmoChanged(int currentMag, int totalInInv)
    {
        UpdateTexts(
            currentMag,
            totalInInv,
            currentWeapon != null && currentWeapon.weaponData != null
                ? currentWeapon.weaponData.DisplayName
                : "Weapon");
    }

    #endregion

    #region UI Updates

    /// <summary>
    /// ��������� ������ HUD (��� ������ � �������).
    /// </summary>
    private void UpdateTexts(int currentMag, int totalInInv, string weaponName)
    {
        if (weaponNameText != null) weaponNameText.text = weaponName;
        if (ammoText != null) ammoText.text = $"{currentMag} / {totalInInv}";
    }

    /// <summary>
    /// ���������� ��������� "��� ������".
    /// </summary>
    private void ShowNoWeapon()
    {
        if (weaponNameText != null) weaponNameText.text = "Unarmed";
        if (ammoText != null) ammoText.text = string.Empty;
    }

    #endregion
}
