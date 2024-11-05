using UnityEngine;

public class Warrior : Unit
{
    void Start()
    {
        _animator     = GetComponent<Animator>();
        _state        = BehaviorState.IDLE;

        _attackSpeed  = 1.0f;
        _attackDamage = 1.0f;
        _maxHP        = 15.0f;
        _hp           = 15.0f;
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
            GameObject go = _enemyQueue.Peek();

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
        GetComponent<BoxCollider2D>().enabled = false;

        _animator.SetTrigger("Die");

        _timeAcc += Time.deltaTime;

        if (_timeAcc >= 1.0f)
        {
            _timeAcc = 0.0f;
            gameObject.SetActive(false);
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
