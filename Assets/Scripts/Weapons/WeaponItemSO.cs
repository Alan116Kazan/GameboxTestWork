using UnityEngine;

/// <summary>
/// ScriptableObject ��� ������.
/// ����������� �� InventoryItemSO (�� ���� �������� ��������� ���������),
/// �������� ��� ��������� ������: �������, �������� ��������, ��� �����������, �������/����������� � ������.
/// </summary>
[CreateAssetMenu(menuName = "Inventory/WeaponItem", fileName = "NewWeapon")]
public class WeaponItemSO : InventoryItemSO
{
    [Header("Weapon Settings")]

    [Tooltip("�������� �������� � �������� ����� ����������.")]
    public float fireRate = 0.1f;

    [Tooltip("������� ��������.")]
    public int magazineSize = 7;

    [Tooltip("������ �� ������� �����������, ������������ ���� �������.")]
    public InventoryItemSO ammoItemReference;

    [Tooltip("�������������� ������ (true) ��� ������������������ (false).")]
    public bool isAutomatic = false;

    [Tooltip("������ ������, ������� ����� �������������� ��� ����������.")]
    public GameObject weaponPrefab;
}
