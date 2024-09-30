using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletController : MonoBehaviour
{
    public void DestroyBullet(float _value)
    {
        Destroy(gameObject, _value);
    }

    private void OnDestroy()
    {

    }

    void FixedUpdate()
    {
        transform.position += transform.forward * Time.fixedDeltaTime * Speed;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Structure"))
        {
            //var _eff = Instantiate("FX_Impact_Bullet_Other");
            Destroy(gameObject);
        }
    }

    [SerializeField]
    private float _damage;
    public float Damage
    {
        get => _damage;
        set
        {
            _damage = value;
        }
    }

    [SerializeField]
    private float _speed;
    public float Speed
    {
        get => _speed;
        set
        {
            _speed = value;
        }
    }
}
