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
    public float bulletSpeed = 20f; // �ӵ��ٶ�
    public float attackSpeed = 1f; // ÿ�빥������
    public float attackRange = 5f; // ������Χ
    public float ReloadTime = 1f;  //����ʱ��
}
