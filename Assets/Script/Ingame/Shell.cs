using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shell : MonoBehaviour
{
    float _fLifeTime = 2f;

    private void Awake()
    {
        Invoke("Extinction", _fLifeTime);
    }

    void Extinction()
    {
        Destroy(gameObject);
    }

}
