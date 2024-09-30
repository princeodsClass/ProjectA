using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using Newtonsoft.Json.Linq;

public class PopupAbyss : UIDialog
{
    [SerializeField]
    GameObject[] _goTabButton, _goTabActiveBG;

    [SerializeField]
    RectTransform[] _rt4ResizeAbyss, _rt4ResizeRank, _rt4ResizeLeague;

    [SerializeField]
    GameObject[] _goPannel;

    [SerializeField]
    Transform[] _tRoot;

    [SerializeField]
    Transform _tRootGrade;

    [SerializeField]
    TextMeshProUGUI _txtAbyss, _txtRank, _txtLeague,
                    _txtTimerTitle, _txtTimerValue, _txtButtonClose,
                    _txtRankHeaderRank, _txtRankHeaderName, _txtRankHeaderFloor, _txtRankHeaderLap,
                    _txtRankCoverDesc;

    [SerializeField]
    ScrollRect[] _viewArea;

    [SerializeField]
    GameObject _goRankCover;

    SlotAbyss[] _abyss;

    int _nCurIndex = -1;
    int _nCurAbyssGroup = 0;
    int _nLastFloor = -1;

    List<AbyssTable> _cGroup;
    List<SlotAbyssRankTable> _liTable;
    List<SlotAbyssLeague> _liLeague;

    Tween ani = null;

    public void InitializeInfo(bool isView = false)
    {
        _cGroup = AbyssTable.GetGroup(_nCurAbyssGroup);
        _abyss = new SlotAbyss[_cGroup.Count];
        _goRankCover.SetActive(false);

        SetGrade();

        if ( isView )
        {
            _goTabButton[0].SetActive(false);
            _goTabButton[1].SetActive(true);
            _goTabButton[2].SetActive(true);

            OnClickTab(1, true);
        }
        else
        {
            _goTabButton[0].SetActive(true);
            _goTabButton[1].SetActive(true);
            _goTabButton[2].SetActive(true);

            OnClickTab(0, true);
        }
    }

    public void SetGrade()
    {
        ComUtil.DestroyChildren(_tRootGrade);

        if ( m_Account.m_nAbyssCurRank > 0 )
        {
            SlotAbyssLeagueGrade grade = m_MenuMgr.LoadComponent<SlotAbyssLeagueGrade>(_tRootGrade, EUIComponent.SlotAbyssLeagueGrade);
            grade.InitializeInfo(LeagueTable.GetDataWithRank(m_Account.m_nAbyssCurRank), true);
        }
    }

    void InitializeLeasgue()
    {
        List<LeagueTable> _liTable = LeagueTable.GetList();
        LeagueTable curLeague = LeagueTable.GetDataWithRank(m_Account.m_nAbyssCurRank);
        _liLeague = new List<SlotAbyssLeague>();

        for ( int i = _liTable.Count - 1; i >= 0; i-- )
        {
            _liLeague.Add(m_MenuMgr.LoadComponent<SlotAbyssLeague>(_tRoot[2], EUIComponent.SlotAbyssLeague));
            _liLeague[_liTable.Count - i - 1].InitializeInfo(_liTable[i], curLeague == _liTable[i]);
        }

        Invoke("SetFocus", 1f);
    }

    void InitializeRank()
    {
        StartCoroutine(GetRankList());
    }

    IEnumerator GetRankList()
    {
        PopupWait4Response wait = m_MenuMgr.OpenPopup<PopupWait4Response>(EUIPopup.PopupWait4Response, true);
        yield return m_DataMgr.GetAbyssRankInfo(this);
        wait.Close();
    }

    public void SetRankList(string result)
    {
        if ( result == "Do not recorded!" || result == string.Empty )
        {
            _goRankCover.SetActive(true);
        }
        else
        {
            _goRankCover.SetActive(false);
            StartCoroutine(SetRank(result));
        }
    }

    IEnumerator SetRank(string result)
    {
        JArray jList = JArray.Parse(result);
        _liTable = new List<SlotAbyssRankTable>();

        WaitForSecondsRealtime waitTime = new WaitForSecondsRealtime(0.06f);

        for (int i = 0; i < jList.Count; i++)
        {
            JObject t = (JObject)jList[i];

            _liTable.Add(m_MenuMgr.LoadComponent<SlotAbyssRankTable>(_tRoot[1], EUIComponent.SlotAbyssRankTable));
            _liTable[i].InitializeInfo(i, this, t);

            yield return waitTime;
        }

        Resize(_rt4ResizeRank);
        SetFocus();
    }

    void InitializeGroup()
    {
        int gcount = _cGroup.Count / 5;
        _nLastFloor = Array.FindLastIndex(m_Account.m_nAbyssBestLap, x => x > 0);
        Array.ForEach(m_Account.m_nAbyssBestLap, el =>
        {
            if ( el > 0 )
            {

            }
        });

        for ( int i = gcount - 1; i >= 0; i-- )
        {
            SlotAbyssGroup group = m_MenuMgr.LoadComponent<SlotAbyssGroup>(_tRoot[0], EUIComponent.SlotAbyssGroup);
            group.InitializeInfo(i, this);
        }

        Resize(_rt4ResizeAbyss);
        Invoke("SetFocus", 1f);
    }

