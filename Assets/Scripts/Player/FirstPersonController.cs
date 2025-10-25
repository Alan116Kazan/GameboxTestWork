using UnityEngine;
using UnityEngine.InputSystem;
using Cinemachine;

/// <summary>
/// ���������� ��������� �� ������� ����.
/// ������������ ��������, ������, ������ ����� � ������������ (FOV).
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class FirstPersonController : MonoBehaviour
{
    #region References

    [Header("References")]
    [Tooltip("������ ��� ������ � Input System.")]
    [SerializeField] private PlayerInputBridge inputBridge;

    [Tooltip("����� �������� ������ (��� ������������� ��������).")]
    [SerializeField] private Transform cameraPivot;

    [Tooltip("Cinemachine Virtual Camera ��� �������� ��������� FOV ��� ������������.")]
    [SerializeField] private CinemachineVirtualCamera vcam;

    #endregion

    #region Movement Settings

    [Header("Movement")]
    [SerializeField, Tooltip("�������� ������ � �/�.")] private float walkSpeed = 5f;
    [SerializeField, Tooltip("��������� �������� ��� ������������.")] private float aimSpeedMultiplier = 0.5f;
    [SerializeField, Tooltip("���� ������.")] private float jumpForce = 5f;
    [SerializeField, Tooltip("���������� (������������� ��������).")] private float gravity = -9.81f;
    [SerializeField, Tooltip("�������� �������� � �������.")] private float airControlMultiplier = 0.25f;

    [Header("Acceleration / Tuning")]
    [SerializeField, Tooltip("��������� �� �����.")] private float groundAcceleration = 50f;
    [SerializeField, Tooltip("���������� �� ����� ��� ���������� �����.")] private float groundDeceleration = 80f;

    #endregion

    #region Look Settings

    [Header("Mouse / Look")]
    [SerializeField, Tooltip("���������������� ����.")] private float mouseSensitivity = 1.2f;
    [SerializeField, Tooltip("�������� ��� Y (1 ��� -1).")] private float invertY = 1f;
    [SerializeField, Tooltip("����������� ���� ������� ������.")] private float minPitch = -80f;
    [SerializeField, Tooltip("������������ ���� ������� ������.")] private float maxPitch = 80f;

    #endregion

    #region FOV / Aim

    [Header("FOV / Aim")]
    [SerializeField, Tooltip("������� ���� ������ ������.")] private float normalFOV = 60f;
    [SerializeField, Tooltip("���� ������ ��� ������������.")] private float aimFOV = 45f;
    [SerializeField, Tooltip("�������� ������������ FOV.")] private float fovSpeed = 8f;

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
            Debug.LogWarning("CameraPivot �� �������� � ����������!");
    }

    private void OnEnable()
    {
        // �������� InputActions
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
    /// ��������� �������� ��������� � ������ �����.
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
    /// ��������� �������� ��������� � ������ ���������, ����������, ������������ � ����������.
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

        // �������� �� �����������
        if (grounded && desiredDir.sqrMagnitude < 0.001f)
            horizontalVelocity = Vector3.MoveTowards(horizontalVelocity, Vector3.zero, groundDeceleration * Time.deltaTime);
        else
            horizontalVelocity = Vector3.MoveTowards(horizontalVelocity, targetHorizontal, accel * Time.deltaTime);

        // ������������ �������� � ����������
        if (!grounded)
            verticalVelocity += gravity * Time.deltaTime;

        Vector3 totalVelocity = horizontalVelocity + Vector3.up * verticalVelocity;
        cc.Move(totalVelocity * Time.deltaTime);
    }

    /// <summary>
    /// ��������� ������.
    /// </summary>
    private void OnJumpPerformed(InputAction.CallbackContext ctx)
    {
        if (cc.isGrounded)
            verticalVelocity = jumpForce;
    }

    #endregion

    #region FOV / Aim

    /// <summary>
    /// ������� ��������� FOV ������ ��� ������������.
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
