using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

/// <summary>
/// �������� �� ����������� ������� ���������� ������ � UI.
/// ���������� PlayerInputBridge ��� ��������� InputAction.
/// </summary>
[DisallowMultipleComponent]
public class ControlsDisplay : MonoBehaviour
{
    #region Serialized Fields

    [Header("References")]
    [Tooltip("������, ���������� InputActions")]
    [SerializeField] private PlayerInputBridge inputBridge;

    [Tooltip("����������� UI Text ��� ����������� ����������")]
    [SerializeField] private Text controlsText;

    #endregion

    #region Unity Callbacks

    private void Start()
    {
        UpdateControlsText();
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// ��������� ����� � �������� ������������ ������.
    /// ����� ��������, ���� ����� ������� �����.
    /// </summary>
    public void UpdateControlsText()
    {
        if (inputBridge == null || controlsText == null) return;

        // �������� ��������� ������������� ����������� ������
        string moveKey = inputBridge.Move?.GetBindingDisplayString(0) ?? "�";
        string jumpKey = inputBridge.Jump?.GetBindingDisplayString(0) ?? "�";
        string fireKey = inputBridge.Fire?.GetBindingDisplayString(0) ?? "�";
        string aimKey = inputBridge.Aim?.GetBindingDisplayString(0) ?? "�";
        string interactKey = inputBridge.Interact?.GetBindingDisplayString(0) ?? "�";
        string nextWeaponKey = inputBridge.NextWeapon?.GetBindingDisplayString(0) ?? "�";
        string inventoryKey = inputBridge.ToggleInventory?.GetBindingDisplayString(0) ?? "�";

        // ��������� ����� ��� UI
        controlsText.text =
            "����������:\n" +
            $"������: {moveKey}\n" +
            $"������: {jumpKey}\n" +
            $"��������: {fireKey}\n" +
            $"������: {aimKey}\n" +
            $"��������������: {interactKey}\n" +
            $"��������� ������: {nextWeaponKey}\n" +
            $"���������: {inventoryKey}";
    }

    #endregion
}
