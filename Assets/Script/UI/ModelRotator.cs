using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModelRotator : MonoBehaviour
{
    public PopupWeapon popupWeapon;
    public PopupBoxWeapon popupBoxWeapon;
    public ComShopPremiumWeaponBox comShopPremiumWeaponBox;
    public Transform _tModel;

    private float _fXRotate, _fXRotateMove,
                  _fYRotate, _fYRotateMove;
    public float _fRotateSpeed = 500.0f;
    
    float _fDuration = 2f;
    bool _isTSet = false;

    Vector3 _vTarget;

    public void Drag()
    {
        if (!_isTSet)
        {
            if ( null != popupWeapon ) popupWeapon.StopRotateWeapon();
            if ( null != popupBoxWeapon) popupBoxWeapon.StopRotateWeapon();
            if ( null != comShopPremiumWeaponBox ) comShopPremiumWeaponBox.StopRotateWeapon();

            _vTarget = _tModel.localRotation.eulerAngles;
            _isTSet = true;
        }

        _fYRotateMove = Input.GetAxis("Mouse X") * Time.deltaTime * _fRotateSpeed;
        _fYRotate = _tModel.eulerAngles.y - _fYRotateMove;

        _fXRotateMove = Input.GetAxis("Mouse Y") * Time.deltaTime * _fRotateSpeed;
        _fXRotate = _tModel.eulerAngles.z - _fXRotateMove;

        _tModel.eulerAngles = new Vector3(_tModel.eulerAngles.x, _fYRotate, _fXRotate);
    }

    public void DragEnd()
    {
        StopCoroutine("CoDragEnd");
        StartCoroutine(CoDragEnd());
    }

    IEnumerator CoDragEnd()
    {
        float runTime = 0f;

        yield return new WaitForSeconds(0.5f);

        while ( runTime < _fDuration)
        {
            runTime += Time.deltaTime;

            _tModel.eulerAngles = Vector3.Lerp(_tModel.localRotation.eulerAngles,
                                               _vTarget,
                                               runTime / _fDuration);

            yield return null;
        }

        _isTSet = false;

        if ( null != popupWeapon ) popupWeapon.StartRotateWeapon();
        if ( null != popupBoxWeapon) popupBoxWeapon.StartRotateWeapon();
        if ( null != comShopPremiumWeaponBox ) comShopPremiumWeaponBox.StartRotateWeapon();
    }
}
