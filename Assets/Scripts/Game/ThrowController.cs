using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// ���������� ������ �������� (������).
/// </summary>
[DisallowMultipleComponent]
public class ThrowController : MonoBehaviour
{
    #region Inspector Fields

    [Header("References")]
    [Tooltip("������ �� PlayerInputBridge ��� ��������� InputAction Throw")]
    [SerializeField] private PlayerInputBridge inputBridge;
    [Tooltip("������ ������, �� ������� ������������ ������")]
    [SerializeField] private Camera playerCamera;

    [Header("Throw Settings")]
    [Tooltip("���� ������")]
    [SerializeField, Min(0f)] private float throwForce = 8f;
    [Tooltip("���� ������ ����� ������������ ����������� ������")]
    [SerializeField, Range(-45f, 45f)] private float throwAngleDeg = 15f;
    [Tooltip("�������� ������� ������ ����� �� ������")]
    [SerializeField, Min(0f)] private float spawnOffset = 1f;
    [Tooltip("������������ �������� ������ (�� Y)")]
    [SerializeField] private float verticalOffset = -0.2f;
    [Tooltip("������ ����� (�����)")]
    [SerializeField, Min(0.01f)] private float sphereRadius = 0.1f;
    [Tooltip("����� ����� (�����)")]
    [SerializeField, Min(0f)] private float sphereMass = 0.2f;
    [Tooltip("����� ����� ������� �� ��������������� ��������")]
    [SerializeField, Min(0f)] private float thrownLifetime = 10f;
    [Tooltip("����, �� ������� ����� ������")]
    [SerializeField] private LayerMask physicsLayer = ~0;

    #endregion

    #region Unity Callbacks

    private void Reset()
    {
        // �������� �� ��������� ��� ���������� ����������
        throwForce = 8f;
        throwAngleDeg = 15f;
        spawnOffset = 1f;
        verticalOffset = -0.2f;
        sphereRadius = 0.12f;
        sphereMass = 1f;
        thrownLifetime = 20f;
    }

    private void Awake()
    {
        // ������������� ������� inputBridge � ������, ���� �� ���������
        if (inputBridge == null) inputBridge = FindObjectOfType<PlayerInputBridge>();
        if (playerCamera == null) playerCamera = Camera.main;
    }

    private void OnEnable()
    {
        if (inputBridge?.Throw != null)
        {
            inputBridge.Throw.performed += OnThrowPerformed;
            inputBridge.Throw.Enable();
        }
    }

    private void OnDisable()
    {
        if (inputBridge?.Throw != null)
        {
            inputBridge.Throw.performed -= OnThrowPerformed;
            inputBridge.Throw.Disable();
        }
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Callback ��� ��������� �������� ������.
    /// </summary>
    /// <param name="ctx">�������� InputAction</param>
    private void OnThrowPerformed(InputAction.CallbackContext ctx)
    {
        SpawnAndThrowStone();
    }

    /// <summary>
    /// ������ ����������� ������ � ������� ��� � ��������� �����������.
    /// </summary>
    private void SpawnAndThrowStone()
    {
        if (playerCamera == null)
        {
            playerCamera = Camera.main;
            if (playerCamera == null)
            {
                Debug.LogWarning("ThrowController: Camera not found.");
                return;
            }
        }

        // ������������ ������� ������
        Vector3 spawnPos = playerCamera.transform.position
                           + playerCamera.transform.forward * spawnOffset
                           + Vector3.up * verticalOffset;

        // ������ ����� (������)
        GameObject stone = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        stone.name = "ThrownStone";
        stone.transform.position = spawnPos;
        stone.transform.localScale = Vector3.one * (sphereRadius * 2f);

        stone.layer = LayerMaskToLayer(physicsLayer);

        // ��������� Rigidbody
        Rigidbody rb = stone.GetComponent<Rigidbody>();
        if (rb == null) rb = stone.AddComponent<Rigidbody>();
        rb.mass = Mathf.Max(0.0001f, sphereMass);
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

        // ������������ ����������� ������ � ������ ����
        Vector3 throwDirection = Quaternion.AngleAxis(throwAngleDeg, playerCamera.transform.right) * playerCamera.transform.forward;
        throwDirection.Normalize();

        // ��������� �������
        rb.AddForce(throwDirection * throwForce, ForceMode.VelocityChange);

        // ��������� �������� ��� ��������
        rb.AddTorque(Random.insideUnitSphere * (throwForce * 0.2f), ForceMode.VelocityChange);

        // ������������ ����� �������� �����
        if (thrownLifetime > 0f)
            Destroy(stone, thrownLifetime);
    }

    /// <summary>
    /// ������������ LayerMask � ������ ������� ��������� ����.
    /// </summary>
    /// <param name="mask">LayerMask</param>
    /// <returns>������ ����</returns>
    private int LayerMaskToLayer(LayerMask mask)
    {
        int layer = 0;
        int val = mask.value;
        while (val > 0)
        {
            if ((val & 1) == 1) return layer;
            val >>= 1;
            layer++;
        }
        return 0; // fallback
    }

    #endregion
}
