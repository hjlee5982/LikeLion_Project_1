using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Spawner : MonoBehaviour
{
    #region 역할
    // 게임매니저에서 명령을 받아 몬스터를 생성하는 곳
    // 몬스터에게 소환 될 위치와 가야 할 길, 목적지를 넘겨주면 됨
    // 경로 가공, 몬스터에게 전달
    #endregion

    // ㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡ
    #region ObjectPooling용 변수
    public GameObject EnemyPrefab;
    public int InitialObjectNumber = 5;

    private Queue<GameObject> _enemyPool = new Queue<GameObject>();

    // 오브젝트 많이 만들어 놓으면 Hierarchy창 복잡해지니까 정리하려고 만들어놓음
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

    // ㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡ

    #region 몬스터에게 넘겨줄 변수
    public struct SpawnDesc
    {
        // 이거는 여기서 만들고 채워줌
        public Spawner spawner;          // 몬스터가 죽으면 나 죽었다고 스포너에게 알려주려고
        public Vector3 spawnPosition;    // 이 몬스터는 여기서 소환
        public Vector3 destinationPoint; // 이 몬스터는 여기가 목적지
        public Queue<Vector3> route;     // 이 몬스터는 이 경로를 통해 목적지로 이동

        // 이거는 게임매니저에서 받아옴
        // 생성 관련 변수
        public int   spawnPoint;    // 이 몬스터는 몇번 스폰포지션에서 소환될꺼임
        public int   initNumber;    // 이만큼 소환할꺼임
        public float spawnInterval; // 이 간격마다 소환할꺼임

        // 유닛 관련 변수
        public float attackDamage; // 공격력
        public float attackSpeed;  // 공격속도
        public float moveSpeed;    // 이동속도
        public float hp;           // 체력
    }
    #endregion

    // ㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡ

    #region 경로 가공용 변수
    public Tilemap PointsTileMap; // 출발지, 도착지, 웨이포인트를 찍어놓은 타일맵임

    // 타일맵을 순회하면서 스프라이트로 if연산 하려고 가져왔음
    public Sprite WayPointSprite;
    public Sprite SpawnPointSprite;
    public Sprite DestinationPointSprite;

    List<Vector3> _wayPoints         = new List<Vector3>();
    List<Vector3> _spawnPoints       = new List<Vector3>();
         Vector3  _destinationPoint;

    List<Queue<Vector3>> _routes = new List<Queue<Vector3>>();
    #endregion

    // ㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡ

    #region 기타 변수
    private SpawnDesc _currentDesc;
    private int       _count          = 0;
    public static  bool      _stageEnd       = false;
    private int       _numOfDestEnemy = 0;
    private int       _numOfDeath     = 0;
    #endregion
    void Start()
    {
        #region Tile, Route 가공
        // 아래 foreach문에서 타일맵의 좌하단부터 우측으로 읽기 때문에

        // 에디터 맵 보고 경로 확인

        // _spawnPoints[0] 에서 스폰된 몬스터는
        // _spawnPoint[0] -> _wayPoint[3] -> _destinationPoint 순으로 이동

        // _spawnPoint[1] 에서 스폰된 몬스터는
        // _spawnPoint[1] -> _wayPoint[2-1-0-6-7-4-5-3] -> _destinationPoint 순으로 이동

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

            // 패턴 매칭, 위에 as Tile줄 부터 if문을 아래처럼 쓸 수 있음
            //if(tile is Tile tileObject && tileObject.sprite == TargetSprite)
            //{
            //    Debug.Log(pos);
            //}
        }

        {
            // 0번 스폰포인트에서 생성된 오브젝트의 이동 경로
            Queue<Vector3> _route = new Queue<Vector3>();
            {
                _route.Enqueue(_wayPoints[3]);
            }
            _routes.Add(_route);
        }
        {
            // 1번 스폰포인트에서 생성된 오브젝트의 이동 경로
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

        // ㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡ

        #region EnemyPool 등록

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
        // 코루틴 멈추기
        //if(true)
        //{
        //    1. 특정 코루틴 멈추기
        //    StopCoroutine(EnemySpawnCoroutine(_route0Desc));
        //      
        //    2. 싹 다 멈추기
        //    StopAllCoroutines();
        //}

        // _count는 생성 다 하고 코루틴 멈추려고 선언해놓은 변수
        if(_currentDesc.initNumber != 0 && _count == _currentDesc.initNumber)
        {
            // 생성 다 했으면 생성기 끄기
            if(_count == _currentDesc.initNumber)
            {
                StopAllCoroutines();
            }
        }

        // 초기 생성 유닛 수 = 게임매니저, Spawner가 알고있음
        // 죽은 유닛 수      = Enemy가 Spawner에게 알려줌
        // 도착한 유닛 수    = Enemy가 Spawner에게 알려줌

        // 죽은 유닛 수 + 도착한 유닛 수 = 초기 생성 유닛 수
        // 가 됐을 때 스테이지가 클리어
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

    // Enemy에서 받아온 도착한 유닛 카운트
    public void Destination()
    {
        _numOfDestEnemy += 1;
    }
    // Enemy에서 받아온 죽은 유닛 카운트
    public void Dead()
    {
        _numOfDeath += 1;
    }

    public int NumOfEnemyInField()
    {
        return _numOfDestEnemy + _numOfDeath;
    }

    // 게임매니저에 보낼 도착한 유닛 카운트
    // Life를 계산할 때 사용
    public int NumOfDestEnemy()
    {
        return _numOfDestEnemy;
    }
}