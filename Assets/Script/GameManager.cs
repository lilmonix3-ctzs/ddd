using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("金币UI")]
    [SerializeField] private TextMeshProUGUI coinsText;
    [SerializeField] private GameObject coinNotificationPrefab;
    [SerializeField] private Transform notificationContainer;

    private int coins = 0;

    private void Awake()
    {
        // 单例模式
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        // 加载保存的金币数
        LoadCoins();
        UpdateCoinsUI();
    }

    // 增加金币
    public void AddCoins(int amount)
    {
        coins += amount;
        UpdateCoinsUI();
        SaveCoins();

        // 显示获得金币通知
        if (coinNotificationPrefab != null && notificationContainer != null)
        {
            GameObject notification = Instantiate(coinNotificationPrefab, notificationContainer);
            TextMeshProUGUI notificationText = notification.GetComponentInChildren<TextMeshProUGUI>();
            if (notificationText != null)
            {
                notificationText.text = $"+{amount}";
            }

            // 自动销毁通知
            Destroy(notification, 2f);
        }
    }

    // 减少金币
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

    // 获取当前金币数
    public int GetCoins()
    {
        return coins;
    }

    // 更新UI
    private void UpdateCoinsUI()
    {
        if (coinsText != null)
        {
            coinsText.text = coins.ToString();
        }
    }

    // 保存金币
    private void SaveCoins()
    {
        PlayerPrefs.SetInt("PlayerCoins", coins);
        PlayerPrefs.Save();
    }

    // 加载金币
    private void LoadCoins()
    {
        coins = PlayerPrefs.GetInt("PlayerCoins", 0);
    }

    // 重置金币（用于测试）
    public void ResetCoins()
    {
        coins = 0;
        UpdateCoinsUI();
        SaveCoins();
    }
}