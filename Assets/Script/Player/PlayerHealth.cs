using UnityEngine;
using System.Collections;
using Cinemachine;

public class PlayerHealth : MonoBehaviour
{

    [SerializeField] private int maxHealth = 100;
    [SerializeField] private float invincibilityTime = 1f; // �޵�ʱ�䣨�룩

    [Header("���������")]
    [SerializeField] private GameObject shockwavePrefab; // �����Ԥ����
    [SerializeField] private float shockwaveRadius = 5f; // ������뾶
    [SerializeField] private float shockwaveForce = 10f; // ���������
    [SerializeField] private float shockwaveDelay = 0.2f; // �ܻ����ӳ����ɳ�

    [Header("��ͷ������")]
    [SerializeField] private float shakeDuration = 0.5f; // �𶯳���ʱ��
    [SerializeField] private float shakeAmplitude = 2f; // �𶯷���
    [SerializeField] private float shakeFrequency = 2f; // ��Ƶ��

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
        // ��ȡCinemachine�������
        virtualCamera = FindObjectOfType<CinemachineVirtualCamera>();
        if (virtualCamera != null)
        {
            // ��ȡ�������
            noise = virtualCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();

            // ���û����������������һ��
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
            Debug.LogWarning("δ�ҵ�Cinemachine���������");
        }
    }

    private void FixedUpdate()
    {
        // �����޵м�ʱ��
        if (invincibilityTimer > 0)
        {
            invincibilityTimer -= Time.deltaTime;
        }

        // ����ͷ�𶯼�ʱ
        if (shakeTimer > 0)
        {
            shakeTimer -= Time.deltaTime;
            //�������������ָ�
            if (noise != null)
            {
                noise.m_AmplitudeGain = Mathf.Lerp(noise.m_AmplitudeGain, originalShakeAmplitude, Time.deltaTime);
                noise.m_FrequencyGain = Mathf.Lerp(noise.m_FrequencyGain, originalShakeFrequency, Time.deltaTime);
            }
            if (shakeTimer <= 0f)
            {
                // �𶯽�����������������
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
        // ��������޵�״̬�������˺�
        if (invincibilityTimer > 0) return;

        currentHealth -= damage;

        // ����UI����ʾ�˺�Ч��
        Debug.Log($"����ܵ��˺�����ǰ����ֵ: {currentHealth}/{maxHealth}");

        // �����޵�ʱ��
        invincibilityTimer = invincibilityTime;

        // ������ͷ��
        CameraShake();

        // ���ɳ����
        StartCoroutine(CreateShockwave());

        if (currentHealth <= 0)
        {
            Die();
        }
    }


    private IEnumerator CreateShockwave()
    {
        // �ӳ����ɳ���������ܻ������Ȳ���
        yield return new WaitForSeconds(shockwaveDelay);

        // ʵ���������
        GameObject shockwave = Instantiate(
            shockwavePrefab,
            transform.position,
            Quaternion.identity
        );
        shockwave.transform.SetParent(transform); // ���ø�����Ϊ���
        // ���ó������С
        shockwave.transform.localScale = Vector3.one * shockwaveRadius * 2;

        // ��ȡ��Χ�ڵ����е���
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(
            transform.position,
            shockwaveRadius
        );

        foreach (Collider2D hitCollider in hitColliders)
        {
            if (hitCollider.CompareTag("Enemy"))
            {
                // ���㷽�� (�����ָ�����)
                Vector2 direction = (hitCollider.transform.position - transform.position).normalized;

                // ��ȡ���˵�Rigidbody2D
                Rigidbody2D enemyRb = hitCollider.GetComponent<Rigidbody2D>();
                if (enemyRb != null)
                {
                    // Ӧ�ó����
                    enemyRb.AddForce(
                        direction * shockwaveForce,
                        ForceMode2D.Impulse
                    );

                    // ��ѡ: �Ե�����ɶ���Ч��
                    Enemy enemy = hitCollider.GetComponent<Enemy>();
                    if (enemy != null)
                    {
                        enemy.TakeDamage(5); // ���˱���������е����⴦��
                    }
                }
            }
        }

        // ���ٳ����Ч��(���Ԥ����û���Զ�����)
        Destroy(shockwave, 0.25f);
    }


    private void CameraShake()
    {
        if (noise != null)
        {
            // �����𶯲���
            noise.m_AmplitudeGain = shakeAmplitude;
            noise.m_FrequencyGain = shakeFrequency;

            // ��ʼ��ʱ
            shakeTimer = shakeDuration;
        }
    }
    private void Die()
    {
        Debug.Log("���������");
        isDead = true;
        // ����ʵ����������߼���������Ϸ������
    }

    public bool IsDead() => isDead;
}