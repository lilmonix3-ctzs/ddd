using UnityEngine;
using System.Collections;

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

    [Header("金币掉落设置")]
    [SerializeField] private GameObject coinPrefab; // 金币预制体
    [SerializeField] private int minCoins = 1; // 最小掉落金币数量
    [SerializeField] private int maxCoins = 3; // 最大掉落金币数量
    [SerializeField] private float coinSpreadForce = 2f; // 金币散开力度
    [SerializeField] private Vector2 coinSpawnOffset = new Vector2(0, 0.5f); // 金币生成偏移

    private Transform playerTransform;
    private EnemySpawner spawner;
    private float attackTimer = 0f;
    private Rigidbody2D rb;
    private Vector2 currentVelocity;
    private bool isDead = false;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        // 使用更安全的方式查找玩家
        FindPlayer();

        // 如果没有找到玩家，尝试在更新中继续查找
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
            // 如果玩家仍然为空，尝试再次查找
            if (Time.frameCount % 60 == 0) // 每60帧尝试一次
            {
                FindPlayer();
            }
            return;
        }

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
        if (isDead) return;

        health -= amount;
        //击退效果
        if (playerTransform != null)
        {
            Vector2 knockbackDirection = (transform.position - playerTransform.position).normalized;
            rb.AddForce(knockbackDirection * KNscale); // 调整200f以改变击退力度
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

        // 掉落金币
        DropCoins();

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

    private void DropCoins()
    {
        if (coinPrefab == null)
        {
            Debug.LogWarning("未设置金币预制体，无法掉落金币");
            return;
        }

        // 随机决定掉落金币数量
        int coinCount = Random.Range(minCoins, maxCoins + 1);

        for (int i = 0; i < coinCount; i++)
        {
            // 计算金币位置（带偏移）
            Vector3 spawnPosition = transform.position + new Vector3(
                coinSpawnOffset.x + Random.Range(-0.2f, 0.2f),
                coinSpawnOffset.y + Random.Range(-0.1f, 0.1f),
                0
            );

            // 实例化金币
            GameObject coin = Instantiate(coinPrefab, spawnPosition, Quaternion.identity);

            // 添加随机的散开力
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

        Debug.Log($"敌人掉落 {coinCount} 枚金币");
    }

    // 设置生成器引用
    public void SetSpawner(EnemySpawner enemySpawner)
    {
        spawner = enemySpawner;
    }
}