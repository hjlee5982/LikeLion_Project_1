using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    #region ����
    // ���� ���ھ� ����

    // (������ȭ �ϱ�)
    // ������ ������, �������ͽ��� �����ؼ� �����ʿ� ������� ����

    // ���ֻ����� ��ġ

    // ���ӿ����� Ŭ���� ����
    #endregion

    // �ѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤ�
    #region ���ھ� ����
    private int  _life       = 10;
    private int  _stageCount = 0;

    int _scoreTemp = 0;

    #endregion
    // �ѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤ�
    #region ���� ���� ����
    private Queue<Spawner.SpawnDesc> _spawnDescs = new Queue<Spawner.SpawnDesc>();
    private Spawner.SpawnDesc _currentDesc;

    public Spawner EnemySpawner;
    public TextMeshProUGUI EnemyCounter;
    public TextMeshProUGUI LifeCounter;

    public Button StartButton;

    private bool _stageStart = false;
    #endregion
    // �ѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤ�
    #region ���� ���� ����
    public  GameObject WarriorPrefab;
    public  GameObject ArcherPrefab;
    public  GameObject MagePrefab;
    private Transform  _root;
    #endregion

    // �ѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤ�
    #region ���ӽý��� ����
    public Canvas GameOverCanvas;
    private Result _resultPopup;
    #endregion
    void Start()
    {
        // ���ȭ�� ����
        Canvas canvas = Instantiate(GameOverCanvas);
        _resultPopup = canvas.GetComponent<Result>();

        // _spawnDescs�� �������� ������ �о���̴� �κ�
        // ������ ������ �ִµ�, ���߿� �����ͷ� �о �ֱ⤡
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

        // ������ ���� ������
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

        // �������� Ŭ���� ���δ� ���⼭ �Ǵ��ؾ� ��
        // ���������� Life�� 0�� ���� ��
        if (_life - EnemySpawner.NumOfDestEnemy() <= 0)
        {
            // ���ӿ��� â�� �߰�
            // ReStart, Exit ��ư�� �߰�

            _resultPopup._isClear = false;
            _resultPopup._stageCount = _stageCount;
            _resultPopup.gameObject.SetActive(true);
            // ���⼭ ĵ������ OnEnable�� ������ �ɰ���
        }
        // ���������� Life�� ������ ��
        //else if(EnemySpawner.IsStageEnd() == true)
        else if (Spawner._stageEnd == true)
        {
            StartButton.gameObject.SetActive(true);

            if (_spawnDescs.Count == 0)
            {
                _resultPopup._isClear = true;
                _resultPopup._stageCount = _stageCount;
                _resultPopup.gameObject.SetActive(true);
                // ���⼭ ĵ������ OnEnable�� ������ �ɰ���
            }
        }
    }

    // ��ư�� ���ε� �س���
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

    // ��ư�� ���ε� �س���
    public void CreateWarrior()
    {
        GameObject go = Instantiate(WarriorPrefab, _root);
        go.GetComponent<Unit>().IsPlacing();

    }
    // ��ư�� ���ε� �س���
    public void CreateArcher()
    {
        GameObject go = Instantiate(ArcherPrefab, _root);
        go.GetComponent<Unit>().IsPlacing();
    }
    // ��ư�� ���ε� �س���
    public void CreateMage()
    {
        GameObject go = Instantiate(MagePrefab, _root);
        go.GetComponent<Unit>().IsPlacing();
    }
}
