using UnityEngine;
using System.Collections.Generic;
using UnityEditor;

public class Archer : Unit
{
    public GameObject ArrowPrefab;
    public Arrow _arrow;

    public float AttackRange = 3.0f;

    void Start()
    {
        GameObject go = Instantiate(ArrowPrefab, this.transform);
        go.SetActive(false);
        _arrow = go.GetComponent<Arrow>();

        _animator    = GetComponent<Animator>();
        _state       = BehaviorState.IDLE;

        _attackSpeed  = 0.5f;
        _attackDamage = 1.0f;
        _maxHP        = 5.0f;
        _hp           = 5.0f;
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

        if (_enemyQueue.Count > 0) 
        {
            if(_enemyQueue.Peek().GetComponent<Enemy>().IsDie() == true)
            {
                _enemyQueue.Dequeue();
            }
            else
            {
                //Vector3 dir = _enemyQueue.Peek().transform.position - transform.position;

                //if (dir.magnitude > AttackRange)
                //{
                //    _enemyQueue.Dequeue();
                //}
            }
        }

        _timeAcc += Time.deltaTime;

        if (_enemyQueue.Count > 0)
        {
            if (_timeAcc >= _attackSpeed)
            {
                _timeAcc = 0.0f;
                _state = BehaviorState.ATTACK;
            }
        }
    }

    protected override void Attack()
    {
        _animator.SetTrigger("Attack");

        if (_enemyQueue.Count > 0)
        {
            Vector3 dir = _enemyQueue.Peek().transform.position - transform.position;

            Flip(dir);
        }

        float length = _animator.GetCurrentAnimatorStateInfo(0).length;

        _timeAcc += Time.deltaTime;

        if (_timeAcc >= length)
        {
            _timeAcc = 0.0f;
            _state = BehaviorState.IDLE;
        }
    }

    // Attack 애니메이션에 바인딩 해놨음
    public void GiveDamage()
    {
        if(_enemyQueue.Count > 0)
        {
            _arrow.gameObject.SetActive(true);

            GameObject go = _enemyQueue.Peek();

            _arrow.Toward(this.gameObject, go);

            if (go.GetComponent<Enemy>().IsDie() == false)
            {
                go.GetComponent<Enemy>().ReceiveDamage(_attackDamage);
            }
        }
        if (_enemyQueue.Count > 0 && _enemyQueue.Peek().GetComponent<Enemy>().IsDie() == true)
        {
            _enemyQueue.Dequeue();
        }
    }

    public void ReceiveDamage(float damage)
    {
        _hp -= damage;

        if (_hp <= 0)
        {
            _state = BehaviorState.DIE;
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
        if (collision.gameObject.CompareTag("Enemy"))
        {
            _enemyQueue.Enqueue(collision.gameObject);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            if (_enemyQueue.Count > 0)
            {
                _enemyQueue.Dequeue();
            }
        }
    }
}
