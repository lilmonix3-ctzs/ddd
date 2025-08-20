using UnityEngine;

public class Coin : MonoBehaviour
{
    [Header("金币设置")]
    [SerializeField] private int value = 1; // 金币价值
    [SerializeField] private float magnetRange = 3f; // 磁吸范围
    [SerializeField] private float magnetSpeed = 5f; // 磁吸速度
    [SerializeField] private float collectionDelay = 0.5f; // 收集延迟（防止刚生成就被吸收）
    [SerializeField] private GameObject collectionEffect; // 收集特效

    private Transform playerTransform;
    private Rigidbody2D rb;
    private bool canBeCollected = false;
    private float spawnTime;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spawnTime = Time.time;

        // 查找玩家
        FindPlayer();

        // 延迟后可被收集
        Invoke(nameof(EnableCollection), collectionDelay);
    }

    private void Update()
    {
        // 如果玩家不存在，尝试查找
        if (playerTransform == null)
        {
            FindPlayer();
            return;
        }

        // 检查是否可以开始磁吸
        if (canBeCollected)
        {
            float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);

            if (distanceToPlayer <= magnetRange)
            {
                // 向玩家移动
                Vector2 direction = (playerTransform.position - transform.position).normalized;

                if (rb != null)
                {
                    // 使用物理移动
                    rb.velocity = direction * magnetSpeed;
                }
                else
                {
                    // 直接移动
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

        // 启用碰撞检测（如果之前禁用了）
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            col.isTrigger = true;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!canBeCollected) return;

        // 检查是否是玩家
        if (other.CompareTag("Player"))
        {
            CollectCoin(other.transform);
        }
    }

    private void CollectCoin(Transform collector)
    {
        // 通知游戏管理器或玩家增加金币
        GameManager gameManager = FindObjectOfType<GameManager>();
        if (gameManager != null)
        {
            gameManager.AddCoins(value);
        }
        else
        {
            // 备用方案：直接查找玩家
            PlayerInventory playerInventory = collector.GetComponent<PlayerInventory>();
            if (playerInventory != null)
            {
                playerInventory.AddCoins(value);
            }
        }

        // 播放收集特效
        if (collectionEffect != null)
        {
            Instantiate(collectionEffect, transform.position, Quaternion.identity);
        }

        // 播放收集音效
        //AudioSource.PlayClipAtPoint(Resources.Load<AudioClip>("Sounds/CoinCollect"), transform.position);

        // 销毁金币
        Destroy(gameObject);
    }

    // 可视化磁吸范围
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, magnetRange);
    }
}