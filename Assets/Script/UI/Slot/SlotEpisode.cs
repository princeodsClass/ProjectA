using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class SlotEpisode : MonoBehaviour
{
    [SerializeField] GameObject _goRootMiniMap, _goRootRTexture;
    [SerializeField] Image _imgBG, _imgVerticalLine, _imgCoverLeft, _imgCoverRight;
    [SerializeField] Transform _tSlotRoot;
    [SerializeField] Camera _rCamera;
    [SerializeField] RawImage _rTarget;
    [SerializeField] TextMeshProUGUI _txtEPTitle, _txtEPName, _txtDesc, _txtButtonCaption, _txtProgressText;
    [SerializeField] Color _cOrigin, _cClear, _cDimmed;

    GameManager m_GameMgr;
    PopupEPRoadMap _pop;
    EpisodeTable _ep;
    RenderTexture _rTexture;

    int _nCurEp, _nMaxStage;
    float _fTargetFill;

    private void Awake()
    {
        if ( null == m_GameMgr ) m_GameMgr = GameManager.Singleton;
    }

    private void OnEnable()
    {
        _imgVerticalLine.fillAmount = 0;
        _rTarget.color = _cOrigin;
    }

    public void OnClickImage()
    {
        PopupLoadingHandlerBattleReady oHandler = new PopupLoadingHandlerBattleReady();
        oHandler.Init(PopupLoadingHandlerBattleReady.MakeParams(EMapInfoType.CAMPAIGN, new STIDInfo(0, 0, _ep.Order - 1 )));

        PopupLoading oLoadingPopup = MenuManager.Singleton.OpenPopup<PopupLoading>(EUIPopup.PopupLoading);
        oLoadingPopup.Init(PopupLoading.MakeParams(PopupLoading.ELoadingType.BATTLE_READY, oHandler, this.OnCompleteLoadingBattleReady));
    }

    private void OnCompleteLoadingBattleReady(PopupLoading a_oSender)
    {
        var oMapInfoDict = CMapInfoTable.Singleton.LoadMapInfosDownloaded(EMapInfoType.CAMPAIGN, 0, _ep.Order - 1);

        GameDataManager.Singleton.SetPlayStageID(0);
        GameDataManager.Singleton.SetContinueTimes(0);
        GameDataManager.Singleton.SetupPlayMapInfo(EMapInfoType.CAMPAIGN, EPlayMode.CAMPAIGN, oMapInfoDict, false);

        FindObjectOfType<PageLobbyBattle>().ExLateCallFunc((a_oFuncSender) =>
        {
            MenuManager.Singleton.SceneEnd();
            MenuManager.Singleton.SceneNext(ESceneType.Battle);
        }, ComType.G_DURATION_LOADING_PROGRESS_ANI);
    }

    public void InitializeInfo(PopupEPRoadMap pop, int ep)
    {
        _pop = pop;
        _ep = EpisodeTable.GetData(ComUtil.GetEpisodeKey(ep));

        _txtEPTitle.text = $"EP.{_ep.Order}";
        _txtEPName.text = NameTable.GetValue(_ep.NameKey);
        // _txtDesc.text = UIStringTable.GetValue("ui_popup_eproadmap_desc");
        _txtDesc.text = DescTable.GetValue(_ep.DescKey);
        _txtButtonCaption.text = UIStringTable.GetValue("ui_start");
        _rCamera.gameObject.SetActive(true);

        InitializeMinimap();
        // InitializeRewards();
        InitializeStage();
        InitializeCover();
    }

    void InitializeStage()
    {
        _nMaxStage = StageTable.GetMax(_nCurEp, 1);
        _txtProgressText.gameObject.SetActive(_nCurEp == _ep.Order);
        _txtProgressText.text = $"{m_GameMgr.user.m_nStage} / {_nMaxStage}";

    }

    void InitializeCover()
    {
        _imgCoverLeft.gameObject.SetActive(_nCurEp < _ep.Order);
        _imgCoverRight.gameObject.SetActive(_nCurEp < _ep.Order);

        // _imgCoverLeft.fillAmount = _nCurEp < _ep.Order ? 1f : 0f;
        // _imgCoverRight.fillAmount = _nCurEp < _ep.Order ? 1f : 0f;
    }

    void InitializeRewards()
    {
        ComUtil.DestroyChildren(_tSlotRoot);

        SlotSimpleItem slot;

        List<ChapterTable> group = ChapterTable.GetEpisodeRewards(_ep.Order);
        group.ForEach(g =>
        {
            if ( g.RewardItemKey > 0 && g.RewardItemCount > 0)
            {
                slot = MenuManager.Singleton.LoadComponent<SlotSimpleItem>(_tSlotRoot, EUIComponent.SlotSimpleItem);
                slot.Initialize(g.RewardItemKey,
                                ComUtil.GetIcon(g.RewardItemKey),
                                g.RewardItemCount, g.RewardItemCount, false);
                //slot.SetVolume(UIStringTable.GetValue("ui_popup_boxweapon_desc"));
                slot.SetVolume($"EP.{_ep.Order} - {(g.Episode == _ep.Order ? g.Order + 1 : 1)}");
            }
        });
    }

    void InitializeMinimap()
    {
        _rTexture = new RenderTexture(256, 256, 24, RenderTextureFormat.Default);

        ComUtil.DestroyChildren(_goRootMiniMap.transform, false);
        GameObject minimap = GameResourceManager.Singleton.CreateObject(EResourceType.ObjectLevel,
                                                                        _ep.PrefebMini,
                                                                        _goRootMiniMap.transform);

        _nCurEp = m_GameMgr.user.m_nEpisode
                  + ( m_GameMgr.user.m_nChapter == ChapterTable.GetMax(m_GameMgr.user.m_nEpisode + 1) ? 2 : 1 );

        _rTarget.texture = _rTexture;
        _rTarget.color = _nCurEp < _ep.Order ? _cDimmed : _cOrigin;
        _txtButtonCaption.transform.parent.gameObject.SetActive(_nCurEp >= _ep.Order);
        _rCamera.transform.LookAt(minimap.transform);
        _rCamera.targetTexture = _rTexture;
        
        StartCoroutine(CameraSet(false));
    }

    IEnumerator CameraSet(bool state)
    {
        yield return null;
        _rCamera.gameObject.SetActive(state);
    }

    public Tween InitializeVLine()
    {
        float doTime = 0;
        float doingTime = 0.5f;

        if ( _nCurEp > _ep.Order )
            _fTargetFill = 1.0f;
        else if ( _nCurEp == _ep.Order )
            _fTargetFill = (float)m_GameMgr.user.m_nStage / _nMaxStage;
        else
            _fTargetFill = 0f;

        if (_nCurEp > _ep.Order)
            _rTarget.DOColor(_cClear, doingTime);

        doTime = doingTime * _fTargetFill;

        //_imgCoverLeft.DOFillAmount(_fTargetFill, 0.5f).SetEase(Ease.Linear);
        //_imgCoverRight.DOFillAmount(_fTargetFill, 0.5f).SetEase(Ease.Linear);

        return _imgVerticalLine.DOFillAmount(_fTargetFill, doTime).SetEase(Ease.Linear);
    }

    private void OnDisable()
    {
        StopAllCoroutines();
        _rTexture.Release();
    }
}
