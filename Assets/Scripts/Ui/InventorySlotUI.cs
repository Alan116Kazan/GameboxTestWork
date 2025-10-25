using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// UI-������, ����������� � InventorySlot. �������� �� ����������� � ��������� ������.
/// </summary>
[RequireComponent(typeof(Button))]
[DisallowMultipleComponent]
public class InventorySlotUI : MonoBehaviour, IPointerClickHandler
{
    #region Serialized Fields

    [Header("References")]
    [Tooltip("����������� ������ ��������")]
    [SerializeField] private Image iconImage;

    [Tooltip("����� ��� ����������� ���������� ���������")]
    [SerializeField] private Text quantityText;

    #endregion

    #region Private Fields

    private InventorySlot boundSlot;         // ����������� ���� ������
    private int boundIndex;                  // ������ ����� � ���������
    private InventoryUIController parentController; // ���������� UI ��� ��������� ������

    #endregion

    #region Public Methods

    /// <summary>
    /// ����������� UI-������ � ����������� InventorySlot.
    /// </summary>
    /// <param name="slot">���� ���������</param>
    /// <param name="index">������ �����</param>
    /// <param name="parent">���������� UI ���������</param>
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
    /// ������������ ���� �� ������.
    /// ����������� ������� � ������������ InventoryUIController.
    /// </summary>
    /// <param name="eventData">������ ������� �����</param>
    public void OnPointerClick(PointerEventData eventData)
    {
        parentController?.OnSlotClicked(boundIndex, eventData.button);
    }

    #endregion
}
