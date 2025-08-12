using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

[CreateAssetMenu(fileName = "New Weapon", menuName = "Weapon")]
public class WeaponSO : ScriptableObject
{
    public string objectName;
    public Sprite objectSprite;
    public Transform prefab;
    public float damage = 10f;
    public float bulletSpeed = 20f; // ÉäËÙ
    public float attackSpeed = 1f; // Ã¿Ãë¹¥»÷´ÎÊý
    public float attackRange = 5f; // ¹¥»÷·¶Î§
}
