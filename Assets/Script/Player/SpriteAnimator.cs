using UnityEngine;

public class SpriteAnimator : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private BaseMovement baseMovement;
    [SerializeField] private PlayerHealth playerHealth;

    private const string RunAnimation = "run";
    private const string DodgeAnimation = "dodge";
    private const string WalkAnimation = "walk";
    private const string DieAnimation = "die";


    void Start()
    {
        animator = GetComponent<Animator>();
    }

    void FixedUpdate()
    {
        animator.SetBool(RunAnimation, baseMovement.IsWalking());
        animator.SetBool(DodgeAnimation, baseMovement.IsDodging());
        animator.SetBool(WalkAnimation, baseMovement.IsSlowed());
        if(playerHealth.IsDead()) animator.SetTrigger(DieAnimation);
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