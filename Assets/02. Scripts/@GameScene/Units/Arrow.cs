using UnityEngine;

public class Arrow : MonoBehaviour
{
    private Vector3 _from;
    private Vector3 _to;

    void Start()
    {

    }

    void Update()
    {
        Vector3 dir = (_to - _from).normalized;
        Vector3 up  = transform.up.normalized;

        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

        transform.rotation = Quaternion.Euler(0, 0, angle - 90);

        transform.position += dir * Time.deltaTime * 30;
    }

    public void Toward(GameObject from, GameObject to)
    {
        _from = from.transform.position;
        _to   = to.transform.position;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            transform.position = _from;
            gameObject.SetActive(false);
        }
    }
}
