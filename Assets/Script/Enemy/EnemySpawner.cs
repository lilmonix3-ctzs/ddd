using UnityEngine;
using System.Collections;

public class EnemySpawner : MonoBehaviour
{
    [Header("生成设置")]
    [SerializeField] private GameObject[] enemyPrefabs; // 敌人预制体数组
    [SerializeField] private float spawnInterval = 3f; // 生成间隔（秒）
    [SerializeField] private int maxEnemies = 10; // 最大敌人数
    [SerializeField] private float spawnRadius = 10f; // 生成半径（玩家为中心）
    [SerializeField] private float minDistanceFromPlayer = 5f; // 距离玩家最小距离
    [SerializeField] private float spawnCheckRadius = 1f; // 生成点检查半径

    [Header("难度设置")]
    [SerializeField] private bool increaseDifficultyOverTime = true; // 随时间增加难度
    [SerializeField] private float difficultyInterval = 30f; // 难度增加间隔（秒）
    [SerializeField] private float minSpawnInterval = 0.5f; // 最小生成间隔
    [SerializeField] private float spawnIntervalDecrease = 0.1f; // 每次难度增加时减少的间隔
    [SerializeField] private int maxEnemiesIncrease = 2; // 每次难度增加时增加的敌人数

    private Transform playerTransform;
    private int currentEnemies = 0;
    private float difficultyTimer = 0f;

    private void Start()
    {
        playerTransform = GameObject.FindGameObjectWithTag("Player").transform;

        if (playerTransform == null)
        {
            Debug.LogError("未找到玩家对象！请确保玩家有'Player'标签");
            return;
        }

        // 开始生成协程
        StartCoroutine(SpawnEnemies());

        // 开始难度协程
        if (increaseDifficultyOverTime)
        {
            StartCoroutine(IncreaseDifficultyOverTime());
        }
    }

    private IEnumerator SpawnEnemies()
    {
        while (true)
        {
            // 如果当前敌人数小于最大值
            if (currentEnemies < maxEnemies)
            {
                SpawnEnemy();
            }

            // 等待生成间隔
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    private void SpawnEnemy()
    {
        if (enemyPrefabs == null || enemyPrefabs.Length == 0)
        {
            Debug.LogWarning("没有设置敌人预制体！");
            return;
        }

        // 随机选择敌人类型
        GameObject enemyPrefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];

        // 获取有效生成位置
        Vector2 spawnPosition = GetValidSpawnPosition();

        // 实例化敌人
        GameObject enemy = Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);

        // 设置敌人生成器引用（用于死亡时减少计数）
        Enemy enemyScript = enemy.GetComponent<Enemy>();
        if (enemyScript != null)
        {
            enemyScript.SetSpawner(this);
        }
        else
        {
            Debug.LogWarning("敌人预制体缺少Enemy脚本组件");
        }

        currentEnemies++;
    }

    private Vector2 GetValidSpawnPosition()
    {
        Vector2 spawnPosition;
        int attempts = 0;
        const int maxAttempts = 30;

        do
        {
            // 生成随机方向
            Vector2 randomDirection = Random.insideUnitCircle.normalized;

            // 计算距离（在最小距离和最大半径之间）
            float distance = Random.Range(minDistanceFromPlayer, spawnRadius);

            // 计算生成位置
            spawnPosition = (Vector2)playerTransform.position + randomDirection * distance;

            // 检查位置是否有效（没有碰撞体）
            Collider2D hit = Physics2D.OverlapCircle(spawnPosition, spawnCheckRadius);

            if (hit == null)
            {
                // 位置有效
                return spawnPosition;
            }

            attempts++;

        } while (attempts < maxAttempts);

        // 如果多次尝试失败，返回玩家位置（应避免这种情况）
        Debug.LogWarning("无法找到有效生成位置，使用备用位置");
        return (Vector2)playerTransform.position + Random.insideUnitCircle.normalized * minDistanceFromPlayer;
    }

    // 敌人死亡时调用
    public void EnemyDied()
    {
        currentEnemies--;
    }

    private IEnumerator IncreaseDifficultyOverTime()
    {
        while (true)
        {
            yield return new WaitForSeconds(difficultyInterval);

            // 增加难度
            spawnInterval = Mathf.Max(minSpawnInterval, spawnInterval - spawnIntervalDecrease);
            maxEnemies += maxEnemiesIncrease;

            Debug.Log($"难度增加！生成间隔: {spawnInterval}, 最大敌人: {maxEnemies}");
        }
    }

    // 调试绘制生成区域
    private void OnDrawGizmosSelected()
    {
        if (playerTransform != null)
        {
            // 绘制最小距离
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(playerTransform.position, minDistanceFromPlayer);

            // 绘制最大半径
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(playerTransform.position, spawnRadius);
        }
    }
}