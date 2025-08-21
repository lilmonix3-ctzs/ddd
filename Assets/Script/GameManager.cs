using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("���UI")]
    [SerializeField] private TextMeshProUGUI coinsText;
    [SerializeField] private GameObject coinNotificationPrefab;
    [SerializeField] private Transform notificationContainer;

    private int coins = 0;

    private void Awake()
    {
        // ����ģʽ
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        // ���ر���Ľ����
        LoadCoins();
        UpdateCoinsUI();
    }

    // ���ӽ��
    public void AddCoins(int amount)
    {
        coins += amount;
        UpdateCoinsUI();
        SaveCoins();

        // ��ʾ��ý��֪ͨ
        if (coinNotificationPrefab != null && notificationContainer != null)
        {
            GameObject notification = Instantiate(coinNotificationPrefab, notificationContainer);
            TextMeshProUGUI notificationText = notification.GetComponentInChildren<TextMeshProUGUI>();
            if (notificationText != null)
            {
                notificationText.text = $"+{amount}";
            }

            // �Զ�����֪ͨ
            Destroy(notification, 2f);
        }
    }

    // ���ٽ��
    public bool SpendCoins(int amount)
    {
        if (coins >= amount)
        {
            coins -= amount;
            UpdateCoinsUI();
            SaveCoins();
            return true;
        }
        return false;
    }

    // ��ȡ��ǰ�����
    public int GetCoins()
    {
        return coins;
    }

    // ����UI
    private void UpdateCoinsUI()
    {
        if (coinsText != null)
        {
            coinsText.text = coins.ToString();
        }
    }

    // ������
    private void SaveCoins()
    {
        PlayerPrefs.SetInt("PlayerCoins", coins);
        PlayerPrefs.Save();
    }

    // ���ؽ��
    private void LoadCoins()
    {
        coins = PlayerPrefs.GetInt("PlayerCoins", 0);
    }

    // ���ý�ң����ڲ��ԣ�
    public void ResetCoins()
    {
        coins = 0;
        UpdateCoinsUI();
        SaveCoins();
    }
}