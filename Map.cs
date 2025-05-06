using UnityEngine;

public class Map : MonoBehaviour
{
    private GameObject _obstacles;
    private Vector3 _initPosition;

    private void Awake()
    {
        _initPosition = transform.position;
    }

    public void ResetObstacle(GameObject obstacles, float mapSize, float mapScale)
    {
        if (_obstacles != null) Destroy(_obstacles);
        _obstacles = Instantiate(obstacles, transform.GetChild(0));
        _obstacles.transform.localPosition = new Vector3(0, -mapSize / 2 / mapScale, 0.3f);
        _obstacles.transform.localScale = Vector3.one / mapScale;
        _obstacles.transform.rotation = Quaternion.identity;
    }

    public void ResetPosition()
    {
        transform.position = _initPosition;
    }
}
