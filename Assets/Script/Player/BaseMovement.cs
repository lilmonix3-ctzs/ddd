using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseMovement : MonoBehaviour
{

    [SerializeField] private float speed = 5f;
    [SerializeField] private float moveSmoothTime = 0.1f; // 平滑移动时间
    [SerializeField] private float playerwidth = .35f;
    [SerializeField] private float playerheight = .5f;
    [SerializeField] private int  dogeSpeed = 10; // 闪避速度

    [SerializeField] private SpriteAnimator animator;

    Rigidbody2D rb;

    private bool iswalking = false;

    private float horizontal = 0f;
    private float vertical = 0f;

    private int Dogedelta = 0;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        Vector2 m_input = GameInput.Instance != null ? GameInput.Instance.GetMovementInputnormalized() : Vector2.zero;
        

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

        if (GameInput.Instance.IsDodgeClicked())
        {
            Dogedelta = 10;
        }

        if (Dogedelta > 0)
        {
            MoveDistance = Time.deltaTime * dogeSpeed;
            Dogedelta--;
        }

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
            {
                animator.SetState(SpriteAnimator.PlayerState.Walk);
            }
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


    public bool IsWalking() => iswalking;

}
