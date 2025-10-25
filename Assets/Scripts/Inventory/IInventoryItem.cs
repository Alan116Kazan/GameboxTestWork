using UnityEngine;

/// <summary>
/// ���������, ����������� ������� ������� ���������.
/// ������������ ��� ScriptableObject ��� ������� ��������.
/// </summary>
public interface IInventoryItem
{
    /// <summary>
    /// ���������� ������������� �������� (��������, "pistol_ammo").
    /// ������������ ��� ������ � ��������� ���������.
    /// </summary>
    string Id { get; }

    /// <summary>
    /// ������������ ��� �������� ��� UI.
    /// </summary>
    string DisplayName { get; }

    /// <summary>
    /// ������ �������� ��� ����������� � UI.
    /// </summary>
    Sprite Icon { get; }

    /// <summary>
    /// ������ �������� ��� ������ � ����.
    /// </summary>
    GameObject Prefab { get; }

    /// <summary>
    /// ����� �� ���������� �������� � ���� ����.
    /// </summary>
    bool IsStackable { get; }

    /// <summary>
    /// ������������ ���������� ��������� � ����� �����.
    /// </summary>
    int MaxStack { get; }
}
