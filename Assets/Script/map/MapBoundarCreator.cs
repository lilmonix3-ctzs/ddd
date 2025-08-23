using UnityEngine;

public class MapBoundaryCreator : MonoBehaviour
{
    [SerializeField] private Vector2 mapSize = new Vector2(20, 15);
    [SerializeField] private GameObject boundaryPrefab;

    void Start()
    {
        CreateMapBoundaries();
    }

    void CreateMapBoundaries()
    {
        // 创建上边界
        CreateBoundary("TopBoundary", new Vector3(0, mapSize.y / 2, 0), new Vector3(mapSize.x, 1, 1));

        // 创建下边界
        CreateBoundary("BottomBoundary", new Vector3(0, -mapSize.y / 2, 0), new Vector3(mapSize.x, 1, 1));

        // 创建左边界
        CreateBoundary("LeftBoundary", new Vector3(-mapSize.x / 2, 0, 0), new Vector3(1, mapSize.y, 1));

        // 创建右边界
        CreateBoundary("RightBoundary", new Vector3(mapSize.x / 2, 0, 0), new Vector3(1, mapSize.y, 1));
    }

    void CreateBoundary(string name, Vector3 position, Vector3 scale)
    {
        GameObject boundary = new GameObject(name);
        boundary.transform.position = position;
        boundary.transform.localScale = scale;
        //boundary.layer = LayerMask.NameToLayer("Boundary");

        BoxCollider2D collider = boundary.AddComponent<BoxCollider2D>();
        collider.isTrigger = true;

        // 如果需要物理碰撞而不是触发器，移除上面的isTrigger = true
        // 并添加下面的代码
        // Rigidbody2D rb = boundary.AddComponent<Rigidbody2D>();
        // rb.isKinematic = true;
    }
}