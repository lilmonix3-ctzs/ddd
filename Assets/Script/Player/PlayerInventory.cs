using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    private int coins = 0;

    // 增加金币
    public void AddCoins(int amount)
    {
        coins += amount;
        Debug.Log($"获得 {amount} 金币，当前总数: {coins}");

        // 通知UI更新
        GameManager gameManager = FindObjectOfType<GameManager>();
        if (gameManager != null)
        {
            gameManager.AddCoins(amount);
        }
    }

    // 获取当前金币数
    public int GetCoins()
    {
        return coins;
    }

    // 消费金币
    public bool SpendCoins(int amount)
    {
        if (coins >= amount)
        {
            coins -= amount;
            Debug.Log($"消费 {amount} 金币，剩余: {coins}");
            return true;
        }
        return false;
    }
}