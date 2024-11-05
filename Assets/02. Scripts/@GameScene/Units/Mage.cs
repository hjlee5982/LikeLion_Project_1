using UnityEngine;
using System.Collections.Generic;
using System.Linq;

// 마법사는 아군을 회복시켜야 함
// 그래서 마법사는 아군 오브젝트를 들고 있게 함 (_units)
// 회복 대상은 마법사의 범위 내의 가장 HP가 적은 유닛을 대상으로 함
// 그러기 위해 _units를 정렬 할 필요가 있음, HP 오름차순 정렬하면 맨 앞에꺼를 빼면 되니까

// 처음 생각한건 List였음, 매 프레임 마다 정렬을 해주면 되겠다
// 그런데 이건 중복해서 계속 컨테이너에 값을 넣었음

// 두번째는 SortedSet, 넣을 때 마다 알아서 정렬되니
// 그런데 비교기준이 HP였고, 초기HP값은 죄다 똑같을 테니 최초 이후에 아군을 넣을 떄
// 중복으로 판단하고 컨테이너에 안넣었음

// 세번째는 List, 넣을 때 _units.Contains로 안에 값이 있나 확인했음

public class Mage : Unit
{
    private List<GameObject> _units = new List<GameObject>();
    private Unit _target;

    void Start()
    {
        _animator = GetComponent<Animator>();
        _state    = BehaviorState.IDLE;

        _attackSpeed  = 1.0f;
        _attackDamage = 2.0f;
        _maxHP = 5.0f;
        _hp    = 5.0f;
    }

    void Update()
    {
        Placing();
        GaugeUpdate();

        switch (_state)
        {
            case BehaviorState.IDLE:
                Idle();
                break;

            case BehaviorState.ATTACK:
                Attack();
                break;

            case BehaviorState.DIE:
                Die();
                break;

            default:
                break;
        }
    }

    protected override void Idle()
    {
        _animator.SetTrigger("Idle");

        _timeAcc += Time.deltaTime;

        _units.Sort
            (
                (x, y) => x.GetComponent<Unit>().GetHP().CompareTo(y.GetComponent<Unit>().GetHP())
            );

        foreach (GameObject go in _units)
        {
            if(go.GetComponent<Unit>().IsDamaged() == true && go.GetComponent<Unit>().gameObject.activeSelf == true)
            {
                _target = go.GetComponent<Unit>();

                if (_timeAcc >= _attackSpeed)
                {
                    _timeAcc = 0.0f;
                    _state = BehaviorState.ATTACK;
                }
                return;
            }
        }
    }

    protected override void Attack()
    {
        _animator.SetTrigger("Attack");

        float length = _animator.GetCurrentAnimatorStateInfo(0).length;

        if(_units.Count > 0)
        {
            Vector3 dir = _target.transform.position - transform.position;

            Flip(dir);
        }

        _timeAcc += Time.deltaTime;

        if (_timeAcc >= length)
        {
            _timeAcc = 0.0f;
            _state   = BehaviorState.IDLE;
        }
    }

    // Attack 애니메이션에 바인딩 해놨음
    public void GiveHeal()
    {
        if (_target.IsDamaged() == true)
        {
            _target.ReceiveHeal(_attackDamage);
        }

    }

    protected override void Die()
    {
        _animator.SetTrigger("Die");

        _timeAcc += Time.deltaTime;

        if (_timeAcc >= 1.0f)
        {
            _timeAcc = 0.0f;
            gameObject.SetActive(false);
        }
    }

    // 좌우 바꾸는거
    private void Flip(Vector3 dir)
    {
        Vector3 localScale = transform.localScale;

        if (dir.x > -0.1f)
        {
            transform.localScale = new Vector3(1.0f, localScale.y, localScale.z);
        }
        else
        {
            transform.localScale = new Vector3(-1.0f, localScale.y, localScale.z);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 이미 맞닿아 있는 콜라이더는 여기를 통과하지 않음
        // Start에서 수동으로 채워주던가
        // OnTriggerStay2D를 써야되는데
        // 후자는 매 프레임마다 호출되니까 별로 안?좋음

        // 근데 전자는 처음 한번만 설정하는데?
        // 힐러가 먼저 생성되고 나머지 유닛이 생성되면?

        // 걍 매 프레임마다 돌리는게 나을듯

        //if (collision.gameObject.CompareTag("Warrior") ||
        //    collision.gameObject.CompareTag("Archer")  ||
        //    collision.gameObject.CompareTag("Mage"))
        //{
        //    _units.Add(collision.gameObject);
        //}
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Warrior") ||
            collision.gameObject.CompareTag("Archer") ||
            collision.gameObject.CompareTag("Mage"))
        {
            if(_units.Contains(collision.gameObject) == false)
            {
                _units.Add(collision.gameObject);
            }
        }
    }
}
