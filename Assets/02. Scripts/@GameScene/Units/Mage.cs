using UnityEngine;
using System.Collections.Generic;
using System.Linq;

// ������� �Ʊ��� ȸ�����Ѿ� ��
// �׷��� ������� �Ʊ� ������Ʈ�� ��� �ְ� �� (_units)
// ȸ�� ����� �������� ���� ���� ���� HP�� ���� ������ ������� ��
// �׷��� ���� _units�� ���� �� �ʿ䰡 ����, HP �������� �����ϸ� �� �տ����� ���� �Ǵϱ�

// ó�� �����Ѱ� List����, �� ������ ���� ������ ���ָ� �ǰڴ�
// �׷��� �̰� �ߺ��ؼ� ��� �����̳ʿ� ���� �־���

// �ι�°�� SortedSet, ���� �� ���� �˾Ƽ� ���ĵǴ�
// �׷��� �񱳱����� HP����, �ʱ�HP���� �˴� �Ȱ��� �״� ���� ���Ŀ� �Ʊ��� ���� ��
// �ߺ����� �Ǵ��ϰ� �����̳ʿ� �ȳ־���

// ����°�� List, ���� �� _units.Contains�� �ȿ� ���� �ֳ� Ȯ������

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

    // Attack �ִϸ��̼ǿ� ���ε� �س���
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

    // �¿� �ٲٴ°�
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
        // �̹� �´�� �ִ� �ݶ��̴��� ���⸦ ������� ����
        // Start���� �������� ä���ִ���
        // OnTriggerStay2D�� ��ߵǴµ�
        // ���ڴ� �� �����Ӹ��� ȣ��Ǵϱ� ���� ��?����

        // �ٵ� ���ڴ� ó�� �ѹ��� �����ϴµ�?
        // ������ ���� �����ǰ� ������ ������ �����Ǹ�?

        // �� �� �����Ӹ��� �����°� ������

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
