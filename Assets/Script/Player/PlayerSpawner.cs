using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerSpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    public GameObject playerPrefab;
    public bool spawnAtWorldCenter = true;
    public Vector3 customSpawnPosition = Vector3.zero;

    private GameObject playerInstance;

    private void Awake()
    {
        // �������е����
        playerInstance = GameObject.FindGameObjectWithTag("Player");

        if (playerInstance == null && playerPrefab != null)
        {
            // ���������
            SpawnPlayer();
        }
        else if (playerInstance != null)
        {
            // �����������λ��
            ResetPlayerPosition();
        }
    }

    private void SpawnPlayer()
    {
        Vector3 spawnPos = spawnAtWorldCenter ? Vector3.zero : customSpawnPosition;
        playerInstance = Instantiate(playerPrefab, spawnPos, Quaternion.identity);
        playerInstance.name = "Player";

        DontDestroyOnLoad(playerInstance);
    }

    private void ResetPlayerPosition()
    {
        Vector3 spawnPos = spawnAtWorldCenter ? Vector3.zero : customSpawnPosition;
        playerInstance.transform.position = spawnPos;
    }

    // �ڱ༭���п��ӻ����ɵ�
    private void OnDrawGizmos()
    {
        Vector3 spawnPos = spawnAtWorldCenter ? Vector3.zero : customSpawnPosition;

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(spawnPos, 0.5f);
        Gizmos.DrawIcon(spawnPos, "PlayerSpawnPoint", true);
    }
}