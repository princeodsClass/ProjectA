using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControllers : MonoBehaviour
{
    public GameObject player;
    public float lerpFactor;
    public float Radius = 50f;
    Vector3 difference;
    Vector3 startPos;
    Vector3 _clampedPosition;
    Transform _transform;

    void Start()
    {
        _transform = transform;
        _clampedPosition = _transform.position;

        startPos = transform.position + player.transform.position;
        transform.position = startPos;
        difference = player.transform.position - transform.position;
    }

    void FixedUpdate()
    {
        Vector3 _posEnd = player.transform.position - difference;
        _posEnd.y = 0f;
        _transform.position = Vector3.Lerp(_transform.position, _posEnd, lerpFactor);
        _clampedPosition = Vector3.ClampMagnitude(_transform.position, Radius);
        transform.position = Vector3.Lerp(_transform.position, _clampedPosition, lerpFactor);
    }
}
