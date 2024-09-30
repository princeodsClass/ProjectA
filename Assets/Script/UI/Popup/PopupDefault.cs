using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Castle.Core.Internal;

public class PopupDefault : UIDialog
{
    [SerializeField] protected TextMeshProUGUI _txtTitle = null;
    [SerializeField] protected TextMeshProUGUI _txtTopMessage = null;
    [SerializeField] protected TextMeshProUGUI _txtMessage = null;
    [SerializeField] protected TextMeshProUGUI _txtPositive = null;
    [SerializeField] protected TextMeshProUGUI _txtNegative = null;
    [SerializeField] protected Transform _tOutline = null;
    [SerializeField] protected RectTransform[] _resize;

    protected Action m_OnCallbackPositive = null;
    protected Action m_OnCallbackNegative = null;

    protected int m_nData = 0;

    string _title = "Infomation";

    public override void Initialize()
    {
        base.Initialize();
    }

    public override void Open()
    {
        base.Open();
    }

    public override void Close()
    {
        base.Close();
    }

    public override void Escape()
    {
        base.Escape();
    }

    private void Awake()
    {
        Initialize();
        _txtTitle.text = _title;
    }

    private void OnEnable()
    {
        _txtTopMessage.text = "";
        _txtMessage.text = "";
    }

    public void SetTitle(string title)
    {
        if (!string.IsNullOrEmpty(title))
        {
            if (UIStringTable.IsContainsKey(title))
                _txtTitle.text = UIStringTable.GetValue(title);
            else
                _txtTitle.text = title;
        }
    }

    public void SetMessage(string message)
    {
        if (!string.IsNullOrEmpty(message))
        {
            if (UIStringTable.IsContainsKey(message))
                _txtMessage.text = UIStringTable.GetValue(message);
            else
                _txtMessage.text = message;
        }
    }

    public void SetTopMessage(string message)
    {
        if (!string.IsNullOrEmpty(message))
        {
            if (UIStringTable.IsContainsKey(message))
                _txtTopMessage.text = UIStringTable.GetValue(message);
            else
                _txtTopMessage.text = message;
        }
    }

    /// <summary>
    /// type
    /// 0 : 연결 / 1 : 줄바꿈 / 2 : 줄 띄우기
    /// </summary>
    /// <param name="message"></param>
    /// <param name="type"></param>
    public void AddMessage(string message, int type = 0)
    {
        if (!string.IsNullOrEmpty(message))
        {
            if (UIStringTable.IsContainsKey(message))
            {
                string gap = string.Empty;

                switch ( type )
                {
                    case 1:
                        gap = "\n";
                        break;
                    case 2:
                        gap = "\n\n";
                        break;
                }

                _txtMessage.text = _txtMessage.text + gap + UIStringTable.GetValue(message);
            }                
        }
    }

    public void SetButtonText(string txtConfirm, string txtCancel, Action positive = null, Action negative = null, string goName = null)
    {
        if (!string.IsNullOrEmpty(txtConfirm))
        {
            if (UIStringTable.IsContainsKey(txtConfirm))
                _txtPositive.text = UIStringTable.GetValue(txtConfirm);
            else
                _txtPositive.text = txtConfirm;
        }
            
        if (!string.IsNullOrEmpty(txtCancel))
        {
            if (UIStringTable.IsContainsKey(txtCancel))
                _txtNegative.text = UIStringTable.GetValue(txtCancel);
            else
                _txtNegative.text = txtCancel;
        }

        m_OnCallbackPositive = positive;
        m_OnCallbackNegative = negative;

        SetImage(goName);
        StartCoroutine(Resize());
    }

    void SetImage(string goName)
    {
        ComUtil.DestroyChildren(_tOutline);

        if (goName.IsNullOrEmpty())
        {
            _tOutline.gameObject.SetActive(false);
        }
        else
        {
            m_ResourceMgr.CreateObject(EResourceType.UIETC, goName, _tOutline);
            _tOutline.gameObject.SetActive(true);

            /*
            RectTransform rt = _image.GetComponent<RectTransform>();

            _image.gameObject.SetActive(true);
            _image.sprite = image;

            float aspectRatio = (float)image.rect.width / image.rect.height;
            float tWidth = rt.rect.width;
            float tHeight = tWidth / aspectRatio;

            rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, tWidth);
            rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, tHeight);
            */
        }
    }

    IEnumerator Resize()
    {
        yield return null;

        Array.ForEach(_resize, rt => LayoutRebuilder.ForceRebuildLayoutImmediate(rt));
    }

    public void OnClickPositive()
    {
        Close();

        if (null != m_OnCallbackPositive) m_OnCallbackPositive.Invoke();
    }

    public void OnClickNegative()
    {

        Close();

        if (null != m_OnCallbackNegative) m_OnCallbackNegative.Invoke();
    }
}
