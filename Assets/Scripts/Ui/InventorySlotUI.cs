using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// UI-ячейка, привязанная к InventorySlot. Отвечает за отображение и обработку кликов.
/// </summary>
[RequireComponent(typeof(Button))]
[DisallowMultipleComponent]
public class InventorySlotUI : MonoBehaviour, IPointerClickHandler
{
    #region Serialized Fields

    [Header("References")]
    [Tooltip("Изображение иконки предмета")]
    [SerializeField] private Image iconImage;

    [Tooltip("Текст для отображения количества предметов")]
    [SerializeField] private Text quantityText;

    #endregion

    #region Private Fields

    private InventorySlot boundSlot;         // Привязанный слот данных
    private int boundIndex;                  // Индекс слота в инвентаре
    private InventoryUIController parentController; // Контроллер UI для обработки кликов

    #endregion

    #region Public Methods

    /// <summary>
    /// Привязывает UI-ячейку к конкретному InventorySlot.
    /// </summary>
    /// <param name="slot">Слот инвентаря</param>
    /// <param name="index">Индекс слота</param>
    /// <param name="parent">Контроллер UI инвентаря</param>
    public void Setup(InventorySlot slot, int index, InventoryUIController parent)
    {
        boundSlot = slot;
        boundIndex = index;
        parentController = parent;

        if (slot == null || slot.IsEmpty())
        {
            if (iconImage) iconImage.sprite = null;
            if (quantityText) quantityText.text = string.Empty;
            return;
        }

        if (iconImage) iconImage.sprite = slot.Item.Icon;
        if (quantityText) quantityText.text = slot.Quantity > 1 ? slot.Quantity.ToString() : string.Empty;
    }

    #endregion

    #region Event Handlers

    /// <summary>
    /// Обрабатывает клик по ячейке.
    /// Прокидывает событие в родительский InventoryUIController.
    /// </summary>
    /// <param name="eventData">Данные события клика</param>
    public void OnPointerClick(PointerEventData eventData)
    {
        parentController?.OnSlotClicked(boundIndex, eventData.button);
    }

    #endregion
}
