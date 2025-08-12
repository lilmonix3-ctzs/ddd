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

        // ����ؿ��ˣ��������ӵ�����Ӧ�ø�����Ϸ��Ƶ����ش�С��
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

    // �����������е�λ�úͳ���
    private void UpdateWeaponHoldPoint()
    {
        if (weaponHoldPoint == null) return;

        // ��ȡ���λ�ã��������꣩
        Vector3 mouseWorldPosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPosition.z = 0f; // ȷ��z��Ϊ0

        // ���λ��
        Vector3 playerPosition = transform.position;

        // �������ҵ����ķ���
        Vector3 directionToMouse = mouseWorldPosition - playerPosition;
        directionToMouse.z = 0;

        // ���ƾ��벻�������Χ
        if (directionToMouse.magnitude > maxDistanceFromPlayer)
        {
            directionToMouse = directionToMouse.normalized * maxDistanceFromPlayer;
        }

        // ���ó��е�λ�ã�����Һ����֮�䣩
        weaponHoldPoint.position = playerPosition + directionToMouse;

        // ʹ���е�ʼ���������λ��
        Vector3 lookDirection = directionToMouse;
        if (lookDirection != Vector3.zero)
        {
            // ������ת�Ƕȣ�2D��
            float angle = Mathf.Atan2(lookDirection.y, lookDirection.x) * Mathf.Rad2Deg;
            weaponHoldPoint.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        }

        //�����λ����������ʱ����ת����
        if (mouseWorldPosition.x < playerPosition.x)
        {
            weaponHoldPoint.localScale = new Vector3(1f, -1f, 1f); // ��ת����
        }
        else
        {
            weaponHoldPoint.localScale = new Vector3(1f, 1f, 1f); // ��������
        }
    }

    // װ������
    public void EquipWeapon(int index)
    {
        if (index < 0 || index >= weaponCount)
        {
            Debug.LogError("Invalid weapon index: " + index);
            return;
        }

        // �����ǰ���е�����
        foreach (Transform child in weaponHoldPoint)
        {
            // ֻ���ٱ��Ϊ"Weapon"�Ķ���
            if (child.CompareTag("Weapon"))
            {
                Destroy(child.gameObject);
            }
        }

        // ʵ�����µ�����
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

            // ����������
            weaponInstance.tag = "Weapon";
        }
        else
        {
            Debug.LogWarning($"Weapon prefab is missing for {weaponSO.objectName}");
        }
    }

    // �л�����
    public void SwitchWeapon(int direction)
    {
        weaponIndex += direction;
        if (weaponIndex < 0) weaponIndex = weaponCount - 1;
        if (weaponIndex >= weaponCount) weaponIndex = 0;
        EquipWeapon(weaponIndex);
    }

    // ��ȡ��ǰ����
    public WeaponSO GetCurrentWeapon()
    {
        if (weaponIndex < 0 || weaponIndex >= weaponCount)
        {
            Debug.LogError("Invalid weapon index: " + weaponIndex);
            return null;
        }
        return weaponSOs[weaponIndex];
    }

    // ��ȡ��ǰ����������
    public int GetWeaponCount() => weaponCount;

    // ��ȡ��ǰ����������
    public int GetCurrentWeaponIndex() => weaponIndex;
}