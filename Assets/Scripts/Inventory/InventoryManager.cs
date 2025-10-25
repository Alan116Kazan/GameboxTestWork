using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Менеджер инвентаря игрока.
/// Обрабатывает подбор предметов, работу с оружием и взаимодействие с UI.
/// Требует наличие компонента Inventory.
/// </summary>
[RequireComponent(typeof(Inventory))]
public class InventoryManager : MonoBehaviour
{
    #region Inspector Fields

    [Header("References")]
    [Tooltip("Ссылка на скрипт PlayerInputBridge для обработки InputActions")]
    [SerializeField] private PlayerInputBridge inputBridge;

    [Tooltip("Менеджер экипировки (оружие)")]
    [SerializeField] private EquipmentManager equipmentManager;

    [Header("Look / UI")]
    [Tooltip("Детектор объектов, на которые смотрит игрок")]
    [SerializeField] private LookInteractionDetector lookDetector;

    [Tooltip("UI подсказка взаимодействия")]
    [SerializeField] private InteractionHintUI interactionHintUI;

    #endregion

    #region Private Fields

    private Inventory inventory;

    // --- Кэш слотов оружия ---
    private readonly List<int> weaponSlotIndices = new List<int>(8);
    private int currentWeaponSlotPosition = -1;

    // --- InputActions ---
    private InputAction interactAction;
    private InputAction nextWeaponAction;
    private InputAction quick1Action;
    private InputAction quick2Action;
    private InputAction reloadAction;
    private InputAction fireAction;

    private string interactBinding = string.Empty;

    // --- Camera Cache ---
    private Camera cachedCamera;
    private Vector3 screenCenter;
    private int lastScreenW, lastScreenH;

    // --- Fire delegates ---
    private Action<InputAction.CallbackContext> onFireStarted;
    private Action<InputAction.CallbackContext> onFireCanceled;

    private const float FallbackRayDistance = 3f;

    #endregion

    #region Unity Callbacks

    private void Awake()
    {
        inventory = GetComponent<Inventory>();

        CacheInputActions();
        UpdateInteractBinding();

        cachedCamera = Camera.main;
        UpdateScreenCenterCache();

        // Действия для стрельбы
        onFireStarted = ctx => equipmentManager?.CurrentWeapon?.StartFiring();
        onFireCanceled = ctx => equipmentManager?.CurrentWeapon?.StopFiring();
    }

    private void OnEnable()
    {
        SubscribeAllInputs();
        SubscribeLookDetector();
        SubscribeInventoryEvents();
        RebuildWeaponSlotCache();
    }

    private void OnDisable()
    {
        UnsubscribeAllInputs();
        UnsubscribeLookDetector();
        UnsubscribeInventoryEvents();
    }

    #endregion

    #region Input Helpers

    private void CacheInputActions()
    {
        interactAction = inputBridge?.Interact;
        nextWeaponAction = inputBridge?.NextWeapon;
        quick1Action = inputBridge?.Quick1;
        quick2Action = inputBridge?.Quick2;
        reloadAction = inputBridge?.Reload;
        fireAction = inputBridge?.Fire;
    }

    private void SubscribeInput(InputAction action, Action<InputAction.CallbackContext> callback)
    {
        if (action == null || callback == null) return;
        action.performed -= callback;
        action.performed += callback;
        if (!action.enabled) action.Enable();
    }

    private void UnsubscribeInput(InputAction action, Action<InputAction.CallbackContext> callback)
    {
        if (action == null || callback == null) return;
        action.performed -= callback;
        if (action.enabled) action.Disable();
    }

    private void SubscribeFireInput(InputAction action)
    {
        if (action == null) return;
        action.started -= onFireStarted;
        action.canceled -= onFireCanceled;
        action.started += onFireStarted;
        action.canceled += onFireCanceled;
        if (!action.enabled) action.Enable();
    }

    private void UnsubscribeFireInput(InputAction action)
    {
        if (action == null) return;
        action.started -= onFireStarted;
        action.canceled -= onFireCanceled;
        if (action.enabled) action.Disable();
    }

    private void SubscribeAllInputs()
    {
        SubscribeInput(interactAction, OnInteractPerformed);
        SubscribeInput(nextWeaponAction, _ => SelectNextWeapon());
        SubscribeInput(quick1Action, _ => SelectQuickSlot(1));
        SubscribeInput(quick2Action, _ => SelectQuickSlot(2));
        SubscribeInput(reloadAction, _ => ReloadCurrentWeapon());
        SubscribeFireInput(fireAction);
    }

    private void UnsubscribeAllInputs()
    {
        UnsubscribeInput(interactAction, OnInteractPerformed);
        UnsubscribeInput(nextWeaponAction, _ => SelectNextWeapon());
        UnsubscribeInput(quick1Action, _ => SelectQuickSlot(1));
        UnsubscribeInput(quick2Action, _ => SelectQuickSlot(2));
        UnsubscribeInput(reloadAction, _ => ReloadCurrentWeapon());
        UnsubscribeFireInput(fireAction);
    }

    #endregion

    #region Look / Pickup

    private void SubscribeLookDetector()
    {
        if (lookDetector == null) return;
        lookDetector.OnTargetEnter += HandleLookTargetEnter;
        lookDetector.OnTargetExit += HandleLookTargetExit;
    }

