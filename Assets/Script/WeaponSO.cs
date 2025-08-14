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
    public int damage = 10;
    public float bulletSpeed = 20f; // 子弹速度
    public float attackSpeed = 1f; // 每秒攻击次数
    public float attackRange = 5f; // 攻击范围
    public float ReloadTime = 1f;  //换弹时间
}
