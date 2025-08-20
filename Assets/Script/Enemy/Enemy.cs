using UnityEngine;
using System.Collections;

public class Enemy : MonoBehaviour
{
    [Header("��������")]
    [SerializeField] private int health = 100;
    [SerializeField] private int damage = 10;
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float attackRange = 1.5f;
    [SerializeField] private float stoppingDistance = 0.5f; // ֹͣ�ƶ��ľ���
    [SerializeField] private float attackCooldown = 1f;
    [SerializeField] private float smoothTime = 0.1f; // ƽ���ƶ�����
    [SerializeField] private float KNscale = 1f;// ������������

    [Header("����Ч��")]
    [SerializeField] private GameObject deathEffect;
    [SerializeField] private float deathEffectDuration = 1f;

    [Header("��ҵ�������")]
    [SerializeField] private GameObject coinPrefab; // ���Ԥ����
    [SerializeField] private int minCoins = 1; // ��С����������
    [SerializeField] private int maxCoins = 3; // ������������
    [SerializeField] private float coinSpreadForce = 2f; // ���ɢ������
    [SerializeField] private Vector2 coinSpawnOffset = new Vector2(0, 0.5f); // �������ƫ��

    private Transform playerTransform;
    private EnemySpawner spawner;
    private float attackTimer = 0f;
    private Rigidbody2D rb;
    private Vector2 currentVelocity;
    private bool isDead = false;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        // ʹ�ø���ȫ�ķ�ʽ�������
        FindPlayer();

        // ���û���ҵ���ң������ڸ����м�������
        if (playerTransform == null)
        {
            StartCoroutine(FindPlayerRoutine());
        }
    }

    private IEnumerator FindPlayerRoutine()
    {
        int attempts = 0;
        while (playerTransform == null && attempts < 10)
        {
            yield return new WaitForSeconds(0.5f);
            FindPlayer();
            attempts++;
        }
    }

    private void FindPlayer()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            playerTransform = playerObj.transform;
        }
    }

    private void FixedUpdate()
    {
        if (playerTransform == null)
        {
            // ��������ȻΪ�գ������ٴβ���
            if (Time.frameCount % 60 == 0) // ÿ60֡����һ��
            {
                FindPlayer();
            }
            return;
        }

        // ���¹�����ʱ��
        if (attackTimer > 0)
        {
            attackTimer -= Time.deltaTime;
        }

        // ���㵽��ҵľ���
        float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);

        if (distanceToPlayer <= attackRange && attackTimer <= 0)
        {
            // �ڹ�����Χ���ҿ��Թ���
            AttackPlayer();
        }
        else if (distanceToPlayer > attackRange)
        {
            // ������ƶ�
            MoveTowardsPlayer();
        }
    }

    private void MoveTowardsPlayer()
    {
        Vector2 direction = (playerTransform.position - transform.position).normalized;
        float distance = Vector2.Distance(transform.position, playerTransform.position);

        if (distance > stoppingDistance)
        {
            // ʹ��ƽ�������ƶ�
            Vector2 targetVelocity = direction * moveSpeed;
            rb.velocity = Vector2.SmoothDamp(
                rb.velocity,
                targetVelocity,
                ref currentVelocity,
                smoothTime
            );
        }
        else
        {
            // �ӽ�ʱ����
            rb.velocity = Vector2.Lerp(rb.velocity, Vector2.zero, 0.1f);
        }
    }

    private void AttackPlayer()
    {
        // ����ʵ�ֹ����߼�����������������ֵ��
        PlayerHealth playerHealth = playerTransform.GetComponent<PlayerHealth>();
        if (playerHealth != null)
        {
            playerHealth.TakeDamage(damage);
        }

        // ���ù�����ȴ
        attackTimer = attackCooldown;
    }

    public void TakeDamage(int amount)
    {
        if (isDead) return;

        health -= amount;
        //����Ч��
        if (playerTransform != null)
        {
            Vector2 knockbackDirection = (transform.position - playerTransform.position).normalized;
            rb.AddForce(knockbackDirection * KNscale); // ����200f�Ըı��������
        }

        if (health <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        if (isDead) return;
        isDead = true;

        // ������
        DropCoins();

        // ֪ͨ��������������
        if (spawner != null)
        {
            spawner.EnemyDied();
        }

        // ��������Ч��
        if (deathEffect != null)
        {
            GameObject effect = Instantiate(deathEffect, transform.position, Quaternion.identity);
            Destroy(effect, deathEffectDuration);
        }

        // ���ٵ���
        Destroy(gameObject);
    }

    private void DropCoins()
    {
        if (coinPrefab == null)
        {
            Debug.LogWarning("δ���ý��Ԥ���壬�޷�������");
            return;
        }

        // �����������������
        int coinCount = Random.Range(minCoins, maxCoins + 1);

        for (int i = 0; i < coinCount; i++)
        {
            // ������λ�ã���ƫ�ƣ�
            Vector3 spawnPosition = transform.position + new Vector3(
                coinSpawnOffset.x + Random.Range(-0.2f, 0.2f),
                coinSpawnOffset.y + Random.Range(-0.1f, 0.1f),
                0
            );

            // ʵ�������
            GameObject coin = Instantiate(coinPrefab, spawnPosition, Quaternion.identity);

            // ��������ɢ����
            Rigidbody2D coinRb = coin.GetComponent<Rigidbody2D>();
            if (coinRb != null)
            {
                Vector2 force = new Vector2(
                    Random.Range(-coinSpreadForce, coinSpreadForce),
                    Random.Range(1f, coinSpreadForce)
                );
                coinRb.AddForce(force, ForceMode2D.Impulse);
            }
        }

        Debug.Log($"���˵��� {coinCount} ö���");
    }

    // ��������������
    public void SetSpawner(EnemySpawner enemySpawner)
    {
        spawner = enemySpawner;
    }
}