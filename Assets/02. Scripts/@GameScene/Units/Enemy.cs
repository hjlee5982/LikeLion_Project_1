using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using UnityEngine.Rendering.Universal;
using System.Collections;
using UnityEngine.UIElements;

public class Enemy : Unit
{
    #region ����
    // ���� �ൿ ���
    // �����ʿ��� �޾ƿ� ��θ� ���� �����̰� ��
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

            // ������ ���� �� 1�� �ڿ� �̵��� ������
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

        // �����ǰ� ������ ��������Ʈ����
        if (_routeDesc.route.Count != 0)
        {
            Vector3 targetPoint = _routeDesc.route.Peek();

            Vector3 dir = targetPoint - transform.position;

            Flip(dir);

            // ��������Ʈ�� �����ߴٸ�
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
        // ������ ��������Ʈ���� ������������
        else
        {
            Vector3 dir = _routeDesc.destinationPoint - transform.position;

            Flip(dir);

            // ���������� �����ߴٸ�
            if (dir.magnitude <= 0.01f)
            {
                gameObject.SetActive(false);

                // �����ϸ� ���������� �����ߴٰ� �˷���
                // �����ʴ� �װ� ��Ƽ� ���ӸŴ����� ����
                _routeDesc.spawner.Destination();
            }

            dir.Normalize();
            transform.position += dir * Time.deltaTime * _routeDesc.moveSpeed;
        }
    }

    protected override void Attack()
    {
        // ���� 1ȸ ���ְ� �����ϸ� Move �ִϸ��̼��� ����־
        // ������ �ƴ� Attack �ִϸ��̼��� ������ ���� ����Ǵ°� ����
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

    // Attack �ִϸ��̼ǿ� ���ε� �س���
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
            
            // �����ʿ��� �� �׾��ٰ� �˷���
            _routeDesc.spawner.Dead();

            gameObject.SetActive(false);
        }
    }

    // Spawner���� �� �Լ��� ȣ���ؼ� ���ڸ� �Ѱܹ޾ƿ�
    public void Spawn(Spawner.SpawnDesc desc)
    {
        _routeDesc = desc;
        _routeDesc.route = new Queue<Vector3>(desc.route);

        _maxHP = desc.hp;
        _hp    = desc.hp;

        transform.position = desc.spawnPosition;
    }

    // �¿� �ٲٴ°�
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

    // ���ְ� ������ ��
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Warrior"))
        {
            go = collision.gameObject;
            _state = BehaviorState.ATTACK;
        }
    }

    // ������ �׾��� ��
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Warrior"))
        {
            _state = BehaviorState.IDLE;
        }
    }
}
