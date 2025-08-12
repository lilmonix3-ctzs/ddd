using UnityEngine;

public class SpriteAnimator : MonoBehaviour
{
    public Sprite[] idleFrames;
    public Sprite[] walkFrames;
    public Sprite[] dodgeFrames;

    [SerializeField] private float frameRate = 0.1f;

    private SpriteRenderer spriteRenderer;
    private float timer;
    private int currentFrame;
    private PlayerState currentState;

    // 将枚举设为public
    public enum PlayerState { Idle, Walk, Dodge }

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        timer += Time.deltaTime;

        if (timer >= frameRate)
        {
            timer = 0;
            currentFrame = (currentFrame + 1) % GetCurrentFrames().Length;
            spriteRenderer.sprite = GetCurrentFrames()[currentFrame];
        }

    }

    private Sprite[] GetCurrentFrames()
    {
        switch (currentState)
        {
            case PlayerState.Walk: return walkFrames;
            case PlayerState.Dodge: return dodgeFrames;
            default: return idleFrames;
        }
    }

    public void SetState(PlayerState newState)
    {
        if (currentState != newState)
        {
            currentState = newState;
            currentFrame = 0;
        }
    }

    public void Turn(bool IsRight)
    {
        if (IsRight)
            transform.localScale = new Vector3(1, 1, 1);
        else
            transform.localScale = new Vector3(-1, 1, 1);
    }
}