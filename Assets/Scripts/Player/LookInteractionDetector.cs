using System;
using UnityEngine;

/// <summary>
/// Детектор того, на что смотрит камера игрока.
/// Выполняет raycast с интервалом checkInterval и вызывает события при входе/выходе цели.
/// </summary>
[DisallowMultipleComponent]
public class LookInteractionDetector : MonoBehaviour
{
    #region Events

    /// <summary>Срабатывает, когда под прицелом появляется новый объект ItemPickup.</summary>
    public event Action<ItemPickup> OnTargetEnter;

    /// <summary>Срабатывает, когда объект покидает прицел.</summary>
    public event Action OnTargetExit;

    #endregion

    #region Settings

    [Header("Camera")]
    [Tooltip("Камера, от которой делается raycast. Если не назначена — используется Camera.main.")]
    [SerializeField] private Camera playerCamera;

    [Header("Raycast")]
    [Tooltip("Слои, по которым проверяется raycast.")]
    [SerializeField] private LayerMask pickupLayer = ~0; // по умолчанию — все слои

    [Tooltip("Максимальная дистанция проверки.")]
    [SerializeField] private float lookDistance = 3f;

    [Tooltip("Интервал проверки в секундах.")]
    [SerializeField, Min(0.02f)] private float checkInterval = 0.12f;

    [Tooltip("Включать ли триггеры при raycast.")]
    [SerializeField] private QueryTriggerInteraction triggerInteraction = QueryTriggerInteraction.Collide;

    #endregion

    #region Private Fields

    private ItemPickup current;          // текущий объект под прицелом
    private Vector3 screenCenter;        // центр экрана
    private int lastW, lastH;            // кэш размеров экрана
    private Coroutine loop;              // ссылка на корутину проверки
    private WaitForSeconds wait;         // кэш интервала ожидания

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

    /// <summary>Возвращает текущий объект под прицелом (или null).</summary>
    public ItemPickup GetCurrentLookTarget() => current;

    /// <summary>Позволяет назначить камеру извне.</summary>
    public Camera PlayerCamera
    {
        get => playerCamera ?? Camera.main;
        set => playerCamera = value;
    }

    #endregion

    #region Check Loop

    /// <summary>Запускает цикл проверки raycast.</summary>
    private void StartChecking()
    {
        if (loop != null) return;
        loop = StartCoroutine(CheckLoop());
    }

    /// <summary>Останавливает цикл проверки raycast.</summary>
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

    /// <summary>Корутин, выполняющий raycast с заданным интервалом.</summary>
    private System.Collections.IEnumerator CheckLoop()
    {
        while (true)
        {
            // Обновляем центр экрана, если изменилось разрешение
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

            // Если ничего не найдено
            if (current != null)
            {
                current = null;
                OnTargetExit?.Invoke();
            }

            yield return wait;
        }
    }

    /// <summary>Обновляет кэш центра экрана.</summary>
    private void UpdateScreenCenter()
    {
        lastW = Screen.width;
        lastH = Screen.height;
        screenCenter = new Vector3(lastW * 0.5f, lastH * 0.5f, 0f);
    }

    #endregion
}
