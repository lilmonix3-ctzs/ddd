using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class Portal : MonoBehaviour
{
    [Header("����������")]
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

        // �������Ƿ��ڽ�����Χ��
        float distance = Vector3.Distance(transform.position, playerTransform.position);
        bool nowInRange = distance <= interactionRadius;

        // ״̬�仯ʱ������ʾ
        if (nowInRange != playerInRange)
        {
            playerInRange = nowInRange;

            if (interactionPrompt != null)
            {
                interactionPrompt.SetActive(playerInRange);
            }
        }

        // �������ڷ�Χ�ڣ��ȴ�3����Զ����ͣ�������½���������������
        if (playerInRange)
        {
            //��ɫ����
            spriteRenderer.color = new Color(1f, 0.5f, 0.5f, 1f); // ���Ը�����Ҫ������ɫ

            if (GameInput.Instance.IsComfirmClicked()) // �����滻Ϊ��������
            {
                StopAllCoroutines();
                TeleportToNextLevel();
            }
            else
            {
                // �ȴ�3����Զ�����
                StartCoroutine(AutoTeleportCoroutine(3f));
            }
        }
        else
        {
            // ֹͣ�Զ�����Э��
            StopAllCoroutines();
            // �ָ���ɫ
            spriteRenderer.color = Color.white; // �ָ�ԭɫ
        }
    }

    // ����Ŀ�곡��
    public void SetTargetScene(string sceneName)
    {
        targetSceneName = sceneName;
    }

    // ��������һ��
    private void TeleportToNextLevel()
    {
        Debug.Log($"������: {targetSceneName}");

        // ������������Ӵ�����Ч����Ч��

        // ������һ����
        SceneManager.LoadScene(targetSceneName);
    }

    private IEnumerator AutoTeleportCoroutine(float delay)
    {
        yield return new WaitForSeconds(delay);
        TeleportToNextLevel();
    }

    // ���ӻ�������Χ
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, interactionRadius);
    }
}