using System;
using UnityEngine;

/// <summary>
/// �������� ����, �� ��� ������� ������ ������.
/// ��������� raycast � ���������� checkInterval � �������� ������� ��� �����/������ ����.
/// </summary>
[DisallowMultipleComponent]
public class LookInteractionDetector : MonoBehaviour
{
    #region Events

    /// <summary>�����������, ����� ��� �������� ���������� ����� ������ ItemPickup.</summary>
    public event Action<ItemPickup> OnTargetEnter;

    /// <summary>�����������, ����� ������ �������� ������.</summary>
    public event Action OnTargetExit;

    #endregion

    #region Settings

    [Header("Camera")]
    [Tooltip("������, �� ������� �������� raycast. ���� �� ��������� � ������������ Camera.main.")]
    [SerializeField] private Camera playerCamera;

    [Header("Raycast")]
    [Tooltip("����, �� ������� ����������� raycast.")]
    [SerializeField] private LayerMask pickupLayer = ~0; // �� ��������� � ��� ����

    [Tooltip("������������ ��������� ��������.")]
    [SerializeField] private float lookDistance = 3f;

    [Tooltip("�������� �������� � ��������.")]
    [SerializeField, Min(0.02f)] private float checkInterval = 0.12f;

    [Tooltip("�������� �� �������� ��� raycast.")]
    [SerializeField] private QueryTriggerInteraction triggerInteraction = QueryTriggerInteraction.Collide;

    #endregion

    #region Private Fields

    private ItemPickup current;          // ������� ������ ��� ��������
    private Vector3 screenCenter;        // ����� ������
    private int lastW, lastH;            // ��� �������� ������
    private Coroutine loop;              // ������ �� �������� ��������
    private WaitForSeconds wait;         // ��� ��������� ��������

    #endregion

    #region Unity Methods

    private void Awake()
    {
        if (playerCamera == null)
            playerCamera = Camera.main;

        UpdateScreenCenter();
        wait = new WaitForSeconds(checkInterval);
    }

    private void OnEnable() => StartChecking();

    private void OnDisable() => StopChecking();

    #endregion

    #region Public API

    /// <summary>���������� ������� ������ ��� �������� (��� null).</summary>
    public ItemPickup GetCurrentLookTarget() => current;

    /// <summary>��������� ��������� ������ �����.</summary>
    public Camera PlayerCamera
    {
        get => playerCamera ?? Camera.main;
        set => playerCamera = value;
    }

    #endregion

    #region Check Loop

    /// <summary>��������� ���� �������� raycast.</summary>
    private void StartChecking()
    {
        if (loop != null) return;
        loop = StartCoroutine(CheckLoop());
    }

    /// <summary>������������� ���� �������� raycast.</summary>
    private void StopChecking()
    {
        if (loop != null)
        {
            StopCoroutine(loop);
            loop = null;
        }

        if (current != null)
        {
            current = null;
            OnTargetExit?.Invoke();
        }
    }

    /// <summary>�������, ����������� raycast � �������� ����������.</summary>
    private System.Collections.IEnumerator CheckLoop()
    {
        while (true)
        {
            // ��������� ����� ������, ���� ���������� ����������
            if (Screen.width != lastW || Screen.height != lastH)
                UpdateScreenCenter();

            if (playerCamera != null)
            {
                Ray ray = playerCamera.ScreenPointToRay(screenCenter);
                if (Physics.Raycast(ray, out RaycastHit hit, lookDistance, pickupLayer, triggerInteraction))
                {
                    var pickup = hit.collider.GetComponentInParent<ItemPickup>();
                    if (pickup != null)
                    {
                        if (pickup != current)
                        {
                            current = pickup;
                            OnTargetEnter?.Invoke(pickup);
                        }

                        yield return wait;
                        continue;
                    }
                }
            }

            // ���� ������ �� �������
            if (current != null)
            {
                current = null;
                OnTargetExit?.Invoke();
            }

            yield return wait;
        }
    }

    /// <summary>��������� ��� ������ ������.</summary>
    private void UpdateScreenCenter()
    {
        lastW = Screen.width;
        lastH = Screen.height;
        screenCenter = new Vector3(lastW * 0.5f, lastH * 0.5f, 0f);
    }

    #endregion
}
