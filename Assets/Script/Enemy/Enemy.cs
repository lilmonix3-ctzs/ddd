using UnityEngine;
using System.Collections;

public class Enemy : MonoBehaviour
{
    [Header("敌人属性")]
    [SerializeField] private EnemySO enemyData;
    [SerializeField] private int health = 100;
    [SerializeField] private int damage = 10;
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float attackRange = 1.5f;
    [SerializeField] private float stoppingDistance = 0.5f;
    [SerializeField] private float attackCooldown = 1f;
    [SerializeField] private float smoothTime = 0.1f;
    [SerializeField] private float KNscale = 1f;

    [Header("特殊敌人设置")]
    [SerializeField] private float chargeDistance = 5f; // 冲锋敌人: 冲锋距离
    [SerializeField] private float chargeSpeed = 8f; // 冲锋敌人: 冲锋速度
    [SerializeField] private float chargeCooldown = 3f; // 冲锋敌人: 冲锋冷却
    [SerializeField] private float rangedAttackDistance = 7f; // 远程敌人: 攻击距离
    [SerializeField] private GameObject projectilePrefab; // 远程敌人: 投射物
    [SerializeField] private Transform projectileSpawnPoint; // 远程敌人: 投射物生成点
    [SerializeField] private int splitCount = 2; // 分裂敌人: 分裂数量
    [SerializeField] private GameObject splitEnemyPrefab; // 分裂敌人: 分裂出的敌人
    [SerializeField] private float lurkerHideTime = 3f; // 潜伏者: 隐藏时间
    [SerializeField] private float lurkerAttackTime = 2f; // 潜伏者: 攻击时间

    [Header("死亡效果")]
    [SerializeField] private GameObject deathEffect;
    [SerializeField] private GameObject hitEffect;
    [SerializeField] private float deathEffectDuration = 1f;

    [Header("金币掉落设置")]
    [SerializeField] private GameObject coinPrefab;
    [SerializeField] private int minCoins = 1;
    [SerializeField] private int maxCoins = 3;
    [SerializeField] private float coinSpreadForce = 2f;
    [SerializeField] private Vector2 coinSpawnOffset = new Vector2(0, 0.5f);

    private Transform playerTransform;
    private EnemySpawner spawner;
    private float attackTimer = 0f;
    private float specialAbilityTimer = 0f;
    private Rigidbody2D rb;
    private Vector2 currentVelocity;
    private bool isDead = false;

    // 特殊敌人状态
    private bool isCharging = false;
    private bool isLurkerHidden = false;
    private Vector2 chargeDirection;
    private float lurkerStateTimer = 0f;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        // 应用EnemySO数据
        ApplyEnemySOData();

        FindPlayer();

        if (playerTransform == null)
        {
            StartCoroutine(FindPlayerRoutine());
        }

