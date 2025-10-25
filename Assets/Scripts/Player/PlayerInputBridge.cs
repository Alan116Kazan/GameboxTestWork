using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

/// <summary>
/// ������ ��� InputActionReference ��� �������� ������� � ��������� ������.
/// ��������� ��������������� ��������/��������� ����.
/// </summary>
[DisallowMultipleComponent]
public class PlayerInputBridge : MonoBehaviour
{
    #region Serialized Fields

    [Header("UI")]
    [Tooltip("�������� ��� ��������/�������� ���������.")]
    [SerializeField] private InputActionReference toggleInventoryAction;

    [Header("Movement / Look")]
    [Tooltip("�������� �������� ���������.")]
    [SerializeField] private InputActionReference moveAction;
    [Tooltip("�������� �������� ������.")]
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

    // ������ ���� InputActionReference ��� �������� ��������� (���������/����������)
    private List<InputActionReference> allActions;

    #endregion

    #region Unity Methods

    private void Awake()
    {
        // ������������� ������ ���� ��������
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
    /// �������� ��� ��������� ��� InputAction.
    /// </summary>
    /// <param name="enabled">�������� ��� ��������� ��������.</param>
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
    /// �������� ��� ��������� ������ ������� �������� (��� UI, ��������, ���������).
    /// </summary>
    /// <param name="enabled">��������/��������� ���� ��� ��������.</param>
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
