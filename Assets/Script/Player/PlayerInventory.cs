using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    private int coins = 0;

    // ���ӽ��
    public void AddCoins(int amount)
    {
        coins += amount;
        Debug.Log($"��� {amount} ��ң���ǰ����: {coins}");

        // ֪ͨUI����
        GameManager gameManager = FindObjectOfType<GameManager>();
        if (gameManager != null)
        {
            gameManager.AddCoins(amount);
        }
    }

    // ��ȡ��ǰ�����
    public int GetCoins()
    {
        return coins;
    }

    // ���ѽ��
    public bool SpendCoins(int amount)
    {
        if (coins >= amount)
        {
            coins -= amount;
            Debug.Log($"���� {amount} ��ң�ʣ��: {coins}");
            return true;
        }
        return false;
    }
}