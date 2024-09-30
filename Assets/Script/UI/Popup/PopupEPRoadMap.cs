using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class PopupEPRoadMap : UIDialog
{
    [SerializeField]
    Transform _tListRoot;

    [SerializeField]
    ScrollRect _viewArea;

    [SerializeField]
    RectTransform[] _rtResize;

    [SerializeField]
    TextMeshProUGUI _txtButtonBack, _txtLevel, _txtExp;

    List<SlotEpisode> _liSlot;
    List<EpisodeTable> _liEpisode = new List<EpisodeTable>();

    Action _wRewards = null;
    Tween _twSlot, _twViewfort;

    string D, h, m, s = string.Empty;

    private void OnEnable()
    {
        _liEpisode = EpisodeTable.GetList();
        InitializeInfo();
    }

    public void InitializeInfo()
    {
        _viewArea.normalizedPosition = new Vector2(0, 1);

        ComUtil.DestroyChildren(_tListRoot);
        _liSlot.Clear();

        SlotEpisode slot;

        for (int i = _liEpisode.Count; i > 0 ; i--)
        {
            slot = m_MenuMgr.LoadComponent<SlotEpisode>(_tListRoot, EUIComponent.SlotEpisode);
            slot.InitializeInfo(this, i);
            _liSlot.Add(slot);
        }

        m_GameMgr._nPreEpisode = m_Account.m_nEpisode;
        m_GameMgr._nPreChapter = m_Account.m_nChapter;
        InitializeView();
    }

    void InitializeView(float tar = 0f)
    {
        RectTransform contentTransform = _viewArea.content.GetComponent<RectTransform>();
        float contentHeight = contentTransform.rect.height;

        RectTransform viewPortTransform = _viewArea.viewport.GetComponent<RectTransform>();
        float viewPortHeight = viewPortTransform.rect.height;

        _twViewfort = _viewArea.DOVerticalNormalizedPos(tar, _liEpisode.Count * 0.075f).SetEase(Ease.InOutSine).OnComplete(InitializeGauge);
    }

    void InitializeGauge()
    {
        StartCoroutine(StartGauge());
        StartCoroutine(StartSetFocus());
    }

    IEnumerator StartGauge()
    {
        _liSlot.Reverse();

        yield return YieldInstructionCache.WaitForSecondsRealtime(.5f);

        for ( int i = 0; i < m_Account.CorrectEpisode + 1; i++ )
        {
            _twSlot = _liSlot[i].InitializeVLine();
            yield return _twSlot.WaitForCompletion();
        }
    }

    IEnumerator StartSetFocus()
    {
        yield return YieldInstructionCache.WaitForSecondsRealtime(1.5f);
        _viewArea.DOVerticalNormalizedPos((float)m_Account.m_nEpisode / _liEpisode.Count,
                                  0.5f * m_Account.m_nEpisode ).SetEase(Ease.Linear);
    }

    public void SetAddAction(Action action)
    {
        _wRewards = action;
    }

    private void Awake()
    {
        Initialize();

        _liSlot = new List<SlotEpisode>();
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

    private void OnDisable()
    {
        if (null != _wRewards)
        {
            _wRewards.Invoke();
            _wRewards = null;
        }

        StopAllCoroutines();
        _twViewfort.Kill();
        _twSlot.Kill();
    }
}
