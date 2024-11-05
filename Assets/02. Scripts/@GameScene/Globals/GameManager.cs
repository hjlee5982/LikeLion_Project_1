using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    #region 역할
    // 게임 스코어 관리

    // (데이터화 하기)
    // 몬스터의 스폰량, 스테이터스를 정의해서 스포너에 생성명령 전달

    // 유닛생성과 배치

    // 게임오버와 클리어 관리
    #endregion

    // ㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡ
    #region 스코어 관련
    private int  _life       = 10;
    private int  _stageCount = 0;

    int _scoreTemp = 0;

    #endregion
    // ㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡ
    #region 몬스터 생성 관련
    private Queue<Spawner.SpawnDesc> _spawnDescs = new Queue<Spawner.SpawnDesc>();
    private Spawner.SpawnDesc _currentDesc;

    public Spawner EnemySpawner;
    public TextMeshProUGUI EnemyCounter;
    public TextMeshProUGUI LifeCounter;

    public Button StartButton;

    private bool _stageStart = false;
    #endregion
    // ㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡ
    #region 유닛 생성 관련
    public  GameObject WarriorPrefab;
    public  GameObject ArcherPrefab;
    public  GameObject MagePrefab;
    private Transform  _root;
    #endregion

    // ㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡ
    #region 게임시스템 관련
    public Canvas GameOverCanvas;
    private Result _resultPopup;
    #endregion
    void Start()
    {
        // 결과화면 생성
        Canvas canvas = Instantiate(GameOverCanvas);
        _resultPopup = canvas.GetComponent<Result>();

        // _spawnDescs에 스테이지 정보를 읽어들이는 부분
        // 지금은 깡으로 넣는데, 나중엔 데이터로 읽어서 넣기ㄱ
        {
            Spawner.SpawnDesc desc = new Spawner.SpawnDesc();
            {
                desc.initNumber    = 10;
                desc.spawnInterval = 1.0f;
                desc.spawnPoint    = 0;

                desc.attackDamage  = 2.0f;
                desc.attackSpeed   = 1.0f;
                desc.moveSpeed     = 2.0f;
                desc.hp            = 2.0f;
            }
            _spawnDescs.Enqueue(desc);
        }
        {
            Spawner.SpawnDesc desc = new Spawner.SpawnDesc();
            {
                desc.initNumber    = 20;
                desc.spawnInterval = 1.0f;
                desc.spawnPoint    = 1;

                desc.attackDamage  = 4.0f;
                desc.attackSpeed   = 2.0f;
                desc.moveSpeed     = 2.0f;
                desc.hp            = 4.0f;
            }
            _spawnDescs.Enqueue(desc);
        }

        // 생성된 유닛 정리용
        GameObject go = new GameObject() { name = "@UnitPool" };
        _root = go.transform;
    }

    void Update()
    {
        if(_stageStart == true)
        {
            if(EnemySpawner.NumOfEnemyInField() != 0)
            {
                _scoreTemp = EnemySpawner.NumOfEnemyInField();
            }
            EnemyCounter.SetText(_scoreTemp.ToString() + "/" + _currentDesc.initNumber.ToString());
        }
        else
        {
            EnemyCounter.SetText("0/0");
        }

        LifeCounter.SetText((_life - EnemySpawner.NumOfDestEnemy()).ToString());

        // 스테이지 클리어 여부는 여기서 판단해야 함
        // 실패조건은 Life가 0이 됐을 때
        if (_life - EnemySpawner.NumOfDestEnemy() <= 0)
        {
            // 게임오버 창이 뜨고
            // ReStart, Exit 버튼이 뜨게

            _resultPopup._isClear = false;
            _resultPopup._stageCount = _stageCount;
            _resultPopup.gameObject.SetActive(true);
            // 여기서 캔버스의 OnEnable이 실행이 될거임
        }
        // 성공조건은 Life가 남았을 때
        //else if(EnemySpawner.IsStageEnd() == true)
        else if (Spawner._stageEnd == true)
        {
            StartButton.gameObject.SetActive(true);

            if (_spawnDescs.Count == 0)
            {
                _resultPopup._isClear = true;
                _resultPopup._stageCount = _stageCount;
                _resultPopup.gameObject.SetActive(true);
                // 여기서 캔버스의 OnEnable이 실행이 될거임
            }
        }
    }

    // 버튼에 바인딩 해놨음
    public void Spawn()
    {
        if (_spawnDescs.Count > 0)
        {
            StartButton.gameObject.SetActive(false);

            _currentDesc = _spawnDescs.Dequeue();

            EnemySpawner.Spawn(_currentDesc);

            _stageStart = true;
            _scoreTemp = 0;
            ++_stageCount;
        }
    }

    // 버튼에 바인딩 해놨음
    public void CreateWarrior()
    {
        GameObject go = Instantiate(WarriorPrefab, _root);
        go.GetComponent<Unit>().IsPlacing();

    }
    // 버튼에 바인딩 해놨음
    public void CreateArcher()
    {
        GameObject go = Instantiate(ArcherPrefab, _root);
        go.GetComponent<Unit>().IsPlacing();
    }
    // 버튼에 바인딩 해놨음
    public void CreateMage()
    {
        GameObject go = Instantiate(MagePrefab, _root);
        go.GetComponent<Unit>().IsPlacing();
    }
}
