using UnityEngine;
using UnityEngine.InputSystem;
using Cinemachine;

/// <summary>
/// Контроллер персонажа от первого лица.
/// Обрабатывает движение, прыжок, взгляд мышью и прицеливание (FOV).
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class FirstPersonController : MonoBehaviour
{
    #region References

    [Header("References")]
    [Tooltip("Скрипт для работы с Input System.")]
    [SerializeField] private PlayerInputBridge inputBridge;

    [Tooltip("Точка поворота камеры (для вертикального вращения).")]
    [SerializeField] private Transform cameraPivot;

    [Tooltip("Cinemachine Virtual Camera для плавного изменения FOV при прицеливании.")]
    [SerializeField] private CinemachineVirtualCamera vcam;

    #endregion

    #region Movement Settings

    [Header("Movement")]
    [SerializeField, Tooltip("Скорость ходьбы в м/с.")] private float walkSpeed = 5f;
    [SerializeField, Tooltip("Множитель скорости при прицеливании.")] private float aimSpeedMultiplier = 0.5f;
    [SerializeField, Tooltip("Сила прыжка.")] private float jumpForce = 5f;
    [SerializeField, Tooltip("Гравитация (отрицательное значение).")] private float gravity = -9.81f;
    [SerializeField, Tooltip("Снижение контроля в воздухе.")] private float airControlMultiplier = 0.25f;

    [Header("Acceleration / Tuning")]
    [SerializeField, Tooltip("Ускорение на земле.")] private float groundAcceleration = 50f;
    [SerializeField, Tooltip("Торможение на земле при отсутствии ввода.")] private float groundDeceleration = 80f;

    #endregion

    #region Look Settings

    [Header("Mouse / Look")]
    [SerializeField, Tooltip("Чувствительность мыши.")] private float mouseSensitivity = 1.2f;
    [SerializeField, Tooltip("Инверсия оси Y (1 или -1).")] private float invertY = 1f;
    [SerializeField, Tooltip("Минимальный угол наклона камеры.")] private float minPitch = -80f;
    [SerializeField, Tooltip("Максимальный угол наклона камеры.")] private float maxPitch = 80f;

    #endregion

    #region FOV / Aim

    [Header("FOV / Aim")]
    [SerializeField, Tooltip("Обычное поле зрения камеры.")] private float normalFOV = 60f;
    [SerializeField, Tooltip("Поле зрения при прицеливании.")] private float aimFOV = 45f;
    [SerializeField, Tooltip("Скорость интерполяции FOV.")] private float fovSpeed = 8f;

    #endregion

    #region Private Fields

    private CharacterController cc;
    private Vector3 horizontalVelocity = Vector3.zero;
    private float verticalVelocity;
    private float pitch = 0f;
    private float yaw = 0f;

    #endregion

    #region Unity Methods

    private void Awake()
    {
        cc = GetComponent<CharacterController>();
        if (cameraPivot == null)
            Debug.LogWarning("CameraPivot не назначен в инспекторе!");
    }

    private void OnEnable()
    {
        // Включаем InputActions
        inputBridge?.Move?.Enable();
        inputBridge?.Look?.Enable();
        inputBridge?.Jump?.Enable();
        inputBridge?.Aim?.Enable();
        inputBridge?.Fire?.Enable();
        inputBridge?.Interact?.Enable();
        inputBridge?.NextWeapon?.Enable();
        inputBridge?.Quick1?.Enable();
        inputBridge?.Quick2?.Enable();
        inputBridge?.Throw?.Enable();

        if (inputBridge?.Jump != null)
            inputBridge.Jump.performed += OnJumpPerformed;
    }

    private void OnDisable()
    {
        if (inputBridge?.Jump != null)
            inputBridge.Jump.performed -= OnJumpPerformed;
    }

    private void Update()
    {
        HandleLook();
        HandleMove();
        HandleFOV();
    }

    #endregion

    #region Look / Mouse

    /// <summary>
    /// Обработка вращения персонажа и камеры мышью.
    /// </summary>
    private void HandleLook()
    {
        if (inputBridge?.Look == null) return;

        Vector2 look = inputBridge.Look.ReadValue<Vector2>();
        float deltaX = look.x * mouseSensitivity;
        float deltaY = look.y * mouseSensitivity * invertY;

        yaw += deltaX;
        transform.localRotation = Quaternion.Euler(0f, yaw, 0f);

        pitch = Mathf.Clamp(pitch - deltaY, minPitch, maxPitch);
        if (cameraPivot != null)
            cameraPivot.localRotation = Quaternion.Euler(pitch, 0f, 0f);
    }

    #endregion

    #region Movement / Jump

    /// <summary>
    /// Обработка движения персонажа с учетом ускорения, замедления, прицеливания и гравитации.
    /// </summary>
    private void HandleMove()
    {
        if (inputBridge?.Move == null) return;

        Vector2 move = inputBridge.Move.ReadValue<Vector2>();
        Vector3 forward = transform.forward;
        Vector3 right = transform.right;

        Vector3 desiredDir = forward * move.y + right * move.x;
        if (desiredDir.sqrMagnitude > 1f) desiredDir.Normalize();

        bool grounded = cc.isGrounded;
        if (grounded && verticalVelocity < 0f) verticalVelocity = -1f;

        bool aiming = (inputBridge.Aim != null && inputBridge.Aim.ReadValue<float>() > 0.5f);
        float maxSpeed = walkSpeed * (aiming ? aimSpeedMultiplier : 1f);

        Vector3 targetHorizontal = desiredDir * maxSpeed;
        float accel = grounded ? groundAcceleration : groundAcceleration * airControlMultiplier;

        // Движение по горизонтали
        if (grounded && desiredDir.sqrMagnitude < 0.001f)
            horizontalVelocity = Vector3.MoveTowards(horizontalVelocity, Vector3.zero, groundDeceleration * Time.deltaTime);
        else
            horizontalVelocity = Vector3.MoveTowards(horizontalVelocity, targetHorizontal, accel * Time.deltaTime);

        // Вертикальная скорость и гравитация
        if (!grounded)
            verticalVelocity += gravity * Time.deltaTime;

        Vector3 totalVelocity = horizontalVelocity + Vector3.up * verticalVelocity;
        cc.Move(totalVelocity * Time.deltaTime);
    }

    /// <summary>
    /// Обработка прыжка.
    /// </summary>
    private void OnJumpPerformed(InputAction.CallbackContext ctx)
    {
        if (cc.isGrounded)
            verticalVelocity = jumpForce;
    }

    #endregion

    #region FOV / Aim

    /// <summary>
    /// Плавное изменение FOV камеры при прицеливании.
    /// </summary>
    private void HandleFOV()
    {
        if (vcam == null) return;

        bool aiming = (inputBridge.Aim != null && inputBridge.Aim.ReadValue<float>() > 0.5f);
        float targetFOV = aiming ? aimFOV : normalFOV;

        var lens = vcam.m_Lens;
        lens.FieldOfView = Mathf.Lerp(lens.FieldOfView, targetFOV, Time.deltaTime * fovSpeed);
        vcam.m_Lens = lens;
    }

    #endregion
}
