using UnityEngine;
using System.Collections;

public class EnemySpawner : MonoBehaviour
{
    [Header("��������")]
    [SerializeField] private GameObject[] enemyPrefabs; // ����Ԥ��������
    [SerializeField] private float spawnInterval = 3f; // ���ɼ�����룩
    [SerializeField] private int maxEnemies = 10; // ��������
    [SerializeField] private float spawnRadius = 10f; // ���ɰ뾶�����Ϊ���ģ�
    [SerializeField] private float minDistanceFromPlayer = 5f; // ���������С����
    [SerializeField] private float spawnCheckRadius = 1f; // ���ɵ���뾶

    [Header("�Ѷ�����")]
    [SerializeField] private bool increaseDifficultyOverTime = true; // ��ʱ�������Ѷ�
    [SerializeField] private float difficultyInterval = 30f; // �Ѷ����Ӽ�����룩
    [SerializeField] private float minSpawnInterval = 0.5f; // ��С���ɼ��
    [SerializeField] private float spawnIntervalDecrease = 0.1f; // ÿ���Ѷ�����ʱ���ٵļ��
    [SerializeField] private int maxEnemiesIncrease = 2; // ÿ���Ѷ�����ʱ���ӵĵ�����

    private Transform playerTransform;
    private int currentEnemies = 0;
    private float difficultyTimer = 0f;

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
    }

    private IEnumerator SpawnEnemies()
    {
        while (true)
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
        return (Vector2)playerTransform.position + Random.insideUnitCircle.normalized * minDistanceFromPlayer;
    }

    // ��������ʱ����
    public void EnemyDied()
    {
        currentEnemies--;
    }

    private IEnumerator IncreaseDifficultyOverTime()
    {
        while (true)
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
}