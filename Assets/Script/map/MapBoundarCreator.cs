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
        // �����ϱ߽�
        CreateBoundary("TopBoundary", new Vector3(0, mapSize.y / 2, 0), new Vector3(mapSize.x, 1, 1));

        // �����±߽�
        CreateBoundary("BottomBoundary", new Vector3(0, -mapSize.y / 2, 0), new Vector3(mapSize.x, 1, 1));

        // ������߽�
        CreateBoundary("LeftBoundary", new Vector3(-mapSize.x / 2, 0, 0), new Vector3(1, mapSize.y, 1));

        // �����ұ߽�
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

        // �����Ҫ������ײ�����Ǵ��������Ƴ������isTrigger = true
        // ���������Ĵ���
        // Rigidbody2D rb = boundary.AddComponent<Rigidbody2D>();
        // rb.isKinematic = true;
    }
}