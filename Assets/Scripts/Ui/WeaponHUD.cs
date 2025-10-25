using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// HUD для отображения текущего оружия и количества патронов.
/// Подписывается на события EquipmentManager и EquippedWeapon.
/// </summary>
[RequireComponent(typeof(Canvas))]
public class WeaponHUD : MonoBehaviour
{
    #region Serialized Fields

    [Header("References")]
    [Tooltip("Менеджер экипировки игрока")]
    [SerializeField] private EquipmentManager equipmentManager;

    [Header("UI Elements")]
    [Tooltip("Текстовое поле для имени оружия")]
    [SerializeField] private Text weaponNameText;

    [Tooltip("Текстовое поле для отображения патронов")]
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

        // Сразу обновляем HUD на текущем оружии
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
    /// Вызывается при смене оружия в EquipmentManager.
    /// Подписываемся на события нового оружия и обновляем HUD.
    /// </summary>
    /// <param name="newWeapon">Новое экипированное оружие</param>
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
    /// Вызывается при изменении патронов в текущем оружии.
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
    /// Обновляет тексты HUD (имя оружия и патроны).
    /// </summary>
    private void UpdateTexts(int currentMag, int totalInInv, string weaponName)
    {
        if (weaponNameText != null) weaponNameText.text = weaponName;
        if (ammoText != null) ammoText.text = $"{currentMag} / {totalInInv}";
    }

    /// <summary>
    /// Показывает состояние "без оружия".
    /// </summary>
    private void ShowNoWeapon()
    {
        if (weaponNameText != null) weaponNameText.text = "Unarmed";
        if (ammoText != null) ammoText.text = string.Empty;
    }

    #endregion
}
