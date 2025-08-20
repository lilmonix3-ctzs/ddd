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
    //private bool Waiting = false;

    // 将枚举设为public
    public enum PlayerState { Idle, Walk, Dodge }

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void FixedUpdate()
    {
        timer += Time.deltaTime;

        if (timer >= frameRate)
        {
            timer = 0;
            currentFrame = (currentFrame + 1) % GetCurrentFrames().Length;
            spriteRenderer.sprite = GetCurrentFrames()[currentFrame];
            //if (currentFrame == 0) Waiting = false;
        }

    }

    private Sprite[] GetCurrentFrames()
    {
        switch (currentState)
        {
            case PlayerState.Walk: return walkFrames;
            case PlayerState.Dodge:  return dodgeFrames;
            default: return idleFrames;
        }
    }

    public void SetState(PlayerState newState)
    {
        if (currentState != newState)
            //&& !Waiting)
        {
            currentState = newState;
            currentFrame = 0;
        }
        //if (newState == PlayerState.Dodge)
        //    Waiting = true;
    }

    public void Turn(bool IsRight)
    {
        if (IsRight)
            transform.localScale = new Vector3(1, 1, 1);
        else
            transform.localScale = new Vector3(-1, 1, 1);
    }

    //public void Continue()
    //{
    //    Waiting = false;
    //}
}