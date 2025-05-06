using System;
using MKDir;
using UnityEngine;

[Serializable]
public class MapRow
{
    public Map[] maps;
}

public enum MapDirType
{
    Right,
    Left,
    Up
}

public class MapManager : MonoBehaviour
{
    [SerializeField] private GameObject _target;
    public MapRow[] mapArr;

    [SerializeField] private GameObject[] _patterns;

    public float MaxHeight => _maxHeight;
    private float _maxHeight = 0;

    public float CurrentHeight => _currentHeight;
    private float _currentHeight = 0;

    private Vector2 _currentTargetPosition;
    private bool _isMapChange = false;
    private int mapHorizontalIndex = 0;
    private int mapVerticalIndex = 0;

    private float _mapSize;

    private void Awake()
    {
        _mapSize = mapArr[0].maps[0].transform.Find("Visual").GetComponent<Collider>().bounds.size.x;
    }

    private void Start()
    {
        GameReset();
    }

    public void GameReset()
    {
        ResetMap();
        _isMapChange = false;
        mapVerticalIndex = 0;
        mapHorizontalIndex = 0;
        _currentHeight = 0;
        UIEvents.ChangeRecordEvent?.Invoke(_currentHeight);
        _currentTargetPosition = _target.transform.position;
    }

    private void Update()
    {
        //* Calculate Height
        if (_target.transform.position.y > _currentHeight)
        {
            _currentHeight = _target.transform.position.y;
            UIEvents.ChangeRecordEvent?.Invoke(_currentHeight);
        }

        //* Map Move
        MapCheck();
    }

    private void ResetMap()
    {
        for (int i = 0; i < mapArr.Length; i++)
            for (int ii = 0; ii < mapArr[i].maps.Length; ii++)
            {
                if (i == 1 && ii == 1) continue;
                var map = mapArr[i].maps[ii];
                map.ResetPosition();
                map.ResetObstacle(_patterns.GetRandomElement(), _mapSize, 2.76f);
            }
    }

    private void MapCheck()
    {
        if (_isMapChange == true)
        {
            return;
        }

        // Right Map Move
        if (_target.transform.position.x > _currentTargetPosition.x + _mapSize)
        {
            MapRelocation(CalculateColumnMap(), MapDirType.Right);
            mapHorizontalIndex = (mapHorizontalIndex + 1) % mapArr.Length;
        }

        // Left Map Move
        if (_target.transform.position.x < _currentTargetPosition.x - _mapSize)
        {
            mapHorizontalIndex = (mapHorizontalIndex + 2) % mapArr.Length;
            MapRelocation(CalculateColumnMap(), MapDirType.Left);
        }

        // Up Map Move
        if (_target.transform.position.y > _currentTargetPosition.y + _mapSize)
        {
            MapRelocation(mapArr[mapVerticalIndex].maps, MapDirType.Up);
            mapVerticalIndex = (mapVerticalIndex + 1) % mapArr.Length;
        }

    }

    private void MapRelocation(Map[] maps, MapDirType type)
    {
        _isMapChange = true;

        foreach (Map map in maps)
        {
            map.ResetObstacle(_patterns.GetRandomElement(), _mapSize, 2.76f);

            switch (type)
            {
                case MapDirType.Right:
                    MapMove(map, _mapSize * 3);
                    break;
                case MapDirType.Left:
                    MapMove(map, -_mapSize * 3);
                    break;
                case MapDirType.Up:
                    MapMove(map, _mapSize * 3, true);
                    break;
            }
        }

        _currentTargetPosition = _target.transform.position;
        _isMapChange = false;
    }

    private void MapMove(Map map, float distance, bool isY = false)
    {
        if (isY == false)
        {
            map.transform.position = new Vector3(map.transform.position.x + distance, map.transform.position.y, map.transform.position.z);
        }
        else
        {
            map.transform.position = new Vector3(map.transform.position.x, map.transform.position.y + distance, map.transform.position.z);
        }
    }

    private Map[] CalculateColumnMap()
    {
        Map[] columnMap = new Map[3];
        columnMap[0] = mapArr[0].maps[mapHorizontalIndex];
        columnMap[1] = mapArr[1].maps[mapHorizontalIndex];
        columnMap[2] = mapArr[2].maps[mapHorizontalIndex];

        return columnMap;
    }

    #if DEBUG
    #region TestCode
    private void TestMapCheck()
    {
        if (_isMapChange == true)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.A))
        {
            mapHorizontalIndex = (mapHorizontalIndex += 2) % mapArr.Length;
            MapRelocation(CalculateColumnMap(), MapDirType.Left);
            Debug.Log($"HorizontalIndex : {mapHorizontalIndex}");
        }

        if (Input.GetKeyDown(KeyCode.D))
        {
            MapRelocation(CalculateColumnMap(), MapDirType.Right);
            mapHorizontalIndex = ++mapHorizontalIndex % mapArr.Length;
            Debug.Log($"HorizontalIndex : {mapHorizontalIndex}");
        }

        if (Input.GetKeyDown(KeyCode.W))
        {
            MapRelocation(mapArr[mapVerticalIndex].maps, MapDirType.Up);
            mapVerticalIndex = ++mapVerticalIndex % mapArr.Length;
            Debug.Log($"VerticalIndex : {mapVerticalIndex}");
        }
    }

    #endregion
#endif
}
