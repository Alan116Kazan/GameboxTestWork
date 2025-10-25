using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// �������� �� ����������� ��������� �������������� � UI.
/// ����� ������ ������� � Canvas. �������� ������ ������ ������� ���������.
/// </summary>
[DisallowMultipleComponent]
public class InteractionHintUI : MonoBehaviour
{
    #region Serialized Fields

    [Tooltip("UI Text ��� ����������� ���������")]
    [SerializeField] private Text hintText;

    #endregion

    #region Public Methods

    /// <summary>
    /// ������������� ����� ���������.
    /// ���� ������� null ��� ������ ������ � ������� �����.
    /// </summary>
    /// <param name="message">��������� ��� �����������</param>
    public void SetHint(string message)
    {
        if (hintText == null) return;
        hintText.text = message ?? string.Empty;
    }

    #endregion
}
