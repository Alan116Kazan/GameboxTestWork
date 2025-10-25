using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

/// <summary>
/// Отвечает за отображение текущих назначений клавиш в UI.
/// Использует PlayerInputBridge для получения InputAction.
/// </summary>
[DisallowMultipleComponent]
public class ControlsDisplay : MonoBehaviour
{
    #region Serialized Fields

    [Header("References")]
    [Tooltip("Скрипт, содержащий InputActions")]
    [SerializeField] private PlayerInputBridge inputBridge;

    [Tooltip("Стандартный UI Text для отображения управления")]
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
    /// Обновляет текст с текущими назначениями клавиш.
    /// Можно вызывать, если игрок поменял бинды.
    /// </summary>
    public void UpdateControlsText()
    {
        if (inputBridge == null || controlsText == null) return;

        // Получаем текстовые представления назначенных клавиш
        string moveKey = inputBridge.Move?.GetBindingDisplayString(0) ?? "—";
        string jumpKey = inputBridge.Jump?.GetBindingDisplayString(0) ?? "—";
        string fireKey = inputBridge.Fire?.GetBindingDisplayString(0) ?? "—";
        string aimKey = inputBridge.Aim?.GetBindingDisplayString(0) ?? "—";
        string interactKey = inputBridge.Interact?.GetBindingDisplayString(0) ?? "—";
        string nextWeaponKey = inputBridge.NextWeapon?.GetBindingDisplayString(0) ?? "—";
        string inventoryKey = inputBridge.ToggleInventory?.GetBindingDisplayString(0) ?? "—";

        // Формируем текст для UI
        controlsText.text =
            "Управление:\n" +
            $"Ходьба: {moveKey}\n" +
            $"Прыжок: {jumpKey}\n" +
            $"Стрелять: {fireKey}\n" +
            $"Прицел: {aimKey}\n" +
            $"Взаимодействие: {interactKey}\n" +
            $"Следующее оружие: {nextWeaponKey}\n" +
            $"Инвентарь: {inventoryKey}";
    }

    #endregion
}
