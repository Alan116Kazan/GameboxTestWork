using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

/// <summary>
/// Обёртка над InputActionReference для быстрого доступа к действиям игрока.
/// Позволяет централизованно включать/отключать ввод.
/// </summary>
[DisallowMultipleComponent]
public class PlayerInputBridge : MonoBehaviour
{
    #region Serialized Fields

    [Header("UI")]
    [Tooltip("Действие для открытия/закрытия инвентаря.")]
    [SerializeField] private InputActionReference toggleInventoryAction;

    [Header("Movement / Look")]
    [Tooltip("Действие движения персонажа.")]
    [SerializeField] private InputActionReference moveAction;
    [Tooltip("Действие вращения камеры.")]
    [SerializeField] private InputActionReference lookAction;

    [Header("Actions")]
    [SerializeField] private InputActionReference jumpAction;
    [SerializeField] private InputActionReference fireAction;
    [SerializeField] private InputActionReference aimAction;
    [SerializeField] private InputActionReference interactAction;
    [SerializeField] private InputActionReference nextWeaponAction;
    [SerializeField] private InputActionReference quick1Action;
    [SerializeField] private InputActionReference quick2Action;
    [SerializeField] private InputActionReference throwAction;
    [SerializeField] private InputActionReference reloadAction;

    #endregion

    #region Public Properties

    public InputAction Move => moveAction?.action;
    public InputAction Look => lookAction?.action;
    public InputAction Jump => jumpAction?.action;
    public InputAction Fire => fireAction?.action;
    public InputAction Aim => aimAction?.action;
    public InputAction Interact => interactAction?.action;
    public InputAction NextWeapon => nextWeaponAction?.action;
    public InputAction Quick1 => quick1Action?.action;
    public InputAction Quick2 => quick2Action?.action;
    public InputAction Throw => throwAction?.action;
    public InputAction Reload => reloadAction?.action;
    public InputAction ToggleInventory => toggleInventoryAction?.action;

    #endregion

    #region Private Fields

    // Список всех InputActionReference для массовой обработки (включение/отключение)
    private List<InputActionReference> allActions;

    #endregion

    #region Unity Methods

    private void Awake()
    {
        // Инициализация списка всех действий
        allActions = new List<InputActionReference>
        {
            moveAction, lookAction, jumpAction, fireAction, aimAction,
            interactAction, nextWeaponAction, quick1Action, quick2Action,
            throwAction, reloadAction, toggleInventoryAction
        };
    }

    private void OnEnable() => SetActionsEnabled(true);
    private void OnDisable() => SetActionsEnabled(false);

    #endregion

    #region Input Management

    /// <summary>
    /// Включает или отключает все InputAction.
    /// </summary>
    /// <param name="enabled">Включить или отключить действия.</param>
    private void SetActionsEnabled(bool enabled)
    {
        foreach (var actionRef in allActions)
        {
            if (actionRef?.action == null) continue;

            if (enabled && !actionRef.action.enabled)
                actionRef.action.Enable();
            else if (!enabled && actionRef.action.enabled)
                actionRef.action.Disable();
        }
    }

    /// <summary>
    /// Включает или отключает только игровые действия (без UI, например, инвентаря).
    /// </summary>
    /// <param name="enabled">Включить/отключить ввод для геймплея.</param>
    public void SetGameplayEnabled(bool enabled)
    {
        var gameplayActions = new[]
        {
            moveAction, lookAction, jumpAction, fireAction, aimAction,
            interactAction, nextWeaponAction, quick1Action, quick2Action,
            throwAction, reloadAction
        };

        foreach (var actionRef in gameplayActions)
        {
            if (actionRef?.action == null) continue;

            if (enabled && !actionRef.action.enabled)
                actionRef.action.Enable();
            else if (!enabled && actionRef.action.enabled)
                actionRef.action.Disable();
        }
    }

    #endregion
}