        // 潜伏者初始状态
        if (enemyData != null && enemyData.enemyType == EnemySO.EnemyType.lurker)
        {
            isLurkerHidden = true;
            SetLurkerVisibility(false);
        }
    }

    // 应用EnemySO数据
    private void ApplyEnemySOData()
    {
        if (enemyData == null) return;

        // 根据敌人类型和等级调整属性
        float rankMultiplier = 1f;

        switch (enemyData.enemyRank)
        {
            case EnemySO.EnemyRank.elite:
                rankMultiplier = 1.5f;
                break;
            case EnemySO.EnemyRank.boss:
                rankMultiplier = 2.5f;
                break;
        }

        // 应用基础属性
        health = Mathf.RoundToInt(health * rankMultiplier);
        damage = Mathf.RoundToInt(damage * rankMultiplier);
        moveSpeed = moveSpeed * rankMultiplier;

        // 根据敌人类型调整特殊属性
        switch (enemyData.enemyType)
        {
            case EnemySO.EnemyType.tank:
                health = Mathf.RoundToInt(health * 2f);
                damage = Mathf.RoundToInt(damage * 1.2f);
                moveSpeed = moveSpeed * 0.7f;
                break;

            case EnemySO.EnemyType.charger:
                moveSpeed = moveSpeed * 1.2f;
                break;

            case EnemySO.EnemyType.ranged:
                health = Mathf.RoundToInt(health * 0.7f);
                moveSpeed = moveSpeed * 0.8f;
                attackRange = rangedAttackDistance;
                break;

            case EnemySO.EnemyType.spliter:
                health = Mathf.RoundToInt(health * 0.8f);
                break;

            case EnemySO.EnemyType.lurker:
                moveSpeed = moveSpeed * 1.3f;
                damage = Mathf.RoundToInt(damage * 1.5f);
                break;
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
            if (Time.frameCount % 60 == 0)
            {
                FindPlayer();
            }
            return;
        }

        // 更新计时器
        if (attackTimer > 0)
        {
            attackTimer -= Time.deltaTime;
        }

        if (specialAbilityTimer > 0)
        {
            specialAbilityTimer -= Time.deltaTime;
        }

        // 根据敌人类型执行不同行为
        if (enemyData != null)
        {
            switch (enemyData.enemyType)
            {
                case EnemySO.EnemyType.melee:
                    MeleeBehavior();
                    break;

                case EnemySO.EnemyType.tank:
                    TankBehavior();
                    break;

                case EnemySO.EnemyType.charger:
                    ChargerBehavior();
                    break;

                case EnemySO.EnemyType.ranged:
                    RangedBehavior();
                    break;

                case EnemySO.EnemyType.spliter:
                    SpliterBehavior();
                    break;

                case EnemySO.EnemyType.lurker:
                    LurkerBehavior();
                    break;
            }
        }
        else
        {
            // 默认行为（近战）
            MeleeBehavior();
        }
    }

    #region 敌人行为实现

    // 近战敌人行为
    private void MeleeBehavior()
    {
        float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);

        if (distanceToPlayer <= attackRange && attackTimer <= 0)
        {
            AttackPlayer();
        }
        else if (distanceToPlayer > attackRange)
        {
            MoveTowardsPlayer();
        }
    }

    // 坦克敌人行为（类似近战但更慢更耐打）
    private void TankBehavior()
    {
        float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);

        if (distanceToPlayer <= attackRange && attackTimer <= 0)
        {
            AttackPlayer();
        }
        else if (distanceToPlayer > attackRange)
        {
            MoveTowardsPlayer();
        }
    }

    // 冲锋敌人行为
    private void ChargerBehavior()
    {
        float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);

        if (isCharging)
        {
            // 正在冲锋
            rb.velocity = chargeDirection * chargeSpeed;

            // 检查冲锋是否结束（撞到墙壁或足够远）
            if (distanceToPlayer > chargeDistance * 1.5f || specialAbilityTimer <= 0)
            {
                isCharging = false;
                rb.velocity = Vector2.zero;
                specialAbilityTimer = chargeCooldown;
            }
        }
        else if (distanceToPlayer <= chargeDistance && specialAbilityTimer <= 0)
        {
            // 开始冲锋
            isCharging = true;
            chargeDirection = (playerTransform.position - transform.position).normalized;
            specialAbilityTimer = chargeCooldown;
        }
        else if (distanceToPlayer <= attackRange && attackTimer <= 0)
        {
            // 近战攻击
            AttackPlayer();
        }
        else
        {
            // 向玩家移动
            MoveTowardsPlayer();
        }
    }

    // 远程敌人行为
    private void RangedBehavior()
    {
        float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);

        if (distanceToPlayer <= rangedAttackDistance && attackTimer <= 0)
        {
            // 远程攻击
            RangedAttack();
        }
        else if (distanceToPlayer > rangedAttackDistance * 1.2f)
        {
            // 向玩家移动
            MoveTowardsPlayer();
        }
        else
        {
            // 保持距离
            rb.velocity = Vector2.zero;
        }
    }

    // 分裂敌人行为
    private void SpliterBehavior()
    {
        // 分裂敌人使用近战行为
        MeleeBehavior();
    }

    // 潜伏者行为
    private void LurkerBehavior()
    {
        lurkerStateTimer += Time.deltaTime;

        if (isLurkerHidden)
        {
            // 隐藏状态
            if (lurkerStateTimer >= lurkerHideTime)
            {
                // 切换到攻击状态
                isLurkerHidden = false;
                lurkerStateTimer = 0f;
                SetLurkerVisibility(true);
            }
        }
        else
        {
            // 攻击状态
            float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);

            if (distanceToPlayer <= attackRange && attackTimer <= 0)
            {
                AttackPlayer();
            }
            else
            {
                MoveTowardsPlayer();
            }

            if (lurkerStateTimer >= lurkerAttackTime)
            {
                // 切换回隐藏状态
                isLurkerHidden = true;
                lurkerStateTimer = 0f;
                SetLurkerVisibility(false);
            }
        }
    }

    #endregion

    #region 通用方法

    private void MoveTowardsPlayer()
    {
        if (isLurkerHidden) return; // 潜伏者隐藏时不移动

        Vector2 direction = (playerTransform.position - transform.position).normalized;
        float distance = Vector2.Distance(transform.position, playerTransform.position);

        if (distance > stoppingDistance)
        {
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
            rb.velocity = Vector2.Lerp(rb.velocity, Vector2.zero, 0.1f);
        }
    }

    private void AttackPlayer()
    {
        PlayerHealth playerHealth = playerTransform.GetComponent<PlayerHealth>();
        if (playerHealth != null)
        {
            playerHealth.TakeDamage(damage);
        }

        attackTimer = attackCooldown;
    }

    // 远程攻击
    private void RangedAttack()
    {
        if (projectilePrefab == null || projectileSpawnPoint == null)
        {
            Debug.LogWarning("远程敌人未设置投射物或生成点");
            return;
        }

        // 创建投射物
        GameObject projectile = Instantiate(
            projectilePrefab,
            projectileSpawnPoint.position,
            Quaternion.identity
        );

        // 设置投射物方向
        Vector2 direction = (playerTransform.position - transform.position).normalized;
        Projectile projectileScript = projectile.GetComponent<Projectile>();
        if (projectileScript != null)
        {
            projectileScript.SetDirection(direction);
            projectileScript.SetDamage(damage);
        }

        attackTimer = attackCooldown;
    }

    // 设置潜伏者可见性
    private void SetLurkerVisibility(bool visible)
    {
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        foreach (Renderer renderer in renderers)
        {
            renderer.enabled = visible;
        }

        Collider2D collider = GetComponent<Collider2D>();
        if (collider != null)
        {
            collider.enabled = visible;
        }
    }

    public void TakeDamage(int amount)
    {
        if (isDead) return;

        if (hitEffect != null)
        {
            GameObject effect = Instantiate(hitEffect, transform.position, Quaternion.identity);
            Destroy(effect, 0.3f);
        }

        health -= amount;

        if (playerTransform != null)
        {
            Vector2 knockbackDirection = (transform.position - playerTransform.position).normalized;
            rb.AddForce(knockbackDirection * KNscale);
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

        DropCoins();

        if (spawner != null)
        {
            spawner.EnemyDied();
        }

        // 分裂敌人死亡时分裂
        if (enemyData != null && enemyData.enemyType == EnemySO.EnemyType.spliter)
        {
            SplitIntoSmallerEnemies();
        }

        if (deathEffect != null)
        {
            GameObject effect = Instantiate(deathEffect, transform.position, Quaternion.identity);
            Destroy(effect, deathEffectDuration);
        }

        Destroy(gameObject);
    }

    // 分裂敌人分裂方法
    private void SplitIntoSmallerEnemies()
    {
        if (splitEnemyPrefab == null || splitCount <= 0) return;

        for (int i = 0; i < splitCount; i++)
        {
            Vector3 spawnPosition = transform.position + new Vector3(
                Random.Range(-1f, 1f),
                Random.Range(-1f, 1f),
                0
            );

            GameObject smallerEnemy = Instantiate(splitEnemyPrefab, spawnPosition, Quaternion.identity);

            // 设置较小的属性
            Enemy enemyScript = smallerEnemy.GetComponent<Enemy>();
            if (enemyScript != null)
            {
                enemyScript.health = Mathf.RoundToInt(health * 0.5f);
                enemyScript.damage = Mathf.RoundToInt(damage * 0.7f);
            }
        }
    }

    private void DropCoins()
    {
        if (coinPrefab == null) return;

        int coinCount = Random.Range(minCoins, maxCoins + 1);

        for (int i = 0; i < coinCount; i++)
        {
            Vector3 spawnPosition = transform.position + new Vector3(
                coinSpawnOffset.x + Random.Range(-0.2f, 0.2f),
                coinSpawnOffset.y + Random.Range(-0.1f, 0.1f),
                0
            );

            GameObject coin = Instantiate(coinPrefab, spawnPosition, Quaternion.identity);

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
    }

    public void SetSpawner(EnemySpawner enemySpawner)
    {
        spawner = enemySpawner;
    }

    public void SetEnemyData(EnemySO data)
    {
        enemyData = data;
        ApplyEnemySOData();
    }

    #endregion
}