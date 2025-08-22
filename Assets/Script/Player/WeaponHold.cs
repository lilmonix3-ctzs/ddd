using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class WeaponHold : MonoBehaviour
{
    //[Header("Animator Settings")]
    Animator animator; // 动画控制器
    private string ShootingAni = "Shoot";
    private string ReloadAni = "Reload";

    [Header("Weapon Settings")]
    [SerializeField] private Transform weaponHoldPoint;
    [SerializeField] private WeaponSO[] weaponSOs;
    [SerializeField] private float maxDistanceFromPlayer = 1.5f;
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private int bulletPoolSize = 20;
    [SerializeField] private float reloadTime = 1.5f;
    [SerializeField] private float gamepadDeadzone = 0.2f; // 手柄死区阈值
    [SerializeField] private Transform Aim;
    [SerializeField] private float aimToMouse = 0.7f; // 瞄准线缩放比
    [SerializeField] private float maxaimScale = 1.5f; // 最大缩放
    [SerializeField] private float autoReloadDelay = 0.5f; // 自动换弹延迟时间

    private int weaponCount;
    private int weaponIndex;
    private Camera mainCamera;
    private Queue<GameObject> bulletPool = new Queue<GameObject>();

    // 射击控制相关变量
    private bool isReloading = false;
    private bool isAutoReload = false;
    private float fireTimer = 0f;
    private WeaponSO currentWeapon;
    private bool isFireing = false;
    private float timeSinceLastFire = 0f; // 上次射击后的时间
    private Coroutine reloadCoroutine; // 换弹协程引用
    private Coroutine autoReloadCoroutine; // 自动换弹协程引用

    private void Awake()
    {
        mainCamera = Camera.main;
        InitializeBulletPool();
    }

    private void InitializeBulletPool()
    {
        // 清空现有的子弹池
        while (bulletPool.Count > 0)
        {
            GameObject bullet = bulletPool.Dequeue();
            Destroy(bullet);
        }

        for (int i = 0; i < bulletPoolSize; i++)
        {
            GameObject bullet = Instantiate(bulletPrefab);
            bullet.SetActive(false);
            bulletPool.Enqueue(bullet);
        }
        Debug.Log($"初始化弹夹: {bulletPool.Count}/{bulletPoolSize}");
    }

    // 新增方法：装满弹夹
    private void RefillMagazine()
    {
        // 清空现有的子弹池
        while (bulletPool.Count > 0)
        {
            GameObject bullet = bulletPool.Dequeue();
            Destroy(bullet);
        }

        // 创建满弹夹
        for (int i = 0; i < bulletPoolSize; i++)
        {
            GameObject bullet = Instantiate(bulletPrefab);
            bullet.SetActive(false);
            bulletPool.Enqueue(bullet);
        }

        Debug.Log($"弹夹已装满: {bulletPool.Count}/{bulletPoolSize}");

        // 如果正在换弹，停止换弹
        if (isReloading && reloadCoroutine != null)
        {
            StopCoroutine(reloadCoroutine);
            isReloading = false;
            Debug.Log("换弹被武器切换中断");
        }

        // 如果正在自动换弹，停止自动换弹
        if (isAutoReload && autoReloadCoroutine != null)
        {
            StopCoroutine(autoReloadCoroutine);
            autoReloadCoroutine = null;
            isAutoReload = false;
            Debug.Log("自动换弹被武器切换中断");
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
        HandleWeaponInteractions();
    }

    private void FixedUpdate()
    {
        if (fireTimer > 0)
        {
            fireTimer -= Time.deltaTime;
        }

        UpdateWeaponHoldPoint();
        HandleAnimator();
        HandleAimVisual();

        // 更新上次射击后的时间
        if (!isFireing)
        {
            timeSinceLastFire += Time.deltaTime;
        }
        else
        {
            timeSinceLastFire = 0f;
        }

        // 检查是否需要自动换弹
        CheckAutoReload();
    }

    private void HandleAimVisual()
    {
        if (Aim == null) return;
        if (isReloading || isAutoReload)
        {
            Aim.gameObject.SetActive(false);
        }
        else
        {
            Aim.gameObject.SetActive(true);
        }
    }

    private void HandleAnimator()
    {
        if (animator == null) return;
        animator.SetBool(ShootingAni, isFireing);
        animator.SetBool(ReloadAni, isReloading || isAutoReload);
    }

    // 检查是否需要自动换弹
    private void CheckAutoReload()
    {
        // 如果不在换弹中，子弹池不为空但不满，且一段时间没有射击
        if (!isReloading &&
            bulletPool.Count < bulletPoolSize &&
            bulletPool.Count >= 0 &&
            timeSinceLastFire >= autoReloadDelay)
        {
            // 如果没有正在进行的自动换弹，则开始自动换弹
            if (autoReloadCoroutine == null)
            {
                autoReloadCoroutine = StartCoroutine(AutoReloadBullets());
                isAutoReload = true;
            }
        }
        else if (timeSinceLastFire < autoReloadDelay && autoReloadCoroutine != null)
        {
            // 如果在自动换弹过程中又开始射击，取消自动换弹
            StopCoroutine(autoReloadCoroutine);
            autoReloadCoroutine = null;
            isAutoReload = false;
            Debug.Log("自动换弹被取消");
        }
    }

    // 自动换弹协程
    private IEnumerator AutoReloadBullets()
    {
        Debug.Log("开始自动换弹");

        while (bulletPool.Count < bulletPoolSize && timeSinceLastFire >= autoReloadDelay)
        {
            yield return new WaitForSeconds(reloadTime / bulletPoolSize);

            // 检查是否在换弹过程中开始了射击
            if (timeSinceLastFire < autoReloadDelay)
            {
                Debug.Log("自动换弹被中断");
                isAutoReload = false;
                break;
            }

            GameObject bullet = Instantiate(bulletPrefab);
            bullet.SetActive(false);
            bulletPool.Enqueue(bullet);

            Debug.Log($"自动换弹中: {bulletPool.Count} / {bulletPoolSize}");
        }

        autoReloadCoroutine = null;
        isAutoReload = false;
        Debug.Log("自动换弹完成");
    }

    // 原有的手动换弹协程
    private IEnumerator ReloadBullets()
    {
        isReloading = true;
        Debug.Log("开始手动换弹");

        while (bulletPool.Count < bulletPoolSize)
        {
            yield return new WaitForSeconds(reloadTime / bulletPoolSize);

            // 检查是否在换弹过程中开始了射击
            if (isFireing)
            {
                Debug.Log("手动换弹被中断");
                isReloading = false;
                yield break;
            }

            GameObject bullet = Instantiate(bulletPrefab);
            bullet.SetActive(false);
            bulletPool.Enqueue(bullet);

            Debug.Log($"手动换弹中: {bulletPool.Count} / {bulletPoolSize}");
        }

        isReloading = false;
        Debug.Log("手动换弹完成");
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
        // 如果在换弹过程中射击，中断换弹
        if (isReloading && reloadCoroutine != null)
        {
            StopCoroutine(reloadCoroutine);
            isReloading = false;
            Debug.Log("换弹被射击中断");
        }

        // 如果正在自动换弹，中断自动换弹
        if (autoReloadCoroutine != null)
        {
            StopCoroutine(autoReloadCoroutine);
            autoReloadCoroutine = null;
            isAutoReload = false;
            Debug.Log("自动换弹被射击中断");
        }

        if (isReloading || fireTimer > 0)
        {
            isFireing = !isReloading;
            return;
        }

        if (bulletPool.Count <= 0)
        {
            isFireing = false;
            reloadCoroutine = StartCoroutine(ReloadBullets());
            return;
        }

        isFireing = true;
        timeSinceLastFire = 0f; // 重置未射击时间
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

        // 获取手柄瞄准输入
        Vector2 gamepadAim = GameInput.Instance.GetAimDir();

        // 检查手柄输入是否超过死区
        if (gamepadAim.magnitude > gamepadDeadzone)
        {
            aimDirection = new Vector3(gamepadAim.x, gamepadAim.y, 0f);
        }
        // 如果没有手柄输入，则使用鼠标
        else
        {
            if (mainCamera == null) return;
            Vector3 mouseWorldPosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            mouseWorldPosition.z = 0f;
            aimDirection = (mouseWorldPosition - playerPosition);
            // 使用鼠标输入时，缩放瞄准线
            float aimScale = Mathf.Clamp(aimDirection.magnitude * aimToMouse, 0f, maxaimScale);
            Aim.localScale = new Vector3(aimScale, 1f, 1f);
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
            animator = weaponInstance.GetComponent<Animator>();
            reloadTime = weaponSO.ReloadTime;
            bulletPoolSize = weaponSO.magazineSize;
            if (animator != null)
            {
                animator.speed = weaponSO.attackSpeed;
            }
            maxaimScale = 0.8f * weaponSO.attackRange;
            bulletPool.Clear();
            //RefillMagazine();
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

    // 获取当前子弹数量
    public int GetCurrentBulletCount() => bulletPool.Count;

    // 获取最大子弹数量
    public int GetMaxBulletCount() => bulletPoolSize;

    // 检查是否正在换弹
    public bool IsReloading() => isReloading;

    // 手动触发换弹
    public void ManualReload()
    {
        if (!isReloading && bulletPool.Count < bulletPoolSize)
        {
            reloadCoroutine = StartCoroutine(ReloadBullets());
        }
    }

    // 新增：强制装满弹夹（可以从其他脚本调用）
    public void ForceRefillMagazine()
    {
        RefillMagazine();
    }
}