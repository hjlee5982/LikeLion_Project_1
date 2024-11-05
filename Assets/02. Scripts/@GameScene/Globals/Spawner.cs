using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Spawner : MonoBehaviour
{
    #region ����
    // ���ӸŴ������� ����� �޾� ���͸� �����ϴ� ��
    // ���Ϳ��� ��ȯ �� ��ġ�� ���� �� ��, �������� �Ѱ��ָ� ��
    // ��� ����, ���Ϳ��� ����
    #endregion

    // �ѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤ�
    #region ObjectPooling�� ����
    public GameObject EnemyPrefab;
    public int InitialObjectNumber = 5;

    private Queue<GameObject> _enemyPool = new Queue<GameObject>();

    // ������Ʈ ���� ����� ������ Hierarchyâ ���������ϱ� �����Ϸ��� ��������
    private Transform _root;
    private Transform Root
    {
        get 
        {
            if(_root == null)
            {
                GameObject go = new GameObject() { name = $"{EnemyPrefab.name}Pool" };
                _root = go.transform;
            }

            return _root;
        }
    }
    #endregion

    // �ѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤ�

    #region ���Ϳ��� �Ѱ��� ����
    public struct SpawnDesc
    {
        // �̰Ŵ� ���⼭ ����� ä����
        public Spawner spawner;          // ���Ͱ� ������ �� �׾��ٰ� �����ʿ��� �˷��ַ���
        public Vector3 spawnPosition;    // �� ���ʹ� ���⼭ ��ȯ
        public Vector3 destinationPoint; // �� ���ʹ� ���Ⱑ ������
        public Queue<Vector3> route;     // �� ���ʹ� �� ��θ� ���� �������� �̵�

        // �̰Ŵ� ���ӸŴ������� �޾ƿ�
        // ���� ���� ����
        public int   spawnPoint;    // �� ���ʹ� ��� ���������ǿ��� ��ȯ�ɲ���
        public int   initNumber;    // �̸�ŭ ��ȯ�Ҳ���
        public float spawnInterval; // �� ���ݸ��� ��ȯ�Ҳ���

        // ���� ���� ����
        public float attackDamage; // ���ݷ�
        public float attackSpeed;  // ���ݼӵ�
        public float moveSpeed;    // �̵��ӵ�
        public float hp;           // ü��
    }
    #endregion

    // �ѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤ�

    #region ��� ������ ����
    public Tilemap PointsTileMap; // �����, ������, ��������Ʈ�� ������ Ÿ�ϸ���

    // Ÿ�ϸ��� ��ȸ�ϸ鼭 ��������Ʈ�� if���� �Ϸ��� ��������
    public Sprite WayPointSprite;
    public Sprite SpawnPointSprite;
    public Sprite DestinationPointSprite;

    List<Vector3> _wayPoints         = new List<Vector3>();
    List<Vector3> _spawnPoints       = new List<Vector3>();
         Vector3  _destinationPoint;

    List<Queue<Vector3>> _routes = new List<Queue<Vector3>>();
    #endregion

    // �ѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤ�

    #region ��Ÿ ����
    private SpawnDesc _currentDesc;
    private int       _count          = 0;
    public static  bool      _stageEnd       = false;
    private int       _numOfDestEnemy = 0;
    private int       _numOfDeath     = 0;
    #endregion
    void Start()
    {
        #region Tile, Route ����
        // �Ʒ� foreach������ Ÿ�ϸ��� ���ϴܺ��� �������� �б� ������

        // ������ �� ���� ��� Ȯ��

        // _spawnPoints[0] ���� ������ ���ʹ�
        // _spawnPoint[0] -> _wayPoint[3] -> _destinationPoint ������ �̵�

        // _spawnPoint[1] ���� ������ ���ʹ�
        // _spawnPoint[1] -> _wayPoint[2-1-0-6-7-4-5-3] -> _destinationPoint ������ �̵�

        foreach (Vector3Int pos in PointsTileMap.cellBounds.allPositionsWithin)
        {
            TileBase tile = PointsTileMap.GetTile(pos);

            Tile tileObject = tile as Tile;

            if (tileObject != null && tileObject.sprite == WayPointSprite)
            {
                _wayPoints.Add(PointsTileMap.GetCellCenterWorld(pos));
            }
            else if (tileObject != null && tileObject.sprite == SpawnPointSprite)
            {
                _spawnPoints.Add(PointsTileMap.GetCellCenterWorld(pos));
            }
            else if (tileObject != null && tileObject.sprite == DestinationPointSprite)
            {
                _destinationPoint = PointsTileMap.GetCellCenterWorld(pos);
            }

            // ���� ��Ī, ���� as Tile�� ���� if���� �Ʒ�ó�� �� �� ����
            //if(tile is Tile tileObject && tileObject.sprite == TargetSprite)
            //{
            //    Debug.Log(pos);
            //}
        }

        {
            // 0�� ��������Ʈ���� ������ ������Ʈ�� �̵� ���
            Queue<Vector3> _route = new Queue<Vector3>();
            {
                _route.Enqueue(_wayPoints[3]);
            }
            _routes.Add(_route);
        }
        {
            // 1�� ��������Ʈ���� ������ ������Ʈ�� �̵� ���
            Queue<Vector3> _route = new Queue<Vector3>();
            {
                _route.Enqueue(_wayPoints[2]);
                _route.Enqueue(_wayPoints[1]);
                _route.Enqueue(_wayPoints[0]);
                _route.Enqueue(_wayPoints[6]);
                _route.Enqueue(_wayPoints[7]);
                _route.Enqueue(_wayPoints[4]);
                _route.Enqueue(_wayPoints[5]);
                _route.Enqueue(_wayPoints[3]);
            }
            _routes.Add(_route);
        }

        #endregion

        // �ѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤ�

        #region EnemyPool ���

        for (int i = 0; i < 50; ++i)
        {
            GameObject go = Instantiate(EnemyPrefab, Root);
            go.SetActive(false);
            _enemyPool.Enqueue(go);
        }

        #endregion
    }

    IEnumerator EnemySpawnCoroutine(SpawnDesc desc)
    {
        while (true)
        {
            if (_enemyPool.Count > 0)
            {
                GameObject go = _enemyPool.Dequeue();

                go.SetActive(true);
                go.GetComponent<Enemy>().Spawn(desc);

                ++_count;
            }

            yield return new WaitForSeconds(desc.spawnInterval);
        }
    }

    public void Spawn(SpawnDesc desc)
    {
        _stageEnd = false;

        desc.spawner          = this;
        desc.spawnPosition    = _spawnPoints[desc.spawnPoint];
        desc.destinationPoint = _destinationPoint;
        desc.route            = _routes[desc.spawnPoint];

        _currentDesc = desc;

        _count = 0;

        StartCoroutine(EnemySpawnCoroutine(desc));
    }

    void Update()
    {
        // �ڷ�ƾ ���߱�
        //if(true)
        //{
        //    1. Ư�� �ڷ�ƾ ���߱�
        //    StopCoroutine(EnemySpawnCoroutine(_route0Desc));
        //      
        //    2. �� �� ���߱�
        //    StopAllCoroutines();
        //}

        // _count�� ���� �� �ϰ� �ڷ�ƾ ���߷��� �����س��� ����
        if(_currentDesc.initNumber != 0 && _count == _currentDesc.initNumber)
        {
            // ���� �� ������ ������ ����
            if(_count == _currentDesc.initNumber)
            {
                StopAllCoroutines();
            }
        }

        // �ʱ� ���� ���� �� = ���ӸŴ���, Spawner�� �˰�����
        // ���� ���� ��      = Enemy�� Spawner���� �˷���
        // ������ ���� ��    = Enemy�� Spawner���� �˷���

        // ���� ���� �� + ������ ���� �� = �ʱ� ���� ���� ��
        // �� ���� �� ���������� Ŭ����
        if (_numOfDeath + _numOfDestEnemy == _currentDesc.initNumber)
        {
            _numOfDeath     = 0;
            _numOfDestEnemy = 0;

            _stageEnd = true;
        }
    }

    //public bool IsStageEnd()
    //{
    //    return _stageEnd;
    //}

    // Enemy���� �޾ƿ� ������ ���� ī��Ʈ
    public void Destination()
    {
        _numOfDestEnemy += 1;
    }
    // Enemy���� �޾ƿ� ���� ���� ī��Ʈ
    public void Dead()
    {
        _numOfDeath += 1;
    }

    public int NumOfEnemyInField()
    {
        return _numOfDestEnemy + _numOfDeath;
    }

    // ���ӸŴ����� ���� ������ ���� ī��Ʈ
    // Life�� ����� �� ���
    public int NumOfDestEnemy()
    {
        return _numOfDestEnemy;
    }
}