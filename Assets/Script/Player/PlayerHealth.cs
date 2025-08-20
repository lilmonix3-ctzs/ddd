using UnityEngine;
using System.Collections;
using Cinemachine;

public class PlayerHealth : MonoBehaviour
{

    [SerializeField] private int maxHealth = 100;
    [SerializeField] private float invincibilityTime = 1f; // 无敌时间（秒）

    [Header("冲击波设置")]
    [SerializeField] private GameObject shockwavePrefab; // 冲击波预制体
    [SerializeField] private float shockwaveRadius = 5f; // 冲击波半径
    [SerializeField] private float shockwaveForce = 10f; // 冲击波力度
    [SerializeField] private float shockwaveDelay = 0.2f; // 受击后延迟生成冲

    [Header("镜头震动设置")]
    [SerializeField] private float shakeDuration = 0.5f; // 震动持续时间
    [SerializeField] private float shakeAmplitude = 2f; // 震动幅度
    [SerializeField] private float shakeFrequency = 2f; // 震动频率

    private float originalShakeAmplitude;
    private float originalShakeFrequency;
    private CinemachineVirtualCamera virtualCamera;
    private CinemachineBasicMultiChannelPerlin noise;
    private float shakeTimer = 0f;


    private int currentHealth;
    private float invincibilityTimer = 0f;

    private bool isDead = false;

    private void Start()
    {
        currentHealth = maxHealth;
        InitializeCameraShake();
    }

    private void InitializeCameraShake()
    {
        // 获取Cinemachine虚拟相机
        virtualCamera = FindObjectOfType<CinemachineVirtualCamera>();
        if (virtualCamera != null)
        {
            // 获取噪声组件
            noise = virtualCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();

            // 如果没有噪声组件，则添加一个
            if (noise == null)
            {
                noise = virtualCamera.AddCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
                //noise.m_NoiseProfile = CinemachineNoiseSettings.DefaultNoiseProfile;
            }

            originalShakeAmplitude = noise.m_AmplitudeGain;
            originalShakeFrequency = noise.m_FrequencyGain;
        }
        else
        {
            Debug.LogWarning("未找到Cinemachine虚拟相机！");
        }
    }

    private void FixedUpdate()
    {
        // 更新无敌计时器
        if (invincibilityTimer > 0)
        {
            invincibilityTimer -= Time.deltaTime;
        }

        // 处理镜头震动计时
        if (shakeTimer > 0)
        {
            shakeTimer -= Time.deltaTime;
            //噪声参数缓慢恢复
            if (noise != null)
            {
                noise.m_AmplitudeGain = Mathf.Lerp(noise.m_AmplitudeGain, originalShakeAmplitude, Time.deltaTime);
                noise.m_FrequencyGain = Mathf.Lerp(noise.m_FrequencyGain, originalShakeFrequency, Time.deltaTime);
            }
            if (shakeTimer <= 0f)
            {
                // 震动结束，重置噪声参数
                if (noise != null)
                {
                    noise.m_AmplitudeGain = originalShakeAmplitude;
                    noise.m_FrequencyGain = originalShakeFrequency;
                }
            }
        }
    }

    public void TakeDamage(int damage)
    {
        // 如果处于无敌状态，忽略伤害
        if (invincibilityTimer > 0) return;

        currentHealth -= damage;

        // 更新UI或显示伤害效果
        Debug.Log($"玩家受到伤害！当前生命值: {currentHealth}/{maxHealth}");

        // 设置无敌时间
        invincibilityTimer = invincibilityTime;

        // 触发镜头震动
        CameraShake();

        // 生成冲击波
        StartCoroutine(CreateShockwave());

        if (currentHealth <= 0)
        {
            Die();
        }
    }


    private IEnumerator CreateShockwave()
    {
        // 延迟生成冲击波，让受击动画先播放
        yield return new WaitForSeconds(shockwaveDelay);

        // 实例化冲击波
        GameObject shockwave = Instantiate(
            shockwavePrefab,
            transform.position,
            Quaternion.identity
        );
        shockwave.transform.SetParent(transform); // 设置父对象为玩家
        // 设置冲击波大小
        shockwave.transform.localScale = Vector3.one * shockwaveRadius * 2;

        // 获取范围内的所有敌人
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(
            transform.position,
            shockwaveRadius
        );

        foreach (Collider2D hitCollider in hitColliders)
        {
            if (hitCollider.CompareTag("Enemy"))
            {
                // 计算方向 (从玩家指向敌人)
                Vector2 direction = (hitCollider.transform.position - transform.position).normalized;

                // 获取敌人的Rigidbody2D
                Rigidbody2D enemyRb = hitCollider.GetComponent<Rigidbody2D>();
                if (enemyRb != null)
                {
                    // 应用冲击力
                    enemyRb.AddForce(
                        direction * shockwaveForce,
                        ForceMode2D.Impulse
                    );

                    // 可选: 对敌人造成额外效果
                    Enemy enemy = hitCollider.GetComponent<Enemy>();
                    if (enemy != null)
                    {
                        enemy.TakeDamage(5); // 敌人被冲击波击中的特殊处理
                    }
                }
            }
        }

        // 销毁冲击波效果(如果预制体没有自动销毁)
        Destroy(shockwave, 0.25f);
    }


    private void CameraShake()
    {
        if (noise != null)
        {
            // 设置震动参数
            noise.m_AmplitudeGain = shakeAmplitude;
            noise.m_FrequencyGain = shakeFrequency;

            // 开始计时
            shakeTimer = shakeDuration;
        }
    }
    private void Die()
    {
        Debug.Log("玩家死亡！");
        isDead = true;
        // 这里实现玩家死亡逻辑（例如游戏结束）
    }

    public bool IsDead() => isDead;
}