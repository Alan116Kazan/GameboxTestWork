using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Контроллер броска объектов (камней).
/// </summary>
[DisallowMultipleComponent]
public class ThrowController : MonoBehaviour
{
    #region Inspector Fields

    [Header("References")]
    [Tooltip("Ссылка на PlayerInputBridge для обработки InputAction Throw")]
    [SerializeField] private PlayerInputBridge inputBridge;
    [Tooltip("Камера игрока, от которой производится бросок")]
    [SerializeField] private Camera playerCamera;

    [Header("Throw Settings")]
    [Tooltip("Сила броска")]
    [SerializeField, Min(0f)] private float throwForce = 8f;
    [Tooltip("Угол броска вверх относительно направления камеры")]
    [SerializeField, Range(-45f, 45f)] private float throwAngleDeg = 15f;
    [Tooltip("Смещение позиции спавна вперёд от камеры")]
    [SerializeField, Min(0f)] private float spawnOffset = 1f;
    [Tooltip("Вертикальное смещение спавна (по Y)")]
    [SerializeField] private float verticalOffset = -0.2f;
    [Tooltip("Радиус сферы (камня)")]
    [SerializeField, Min(0.01f)] private float sphereRadius = 0.1f;
    [Tooltip("Масса сферы (камня)")]
    [SerializeField, Min(0f)] private float sphereMass = 0.2f;
    [Tooltip("Время жизни объекта до автоматического удаления")]
    [SerializeField, Min(0f)] private float thrownLifetime = 10f;
    [Tooltip("Слои, на которых будет объект")]
    [SerializeField] private LayerMask physicsLayer = ~0;

    #endregion

    #region Unity Callbacks

    private void Reset()
    {
        // Значения по умолчанию при добавлении компонента
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
        // Автоматически находим inputBridge и камеру, если не назначены
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
    /// Callback при активации действия броска.
    /// </summary>
    /// <param name="ctx">Контекст InputAction</param>
    private void OnThrowPerformed(InputAction.CallbackContext ctx)
    {
        SpawnAndThrowStone();
    }

    /// <summary>
    /// Создаёт сферический объект и бросает его с заданными параметрами.
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

        // Рассчитываем позицию спавна
        Vector3 spawnPos = playerCamera.transform.position
                           + playerCamera.transform.forward * spawnOffset
                           + Vector3.up * verticalOffset;

        // Создаём сферу (камень)
        GameObject stone = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        stone.name = "ThrownStone";
        stone.transform.position = spawnPos;
        stone.transform.localScale = Vector3.one * (sphereRadius * 2f);

        stone.layer = LayerMaskToLayer(physicsLayer);

        // Добавляем Rigidbody
        Rigidbody rb = stone.GetComponent<Rigidbody>();
        if (rb == null) rb = stone.AddComponent<Rigidbody>();
        rb.mass = Mathf.Max(0.0001f, sphereMass);
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

        // Рассчитываем направление броска с учётом угла
        Vector3 throwDirection = Quaternion.AngleAxis(throwAngleDeg, playerCamera.transform.right) * playerCamera.transform.forward;
        throwDirection.Normalize();

        // Применяем импульс
        rb.AddForce(throwDirection * throwForce, ForceMode.VelocityChange);

        // Случайное вращение для реализма
        rb.AddTorque(Random.insideUnitSphere * (throwForce * 0.2f), ForceMode.VelocityChange);

        // Автоудаление через заданное время
        if (thrownLifetime > 0f)
            Destroy(stone, thrownLifetime);
    }

    /// <summary>
    /// Конвертирует LayerMask в индекс первого активного слоя.
    /// </summary>
    /// <param name="mask">LayerMask</param>
    /// <returns>Индекс слоя</returns>
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
