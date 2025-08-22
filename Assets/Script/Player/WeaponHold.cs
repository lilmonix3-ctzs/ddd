using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class WeaponHold : MonoBehaviour
{
    //[Header("Animator Settings")]
    Animator animator; // ����������
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
    [SerializeField] private float gamepadDeadzone = 0.2f; // �ֱ�������ֵ
    [SerializeField] private Transform Aim;
    [SerializeField] private float aimToMouse = 0.7f; // ��׼�����ű�
    [SerializeField] private float maxaimScale = 1.5f; // �������
    [SerializeField] private float autoReloadDelay = 0.5f; // �Զ������ӳ�ʱ��

    private int weaponCount;
    private int weaponIndex;
    private Camera mainCamera;
    private Queue<GameObject> bulletPool = new Queue<GameObject>();

    // ���������ر���
    private bool isReloading = false;
    private bool isAutoReload = false;
    private float fireTimer = 0f;
    private WeaponSO currentWeapon;
    private bool isFireing = false;
    private float timeSinceLastFire = 0f; // �ϴ�������ʱ��
    private Coroutine reloadCoroutine; // ����Э������
    private Coroutine autoReloadCoroutine; // �Զ�����Э������

    private void Awake()
    {
        mainCamera = Camera.main;
        InitializeBulletPool();
    }

    private void InitializeBulletPool()
    {
        // ������е��ӵ���
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
        Debug.Log($"��ʼ������: {bulletPool.Count}/{bulletPoolSize}");
    }

    // ����������װ������
    private void RefillMagazine()
    {
        // ������е��ӵ���
        while (bulletPool.Count > 0)
        {
            GameObject bullet = bulletPool.Dequeue();
            Destroy(bullet);
        }

        // ����������
        for (int i = 0; i < bulletPoolSize; i++)
        {
            GameObject bullet = Instantiate(bulletPrefab);
            bullet.SetActive(false);
            bulletPool.Enqueue(bullet);
        }

        Debug.Log($"������װ��: {bulletPool.Count}/{bulletPoolSize}");

        // ������ڻ�����ֹͣ����
        if (isReloading && reloadCoroutine != null)
        {
            StopCoroutine(reloadCoroutine);
            isReloading = false;
            Debug.Log("�����������л��ж�");
        }

        // ��������Զ�������ֹͣ�Զ�����
        if (isAutoReload && autoReloadCoroutine != null)
        {
            StopCoroutine(autoReloadCoroutine);
            autoReloadCoroutine = null;
            isAutoReload = false;
            Debug.Log("�Զ������������л��ж�");
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

        // �����ϴ�������ʱ��
        if (!isFireing)
        {
            timeSinceLastFire += Time.deltaTime;
        }
        else
        {
            timeSinceLastFire = 0f;
        }

        // ����Ƿ���Ҫ�Զ�����
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

    // ����Ƿ���Ҫ�Զ�����
    private void CheckAutoReload()
    {
        // ������ڻ����У��ӵ��ز�Ϊ�յ���������һ��ʱ��û�����
        if (!isReloading &&
            bulletPool.Count < bulletPoolSize &&
            bulletPool.Count >= 0 &&
            timeSinceLastFire >= autoReloadDelay)
        {
            // ���û�����ڽ��е��Զ���������ʼ�Զ�����
            if (autoReloadCoroutine == null)
            {
                autoReloadCoroutine = StartCoroutine(AutoReloadBullets());
                isAutoReload = true;
            }
        }
        else if (timeSinceLastFire < autoReloadDelay && autoReloadCoroutine != null)
        {
            // ������Զ������������ֿ�ʼ�����ȡ���Զ�����
            StopCoroutine(autoReloadCoroutine);
            autoReloadCoroutine = null;
            isAutoReload = false;
            Debug.Log("�Զ�������ȡ��");
        }
    }

    // �Զ�����Э��
    private IEnumerator AutoReloadBullets()
    {
        Debug.Log("��ʼ�Զ�����");

        while (bulletPool.Count < bulletPoolSize && timeSinceLastFire >= autoReloadDelay)
        {
            yield return new WaitForSeconds(reloadTime / bulletPoolSize);

            // ����Ƿ��ڻ��������п�ʼ�����
            if (timeSinceLastFire < autoReloadDelay)
            {
                Debug.Log("�Զ��������ж�");
                isAutoReload = false;
                break;
            }

            GameObject bullet = Instantiate(bulletPrefab);
            bullet.SetActive(false);
            bulletPool.Enqueue(bullet);

            Debug.Log($"�Զ�������: {bulletPool.Count} / {bulletPoolSize}");
        }

        autoReloadCoroutine = null;
        isAutoReload = false;
        Debug.Log("�Զ��������");
    }

    // ԭ�е��ֶ�����Э��
    private IEnumerator ReloadBullets()
    {
        isReloading = true;
        Debug.Log("��ʼ�ֶ�����");

        while (bulletPool.Count < bulletPoolSize)
        {
            yield return new WaitForSeconds(reloadTime / bulletPoolSize);

            // ����Ƿ��ڻ��������п�ʼ�����
            if (isFireing)
            {
                Debug.Log("�ֶ��������ж�");
                isReloading = false;
                yield break;
            }

            GameObject bullet = Instantiate(bulletPrefab);
            bullet.SetActive(false);
            bulletPool.Enqueue(bullet);

            Debug.Log($"�ֶ�������: {bulletPool.Count} / {bulletPoolSize}");
        }

        isReloading = false;
        Debug.Log("�ֶ��������");
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
        // ����ڻ���������������жϻ���
        if (isReloading && reloadCoroutine != null)
        {
            StopCoroutine(reloadCoroutine);
            isReloading = false;
            Debug.Log("����������ж�");
        }

        // ��������Զ��������ж��Զ�����
        if (autoReloadCoroutine != null)
        {
            StopCoroutine(autoReloadCoroutine);
            autoReloadCoroutine = null;
            isAutoReload = false;
            Debug.Log("�Զ�����������ж�");
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
        timeSinceLastFire = 0f; // ����δ���ʱ��
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

        // ��ȡ�ֱ���׼����
        Vector2 gamepadAim = GameInput.Instance.GetAimDir();

        // ����ֱ������Ƿ񳬹�����
        if (gamepadAim.magnitude > gamepadDeadzone)
        {
            aimDirection = new Vector3(gamepadAim.x, gamepadAim.y, 0f);
        }
        // ���û���ֱ����룬��ʹ�����
        else
        {
            if (mainCamera == null) return;
            Vector3 mouseWorldPosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            mouseWorldPosition.z = 0f;
            aimDirection = (mouseWorldPosition - playerPosition);
            // ʹ���������ʱ��������׼��
            float aimScale = Mathf.Clamp(aimDirection.magnitude * aimToMouse, 0f, maxaimScale);
            Aim.localScale = new Vector3(aimScale, 1f, 1f);
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

    // ��ȡ��ǰ�ӵ�����
    public int GetCurrentBulletCount() => bulletPool.Count;

    // ��ȡ����ӵ�����
    public int GetMaxBulletCount() => bulletPoolSize;

    // ����Ƿ����ڻ���
    public bool IsReloading() => isReloading;

    // �ֶ���������
    public void ManualReload()
    {
        if (!isReloading && bulletPool.Count < bulletPoolSize)
        {
            reloadCoroutine = StartCoroutine(ReloadBullets());
        }
    }

    // ������ǿ��װ�����У����Դ������ű����ã�
    public void ForceRefillMagazine()
    {
        RefillMagazine();
    }
}