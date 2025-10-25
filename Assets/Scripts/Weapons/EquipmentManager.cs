using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// �������� ���������� ������:
/// - ���������� �������
/// - ��������� ����� ������ ��� ������� ������
/// - ���������� ������ ������ ������ ��� �����
/// </summary>
public class EquipmentManager : MonoBehaviour
{
    #region Serialized Fields

    [Header("Hands / Holster")]
    [Tooltip("��������� ��� ��������� ������ � �����")]
    [SerializeField] private Transform rightHand;

    [Tooltip("��������� ��� �������� ������ � ������")]
    [SerializeField] private Transform holsterParent;

    #endregion

    #region Private Fields

    private EquippedWeapon currentWeaponInstance;
    private WeaponItemSO currentWeaponData;

    private readonly Dictionary<string, EquippedWeapon> holsteredWeapons = new(StringComparer.Ordinal);

    // ����� ������ ��� ������� ������
    private readonly Dictionary<string, int> tempAmmoBuffer = new(StringComparer.Ordinal);

    #endregion

    #region Events

    public event Action<EquippedWeapon> OnWeaponChanged;
    public event Action<string, int> OnTempAmmoBufferChanged;

    #endregion

    #region Public Properties

    public EquippedWeapon CurrentWeapon => currentWeaponInstance;

    #endregion

    #region Equip / Holster / Drop

    /// <summary>
    /// ��������� ������ �� ��������� ��� ������
    /// </summary>
    public void EquipWeapon(WeaponItemSO weapon, Inventory inv)
    {
        HolsterCurrent(); // ������ ������� ������ � ������, ������� �������� ������

        if (weapon?.weaponPrefab == null)
        {
            SetCurrentWeapon(null, null);
            return;
        }

        // �������� ����� ������ �� ������, ����� ������ ����� ���������
        var eq = RetrieveFromHolster(weapon) ?? InstantiateWeapon(weapon);

        // ���� ������ �����, ��������� ������� �� ������/���������
        if (eq.CurrentMagazine <= 0)
        {
            int initialAmmo = LoadAmmoFromBufferAndInventory(weapon, inv);
            eq.Initialize(weapon, initialAmmo, inv);
        }
        else
        {
            // ������� �������� ������ ������
            eq.Initialize(weapon, eq.CurrentMagazine, inv);
        }

        SetCurrentWeapon(eq, weapon);
    }

    /// <summary>
    /// ������� ������� ������ � ������. ������� �������� ������.
    /// </summary>
    private void HolsterCurrent()
    {
        if (currentWeaponInstance == null || currentWeaponData == null) return;

        StopAndHolster(currentWeaponInstance);

        // ��������� � ������ � ������� ����������� ������
        holsteredWeapons[currentWeaponData.Id] = currentWeaponInstance;

        SetCurrentWeapon(null, null);
    }

    /// <summary>
    /// ��������� ������� ������� ������ (������� �� ������������ � ���������)
    /// </summary>
    public void UnequipCurrent()
    {
        if (currentWeaponInstance == null) return;

        Destroy(currentWeaponInstance.gameObject);
        SetCurrentWeapon(null, null);
    }

    public void DropCurrent() => UnequipCurrent();

    #endregion

    #region Fire / Reload

    public void TryFireCurrent() => currentWeaponInstance?.TryFire();

    /// <summary>
    /// ������� ����������� �������� ������ �� ��������� � ������
    /// </summary>
    public int TryReloadCurrentFromInventory(Inventory inv)
    {
        if (currentWeaponInstance?.weaponData?.ammoItemReference == null) return 0;

        string ammoId = currentWeaponInstance.weaponData.ammoItemReference.Id;
        int needed = currentWeaponInstance.weaponData.magazineSize - currentWeaponInstance.CurrentMagazine;
        if (needed <= 0) return 0;

        int transferred = TakeFromBuffer(ammoId, ref needed);

        if (needed > 0 && inv != null)
        {
            int take = Mathf.Min(needed, inv.GetTotalQuantity(ammoId));
            if (take > 0)
            {
                inv.RemoveItem(ammoId, take);
                currentWeaponInstance.AddAmmo(take);
                transferred += take;
            }
        }

        return transferred;
    }

    #endregion

    #region Ammo Buffer

    public IReadOnlyDictionary<string, int> GetTempAmmoBuffer() => tempAmmoBuffer;

    public int GetBufferedAmount(string ammoId) =>
        string.IsNullOrEmpty(ammoId) ? 0 : tempAmmoBuffer.TryGetValue(ammoId, out var val) ? val : 0;

    public void ClearTempBufferForAmmo(string ammoId)
    {
        if (string.IsNullOrEmpty(ammoId)) return;
        tempAmmoBuffer.Remove(ammoId);
        OnTempAmmoBufferChanged?.Invoke(ammoId, 0);
    }

    private int TakeFromBuffer(string ammoId, ref int needed)
    {
        if (string.IsNullOrEmpty(ammoId) || needed <= 0) return 0;
        if (!tempAmmoBuffer.TryGetValue(ammoId, out int buffered) || buffered <= 0) return 0;

        int take = Mathf.Min(buffered, needed);
        needed -= take;
        buffered -= take;

        if (buffered > 0) tempAmmoBuffer[ammoId] = buffered;
        else tempAmmoBuffer.Remove(ammoId);

        currentWeaponInstance?.AddAmmo(take);
        OnTempAmmoBufferChanged?.Invoke(ammoId, buffered);
        return take;
    }

    #endregion

    #region Private Helpers

    private void SetCurrentWeapon(EquippedWeapon instance, WeaponItemSO data)
    {
        currentWeaponInstance = instance;
        currentWeaponData = data;
        OnWeaponChanged?.Invoke(currentWeaponInstance);
    }

    private EquippedWeapon RetrieveFromHolster(WeaponItemSO weapon)
    {
        if (weapon == null || !holsteredWeapons.TryGetValue(weapon.Id, out var eq)) return null;

        holsteredWeapons.Remove(weapon.Id);
        ActivateWeapon(eq, rightHand);
        eq.OnUnholstered(null);
        return eq;
    }

    private EquippedWeapon InstantiateWeapon(WeaponItemSO weapon)
    {
        var go = Instantiate(weapon.weaponPrefab, rightHand);
        var eq = go.GetComponent<EquippedWeapon>() ?? go.AddComponent<EquippedWeapon>();
        ActivateWeapon(eq, rightHand);
        return eq;
    }

    private void ActivateWeapon(EquippedWeapon eq, Transform parent)
    {
        eq.transform.SetParent(parent, false);
        eq.transform.localPosition = Vector3.zero;
        eq.transform.localRotation = Quaternion.identity;
        eq.gameObject.SetActive(true);
    }

    private void StopAndHolster(EquippedWeapon eq)
    {
        eq.StopFiring();
        eq.OnHolstered();

        if (holsterParent != null) ActivateWeapon(eq, holsterParent);
        eq.gameObject.SetActive(false);
    }

    private int LoadAmmoFromBufferAndInventory(WeaponItemSO weapon, Inventory inv)
    {
        if (weapon?.ammoItemReference == null) return 0;

        string ammoId = weapon.ammoItemReference.Id;
        int needed = weapon.magazineSize;
        int loaded = TakeFromBuffer(ammoId, ref needed);

        if (needed > 0 && inv != null)
        {
            int take = Mathf.Min(needed, inv.GetTotalQuantity(ammoId));
            if (take > 0)
            {
                inv.RemoveItem(ammoId, take);
                loaded += take;
            }
        }

        return loaded;
    }

    #endregion
}
