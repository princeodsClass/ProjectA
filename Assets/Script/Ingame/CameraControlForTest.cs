using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControlForTest : MonoBehaviour
{
    private float _fXRotate, _fXRotateMove, _fYRotate, _fYRotateMove;
    public float _fRotateSpeed = 5.0f;
    public float _fMoveSpeed = 1.0f;

    public void Drag()
    {
        _fXRotateMove = Input.GetAxis("Mouse X") * Time.deltaTime * _fRotateSpeed;
        _fYRotateMove = Input.GetAxis("Mouse Y") * Time.deltaTime * _fRotateSpeed;

        _fXRotate = transform.eulerAngles.x - _fYRotateMove;
        _fYRotate = transform.eulerAngles.y + _fXRotateMove;

        transform.eulerAngles = new Vector3(_fXRotate, _fYRotate, 0f);
    }

    private void Update()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical") / 5f;

        if (Input.GetKey(KeyCode.W) ||
            Input.GetKey(KeyCode.A) ||
            Input.GetKey(KeyCode.S) ||
            Input.GetKey(KeyCode.D))
        {
            transform.position = transform.position +
                                 transform.forward * vertical +
                                 transform.right * horizontal *
                                 _fMoveSpeed * Time.deltaTime;
        }
    }
}
