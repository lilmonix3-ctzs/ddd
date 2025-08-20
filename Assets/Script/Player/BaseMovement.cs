using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseMovement : MonoBehaviour
{

    [SerializeField] private float originSpeed = 5f;
    [SerializeField] private float shootingSpeed = 3f;
    [SerializeField] private float moveSmoothTime = 0.1f; // 平滑移动时间
    [SerializeField] private float playerwidth = .35f;
    [SerializeField] private float playerheight = .5f;
    [SerializeField] private int  dogeSpeed = 10; // 闪避速度

    [SerializeField] private WeaponHold weaponHold; // 武器持有组件

    [SerializeField] private SpriteAnimator animator;

    Rigidbody2D rb;

    float speed;
    private bool iswalking = false;
    private bool isdodging = false;

    private float horizontal = 0f;
    private float vertical = 0f;

    private int Dogedelta = 0;
    private Vector2 lastMovement = Vector2.zero;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void FixedUpdate()
    {
        Vector2 m_input = GameInput.Instance != null ? GameInput.Instance.GetMovementInputnormalized() : Vector2.zero;

        speed = weaponHold != null && weaponHold.IsFiring() ? shootingSpeed : originSpeed;

        horizontal = Mathf.MoveTowards(horizontal, m_input.x, moveSmoothTime);
        vertical = Mathf.MoveTowards(vertical, m_input.y, moveSmoothTime);

        Vector2 move = new Vector2(horizontal, vertical);

        float MoveDistance = Time.deltaTime * speed;

        if(move.x > 0)
        {
            animator?.Turn(true);
        }
        else if (move.x < 0)
        {
            animator?.Turn(false);
        }

        if (GameInput.Instance.IsDodgeClicked() && !isdodging)
        {
            Dogedelta = 30;
            isdodging = true;
            // 更新动画状态
            if (animator != null && lastMovement != Vector2.zero)
            {
                animator.SetState(SpriteAnimator.PlayerState.Dodge);
            }
        }

        if (Dogedelta > 0)
        {
            MoveDistance = Time.deltaTime * dogeSpeed;
            Dogedelta--;
            rb.MovePosition(rb.position + lastMovement * MoveDistance);
        }
        else
        {
            isdodging = false;

            if (move != Vector2.zero)
            {

                iswalking = true;

                //Vector2 boxCenter = (Vector2)transform.position + Vector2.up * playerheight;
                //if (!Physics2D.BoxCast(boxCenter, new Vector2(playerwidth, playerheight) * 2, 0f, move.normalized, MoveDistance))
                //{
                //    transform.position += (Vector3)move * MoveDistance;
                //}
                rb.MovePosition(rb.position + move * MoveDistance);
                // 更新动画状态
                if (animator != null)
                //&& !isdodging)
                {
                    animator.SetState(SpriteAnimator.PlayerState.Walk);
                }
                lastMovement = move.normalized;
            }
            else
            {
                iswalking = false;
                if (animator != null)
                {
                    animator.SetState(SpriteAnimator.PlayerState.Idle);
                }
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        lastMovement = Vector2.zero;
    }

    public bool IsWalking() => iswalking;

}
