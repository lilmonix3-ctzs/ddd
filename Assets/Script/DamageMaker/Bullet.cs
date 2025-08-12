using UnityEngine;

public class Bullet : MonoBehaviour
{
    [Header("Effects")]
    [SerializeField] private GameObject hitEffectPrefab;
    [SerializeField] private AudioClip hitSound;

    private float speed;
    private float lifetime;
    private float damage;
    private Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            Debug.LogError("Rigidbody2D component missing on bullet!", this);
            enabled = false;
        }
    }

    public void Initialize(Quaternion shootDirection, float Speed, float AttackRange, float Damage)
    {
        speed = Speed;
        lifetime = AttackRange / speed;
        damage = Damage;

        // 使用射击方向的前向向量
        Vector2 direction = shootDirection * Vector2.right;
        rb.velocity = direction * speed;


        Destroy(gameObject, lifetime);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        //// 忽略不需要碰撞的对象
        if(!other.CompareTag("Enemy"))
            return;
        //if (other.CompareTag("Bullet") || other.CompareTag("Player"))
        //    return;

        //// 伤害处理
        //Health health = other.GetComponent<Health>();
        //if (health != null)
        //{
        //    health.TakeDamage(damage);
        //}

        //// 命中效果
        //if (hitEffectPrefab != null)
        //    Instantiate(hitEffectPrefab, transform.position, Quaternion.identity);

        if (hitSound != null)
            AudioSource.PlayClipAtPoint(hitSound, transform.position);

        Destroy(gameObject);
    }
}