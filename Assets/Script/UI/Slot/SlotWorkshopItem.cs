using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SlotWorkshopItem : MonoBehaviour
{
    [SerializeField]
    Image _imgFrame, _imgGlow, _imgIcon;

    [SerializeField]
    TextMeshProUGUI _txtVolume, _txtName, _txtCount;

    [SerializeField]
    Color[] _colorFrame, _colorGlow;

    [SerializeField]
    GameObject _goCounter, _goBottom, _goMaker;

    [SerializeField]
    SoundButton _sbBase, _sbAdd, _sbMinus;

    Animator animator = null;
    MaterialTable _material;
    PopupWorkshopSelect _popupWorkshopSelect;

    int _counter;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        _popupWorkshopSelect = GameObject.Find("PopupWorkshopSelect").GetComponent<PopupWorkshopSelect>();
    }

    /// <summary>
    /// pk : 해당 아이템 키
    /// count : 보유 수량
    /// b : 하단 버튼 사용 여부.
    /// 사용 시 base 버튼 비활성.
    /// </summary>
    /// <param name="pk"></param>
    /// <param name="count"></param>
    /// <param name="b"></param>
    public void Initialize(uint pk, int count, bool b)
    {
        _material = MaterialTable.GetData(pk);

        _sbBase.interactable = !b;
        _goBottom.SetActive(b);
        _goMaker.SetActive(false);

        _imgIcon.sprite = GameResourceManager.Singleton.LoadSprite(EAtlasType.Icons, _material.Icon);

        _imgFrame.color = _colorFrame[_material.Grade];
        _imgGlow.color = _colorGlow[_material.Grade];

        _txtName.text = NameTable.GetValue(_material.NameKey);
        _txtVolume.text = count.ToString();

        _counter = 0;

        Resize();
    }

    public void SetAppear()
    {
        _txtName.gameObject.SetActive(true);
        gameObject.SetActive(true);
        animator.SetTrigger("Pop");
    }

    public void SetRestart()
    {
        _txtName.gameObject.SetActive(false);
        gameObject.SetActive(true);
        animator.SetTrigger("Restart");
    }

    public void OnClickBase()
    {
        if (_popupWorkshopSelect.RemainSelectCount() > 0)
        {
            _popupWorkshopSelect.SetResult(_material.PrimaryKey, 1);
            _counter++;
            _goMaker.SetActive(true);
        }
        else
        {
            if ( _goMaker.activeSelf )
            {
                _popupWorkshopSelect.SetResult(_material.PrimaryKey, -1);
                _counter--;
                _goMaker.SetActive(false);
            }
        }

        SetCounter();
    }

    public void OnClickAdd()
    {
        if (_popupWorkshopSelect.RemainSelectCount() > 0)
        {
            _popupWorkshopSelect.SetResult(_material.PrimaryKey, 1);
            _counter++;

            SetCounter();
        }
    }

    public void OcClickMinus()
    {
        if (_counter > 0)
        {
            _popupWorkshopSelect.SetResult(_material.PrimaryKey, -1);
            _counter--;

            SetCounter();
        }
    }

    void SetCounter()
    {
        _txtCount.text = _counter.ToString();
        _goCounter.SetActive(_counter > 0);
    }

    public void Resize()
    {
        LayoutRebuilder.ForceRebuildLayoutImmediate(GetComponent<RectTransform>());
    }
}
