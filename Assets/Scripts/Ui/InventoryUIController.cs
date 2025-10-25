using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

/// <summary>
/// ���������� UI ���������. ��������� ������������ ������ � ���������/��������� ������.
/// ������������ ������������� ���������� ������ � ��������� ��� ��� UI-���������.
/// ��� �������� UI ��������� ������� ���� ����� PlayerInputBridge.SetGameplayEnabled(false).
/// </summary>
[DisallowMultipleComponent]
public class InventoryUIController : MonoBehaviour
{
    #region Serialized Fields

    [Header("Wiring")]
    [Tooltip("������ �� ������ PlayerInputBridge ��� ���������� ������")]
    [SerializeField] private PlayerInputBridge inputBridge;

    [Tooltip("������ �� Inventory � �������")]
    [SerializeField] private Inventory inventory;

    [Header("UI References")]
    [Tooltip("������ ���������")]
    [SerializeField] private GameObject inventoryPanel;

    [Tooltip("�������� ��� ������ (Grid Layout)")]
    [SerializeField] private RectTransform gridParent;

    [Tooltip("������ UI-�����")]
    [SerializeField] private GameObject slotPrefab;

    #endregion

    #region Private Fields

    private readonly List<GameObject> slotPool = new List<GameObject>(); // ��� �������� ������

    #endregion

    #region Unity Callbacks

    private void Awake()
    {
        if (inventory == null) inventory = FindObjectOfType<Inventory>();
        if (inputBridge == null) inputBridge = FindObjectOfType<PlayerInputBridge>();

        if (inventoryPanel != null)
            inventoryPanel.SetActive(false);
    }

    private void OnEnable()
    {
        if (inputBridge?.ToggleInventory != null)
            inputBridge.ToggleInventory.performed += OnToggleInventoryPerformed;

        if (inventory != null)
            inventory.OnInventoryChanged += RefreshUI;

        inputBridge?.SetGameplayEnabled(true);
        RefreshUI();
    }

    private void OnDisable()
    {
        if (inputBridge?.ToggleInventory != null)
            inputBridge.ToggleInventory.performed -= OnToggleInventoryPerformed;

        if (inventory != null)
            inventory.OnInventoryChanged -= RefreshUI;
    }

    #endregion

    #region Input Handling

    private void OnToggleInventoryPerformed(InputAction.CallbackContext ctx) => ToggleInventory();

    /// <summary>
    /// ����������� ��������� ������ ���������.
    /// ��������� ��� ������������ ������� ����.
    /// </summary>
    public void ToggleInventory()
    {
        if (inventoryPanel == null) return;

        bool willOpen = !inventoryPanel.activeSelf;
        inventoryPanel.SetActive(willOpen);

        if (willOpen)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            inputBridge?.SetGameplayEnabled(false);
            RefreshUI();
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            inputBridge?.SetGameplayEnabled(true);
        }
    }

    #endregion

    #region Slot Pooling

    /// <summary>
    /// �������� UI-���� �� ���� ��� ������ �����.
    /// </summary>
    private GameObject RentSlotObject()
    {
        for (int i = 0; i < slotPool.Count; i++)
        {
            if (!slotPool[i].activeSelf)
            {
                slotPool[i].SetActive(true);
                return slotPool[i];
            }
        }

        var go = Instantiate(slotPrefab, gridParent);
        slotPool.Add(go);
        return go;
    }

    /// <summary>
    /// ���������� ��� ����� � ��� (��������� � ���������� � transform).
    /// </summary>
    private void ReturnAllToPool()
    {
        foreach (var go in slotPool)
        {
            go.SetActive(false);
            go.transform.SetParent(transform, false);
        }
    }

    #endregion

    #region UI Refresh

    /// <summary>
    /// ��������� ��� ����� UI � ������������ � �������� ������� Inventory.
    /// </summary>
    public void RefreshUI()
    {
        if (gridParent == null || slotPrefab == null || inventory == null) return;

        ReturnAllToPool();

        int totalSlots = inventory.MaxSlots;
        
        for (int i = slotPool.Count; i < totalSlots; i++)
        {
            var go = Instantiate(slotPrefab, gridParent);
            go.SetActive(false);
            slotPool.Add(go);
        }

        for (int i = 0; i < totalSlots; i++)
        {
            GameObject go = i < slotPool.Count ? slotPool[i] : RentSlotObject();
            go.SetActive(true);
            go.transform.SetParent(gridParent, false);

            var slotUI = go.GetComponent<InventorySlotUI>();
            if (slotUI == null)
            {
                Debug.LogWarning("Slot prefab �� �������� InventorySlotUI");
                continue;
            }

            var slotData = inventory.GetSlotAt(i);
            slotUI.Setup(slotData, i, this);
        }
    }

    #endregion

    #region Slot Interaction

    /// <summary>
    /// ��������� ����� �� �����. ����� ���� ������� 1 �������, ������ � ������� ����.
    /// </summary>
    /// <param name="slotIndex">������ �����</param>
    /// <param name="button">������ ����</param>
    public void OnSlotClicked(int slotIndex, PointerEventData.InputButton button)
    {
        if (inventory == null) return;

        if (button == PointerEventData.InputButton.Left)
        {
            int removed = inventory.RemoveFromSlotAt(slotIndex, 1);
            Debug.Log($"Removed {removed} from slot {slotIndex}");
        }
        else if (button == PointerEventData.InputButton.Right)
        {
            int removed = inventory.ClearSlotAt(slotIndex);
            Debug.Log($"Cleared slot {slotIndex}, removed {removed}");
        }
    }

    #endregion
}
