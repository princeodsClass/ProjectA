using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterRotator : MonoBehaviour
{
    public Transform _tCharacter;

    private float _fYRotate, _fYRotateMove;
    public float _fRotateSpeed = 500.0f;
    public float _fDuration = 5f;

    bool _ableToOnClick = true;

    public void Drag()
    {
        _fYRotateMove = Input.GetAxis("Mouse X") * Time.deltaTime * _fRotateSpeed;
        _fYRotate = _tCharacter.eulerAngles.y - _fYRotateMove;
        _tCharacter.eulerAngles = new Vector3(0f, _fYRotate, 0f);
        _ableToOnClick = false;
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

            _tCharacter.eulerAngles = Vector3.Lerp(_tCharacter.eulerAngles,
                                                   new Vector3(0f, 180f, 0f),
                                                   runTime / _fDuration);
            yield return null;
        }

        _ableToOnClick = true;
    }

    public void OnClick()
    {
        if ( _ableToOnClick )
        {
            PopupEPRoadMap pop = MenuManager.Singleton.OpenPopup<PopupEPRoadMap>(EUIPopup.PopupEPRoadMap);
        }
    }
}
