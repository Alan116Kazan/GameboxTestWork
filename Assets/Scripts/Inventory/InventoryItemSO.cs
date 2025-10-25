using UnityEngine;

/// <summary>
/// ScriptableObject ��� �������� ���������.
/// ��������� ��������� IInventoryItem.
/// ��������� ��������� �������� � ���������� ���������������, �������, �������� � ����������� �����.
/// </summary>
[CreateAssetMenu(menuName = "Inventory/Item", fileName = "NewItem")]
public class InventoryItemSO : ScriptableObject, IInventoryItem
{
    #region Identity

    [Header("Identity")]
    [Tooltip("���������� ������������� �������� (��������, 'pistol_ammo').")]
    [SerializeField] private string id = "new_item";

    [Tooltip("������������ ��� �������� ��� UI.")]
    [SerializeField] private string displayName = "New Item";

    #endregion

    #region Visuals

    [Header("Visuals")]
    [Tooltip("������ �������� ��� UI.")]
    [SerializeField] private Sprite icon;

    [Tooltip("������ �������� ��� ������ � ����.")]
    [SerializeField] private GameObject prefab;

    #endregion

    #region Stack Settings

    [Header("Stack Settings")]
    [Tooltip("����� �� ���������� �������� � ����.")]
    [SerializeField] private bool isStackable = false;

    [Tooltip("������������ ������ ����� ��������.")]
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
    /// �������� ������������ ����� ��� ��������� � ����������.
    /// </summary>
    private void OnValidate()
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            Debug.LogWarning($"InventoryItemSO '{name}' ����� ������ Id. ������� ���������� Id.");
            id = name.ToLower().Replace(" ", "_");
        }

        if (maxStack < 1)
            maxStack = 1;
    }

    #endregion
}
