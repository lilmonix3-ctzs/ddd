using UnityEngine;

public class SpriteAnimator : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private BaseMovement baseMovement;

    private const string RunAnimation = "run";
    private const string DodgeAnimation = "dodge";
    private const string WalkAnimation = "walk";


    void Start()
    {
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        animator.SetBool(RunAnimation, baseMovement.IsWalking());
        animator.SetBool(DodgeAnimation, baseMovement.IsDodging());
        animator.SetBool(WalkAnimation, baseMovement.IsSlowed());
        if (baseMovement.IsFacingRight())
        {
            transform.localScale = new Vector3(1, 1, 1);
        }
        else
        {
            transform.localScale = new Vector3(-1, 1, 1);
        }

    }



}