using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json.Linq;
using TMPro;

public class PopupNotice : UIDialog
{
    [SerializeField]
    Transform _tRootMessage;

    [SerializeField]
    TextMeshProUGUI _txtTitle;

    [SerializeField]
    GameObject _goNull;

    ComShopVIP _comp;

    public void SetMessage(JArray notice)
    {
        ComUtil.DestroyChildren(_tRootMessage);
        _goNull.SetActive(notice.Count < 1);

        for ( int i = 0; i < notice.Count; i++ )
        {
            JObject no = (JObject)notice[i];

            SlotMessage me = m_MenuMgr.LoadComponent<SlotMessage>(_tRootMessage, EUIComponent.SlotMessage);
            me.InitializeInfo((string)no.GetValue("title"),
                              (string)no.GetValue("message"),
                              (string)no.GetValue("url"));
        }
    }

    IEnumerator GetNotice()
    {
        PopupWait4Response wait = m_MenuMgr.OpenPopup<PopupWait4Response>(EUIPopup.PopupWait4Response, true);

        yield return m_DataMgr.GetNotice(this);

        wait.Close();
    }

    void InitializeText()
    {
        _txtTitle.text = UIStringTable.GetValue("ui_popup_notice_message");
    }

    private void OnEnable()
    {
        StartCoroutine(GetNotice());
    }

    private void Awake()
    {
        Initialize();
        InitializeText();
    }

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
}
