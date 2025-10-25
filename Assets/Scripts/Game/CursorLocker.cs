using UnityEngine;
using System;

/// <summary>
/// Глобальный менеджер состояния курсора.
/// Отвечает за блокировку/разблокировку и скрытие/отображение курсора.
/// Используется как singleton: CursorLocker.Instance.
/// </summary>
[DisallowMultipleComponent]
public class CursorLocker : MonoBehaviour
{
    #region Singleton
    /// <summary>
    /// Единственный экземпляр менеджера курсора.
    /// Позволяет обращаться к функционалу из любого места.
    /// </summary>
    public static CursorLocker Instance { get; private set; }
    #endregion

    #region Inspector Fields
    [Header("Настройки при старте")]
    [Tooltip("Блокировать курсор при старте")]
    [SerializeField] private bool lockOnStart = true;

    [Tooltip("Скрывать курсор при старте")]
    [SerializeField] private bool hideOnStart = true;
    #endregion

    #region Properties
    /// <summary>
    /// Доступ к настройке блокировки курсора при старте.
    /// Можно изменять в рантайме.
    /// </summary>
    public bool LockOnStart
    {
        get => lockOnStart;
        set => lockOnStart = value;
    }

    /// <summary>
    /// Доступ к настройке скрытия курсора при старте.
    /// Можно изменять в рантайме.
    /// </summary>
    public bool HideOnStart
    {
        get => hideOnStart;
        set => hideOnStart = value;
    }
    #endregion

    #region Events
    /// <summary>
    /// Событие вызывается при смене состояния курсора.
    /// bool первый параметр: заблокирован ли курсор
    /// bool второй параметр: скрыт ли курсор
    /// </summary>
    public event Action<bool, bool> OnCursorStateChanged;
    #endregion

    #region Unity Callbacks
    private void Awake()
    {
        // Реализация паттерна Singleton
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); // Если уже есть экземпляр, уничтожаем дублирующий объект
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject); // Сохраняем объект между сценами
    }

    private void Start()
    {
        // Применяем стартовое состояние курсора
        Apply(lockOnStart, hideOnStart);
    }
    #endregion

    #region Public Methods
    /// <summary>
    /// Применяет новое состояние курсора.
    /// </summary>
    /// <param name="shouldLock">Блокировать курсор (true) или нет (false)</param>
    /// <param name="shouldHide">Скрывать курсор (true) или показывать (false)</param>
    public void Apply(bool shouldLock, bool shouldHide)
    {
        Cursor.lockState = shouldLock ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !shouldHide;

        // Уведомляем подписчиков о смене состояния
        OnCursorStateChanged?.Invoke(shouldLock, shouldHide);
    }

    /// <summary>
    /// Разблокировать курсор и показать его.
    /// </summary>
    public void UnlockCursor() => Apply(false, false);

    /// <summary>
    /// Заблокировать курсор и скрыть его.
    /// </summary>
    public void LockCursor() => Apply(true, true);
    #endregion
}
