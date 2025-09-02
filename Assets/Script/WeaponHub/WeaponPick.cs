using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponPick : MonoBehaviour
{
    [SerializeField] private WeaponSO weaponSO;
    [SerializeField] private Transform weaponPoint;
    [SerializeField] private Transform spritePoint;
    [SerializeField] private BoxCollider2D interactionCollider;
    [SerializeField] private float weaponScale = 5f;
    [SerializeField] private float rotateSpeed = 1000f; // 鼠标拖动时的加速度
    [SerializeField] private float angularDrag = 10f;   // 角速度阻尼（阻力）
    [SerializeField] private float returnSpeed = 5f;     // 回正速度
    [SerializeField] private SpriteRenderer barSprite;
    [SerializeField] private Animator barAnimator;

    private Transform weapon; // 缓存武器实例
    private Transform weaponSprite;

    private float angularVelocity = 0f; // 当前角速度
    private Quaternion initialRotation;  // 初始旋转
    private Vector3 FloatTop;
    private Vector3 Floatbutton;

    private bool isMouseOver = false;
    private bool isAppearing = false;

    void Start()
    {
        if(barAnimator != null) barAnimator.enabled = false;
        if(barSprite != null) barSprite.color = new Color(0.5f, 0.5f, 0.5f, 1);

        //将武器的精灵设置到角色的精灵点
        spritePoint.GetComponent<SpriteRenderer>().sprite = weaponSO.objectSprite;
    }

    void Update()
    {
        CheckMousePosition();
        if (isAppearing)
        {
            FloatUp();
            HandleRotate();
        }
        else
        {
            waitForClick();
        }
    }

    private void waitForClick()
    {
        //TODO: 完善点击交互逻辑，最好是从图表处上拉得到武器
        if (isMouseOver && Input.GetMouseButtonDown(0))
        {
            isAppearing = true;
            if (barAnimator != null) barAnimator.enabled = true;
            if (barSprite != null) barSprite.color = new Color(1, 1, 1, 1);

            //将武器的预制体实例化到武器点
            weapon = Instantiate(weaponSO.prefab, Vector3.zero, weaponPoint.rotation);
            weapon.SetParent(weaponPoint);

            weaponSprite = weapon.GetChild(0);

            // 初始化时让武器斜向右上45度
            weaponSprite.localEulerAngles = new Vector3(0f, 0f, 45f);
            weaponSprite.localScale = Vector3.one * weaponScale;

            FloatTop = weaponSprite.localPosition + new Vector3(0, 0.5f, 0);
            Floatbutton = weaponSprite.localPosition;

            // 记录初始旋转
            initialRotation = weaponSprite.rotation;
        }
    }

    private void HandleRotate()
    {

        // 鼠标左键按下时，检测鼠标X轴移动并施加角加速度
        if (Input.GetMouseButton(0) && weaponSprite != null && isMouseOver)
        {
            float mouseX = Input.GetAxis("Mouse X");
            // 施加角加速度（力作用）
            angularVelocity += -mouseX * rotateSpeed * Time.deltaTime;
        }

        // 用当前角速度驱动旋转（绕世界y轴）
        if (weaponSprite != null)
        {
            weaponSprite.Rotate(0f, angularVelocity * Time.deltaTime, 0f, Space.World);

            // 角速度足够小且没有鼠标操作时，缓慢回正
            if (Mathf.Abs(angularVelocity) < 1f && !(Input.GetMouseButton(0) && isMouseOver))
            {
                weaponSprite.rotation = Quaternion.Lerp(weaponSprite.rotation, initialRotation, returnSpeed * Time.deltaTime);
            }
        }

        // 角速度阻尼（模拟摩擦力/空气阻力），让旋转逐渐停下来
        angularVelocity = Mathf.Lerp(angularVelocity, 0f, angularDrag * Time.deltaTime);
    }
    private void CheckMousePosition()
    {
        if (interactionCollider == null) return;
        
        //如果鼠标在世界外面，直接返回
        if (!Camera.main.pixelRect.Contains(Input.mousePosition))
        {
            isMouseOver = false;
            return;
        }

        // 获取鼠标在世界空间中的位置
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = 0;

        // 使用碰撞器的bounds检查鼠标是否在范围内
        isMouseOver = interactionCollider.bounds.Contains(mouseWorldPos);
    }

    private void FloatUp()
    {
        if (isMouseOver)
        {
            weaponSprite.localPosition = Vector3.Lerp(weaponSprite.localPosition, FloatTop, Time.deltaTime * 2f);
        }
        else
        {
            weaponSprite.localPosition = Vector3.Lerp(weaponSprite.localPosition, Floatbutton, Time.deltaTime * 2f);
        }
    }
}
