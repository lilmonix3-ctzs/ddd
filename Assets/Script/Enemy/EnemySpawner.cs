using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEditor.Experimental.GraphView;

public class EnemySpawner : MonoBehaviour
{
    [Header("��������")]
    [SerializeField] private GameObject[] enemyPrefabs; // ����Ԥ��������
    [SerializeField] private float spawnInterval = 3f; // ���ɼ�����룩
    [SerializeField] private int maxEnemies = 10; // ��������
    [SerializeField] private float spawnRadius = 10f; // ���ɰ뾶�����Ϊ���ģ�
    [SerializeField] private float minDistanceFromPlayer = 5f; // ���������С����
    [SerializeField] private float spawnCheckRadius = 1f; // ���ɵ���뾶
    [SerializeField] private int totalTimeToSpawn = 90; // ������ʱ�䣨�룩

    [Header("�Ѷ�����")]
    [SerializeField] private bool increaseDifficultyOverTime = true; // ��ʱ�������Ѷ�
    [SerializeField] private float difficultyInterval = 30f; // �Ѷ����Ӽ�����룩
    [SerializeField] private float minSpawnInterval = 0.5f; // ��С���ɼ��
    [SerializeField] private float spawnIntervalDecrease = 0.1f; // ÿ���Ѷ�����ʱ���ٵļ��
    [SerializeField] private int maxEnemiesIncrease = 2; // ÿ���Ѷ�����ʱ���ӵĵ�����

    [Header("����������")]
    [SerializeField] private GameObject portalPrefab; // ������Ԥ����
    [SerializeField] private Vector3 portalSpawnOffset = new Vector3(0, 0, 0); // ����������ƫ��
    [SerializeField] private string nextSceneName = "NextLevel"; // ��һ�س�������

    private Transform playerTransform;
    private int currentEnemies = 0;
    private float difficultyTimer = 0f;
    private float spawnTimer = 0f;
    private bool spawningActive = true;
    private bool portalSpawned = false;
    private GameObject portalInstance;

    private void Start()
    {

        playerTransform = GameObject.FindGameObjectWithTag("Player").transform;

        if (playerTransform == null)
        {
            Debug.LogError("δ�ҵ���Ҷ�����ȷ�������'Player'��ǩ");
            return;
        }

        // ��ʼ����Э��
        StartCoroutine(SpawnEnemies());

        // ��ʼ�Ѷ�Э��
        if (increaseDifficultyOverTime)
        {
            StartCoroutine(IncreaseDifficultyOverTime());
        }

        // ��ʼ��ʱ���ʱ
        StartCoroutine(TotalSpawnTimeCountdown());
    }

    private void Update()
    {
        // ����Ƿ�Ӧ�����ɴ�����
        CheckForPortalSpawn();
    }

    private IEnumerator SpawnEnemies()
    {
        while (spawningActive)
        {
            // �����ǰ������С�����ֵ
            if (currentEnemies < maxEnemies)
            {
                SpawnEnemy();
            }

            // �ȴ����ɼ��
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    private IEnumerator TotalSpawnTimeCountdown()
    {
        yield return new WaitForSeconds(totalTimeToSpawn);

        // �����������ʱ�䣬ֹͣ����
        spawningActive = false;
        Debug.Log("�����������ʱ�䣬ֹͣ���ɵ���");

        // ����Ѿ�û�е��ˣ�ֱ�����ɴ�����
        if (currentEnemies <= 0)
        {
            SpawnPortal();
        }
    }

    private void SpawnEnemy()
    {
        if (enemyPrefabs == null || enemyPrefabs.Length == 0)
        {
            Debug.LogWarning("û�����õ���Ԥ���壡");
            return;
        }

        // ���ѡ���������
        GameObject enemyPrefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];

        // ��ȡ��Ч����λ��
        Vector2 spawnPosition = GetValidSpawnPosition();

        // ʵ��������
        GameObject enemy = Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);

        // ���õ������������ã���������ʱ���ټ�����
        Enemy enemyScript = enemy.GetComponent<Enemy>();
        if (enemyScript != null)
        {
            enemyScript.SetSpawner(this);
        }
        else
        {
            Debug.LogWarning("����Ԥ����ȱ��Enemy�ű����");
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
            // �����������
            Vector2 randomDirection = Random.insideUnitCircle.normalized;

            // ������루����С��������뾶֮�䣩
            float distance = Random.Range(minDistanceFromPlayer, spawnRadius);

            // ��������λ��
            spawnPosition = (Vector2)playerTransform.position + randomDirection * distance;

            // ���λ���Ƿ���Ч��û����ײ�壩
            Collider2D hit = Physics2D.OverlapCircle(spawnPosition, spawnCheckRadius);

            if (hit == null)
            {
                // λ����Ч
                return spawnPosition;
            }

            attempts++;

        } while (attempts < maxAttempts);

        // �����γ���ʧ�ܣ��������λ�ã�Ӧ�������������
        Debug.LogWarning("�޷��ҵ���Ч����λ�ã�ʹ�ñ���λ��");
        return (Vector2)playerTransform.position + Random.insideUnitCircle.normalized * spawnRadius;
    }

    // ��������ʱ����
    public void EnemyDied()
    {
        currentEnemies--;

        // ����Ƿ�Ӧ�����ɴ�����
        CheckForPortalSpawn();
    }

    private void CheckForPortalSpawn()
    {
        // ����Ѿ����ɹ������ţ����ټ��
        if (portalSpawned) return;

        // ���ֹͣ������û�е��ˣ����ɴ�����
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
            // ����Ҹ������ɴ�����
            Vector3 portalPosition = playerTransform.position + portalSpawnOffset;
            portalInstance = Instantiate(portalPrefab, portalPosition, Quaternion.identity);

            // ���ô�������Ϊ
            Portal portalScript = portalInstance.GetComponent<Portal>();
            if (portalScript != null)
            {
                portalScript.SetTargetScene(nextSceneName);
            }

            Debug.Log("������������");
        }
        else
        {
            Debug.LogWarning("δ���ô�����Ԥ���壡");
        }
    }

    private IEnumerator IncreaseDifficultyOverTime()
    {
        while (spawningActive)
        {
            yield return new WaitForSeconds(difficultyInterval);

            // �����Ѷ�
            spawnInterval = Mathf.Max(minSpawnInterval, spawnInterval - spawnIntervalDecrease);
            maxEnemies += maxEnemiesIncrease;

            Debug.Log($"�Ѷ����ӣ����ɼ��: {spawnInterval}, ������: {maxEnemies}");
        }
    }

    // ���Ի�����������
    private void OnDrawGizmosSelected()
    {
        if (playerTransform != null)
        {
            // ������С����
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(playerTransform.position, minDistanceFromPlayer);

            // �������뾶
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(playerTransform.position, spawnRadius);
        }
    }

    // ���������������ⲿǿ��ֹͣ����
    public void StopSpawning()
    {
        spawningActive = false;
    }

    // �������������ڻ�ȡ��ǰ״̬
    public bool IsSpawningActive()
    {
        return spawningActive;
    }

    public int GetCurrentEnemies()
    {
        return currentEnemies;
    }
}