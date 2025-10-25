using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

/// <summary>
/// Контроллер UI инвентаря. Управляет отображением слотов и открытием/закрытием панели.
/// Поддерживает фиксированное количество слотов и объектный пул для UI-элементов.
/// При открытии UI отключает игровой ввод через PlayerInputBridge.SetGameplayEnabled(false).
/// </summary>
[DisallowMultipleComponent]
public class InventoryUIController : MonoBehaviour
{
    #region Serialized Fields

    [Header("Wiring")]
    [Tooltip("Ссылка на скрипт PlayerInputBridge для управления вводом")]
    [SerializeField] private PlayerInputBridge inputBridge;

    [Tooltip("Ссылка на Inventory с данными")]
    [SerializeField] private Inventory inventory;

    [Header("UI References")]
    [Tooltip("Панель инвентаря")]
    [SerializeField] private GameObject inventoryPanel;

    [Tooltip("Родитель для слотов (Grid Layout)")]
    [SerializeField] private RectTransform gridParent;

    [Tooltip("Префаб UI-слота")]
    [SerializeField] private GameObject slotPrefab;

    #endregion

    #region Private Fields

    private readonly List<GameObject> slotPool = new List<GameObject>(); // Пул объектов слотов

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
    /// Переключает видимость панели инвентаря.
    /// Блокирует или разблокирует игровой ввод.
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
    /// Арендует UI-слот из пула или создаёт новый.
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
    /// Возвращает все слоты в пул (отключает и перемещает в transform).
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
    /// Обновляет все слоты UI в соответствии с текущими данными Inventory.
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
                Debug.LogWarning("Slot prefab не содержит InventorySlotUI");
                continue;
            }

            var slotData = inventory.GetSlotAt(i);
            slotUI.Setup(slotData, i, this);
        }
    }

    #endregion

    #region Slot Interaction

    /// <summary>
    /// Обработка клика по слоту. Левый клик снимает 1 предмет, правый — очищает слот.
    /// </summary>
    /// <param name="slotIndex">Индекс слота</param>
    /// <param name="button">Кнопка мыши</param>
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
