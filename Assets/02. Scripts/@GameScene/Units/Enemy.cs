using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using UnityEngine.Rendering.Universal;
using System.Collections;
using UnityEngine.UIElements;

public class Enemy : Unit
{
    #region 역할
    // 적의 행동 기술
    // 스포너에서 받아온 경로를 따라 움직이게 함
    #endregion

    private Spawner.SpawnDesc _routeDesc;
    private GameObject go;

    private void Start()
    {
        _animator = GetComponent<Animator>();

        _state        = BehaviorState.MOVE;
    }

    void Update()
    {
        GaugeUpdate();

        switch (_state)
        {
            case BehaviorState.IDLE:
                Idle();
                break;

            case BehaviorState.MOVE:
                Move();
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

        if(_hp <= 0)
        {
            _state = BehaviorState.DIE;
            return;
        }

        Warrior w = go.GetComponent<Warrior>();

        if (w.IsDie() == false)
        {
            _timeAcc += Time.deltaTime;

            if (_timeAcc >= _routeDesc.attackSpeed)
            {
                _timeAcc = 0.0f;
                _state = BehaviorState.ATTACK;
            }
        }
        else
        {
            _timeAcc += Time.deltaTime;

            // 유닛을 죽인 후 1초 뒤에 이동을 시작함
            if(_timeAcc >= 2.0f)
            {
                _timeAcc = 0.0f;
                _state = BehaviorState.MOVE;
            }
        }
    }

    protected override void Move()
    {
        _animator.SetTrigger("Move");

        // 생성되고 마지막 웨이포인트까지
        if (_routeDesc.route.Count != 0)
        {
            Vector3 targetPoint = _routeDesc.route.Peek();

            Vector3 dir = targetPoint - transform.position;

            Flip(dir);

            // 웨이포인트에 도착했다면
            if (dir.magnitude <= 0.01f)
            {
                _routeDesc.route.Dequeue();
            }
            else
            {
                dir.Normalize();
                transform.position += dir * Time.deltaTime * _routeDesc.moveSpeed;
            }
        }
        // 마지막 웨이포인트에서 도착지점까지
        else
        {
            Vector3 dir = _routeDesc.destinationPoint - transform.position;

            Flip(dir);

            // 도착지점에 도착했다면
            if (dir.magnitude <= 0.01f)
            {
                gameObject.SetActive(false);

                // 도착하면 스포너한테 도착했다고 알려줌
                // 스포너는 그걸 모아서 게임매니저에 보냄
                _routeDesc.spawner.Destination();
            }

            dir.Normalize();
            transform.position += dir * Time.deltaTime * _routeDesc.moveSpeed;
        }
    }

    protected override void Attack()
    {
        // 최초 1회 유닛과 조우하면 Move 애니메이션이 살아있어서
        // 루프가 아닌 Attack 애니메이션이 끝나면 마저 재생되는거 같음
        _animator.ResetTrigger("Move");
        _animator.SetTrigger("Attack");

        float length = _animator.GetCurrentAnimatorStateInfo(0).length;

        _timeAcc += Time.deltaTime;

        if(_timeAcc >= length)
        {
            _timeAcc = 0.0f;
            _state   = BehaviorState.IDLE;
        }
    }

    // Attack 애니메이션에 바인딩 해놨음
    public void GiveDamage()
    {
        Warrior w = go.GetComponent<Warrior>();

        go.GetComponent<Warrior>().ReceiveDamage(_routeDesc.attackDamage);
    }

    public void ReceiveDamage(float damage)
    {
        _hp -= damage;

        if(_hp <= 0)
        {
            _state = BehaviorState.DIE;
        }
    }

    public bool IsDie()
    {
        return _hp <= 0 ? true : false;
    }

    protected override void Die()
    {
        _animator.SetTrigger("Die");

        

        float length = _animator.GetCurrentAnimatorStateInfo(0).length;

        _timeAcc += Time.deltaTime;

        if (_timeAcc >= length)
        {
            _timeAcc = 0.0f;
            
            // 스포너에게 나 죽었다고 알려줌
            _routeDesc.spawner.Dead();

            gameObject.SetActive(false);
        }
    }

    // Spawner에서 이 함수를 호출해서 인자를 넘겨받아옴
    public void Spawn(Spawner.SpawnDesc desc)
    {
        _routeDesc = desc;
        _routeDesc.route = new Queue<Vector3>(desc.route);

        _maxHP = desc.hp;
        _hp    = desc.hp;

        transform.position = desc.spawnPosition;
    }

    // 좌우 바꾸는거
    private void Flip(Vector3 dir)
    {
        Vector3 localScale = transform.localScale;

        if (dir.x > -0.1f)
        {
            transform.localScale = new Vector3(-1.0f, localScale.y, localScale.z);
        }
        else
        {
            transform.localScale = new Vector3(1.0f, localScale.y, localScale.z);
        }
    }

    // 유닛과 만났을 때
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Warrior"))
        {
            go = collision.gameObject;
            _state = BehaviorState.ATTACK;
        }
    }

    // 유닛이 죽었을 때
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Warrior"))
        {
            _state = BehaviorState.IDLE;
        }
    }
}
