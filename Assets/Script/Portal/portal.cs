using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class Portal : MonoBehaviour
{
    [Header("传送门设置")]
    [SerializeField] private string targetSceneName = "NextLevel";
    [SerializeField] private float interactionRadius = 2f;
    [SerializeField] private GameObject interactionPrompt;
    [SerializeField] private SpriteRenderer spriteRenderer;

    private Transform playerTransform;
    private bool playerInRange = false;

    private void Start()
    {
        playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
        if (interactionPrompt != null)
        {
            interactionPrompt.SetActive(false);
        }
    }

    private void Update()
    {
        if (playerTransform == null) return;

        // 检查玩家是否在交互范围内
        float distance = Vector3.Distance(transform.position, playerTransform.position);
        bool nowInRange = distance <= interactionRadius;

        // 状态变化时更新提示
        if (nowInRange != playerInRange)
        {
            playerInRange = nowInRange;

            if (interactionPrompt != null)
            {
                interactionPrompt.SetActive(playerInRange);
            }
        }

        // 如果玩家在范围内，等待3秒后自动传送，如果按下交互键则立即传送
        if (playerInRange)
        {
            //颜色加深
            spriteRenderer.color = new Color(1f, 0.5f, 0.5f, 1f); // 可以根据需要调整颜色

            if (GameInput.Instance.IsComfirmClicked()) // 可以替换为其他按键
            {
                StopAllCoroutines();
                TeleportToNextLevel();
            }
            else
            {
                // 等待3秒后自动传送
                StartCoroutine(AutoTeleportCoroutine(3f));
            }
        }
        else
        {
            // 停止自动传送协程
            StopAllCoroutines();
            // 恢复颜色
            spriteRenderer.color = Color.white; // 恢复原色
        }
    }

    // 设置目标场景
    public void SetTargetScene(string sceneName)
    {
        targetSceneName = sceneName;
    }

    // 传送至下一关
    private void TeleportToNextLevel()
    {
        Debug.Log($"传送至: {targetSceneName}");

        // 可以在这里添加传送特效、音效等

        // 加载下一场景
        SceneManager.LoadScene(targetSceneName);
    }

    private IEnumerator AutoTeleportCoroutine(float delay)
    {
        yield return new WaitForSeconds(delay);
        TeleportToNextLevel();
    }

    // 可视化交互范围
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, interactionRadius);
    }
}