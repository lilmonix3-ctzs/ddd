using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Enemy", menuName = "Enemy")]
public class EnemySO : ScriptableObject
{
    public enum EnemyType
    {
        melee,
        ranged,
        tank,
        charger,
        spliter,
        lurker
    };

    public enum EnemyRank
    {
        normal,
        elite,
        boss
    };

    public string objectName;
    public Sprite objectSprite;
    public Transform prefab;
    public EnemyType enemyType;
    public EnemyRank enemyRank;
}
