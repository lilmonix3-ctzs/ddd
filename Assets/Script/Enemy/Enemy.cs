using UnityEngine;

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

    private Transform playerTransform;
    private EnemySpawner spawner;
    private float attackTimer = 0f;
    private Rigidbody2D rb;
    private Vector2 currentVelocity;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        playerTransform = GameObject.FindGameObjectWithTag("Player").transform;

        if (playerTransform == null)
        {
            Debug.LogError("δ�ҵ���Ҷ�����ȷ�������'Player'��ǩ");
        }
    }

    private void FixedUpdate()
    {
        if (playerTransform == null) return;

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
        health -= amount;
        //����Ч��
        Vector2 knockbackDirection = (transform.position - playerTransform.position).normalized;
        rb.AddForce(knockbackDirection * KNscale); // ����200f�Ըı��������

        if (health <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
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

    // ��������������
    public void SetSpawner(EnemySpawner enemySpawner)
    {
        spawner = enemySpawner;
    }
}