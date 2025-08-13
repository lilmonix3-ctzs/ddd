using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class WeaponHold : MonoBehaviour
{
    [Header("Weapon Settings")]
    [SerializeField] private Transform weaponHoldPoint;
    [SerializeField] private WeaponSO[] weaponSOs;
    [SerializeField] private float maxDistanceFromPlayer = 1.5f;
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private int bulletPoolSize = 20;
    [SerializeField] private float reloadTime = 1.5f;
    [SerializeField] private float gamepadDeadzone = 0.2f; // 手柄死区阈值

    private int weaponCount;
    private int weaponIndex;
    private Camera mainCamera;
    private Queue<GameObject> bulletPool = new Queue<GameObject>();

    // 射击控制相关变量
    private bool isReloading = false;
    private float fireTimer = 0f;
    private WeaponSO currentWeapon;
    private bool isFireing = false;

    private void Awake()
    {
        mainCamera = Camera.main;
        InitializeBulletPool();
    }

    private void InitializeBulletPool()
    {
        for (int i = 0; i < bulletPoolSize; i++)
        {
            GameObject bullet = Instantiate(bulletPrefab);
            bullet.SetActive(false);
            bulletPool.Enqueue(bullet);
        }
    }

    private GameObject GetBulletFromPool()
    {
        if (bulletPool.Count > 0)
        {
            return bulletPool.Dequeue();
        }
        return Instantiate(bulletPrefab);
    }

    private void ReturnBulletToPool(GameObject bullet)
    {
        bullet.SetActive(false);
        bulletPool.Enqueue(bullet);
    }

    private void Start()
    {
        weaponCount = weaponSOs.Length;
        if (weaponCount > 0)
        {
            EquipWeapon(weaponIndex);
            currentWeapon = GetCurrentWeapon();
        }
    }

    private void Update()
    {
        if (fireTimer > 0)
        {
            fireTimer -= Time.deltaTime;
        }

        UpdateWeaponHoldPoint();
        HandleWeaponInteractions();

        if (!isReloading && bulletPool.Count <= 0)
        {
            StartCoroutine(ReloadBullets());
        }
    }

    private IEnumerator ReloadBullets()
    {
        isReloading = true;

        while (bulletPool.Count < bulletPoolSize)
        {
            yield return new WaitForSeconds(reloadTime / bulletPoolSize);
            GameObject bullet = Instantiate(bulletPrefab);
            bullet.SetActive(false);
            bulletPool.Enqueue(bullet);

            Debug.Log($"Reloading: {bulletPool.Count} / {bulletPoolSize}");
        }

        isReloading = false;
    }

    private void HandleWeaponInteractions()
    {
        if (GameInput.Instance.IsSwitchWeaponClicked())
        {
            SwitchWeapon(1);
        }

        if (GameInput.Instance.IsAttackPressed())
        {
            if (currentWeapon != null)
            {
                TryFireBullet();
            }
        }
        else
        {
            isFireing = false;
        }
    }

    private void TryFireBullet()
    {
        if (isReloading || fireTimer > 0)
        {
            isFireing = !isReloading;
            return;
        }

        if (bulletPool.Count <= 0)
        {
            isFireing = false;
            StartCoroutine(ReloadBullets());
            return;
        }
        isFireing = true;
        FireBullet();
        fireTimer = 1f / currentWeapon.attackSpeed;
    }

    private void FireBullet()
    {
        GameObject bullet = GetBulletFromPool();
        bullet.transform.position = firePoint.position;
        bullet.transform.rotation = firePoint.rotation;
        bullet.SetActive(true);

        Bullet bulletScript = bullet.GetComponent<Bullet>();
        if (bulletScript != null)
        {
            bulletScript.Initialize(
                firePoint.rotation,
                currentWeapon.bulletSpeed,
                currentWeapon.attackRange,
                currentWeapon.damage);
        }
    }

    // 修改点：更新武器持有点位置和朝向（支持手柄和鼠标）
    private void UpdateWeaponHoldPoint()
    {
        if (weaponHoldPoint == null) return;

        Vector3 playerPosition = transform.position;
        Vector3 aimDirection = Vector3.zero;
        bool usingGamepad = false;

        // 获取手柄瞄准输入
        Vector2 gamepadAim = GameInput.Instance.GetAimDir();

        // 检查手柄输入是否超过死区
        if (gamepadAim.magnitude > gamepadDeadzone)
        {
            aimDirection = new Vector3(gamepadAim.x, gamepadAim.y, 0f);
            usingGamepad = true;
        }
        // 如果没有手柄输入，则使用鼠标
        else
        {
            Vector3 mouseWorldPosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            mouseWorldPosition.z = 0f;
            aimDirection = (mouseWorldPosition - playerPosition);
        }

        // 限制距离不超过最大范围
        if (aimDirection.magnitude > maxDistanceFromPlayer)
        {
            aimDirection = aimDirection.normalized * maxDistanceFromPlayer;
        }

        // 设置持有点位置
        weaponHoldPoint.position = playerPosition + aimDirection;

        // 使持有点始终面向瞄准方向
        if (aimDirection != Vector3.zero)
        {
            float angle = Mathf.Atan2(aimDirection.y, aimDirection.x) * Mathf.Rad2Deg;
            weaponHoldPoint.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        }

        // 当瞄准方向在玩家左侧时，翻转武器
        if (aimDirection.x < 0)
        {
            weaponHoldPoint.localScale = new Vector3(1f, -1f, 1f);
        }
        else
        {
            weaponHoldPoint.localScale = new Vector3(1f, 1f, 1f);
        }
    }

    public void EquipWeapon(int index)
    {
        if (index < 0 || index >= weaponCount) return;

        foreach (Transform child in weaponHoldPoint)
        {
            if (child.CompareTag("Weapon"))
            {
                Destroy(child.gameObject);
            }
        }

        WeaponSO weaponSO = weaponSOs[index];
        if (weaponSO.prefab != null)
        {
            Transform weaponInstance = Instantiate(
                weaponSO.prefab,
                weaponHoldPoint.position,
                weaponHoldPoint.rotation,
                weaponHoldPoint
            );
            weaponInstance.name = weaponSO.objectName;
            weaponInstance.tag = "Weapon";
            currentWeapon = weaponSO;
        }
    }

    public void SwitchWeapon(int direction)
    {
        weaponIndex += direction;
        if (weaponIndex < 0) weaponIndex = weaponCount - 1;
        if (weaponIndex >= weaponCount) weaponIndex = 0;
        EquipWeapon(weaponIndex);
    }

    public WeaponSO GetCurrentWeapon()
    {
        if (weaponIndex < 0 || weaponIndex >= weaponCount) return null;
        return weaponSOs[weaponIndex];
    }

    public int GetWeaponCount() => weaponCount;
    public int GetCurrentWeaponIndex() => weaponIndex;
    public bool IsFiring() => isFireing;
}