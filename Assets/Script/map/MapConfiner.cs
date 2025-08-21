using UnityEngine;

[RequireComponent(typeof(PolygonCollider2D))]
public class MapConfiner : MonoBehaviour
{
    void Start()
    {
        SetupMapConfiner();
    }

    void SetupMapConfiner()
    {
        PolygonCollider2D polyCollider = GetComponent<PolygonCollider2D>();
        polyCollider.isTrigger = true;

        // 设置碰撞体为地图边界
        // 你可以在这里通过代码设置顶点，或者在Inspector中手动设置
        //Vector2[] points = new Vector2[4];
        //points[0] = new Vector2(-10, -7.5f);
        //points[1] = new Vector2(10, -7.5f);
        //points[2] = new Vector2(10, 7.5f);
        //points[3] = new Vector2(-10, 7.5f);
        //polyCollider.points = points;
    }
}