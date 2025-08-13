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
    [SerializeField] private float gamepadDeadzone = 0.2f; // �ֱ�������ֵ

    private int weaponCount;
    private int weaponIndex;
    private Camera mainCamera;
    private Queue<GameObject> bulletPool = new Queue<GameObject>();

    // ���������ر���
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

    // �޸ĵ㣺�����������е�λ�úͳ���֧���ֱ�����꣩
    private void UpdateWeaponHoldPoint()
    {
        if (weaponHoldPoint == null) return;

        Vector3 playerPosition = transform.position;
        Vector3 aimDirection = Vector3.zero;
        bool usingGamepad = false;

        // ��ȡ�ֱ���׼����
        Vector2 gamepadAim = GameInput.Instance.GetAimDir();

        // ����ֱ������Ƿ񳬹�����
        if (gamepadAim.magnitude > gamepadDeadzone)
        {
            aimDirection = new Vector3(gamepadAim.x, gamepadAim.y, 0f);
            usingGamepad = true;
        }
        // ���û���ֱ����룬��ʹ�����
        else
        {
            Vector3 mouseWorldPosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            mouseWorldPosition.z = 0f;
            aimDirection = (mouseWorldPosition - playerPosition);
        }

        // ���ƾ��벻�������Χ
        if (aimDirection.magnitude > maxDistanceFromPlayer)
        {
            aimDirection = aimDirection.normalized * maxDistanceFromPlayer;
        }

        // ���ó��е�λ��
        weaponHoldPoint.position = playerPosition + aimDirection;

        // ʹ���е�ʼ��������׼����
        if (aimDirection != Vector3.zero)
        {
            float angle = Mathf.Atan2(aimDirection.y, aimDirection.x) * Mathf.Rad2Deg;
            weaponHoldPoint.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        }

        // ����׼������������ʱ����ת����
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