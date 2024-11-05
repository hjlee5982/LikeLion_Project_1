using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

public class Unit : MonoBehaviour
{
    // �ڽĵ��� �ִϸ�����
    protected Animator _animator;

    // HP ǥ�ÿ�
    public Image hpGauge;

    // ���� ����
    public enum BehaviorState
    {
        NONE,

        IDLE,
        MOVE,
        ATTACK,
        DIE,

        END
    }
    public BehaviorState _state;

    // Ž���� ���͸� ������� �����ϱ� ���� ť�� �������
    protected Queue<GameObject> _enemyQueue = new Queue<GameObject>();

    // �ݶ��̴� Ž��, ���º�ȭ�� ����
    protected float _timeAcc     = 0.0f;

    // ���ֿ� ����
    protected float _attackSpeed  = 0;
    protected float _attackDamage = 0;
    protected float _maxHP        = 0;
    protected float _hp           = 0;

    protected virtual void Idle()   { }
    protected virtual void Move()   { }
    protected virtual void Attack() { }
    protected virtual void Die()    { }

    // ��ŷ�� ����
    protected bool _isPlacing = false;

    public float GetHP()
    {
        return _hp;
    }

    public void ReceiveHeal(float hp)
    {
        if (_hp < _maxHP)
        {
            _hp += hp;
        }

        if (_hp >= _maxHP)
        {
            _hp = _maxHP;
        }
    }

    public bool IsDamaged()
    {
        return _maxHP == _hp ? false : true;
    }

    public void IsPlacing()
    {
        _isPlacing = true;
    }

    protected void Placing()
    {
        if (_isPlacing == true)
        {
            Vector3 mousePosition = Input.mousePosition;

            Vector3 mouseWorldPosition = Camera.main.ScreenToWorldPoint
                (
                    new Vector3(mousePosition.x, mousePosition.y, 0)
                );

            transform.position = new Vector3
                (
                    mouseWorldPosition.x, mouseWorldPosition.y, transform.position.z
                );
        }
        if (Input.GetMouseButtonDown(0) == true)
        {
            _isPlacing = false;
        }
    }

    protected void GaugeUpdate()
    {
        hpGauge.fillAmount = _hp / _maxHP;
        hpGauge.color = Color.HSVToRGB(hpGauge.fillAmount / 3, 1.0f, 1.0f);

        hpGauge.transform.localScale = new Vector3
            (
                transform.localScale.x, transform.localScale.y, transform.localScale.z
            );
    }
}
