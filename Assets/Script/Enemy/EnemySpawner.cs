using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class EnemySpawner : MonoBehaviour
{
    [Header("生成设置")]
    [SerializeField] private EnemySO[] enemyTypes;
    [SerializeField] private float spawnInterval = 3f;
    [SerializeField] private int maxEnemies = 10;
    [SerializeField] private float spawnRadius = 10f;
    [SerializeField] private float minDistanceFromPlayer = 5f;
    [SerializeField] private float spawnCheckRadius = 1f;
    [SerializeField] private int totalTimeToSpawn = 90;

    [Header("敌人类型权重")]
    [SerializeField] private float normalWeight = 0.7f;
    [SerializeField] private float eliteWeight = 0.25f;
    [SerializeField] private float bossWeight = 0.05f;

    [Header("难度设置")]
    [SerializeField] private bool increaseDifficultyOverTime = true;
    [SerializeField] private float difficultyInterval = 30f;
    [SerializeField] private float minSpawnInterval = 0.5f;
    [SerializeField] private float spawnIntervalDecrease = 0.1f;
    [SerializeField] private int maxEnemiesIncrease = 2;
    [SerializeField] private float eliteSpawnChanceIncrease = 0.1f;
    [SerializeField] private float bossSpawnChanceIncrease = 0.02f;

    [Header("传送门设置")]
    [SerializeField] private GameObject portalPrefab;
    [SerializeField] private float portalDistanceFromPlayer = 3f; // 传送门距离玩家的距离
    [SerializeField] private string nextSceneName = "NextLevel";

    private Transform playerTransform;
    private int currentEnemies = 0;
    private float difficultyTimer = 0f;
    private float spawnTimer = 0f;
    private bool spawningActive = true;
    private bool portalSpawned = false;
    private GameObject portalInstance;
    private float currentEliteChance = 0f;
    private float currentBossChance = 0f;

    // 敌人类型分类
    private List<EnemySO> normalEnemies = new List<EnemySO>();
    private List<EnemySO> eliteEnemies = new List<EnemySO>();
    private List<EnemySO> bossEnemies = new List<EnemySO>();

    // 世界中心点（可配置）
    private Vector3 worldCenter = Vector3.zero;

    private void Start()
    {
        playerTransform = GameObject.FindGameObjectWithTag("Player").transform;

        if (playerTransform == null)
        {
            Debug.LogError("未找到玩家对象！请确保玩家有'Player'标签");
            return;
        }

        // 分类敌人类型
        ClassifyEnemies();

        // 开始生成协程
        StartCoroutine(SpawnEnemies());

        // 开始难度协程
        if (increaseDifficultyOverTime)
        {
            StartCoroutine(IncreaseDifficultyOverTime());
        }

        // 开始总时间计时
        StartCoroutine(TotalSpawnTimeCountdown());
    }

    private void Update()
    {
        // 检查是否应该生成传送门
        CheckForPortalSpawn();
    }

    // 分类敌人类型
    private void ClassifyEnemies()
    {
        normalEnemies.Clear();
        eliteEnemies.Clear();
        bossEnemies.Clear();

        foreach (EnemySO enemy in enemyTypes)
        {
            if (enemy == null) continue;

            switch (enemy.enemyRank)
            {
                case EnemySO.EnemyRank.normal:
                    normalEnemies.Add(enemy);
                    break;
                case EnemySO.EnemyRank.elite:
                    eliteEnemies.Add(enemy);
                    break;
                case EnemySO.EnemyRank.boss:
                    bossEnemies.Add(enemy);
                    break;
            }
        }
    }

    private IEnumerator SpawnEnemies()
    {
        while (spawningActive)
        {
            if (currentEnemies < maxEnemies)
            {
                SpawnEnemy();
            }

            yield return new WaitForSeconds(spawnInterval);
        }
    }

    private IEnumerator TotalSpawnTimeCountdown()
    {
        yield return new WaitForSeconds(totalTimeToSpawn);

        spawningActive = false;
        Debug.Log("到达最大生成时间，停止生成敌人");

        if (currentEnemies <= 0)
        {
            SpawnPortal();
        }
    }

    private void SpawnEnemy()
    {
        if (enemyTypes == null || enemyTypes.Length == 0)
        {
            Debug.LogWarning("没有设置敌人类型！");
            return;
        }

        // 根据权重选择敌人类型
        EnemySO selectedEnemyType = SelectEnemyTypeByWeight();

        if (selectedEnemyType == null || selectedEnemyType.prefab == null)
        {
            Debug.LogWarning("选中的敌人类型或预制体为空！");
            return;
        }

        // 获取有效生成位置
        Vector2 spawnPosition = GetValidSpawnPosition();

        // 实例化敌人
        GameObject enemy = Instantiate(
            selectedEnemyType.prefab.gameObject,
            spawnPosition,
            Quaternion.identity
        );

        // 设置敌人数据
        Enemy enemyScript = enemy.GetComponent<Enemy>();
        if (enemyScript != null)
        {
            enemyScript.SetEnemyData(selectedEnemyType);
            enemyScript.SetSpawner(this);
        }
        else
        {
            Debug.LogWarning("敌人预制体缺少Enemy脚本组件");
        }

        currentEnemies++;

        // 记录生成信息
        Debug.Log($"生成敌人: {selectedEnemyType.objectName} ({selectedEnemyType.enemyRank})");
    }

    // 根据权重选择敌人类型
    private EnemySO SelectEnemyTypeByWeight()
    {
        // 计算随机值
        float randomValue = Random.value;

        // 确定敌人等级
        EnemySO.EnemyRank selectedRank;

        if (randomValue < currentBossChance && bossEnemies.Count > 0)
        {
            selectedRank = EnemySO.EnemyRank.boss;
        }
        else if (randomValue < currentBossChance + currentEliteChance && eliteEnemies.Count > 0)
        {
            selectedRank = EnemySO.EnemyRank.elite;
        }
        else
        {
            selectedRank = EnemySO.EnemyRank.normal;
        }

        // 从对应等级的敌人列表中随机选择一个
        List<EnemySO> selectedList;

        switch (selectedRank)
        {
            case EnemySO.EnemyRank.elite:
                selectedList = eliteEnemies;
                break;
            case EnemySO.EnemyRank.boss:
                selectedList = bossEnemies;
                break;
            default:
                selectedList = normalEnemies;
                break;
        }

        if (selectedList.Count == 0)
        {
            // 如果选择的列表为空，回退到普通敌人
            selectedList = normalEnemies;
        }

        return selectedList[Random.Range(0, selectedList.Count)];
    }

    private Vector2 GetValidSpawnPosition()
    {
        Vector2 spawnPosition;
        int attempts = 0;
        const int maxAttempts = 30;

        do
        {
            Vector2 randomDirection = Random.insideUnitCircle.normalized;
            float distance = Random.Range(minDistanceFromPlayer, spawnRadius);
            spawnPosition = (Vector2)playerTransform.position + randomDirection * distance;

            Collider2D hit = Physics2D.OverlapCircle(spawnPosition, spawnCheckRadius);

            if (hit == null)
            {
                return spawnPosition;
            }

            attempts++;

        } while (attempts < maxAttempts);

        Debug.LogWarning("无法找到有效生成位置，使用备用位置");
        return (Vector2)playerTransform.position + Random.insideUnitCircle.normalized * spawnRadius;
    }

    // 敌人死亡时调用
    public void EnemyDied()
    {
        currentEnemies--;
        CheckForPortalSpawn();
    }

    private void CheckForPortalSpawn()
    {
        if (portalSpawned) return;

        if (!spawningActive && currentEnemies <= 0)
        {
            SpawnPortal();
        }
    }

    private void SpawnPortal()
    {
        if (portalSpawned) return;

        portalSpawned = true;

        if (portalPrefab != null)
        {
            // 计算传送门位置
            Vector3 portalPosition = CalculatePortalPosition();

            portalInstance = Instantiate(portalPrefab, portalPosition, Quaternion.identity);

            Portal portalScript = portalInstance.GetComponent<Portal>();
            if (portalScript != null)
            {
                portalScript.SetTargetScene(nextSceneName);
            }

            Debug.Log($"传送门已生成在位置: {portalPosition}");
        }
        else
        {
            Debug.LogWarning("未设置传送门预制体！");
        }
    }

    // 计算传送门位置（在世界中心与玩家连线上）
    private Vector3 CalculatePortalPosition()
    {
        Vector3 playerPosition = playerTransform.position;

        // 如果玩家就在世界中心，直接在世界中心生成传送门
        if (Vector3.Distance(playerPosition, worldCenter) < 0.1f)
        {
            return worldCenter;
        }

        // 计算从世界中心指向玩家的方向
        Vector3 directionFromCenter = (playerPosition - worldCenter).normalized;

        // 在世界中心与玩家连线上，距离玩家一定距离的位置生成传送门
        return playerPosition - directionFromCenter * portalDistanceFromPlayer;
    }

    private IEnumerator IncreaseDifficultyOverTime()
    {
        // 初始化几率
        currentEliteChance = eliteWeight;
        currentBossChance = bossWeight;

        while (spawningActive)
        {
            yield return new WaitForSeconds(difficultyInterval);

            // 增加难度
            spawnInterval = Mathf.Max(minSpawnInterval, spawnInterval - spawnIntervalDecrease);
            maxEnemies += maxEnemiesIncrease;

            // 增加精英和BOSS生成几率
            currentEliteChance = Mathf.Min(0.8f, currentEliteChance + eliteSpawnChanceIncrease);
            currentBossChance = Mathf.Min(0.2f, currentBossChance + bossSpawnChanceIncrease);

            Debug.Log($"难度增加！生成间隔: {spawnInterval}, 最大敌人: {maxEnemies}, " +
                     $"精英几率: {currentEliteChance:P0}, BOSS几率: {currentBossChance:P0}");
        }
    }

    // 调试绘制生成区域
    private void OnDrawGizmosSelected()
    {
        if (playerTransform != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(playerTransform.position, minDistanceFromPlayer);

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(playerTransform.position, spawnRadius);

            // 绘制传送门预期位置
            if (portalSpawned && portalInstance != null)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawWireSphere(portalInstance.transform.position, 1f);
            }
            else
            {
                // 绘制传送门可能生成的位置
                Vector3 portalPos = CalculatePortalPosition();
                Gizmos.color = Color.cyan;
                Gizmos.DrawWireSphere(portalPos, 0.5f);
                Gizmos.DrawLine(playerTransform.position, portalPos);
            }
        }

        // 绘制世界中心
        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(worldCenter, 0.3f);
    }

    // 公开方法，用于外部强制停止生成
    public void StopSpawning()
    {
        spawningActive = false;
    }

    // 公开方法，用于获取当前状态
    public bool IsSpawningActive()
    {
        return spawningActive;
    }

    public int GetCurrentEnemies()
    {
        return currentEnemies;
    }

    // 重新分类敌人（用于运行时动态添加敌人类型）
    public void ReclassifyEnemies()
    {
        ClassifyEnemies();
    }

    // 添加新的敌人类型
    public void AddEnemyType(EnemySO newEnemyType)
    {
        if (newEnemyType == null) return;

        // 检查是否已存在
        foreach (EnemySO enemy in enemyTypes)
        {
            if (enemy == newEnemyType) return;
        }

        // 添加到数组
        System.Array.Resize(ref enemyTypes, enemyTypes.Length + 1);
        enemyTypes[enemyTypes.Length - 1] = newEnemyType;

        // 重新分类
        ReclassifyEnemies();
    }

    // 移除敌人类型
    public void RemoveEnemyType(EnemySO enemyTypeToRemove)
    {
        if (enemyTypeToRemove == null) return;

        List<EnemySO> newList = new List<EnemySO>();
        foreach (EnemySO enemy in enemyTypes)
        {
            if (enemy != enemyTypeToRemove)
            {
                newList.Add(enemy);
            }
        }

        enemyTypes = newList.ToArray();
        ReclassifyEnemies();
    }

    // 设置世界中心（如果需要）
    public void SetWorldCenter(Vector3 center)
    {
        worldCenter = center;
    }
}