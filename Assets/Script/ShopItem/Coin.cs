using UnityEngine;

public class Coin : MonoBehaviour
{
    [Header("金币设置")]
    [SerializeField] private int value = 1; // 金币价值
    [SerializeField] private float magnetRange = 3f; // 磁吸范围
    [SerializeField] private float magnetSpeed = 5f; // 磁吸速度
    [SerializeField] private float collectionDelay = 0.5f; // 收集延迟
    [SerializeField] private float directCollectionRange = 0.2f; // 直接收集范围
    [SerializeField] private GameObject collectionEffect; // 收集特效

    private Transform playerTransform;
    private Rigidbody2D rb;
    private bool canBeCollected = false;
    private bool isBeingCollected = false; // 防止重复收集
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

    private void FixedUpdate()
    {
        // 如果玩家不存在，尝试查找
        if (playerTransform == null)
        {
            FindPlayer();
            return;
        }

        // 检查是否可以开始磁吸和收集
        if (canBeCollected && !isBeingCollected)
        {
            HandleMagnetBehavior();
            CheckDirectCollection(); // 主动检测是否应该被收集
        }
    }

    // 处理磁吸行为
    private void HandleMagnetBehavior()
    {
        float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);

        if (distanceToPlayer <= magnetRange)
        {
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
                    magnetSpeed * Time.fixedDeltaTime
                );
            }
        }
    }

    // 主动检测是否应该被收集
    private void CheckDirectCollection()
    {
        if (playerTransform == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);

        // 如果非常接近玩家，直接收集（避免错过碰撞检测）
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

        // 启用碰撞检测
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            col.isTrigger = true;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!canBeCollected || isBeingCollected) return;

        // 检查是否是玩家
        if (other.CompareTag("Player"))
        {
            isBeingCollected = true;
            CollectCoin(other.transform);
        }
    }

    private void CollectCoin(Transform collector)
    {
        // 立即禁用所有组件，防止重复处理
        DisableCoinComponents();

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

        // 销毁金币
        Destroy(gameObject);
    }

    // 立即禁用所有相关组件
    private void DisableCoinComponents()
    {
        // 禁用碰撞体
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        // 禁用刚体物理模拟
        if (rb != null) rb.simulated = false;

        // 禁用渲染器
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null) renderer.enabled = false;

        // 禁用所有子物体的渲染器
        foreach (Transform child in transform)
        {
            Renderer childRenderer = child.GetComponent<Renderer>();
            if (childRenderer != null) childRenderer.enabled = false;
        }
    }

    // 可视化磁吸范围和直接收集范围
    private void OnDrawGizmosSelected()
    {
        // 磁吸范围（黄色）
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, magnetRange);

        // 直接收集范围（红色）
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, directCollectionRange);
    }

    // 在编辑器中重置状态的方法
    public void ResetCoin()
    {
        canBeCollected = false;
        isBeingCollected = false;

        // 重新启用组件
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