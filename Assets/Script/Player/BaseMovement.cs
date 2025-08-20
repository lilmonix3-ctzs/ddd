using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class BaseMovement : MonoBehaviour
{

    [SerializeField] private float originSpeed = 5f;
    [SerializeField] private float shootingSpeed = 3f;
    [SerializeField] private float moveSmoothTime = 0.1f; // ƽ���ƶ�ʱ��
    [SerializeField] private float playerwidth = .35f;
    [SerializeField] private float playerheight = .5f;
    [SerializeField] private int dogeSpeed = 10; // �����ٶ�
    [SerializeField] private int dogeTime = 20; // ���ܳ���ʱ��
    [SerializeField] private float dodgeinterval = 2f; // ���ܼ��ʱ��

    [SerializeField] private WeaponHold weaponHold; // �����������

    [SerializeField] private SpriteAnimator animator;

    Rigidbody2D rb;
    private CinemachineVirtualCamera virtualCamera;

    float speed;
    private bool iswalking = false;
    private bool isdodging = false;
    private bool toRight = true;

    private bool canDoge = true; // �Ƿ��������

    private float horizontal = 0f;
    private float vertical = 0f;

    private int Dogedelta = 0;
    private Vector2 lastMovement = Vector2.zero;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        Initializecamerafollow();
    }

    private void Initializecamerafollow()
    {
        virtualCamera = GameObject.FindObjectOfType<CinemachineVirtualCamera>();
        if (virtualCamera != null)
        {
            virtualCamera.Follow = this.transform;
            virtualCamera.LookAt = this.transform;
        }
    }

    private void Update()
    {
        if (GameInput.Instance.IsDodgeClicked() && !isdodging && canDoge)
        {
            Dogedelta = dogeTime;
            isdodging = true;
            canDoge = false; // ��ֹ�ٴ�����
            // �������ܼ����ʱ��
            StartCoroutine(DodgeCooldown());
        }
        HandlePushForce();
    }

    private void HandlePushForce()
    {
        if (!iswalking)
        {
            //������������ϵ���������Ϊ0
            rb.velocity = Vector2.zero;
        }
    }

    private IEnumerator DodgeCooldown()
    {
        yield return new WaitForSeconds(dodgeinterval);
        canDoge = true; // ���ܼ�������������ٴ�����
    }

    void FixedUpdate()
    {
        Vector2 m_input = GameInput.Instance != null ? GameInput.Instance.GetMovementInputnormalized() : Vector2.zero;

        speed = weaponHold != null && weaponHold.IsFiring() || !canDoge ? shootingSpeed : originSpeed;

        horizontal = Mathf.MoveTowards(horizontal, m_input.x, moveSmoothTime);
        vertical = Mathf.MoveTowards(vertical, m_input.y, moveSmoothTime);

        Vector2 move = new Vector2(horizontal, vertical);

        float MoveDistance = Time.deltaTime * speed;

        if (move.x > 0)
        {
            toRight = true;
        }
        else if (move.x < 0)
        {
            toRight = false;
        }

        if (Dogedelta > 0)
        {
            iswalking = false;
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
                lastMovement = move.normalized;
            }
            else
            {
                iswalking = false;
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        lastMovement = Vector2.zero;
    }

    public bool IsWalking() => iswalking;
    public bool IsDodging() => isdodging;
    public bool IsFacingRight() => toRight;
    public bool IsSlowed() => speed == shootingSpeed && iswalking;
}