    private void UnsubscribeLookDetector()
    {
        if (lookDetector == null) return;
        lookDetector.OnTargetEnter -= HandleLookTargetEnter;
        lookDetector.OnTargetExit -= HandleLookTargetExit;
    }

    private void SubscribeInventoryEvents()
    {
        if (inventory != null)
            inventory.OnInventoryChanged += RebuildWeaponSlotCache;
    }

    private void UnsubscribeInventoryEvents()
    {
        if (inventory != null)
            inventory.OnInventoryChanged -= RebuildWeaponSlotCache;
    }

    private void HandleLookTargetEnter(ItemPickup pickup)
    {
        if (interactionHintUI == null || pickup == null) return;

        string itemName = pickup.item?.DisplayName ?? "предмет";
        string hint = string.IsNullOrEmpty(interactBinding)
            ? $"Нажмите, чтобы подобрать {itemName}"
            : $"Нажмите [{interactBinding}], чтобы подобрать {itemName}";

        interactionHintUI.SetHint(hint);
    }

    private void HandleLookTargetExit() => interactionHintUI?.SetHint(string.Empty);

    private void OnInteractPerformed(InputAction.CallbackContext ctx) => TryPickupInLook();

    private void TryPickupInLook()
    {
        var pickup = lookDetector?.GetCurrentLookTarget() ?? RaycastFallbackPickup();
        if (pickup == null) return;

        pickup.TryPickup(inventory);
        interactionHintUI?.SetHint(string.Empty);
    }

    /// <summary>
    /// Фолбэк на случай, если детектор LookInteractionDetector не вернул объект.
    /// </summary>
    private ItemPickup RaycastFallbackPickup()
    {
        EnsureCameraCache();
        if (cachedCamera == null) return null;

        if (Screen.width != lastScreenW || Screen.height != lastScreenH)
            UpdateScreenCenterCache();

        if (Physics.Raycast(cachedCamera.ScreenPointToRay(screenCenter),
                            out RaycastHit hit,
                            FallbackRayDistance,
                            ~0,
                            QueryTriggerInteraction.Collide))
        {
            return hit.collider.GetComponentInParent<ItemPickup>();
        }
        return null;
    }

    #endregion

    #region Weapon Management

    /// <summary>
    /// Строит кэш слотов с оружием для быстрого переключения
    /// </summary>
    private void RebuildWeaponSlotCache()
    {
        weaponSlotIndices.Clear();
        if (inventory == null)
        {
            currentWeaponSlotPosition = -1;
            return;
        }

        for (int i = 0; i < inventory.Slots.Count; i++)
        {
            if (!inventory.Slots[i].IsEmpty() && inventory.Slots[i].Item is WeaponItemSO)
                weaponSlotIndices.Add(i);
        }

        if (weaponSlotIndices.Count == 0)
            currentWeaponSlotPosition = -1;
        else if (currentWeaponSlotPosition >= weaponSlotIndices.Count)
            currentWeaponSlotPosition = 0;
    }

    private void SelectNextWeapon()
    {
        if (weaponSlotIndices.Count == 0) return;
        currentWeaponSlotPosition = (currentWeaponSlotPosition + 1) % weaponSlotIndices.Count;
        EquipWeaponFromSlot(weaponSlotIndices[currentWeaponSlotPosition]);
    }

    private void SelectQuickSlot(int slotNumber)
    {
        if (weaponSlotIndices.Count == 0) return;
        int pick = Mathf.Clamp(slotNumber - 1, 0, weaponSlotIndices.Count - 1);
        currentWeaponSlotPosition = pick;
        EquipWeaponFromSlot(weaponSlotIndices[pick]);
    }

    private void EquipWeaponFromSlot(int slotIndex)
    {
        var slot = inventory.GetSlotAt(slotIndex);
        if (slot != null && !slot.IsEmpty() && slot.Item is WeaponItemSO weapon)
        {
            equipmentManager?.EquipWeapon(weapon, inventory);
        }
    }

    public void ReloadCurrentWeapon() => equipmentManager?.TryReloadCurrentFromInventory(inventory);

    #endregion

    #region Binding & Camera Cache

    private void UpdateInteractBinding()
    {
        if (interactAction == null)
        {
            interactBinding = string.Empty;
            return;
        }

        try
        {
            string display = interactAction.GetBindingDisplayString();
            if (string.IsNullOrEmpty(display))
                display = interactAction.GetBindingDisplayString(0);

            interactBinding = string.IsNullOrEmpty(display) ? string.Empty : display;
        }
        catch
        {
            interactBinding = string.Empty;
        }
    }

    /// <summary>
    /// Обновляет привязку клавиши Interact при изменении InputActions
    /// </summary>
    public void RefreshBindings()
    {
        CacheInputActions();
        UpdateInteractBinding();
    }

    private void EnsureCameraCache()
    {
        if (cachedCamera == null)
            cachedCamera = Camera.main;
    }

    private void UpdateScreenCenterCache()
    {
        lastScreenW = Screen.width;
        lastScreenH = Screen.height;
        screenCenter = new Vector3(lastScreenW * 0.5f, lastScreenH * 0.5f, 0f);
    }

    #endregion
}
