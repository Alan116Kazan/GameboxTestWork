using System;
using UnityEngine;

/// <summary>
/// Логика отдельного оружия игрока:
/// - стрельба, магазин, перезарядка
/// - дульная вспышка (ParticleSystem)
/// - Raycast попадания и декали
/// - интеграция с Inventory для патрон
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class EquippedWeapon : MonoBehaviour
{
    #region Serialized Fields

    [Header("Weapon Data")]
    [Tooltip("Ссылка на ScriptableObject с параметрами оружия")]
    public WeaponItemSO weaponData;

    [Header("Audio")]
    public AudioClip fireClip;
    public AudioClip emptyClip;

    [Header("VFX")]
    [Tooltip("Эффект дульной вспышки")]
    [SerializeField] private ParticleSystem muzzleFlash;
    [Tooltip("Точка выхода пуль, где запускается вспышка")]
    [SerializeField] private Transform muzzlePoint;

    [Header("Hit / Decal Settings")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private float hitRange = 100f;
    [SerializeField] private LayerMask hitMask = ~0;
    [SerializeField] private GameObject hitDecalPrefab;
    [SerializeField] private Vector3 decalScale = new Vector3(0.25f, 0.25f, 0.25f);
    [SerializeField] private float decalOffset = 0.01f;
    [SerializeField] private float decalLifetime = 10f;
    [SerializeField] private bool attachDecalToHitObject = true;
    [SerializeField] private QueryTriggerInteraction triggerInteraction = QueryTriggerInteraction.Ignore;

    #endregion

    #region Public Properties

    public int CurrentMagazine { get; private set; } = 0;

    #endregion

    #region Events

    /// <summary>Событие для UI: (текущий магазин, патроны в инвентаре)</summary>
    public event Action<int, int> OnAmmoChanged;

    /// <summary>Событие попадания (RaycastHit)</summary>
    public event Action<RaycastHit> OnHitRegistered;

    #endregion

    #region Private Fields

    private AudioSource audioSource;
    private Inventory inventoryRef;
    private Coroutine autoFireCoroutine;
    private Transform decalsParent;

    #endregion

    #region Unity Callbacks

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();

        if (playerCamera == null)
            playerCamera = Camera.main;

        // Создаём контейнер для декалей
        var go = new GameObject("HitDecals");
        go.transform.SetParent(transform.root, false);
        decalsParent = go.transform;
    }

    private void OnDestroy()
    {
        if (inventoryRef != null)
            inventoryRef.OnInventoryChanged -= OnInventoryChanged;
    }

    #endregion

    #region Initialization

    /// <summary>
    /// Инициализация оружия: данные, патроны, ссылка на инвентарь
    /// </summary>
    public void Initialize(WeaponItemSO data, int initialLoaded, Inventory inv)
    {
        weaponData = data;
        inventoryRef = inv;
        CurrentMagazine = Mathf.Clamp(initialLoaded, 0, data != null ? data.magazineSize : initialLoaded);

        if (inventoryRef != null)
            inventoryRef.OnInventoryChanged += OnInventoryChanged;

        UpdateAmmoUI();
    }

    private void OnInventoryChanged() => UpdateAmmoUI();

    #endregion

    #region Fire / AutoFire

    public void StartFiring()
    {
        if (weaponData == null) return;

        if (weaponData.isAutomatic)
        {
            if (autoFireCoroutine == null)
                autoFireCoroutine = StartCoroutine(AutoFireLoop());
        }
        else
        {
            TryFire();
        }
    }

    public void StopFiring()
    {
        if (autoFireCoroutine != null)
        {
            StopCoroutine(autoFireCoroutine);
            autoFireCoroutine = null;
        }
    }

    private System.Collections.IEnumerator AutoFireLoop()
    {
        while (true)
        {
            TryFire();
            yield return new WaitForSeconds(weaponData.fireRate);
        }
    }

    /// <summary>
    /// Попытка выстрела: уменьшает магазин, проигрывает звук, запускает эффекты и регистрирует попадание
    /// </summary>
    public bool TryFire()
    {
        if (CurrentMagazine > 0)
        {
            CurrentMagazine--;
            audioSource?.PlayOneShot(fireClip);

            PlayMuzzleFlash();
            RegisterHitByRayFromCamera();

            UpdateAmmoUI();
            return true;
        }
        else
        {
            audioSource?.PlayOneShot(emptyClip);
            UpdateAmmoUI();
            return false;
        }
    }

    #endregion

    #region MuzzleFlash

    private void PlayMuzzleFlash()
    {
        if (muzzleFlash == null || muzzlePoint == null) return;

        muzzleFlash.transform.position = muzzlePoint.position;
        muzzleFlash.transform.rotation = muzzlePoint.rotation;
        muzzleFlash.Play();
    }

    #endregion

    #region Hit / Decals

    private void RegisterHitByRayFromCamera()
    {
        if (playerCamera == null)
        {
            playerCamera = Camera.main;
            if (playerCamera == null)
            {
                Debug.LogWarning("EquippedWeapon: playerCamera не назначена и Camera.main не найдена.");
                return;
            }
        }

        Vector3 origin = playerCamera.transform.position;
        Vector3 dir = playerCamera.transform.forward;

        if (Physics.Raycast(origin, dir, out RaycastHit hit, hitRange, hitMask, triggerInteraction))
        {
            CreateDecalAt(hit.point, hit.normal, hit.collider != null ? hit.collider.transform : null);
            OnHitRegistered?.Invoke(hit);
        }
    }

    private void CreateDecalAt(Vector3 worldPosition, Vector3 normal, Transform hitTransform)
    {
        GameObject decal;

        if (hitDecalPrefab != null)
        {
            decal = Instantiate(hitDecalPrefab, decalsParent);
            decal.transform.position = worldPosition + normal * decalOffset;
            decal.transform.rotation = Quaternion.LookRotation(normal);
            decal.transform.localScale = decalScale;
        }
        else
        {
            decal = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            decal.name = "HitMarker";
            decal.transform.SetParent(decalsParent, true);
            decal.transform.position = worldPosition + normal * decalOffset;
            float s = Mathf.Clamp(Mathf.Min(decalScale.x, decalScale.y, decalScale.z), 0.02f, 0.5f);
            decal.transform.localScale = Vector3.one * s;

            var r = decal.GetComponent<Renderer>();
            if (r != null)
            {
                var mat = new Material(Shader.Find("Standard"));
                mat.SetColor("_Color", Color.yellow);
                r.material = mat;
            }

            var col = decal.GetComponent<Collider>();
            if (col != null) Destroy(col);
        }

        if (attachDecalToHitObject && hitTransform != null)
            decal.transform.SetParent(hitTransform, true);

        if (decalLifetime > 0f)
            Destroy(decal, decalLifetime);
    }

    #endregion

    #region Ammo Management

    public void AddAmmo(int amount)
    {
        if (weaponData == null || amount <= 0) return;
        CurrentMagazine = Mathf.Clamp(CurrentMagazine + amount, 0, weaponData.magazineSize);
        UpdateAmmoUI();
    }

    private void UpdateAmmoUI()
    {
        int totalInInv = GetTotalAmmoInInventory();
        OnAmmoChanged?.Invoke(CurrentMagazine, totalInInv);
    }

    public int GetTotalAmmoInInventory()
    {
        if (weaponData == null || weaponData.ammoItemReference == null || inventoryRef == null) return 0;
        return inventoryRef.GetTotalQuantity(weaponData.ammoItemReference.Id);
    }

    #endregion

    #region Holster / Unholster

    public void OnHolstered()
    {
        StopFiring();
        if (inventoryRef != null) inventoryRef.OnInventoryChanged -= OnInventoryChanged;
        gameObject.SetActive(false);
    }

    public void OnUnholstered(Inventory inv)
    {
        inventoryRef = inv;
        if (inventoryRef != null) inventoryRef.OnInventoryChanged += OnInventoryChanged;
        gameObject.SetActive(true);
        UpdateAmmoUI();
    }

    #endregion
}
