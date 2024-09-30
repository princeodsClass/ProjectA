using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    [SerializeField]
    GameObject _objWarhead;

    [SerializeField]
    GameObject _objShell;

    [SerializeField]
    Transform _tFirePosition;

    [SerializeField]
    Transform _tShellPosition;

    float _fSpeed = 10f;

    public Transform GetFirePosition()
    {
        return _tFirePosition;
    }

    public Transform GetShellPosition()
    {
        return _tShellPosition;
    }

    public void Fire(GameObject enemy = null)
    {
        GameObject insWarHead = Instantiate(_objWarhead, _tFirePosition.position, _tFirePosition.rotation);
        Rigidbody rigWarhead = insWarHead.GetComponent<Rigidbody>();

        if (enemy == null)
        {
            rigWarhead.velocity = insWarHead.transform.forward * _fSpeed;
        }
        else
        {
            insWarHead.transform.LookAt(enemy.transform);
            rigWarhead.velocity = (enemy.transform.position + insWarHead.transform.up - insWarHead.transform.position) * _fSpeed;
        }

        ShellFire();
    }

    void ShellFire()
    {
        GameObject insShell = Instantiate(_objShell, _tShellPosition.position, _tShellPosition.rotation);
        Rigidbody rigShell = insShell.GetComponent<Rigidbody>();

        rigShell.velocity = insShell.transform.forward * Random.Range(-2f, 2f)
                            + insShell.transform.up * Random.Range(15f, 35f)
                            + insShell.transform.right * Random.Range(3f,12f);
    }
}
