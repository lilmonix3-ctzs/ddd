using UnityEngine;

public class Coin : MonoBehaviour
{
    [Header("�������")]
    [SerializeField] private int value = 1; // ��Ҽ�ֵ
    [SerializeField] private float magnetRange = 3f; // ������Χ
    [SerializeField] private float magnetSpeed = 5f; // �����ٶ�
    [SerializeField] private float collectionDelay = 0.5f; // �ռ��ӳ٣���ֹ�����ɾͱ����գ�
    [SerializeField] private GameObject collectionEffect; // �ռ���Ч

    private Transform playerTransform;
    private Rigidbody2D rb;
    private bool canBeCollected = false;
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

    private void Update()
    {
        // �����Ҳ����ڣ����Բ���
        if (playerTransform == null)
        {
            FindPlayer();
            return;
        }

        // ����Ƿ���Կ�ʼ����
        if (canBeCollected)
        {
            float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);

            if (distanceToPlayer <= magnetRange)
            {
                // ������ƶ�
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
                        magnetSpeed * Time.deltaTime
                    );
                }
            }
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

        // ������ײ��⣨���֮ǰ�����ˣ�
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            col.isTrigger = true;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!canBeCollected) return;

        // ����Ƿ������
        if (other.CompareTag("Player"))
        {
            CollectCoin(other.transform);
        }
    }

    private void CollectCoin(Transform collector)
    {
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

        // �����ռ���Ч
        //AudioSource.PlayClipAtPoint(Resources.Load<AudioClip>("Sounds/CoinCollect"), transform.position);

        // ���ٽ��
        Destroy(gameObject);
    }

    // ���ӻ�������Χ
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, magnetRange);
    }
}