using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("敌人属性")]
    [SerializeField] private int health = 100;
    [SerializeField] private int damage = 10;
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float attackRange = 1.5f;
    [SerializeField] private float stoppingDistance = 0.5f; // 停止移动的距离
    [SerializeField] private float attackCooldown = 1f;
    [SerializeField] private float smoothTime = 0.1f; // 平滑移动参数
    [SerializeField] private float KNscale = 1f;// 击退力度缩放

    [Header("死亡效果")]
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
            Debug.LogError("未找到玩家对象！请确保玩家有'Player'标签");
        }
    }

    private void FixedUpdate()
    {
        if (playerTransform == null) return;

        // 更新攻击计时器
        if (attackTimer > 0)
        {
            attackTimer -= Time.deltaTime;
        }

        // 计算到玩家的距离
        float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);

        if (distanceToPlayer <= attackRange && attackTimer <= 0)
        {
            // 在攻击范围内且可以攻击
            AttackPlayer();
        }
        else if (distanceToPlayer > attackRange)
        {
            // 向玩家移动
            MoveTowardsPlayer();
        }
    }

    private void MoveTowardsPlayer()
    {

        Vector2 direction = (playerTransform.position - transform.position).normalized;
        float distance = Vector2.Distance(transform.position, playerTransform.position);

        if (distance > stoppingDistance)
        {
            // 使用平滑阻尼移动
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
            // 接近时减速
            rb.velocity = Vector2.Lerp(rb.velocity, Vector2.zero, 0.1f);
        }
    }

    private void AttackPlayer()
    {
        // 这里实现攻击逻辑（例如减少玩家生命值）
        PlayerHealth playerHealth = playerTransform.GetComponent<PlayerHealth>();
        if (playerHealth != null)
        {
            playerHealth.TakeDamage(damage);
        }

        // 重置攻击冷却
        attackTimer = attackCooldown;
    }

    public void TakeDamage(int amount)
    {
        health -= amount;
        //击退效果
        Vector2 knockbackDirection = (transform.position - playerTransform.position).normalized;
        rb.AddForce(knockbackDirection * KNscale); // 调整200f以改变击退力度

        if (health <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        // 通知生成器敌人死亡
        if (spawner != null)
        {
            spawner.EnemyDied();
        }

        // 播放死亡效果
        if (deathEffect != null)
        {
            GameObject effect = Instantiate(deathEffect, transform.position, Quaternion.identity);
            Destroy(effect, deathEffectDuration);
        }

        // 销毁敌人
        Destroy(gameObject);
    }

    // 设置生成器引用
    public void SetSpawner(EnemySpawner enemySpawner)
    {
        spawner = enemySpawner;
    }
}