    public void SetFloor(int floor, SlotAbyss slot)
    {
        _abyss[floor - 1] = slot;

        if ( _nLastFloor == -1 )
        {
            slot.SetLock(1 < floor);
        }
        else
        {
            TimeSpan time = TimeSpan.FromMilliseconds(m_Account.m_nAbyssBestLap[_nLastFloor]);
            slot.SetLock((6 - time.Minutes) + _nLastFloor < floor);
        }
    }

    public SlotAbyss GetFloor(int floor)
    {
        return _abyss[floor - 1];
    }

    public void SelectFloor(int floor)
    {
        for (int i = 0; i < _abyss.Length; i++)
            if ( (floor - 1) != i )
                _abyss[i].SetSelect(false);
    }

    public void OnClickTab(int index)
    {
        OnClickTab(index, false);
    }

    public void OnClickTab(int index, bool force)
    {
        if ( _nCurIndex == index && !force ) return;

        _nCurIndex = index;
        ResetAbyss(index);

        for ( int i = 0; i < _goPannel.Length; i++ )
        {
            _goPannel[i].SetActive(index == i);
            _goTabActiveBG[i].SetActive(index == i);
        }

        switch ( index )
        {
            case 0:
                InitializeGroup();
                break;
            case 1:
                InitializeRank();
                break;
            case 2:
                InitializeLeasgue();
                break;
        }        
    }

    void Resize(RectTransform[] target)
    {
        Array.ForEach(target, rt => LayoutRebuilder.ForceRebuildLayoutImmediate(rt));
    }

    void InitializeText()
    {
        _txtAbyss.text = UIStringTable.GetValue("ui_popup_abyss_tab_abyss");
        _txtRank.text = UIStringTable.GetValue("ui_popup_abyss_tab_rank");
        _txtLeague.text = UIStringTable.GetValue("ui_popup_abyss_tab_league");

        _txtButtonClose.text = UIStringTable.GetValue("ui_common_close");

        _txtTimerTitle.text = UIStringTable.GetValue("ui_popup_adventure_endin");

        _txtRankHeaderRank.text = UIStringTable.GetValue("ui_popup_abyss_rank_header_rank");
        _txtRankHeaderName.text = UIStringTable.GetValue("ui_popup_abyss_rank_header_name");
        _txtRankHeaderFloor.text = UIStringTable.GetValue("ui_popup_abyss_rank_header_floor");
        _txtRankHeaderLap.text = UIStringTable.GetValue("ui_popup_abyss_rank_header_laptime");

        _txtRankCoverDesc.text = UIStringTable.GetValue("ui_error_abyss_donotrecorded");
    }

    public void SetTimerValue(string time)
    {
        _txtTimerValue.text = time;
    }    

    void ResetAbyss(int index)
    {
        ComUtil.DestroyChildren(_tRoot[index]);
    }

    void SetFocus()
    {
        if (null != ani) ani.Kill();

        RectTransform rtTarget = null;
        
        switch ( _nCurIndex )
        {
            case 0:
                rtTarget = FocusConditionAbyss();
                break;
            case 1:
                rtTarget = FocusConditionRank();
                break;
            case 2:
                rtTarget = FocusConditionLeague();
                break;
        }

        if (null != _viewArea && null != rtTarget)
        {
            Vector3[] corners = new Vector3[4];
            rtTarget.GetWorldCorners(corners);

            bool isVisible = AreaContain(_viewArea[_nCurIndex].viewport, corners[0]) && AreaContain(_viewArea[_nCurIndex].viewport, corners[1]) &&
                             AreaContain(_viewArea[_nCurIndex].viewport, corners[2]) && AreaContain(_viewArea[_nCurIndex].viewport, corners[3]);

            if (!isVisible)
            {
                Vector3 targetPos = rtTarget.position;
                Vector3 contentPos = _viewArea[_nCurIndex].content.position;

                GridLayoutGroup layout = _viewArea[_nCurIndex].content.GetComponent<GridLayoutGroup>();

                contentPos.y = contentPos.y - targetPos.y + rtTarget.rect.height
                               + layout.padding.top + layout.padding.bottom;

                ani = DOTween.To(GetContentPosition, SetContentPosition, contentPos, 0.5f).OnComplete(OnFocusComplete);
            }
        }
    }

    void OnFocusComplete()
    {
        if ( _nCurIndex == 0 )
            _abyss[_nLastFloor].SetNewRecord();
    }

    RectTransform FocusConditionAbyss()
    {
        foreach (SlotAbyss rt in _abyss)
        {
            if ( rt.GetAbyssFloor() == _nLastFloor + 1 )
                return rt.GetComponent<RectTransform>();
        }

        return _abyss[0].GetComponent<RectTransform>();
    }

    RectTransform FocusConditionRank()
    {
        foreach (SlotAbyssRankTable rt in _liTable)
        {
            if ( rt.IsOwn() )
                return rt.GetComponent<RectTransform>();
        }

        return null;
    }

    RectTransform FocusConditionLeague()
    {
        foreach (SlotAbyssLeague rt in _liLeague)
        {
            if ( rt.IsCurrent() )
                return rt.GetComponent<RectTransform>();
        }

        return null;
    }

    bool AreaContain(RectTransform area, Vector3 tar)
    {
        return RectTransformUtility.RectangleContainsScreenPoint(area, tar);
    }

    Vector3 GetContentPosition()
    {
        return _viewArea[_nCurIndex].content.position;
    }

    void SetContentPosition(Vector3 pos)
    {
        _viewArea[_nCurIndex].content.position = new Vector3(pos.x, pos.y, pos.z);
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
