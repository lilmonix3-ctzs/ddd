using UnityEngine;
using System.Collections.Generic;

public class WeaponHold : MonoBehaviour
{
    [Header("Weapon Settings")]
    [SerializeField] private Transform weaponHoldPoint;
    [SerializeField] private WeaponSO[] weaponSOs;
    [SerializeField] private float maxDistanceFromPlayer = 1.5f;
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private int bulletPoolSize = 20;

    private int weaponCount;
    private int weaponIndex;
    private Camera mainCamera;
    private Queue<GameObject> bulletPool = new Queue<GameObject>();

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

        // 如果池空了，创建新子弹（但应该根据游戏设计调整池大小）
        Debug.LogWarning("Bullet pool empty, instantiating new bullet");
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
        if (weaponCount > 0) EquipWeapon(weaponIndex);
    }

    private void Update()
    {
        UpdateWeaponHoldPoint();
        HandleWeaponInteractions();
    }

    private void HandleWeaponInteractions()
    {
        if (GameInput.Instance.IsSwitchWeaponClicked())
        {
            SwitchWeapon(1);
        }

        if (GameInput.Instance.IsAttackClick())
        {
            WeaponSO currentWeapon = GetCurrentWeapon();
            if (currentWeapon != null)
            {
                FireBullet(currentWeapon);
            }
        }
    }

    private void FireBullet(WeaponSO weapon)
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
                weapon.attackSpeed,
                weapon.attackRange,
                weapon.damage);
        }
        else
        {
            Debug.LogError("Bullet script missing on bullet prefab!");
        }
    }

    // 更新武器持有点位置和朝向
    private void UpdateWeaponHoldPoint()
    {
        if (weaponHoldPoint == null) return;

        // 获取鼠标位置（世界坐标）
        Vector3 mouseWorldPosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPosition.z = 0f; // 确保z轴为0

        // 玩家位置
        Vector3 playerPosition = transform.position;

        // 计算从玩家到鼠标的方向
        Vector3 directionToMouse = mouseWorldPosition - playerPosition;
        directionToMouse.z = 0;

        // 限制距离不超过最大范围
        if (directionToMouse.magnitude > maxDistanceFromPlayer)
        {
            directionToMouse = directionToMouse.normalized * maxDistanceFromPlayer;
        }

        // 设置持有点位置（在玩家和鼠标之间）
        weaponHoldPoint.position = playerPosition + directionToMouse;

        // 使持有点始终面向鼠标位置
        Vector3 lookDirection = directionToMouse;
        if (lookDirection != Vector3.zero)
        {
            // 计算旋转角度（2D）
            float angle = Mathf.Atan2(lookDirection.y, lookDirection.x) * Mathf.Rad2Deg;
            weaponHoldPoint.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        }

        //当鼠标位置在玩家左侧时，翻转武器
        if (mouseWorldPosition.x < playerPosition.x)
        {
            weaponHoldPoint.localScale = new Vector3(1f, -1f, 1f); // 翻转武器
        }
        else
        {
            weaponHoldPoint.localScale = new Vector3(1f, 1f, 1f); // 正常方向
        }
    }

    // 装备武器
    public void EquipWeapon(int index)
    {
        if (index < 0 || index >= weaponCount)
        {
            Debug.LogError("Invalid weapon index: " + index);
            return;
        }

        // 清除当前持有的武器
        foreach (Transform child in weaponHoldPoint)
        {
            // 只销毁标记为"Weapon"的对象
            if (child.CompareTag("Weapon"))
            {
                Destroy(child.gameObject);
            }
        }

        // 实例化新的武器
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

            // 添加武器标记
            weaponInstance.tag = "Weapon";
        }
        else
        {
            Debug.LogWarning($"Weapon prefab is missing for {weaponSO.objectName}");
        }
    }

    // 切换武器
    public void SwitchWeapon(int direction)
    {
        weaponIndex += direction;
        if (weaponIndex < 0) weaponIndex = weaponCount - 1;
        if (weaponIndex >= weaponCount) weaponIndex = 0;
        EquipWeapon(weaponIndex);
    }

    // 获取当前武器
    public WeaponSO GetCurrentWeapon()
    {
        if (weaponIndex < 0 || weaponIndex >= weaponCount)
        {
            Debug.LogError("Invalid weapon index: " + weaponIndex);
            return null;
        }
        return weaponSOs[weaponIndex];
    }

    // 获取当前武器的数量
    public int GetWeaponCount() => weaponCount;

    // 获取当前武器的索引
    public int GetCurrentWeaponIndex() => weaponIndex;
}