using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// ќтвечает за отображение подсказки взаимодействи€ в UI.
/// “екст всегда активен в Canvas. ѕередача пустой строки очищает подсказку.
/// </summary>
[DisallowMultipleComponent]
public class InteractionHintUI : MonoBehaviour
{
    #region Serialized Fields

    [Tooltip("UI Text дл€ отображени€ подсказки")]
    [SerializeField] private Text hintText;

    #endregion

    #region Public Methods

    /// <summary>
    /// ”станавливает текст подсказки.
    /// ≈сли передан null или пуста€ строка Ч очищает текст.
    /// </summary>
    /// <param name="message">—ообщение дл€ отображени€</param>
    public void SetHint(string message)
    {
        if (hintText == null) return;
        hintText.text = message ?? string.Empty;
    }

    #endregion
}
