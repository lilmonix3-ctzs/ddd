using UnityEngine;

public class Coin : MonoBehaviour
{
    [Header("�������")]
    [SerializeField] private int value = 1; // ��Ҽ�ֵ
    [SerializeField] private float magnetRange = 3f; // ������Χ
    [SerializeField] private float magnetSpeed = 5f; // �����ٶ�
    [SerializeField] private float collectionDelay = 0.5f; // �ռ��ӳ�
    [SerializeField] private float directCollectionRange = 0.2f; // ֱ���ռ���Χ
    [SerializeField] private GameObject collectionEffect; // �ռ���Ч

    private Transform playerTransform;
    private Rigidbody2D rb;
    private bool canBeCollected = false;
    private bool isBeingCollected = false; // ��ֹ�ظ��ռ�
    private float spawnTime;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spawnTime = Time.time;

        // �������
        FindPlayer();

        // �ӳٺ�ɱ��ռ�
        Invoke(nameof(EnableCollection), collectionDelay);
    }

    private void FixedUpdate()
    {
        // �����Ҳ����ڣ����Բ���
        if (playerTransform == null)
        {
            FindPlayer();
            return;
        }

        // ����Ƿ���Կ�ʼ�������ռ�
        if (canBeCollected && !isBeingCollected)
        {
            HandleMagnetBehavior();
            CheckDirectCollection(); // ��������Ƿ�Ӧ�ñ��ռ�
        }
    }

    // ���������Ϊ
    private void HandleMagnetBehavior()
    {
        float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);

        if (distanceToPlayer <= magnetRange)
        {
            Vector2 direction = (playerTransform.position - transform.position).normalized;

            if (rb != null)
            {
                // ʹ�������ƶ�
                rb.velocity = direction * magnetSpeed;
            }
            else
            {
                // ֱ���ƶ�
                transform.position = Vector2.MoveTowards(
                    transform.position,
                    playerTransform.position,
                    magnetSpeed * Time.fixedDeltaTime
                );
            }
        }
    }

    // ��������Ƿ�Ӧ�ñ��ռ�
    private void CheckDirectCollection()
    {
        if (playerTransform == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);

        // ����ǳ��ӽ���ң�ֱ���ռ�����������ײ��⣩
        if (distanceToPlayer < directCollectionRange)
        {
            isBeingCollected = true;
            CollectCoin(playerTransform);
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

    private void EnableCollection()
    {
        canBeCollected = true;

        // ������ײ���
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            col.isTrigger = true;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!canBeCollected || isBeingCollected) return;

        // ����Ƿ������
        if (other.CompareTag("Player"))
        {
            isBeingCollected = true;
            CollectCoin(other.transform);
        }
    }

    private void CollectCoin(Transform collector)
    {
        // �������������������ֹ�ظ�����
        DisableCoinComponents();

        // ֪ͨ��Ϸ��������������ӽ��
        GameManager gameManager = FindObjectOfType<GameManager>();
        if (gameManager != null)
        {
            gameManager.AddCoins(value);
        }
        else
        {
            // ���÷�����ֱ�Ӳ������
            PlayerInventory playerInventory = collector.GetComponent<PlayerInventory>();
            if (playerInventory != null)
            {
                playerInventory.AddCoins(value);
            }
        }

        // �����ռ���Ч
        if (collectionEffect != null)
        {
            Instantiate(collectionEffect, transform.position, Quaternion.identity);
        }

        // ���ٽ��
        Destroy(gameObject);
    }

    // ������������������
    private void DisableCoinComponents()
    {
        // ������ײ��
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        // ���ø�������ģ��
        if (rb != null) rb.simulated = false;

        // ������Ⱦ��
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null) renderer.enabled = false;

        // �����������������Ⱦ��
        foreach (Transform child in transform)
        {
            Renderer childRenderer = child.GetComponent<Renderer>();
            if (childRenderer != null) childRenderer.enabled = false;
        }
    }

    // ���ӻ�������Χ��ֱ���ռ���Χ
    private void OnDrawGizmosSelected()
    {
        // ������Χ����ɫ��
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, magnetRange);

        // ֱ���ռ���Χ����ɫ��
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, directCollectionRange);
    }

    // �ڱ༭��������״̬�ķ���
    public void ResetCoin()
    {
        canBeCollected = false;
        isBeingCollected = false;

        // �����������
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = true;

        if (rb != null) rb.simulated = true;

        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null) renderer.enabled = true;

        foreach (Transform child in transform)
        {
            Renderer childRenderer = child.GetComponent<Renderer>();
            if (childRenderer != null) childRenderer.enabled = true;
        }
    }
}