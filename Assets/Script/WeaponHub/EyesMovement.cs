using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EyesMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float moveSmoothTime = 0.1f; // 移动平滑时间

    [Header("Collision Settings")]
    [SerializeField] private float playerWidth = .35f;
    [SerializeField] private PolygonCollider2D boundCollider;
    private Vector2 mapMinBounds = new Vector2(-16, -16);
    private Vector2 mapMaxBounds = new Vector2(16, 16);

    private Rigidbody2D rb;
    private float horizontal = 0f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        // 初始化地图边界
        if (boundCollider != null)
        {
            mapMaxBounds = boundCollider.points[0];
            mapMinBounds = boundCollider.points[2];
        }
    }

    void FixedUpdate()
    {
        // 获取水平输入
        float inputX = Input.GetAxisRaw("Horizontal");

        // 平滑输入
        horizontal = Mathf.MoveTowards(horizontal, inputX, moveSmoothTime);

        // 移动角色
        if (horizontal != 0)
        {

            // 计算新位置并应用边界限制
            float moveDistance = horizontal * moveSpeed * Time.fixedDeltaTime;
            Vector2 newPosition = rb.position + new Vector2(moveDistance, 0);
            newPosition.x = Mathf.Clamp(newPosition.x, mapMinBounds.x + playerWidth, mapMaxBounds.x - playerWidth);

            // 应用移动
            rb.MovePosition(newPosition);
        }
    }

    // 在Scene视图中绘制边界
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Vector3 center = new Vector3(
            (mapMinBounds.x + mapMaxBounds.x) / 2,
            (mapMinBounds.y + mapMaxBounds.y) / 2,
            0
        );
        Vector3 size = new Vector3(
            mapMaxBounds.x - mapMinBounds.x,
            mapMaxBounds.y - mapMinBounds.y,
            1
        );
        Gizmos.DrawWireCube(center, size);
    }
}
