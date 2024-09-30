using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class SlotGear : MonoBehaviour
{ 
    [SerializeField]
    Image _imgFrame, _imgGlow, _Icon, _imgType;

    [SerializeField]
    Color[] _colorFrame, _colorGlow;

    [SerializeField]
    TextMeshProUGUI _textUpgrade, _textCP;

    [SerializeField]
    GameObject _objUpgradeArrow, _objReinforceArrow, _objLimitbreakArrow,
               _objLock, _objNewTag, _objDimed, _objSelected;

    [Header("그레이드 별")]
    [SerializeField]
    GameObject[] _objGradeIndicator;

    [Header("리미트 브레이크 표기용 별")]
    [SerializeField]
    GameObject[] _objLimitbreakIndicator;

    [Header("강과 단계 표기 배경")]
    [SerializeField]
    GameObject[] _objReinforceIndicatorBG;

    [Header("강화 단계")]
    [SerializeField]
    GameObject[] _objReinforceIndicator;

    public ItemGear _Item;

    Animator _animator;
    PageLobbyInventory _invenPage;

    uint _goodsKey;
    int u, _maxLevel, _cp, _slotNumber;

    public bool _ableToLimitBreak, _ableToReInforce, _ableToUpgrade;

    public enum SlotState
    {
        normal,
        unequiped,
        equiped,
        recycleUnselected, 
        recycleSelected,
        compareUnselected,
        compareSelected,
        compareNormal,
        reward,
        goods,
    }

    public enum EPresentType
    {
        current,
        origin,
        max,
    }

    SlotState _state = SlotState.normal;

    static long m_nMainID;

    public SlotGear() { }

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _objNewTag.SetActive(false);

        u = GlobalTable.GetData<int>("countStandardUpgrade");

        if (MenuManager.Singleton.CurScene == ESceneType.Lobby)
            _invenPage = GameObject.Find("InventoryPage").GetComponent<PageLobbyInventory>();
    }

    public void Initialize(ItemGear item, bool isButton = true, EPresentType type = EPresentType.current, uint goodsKey = 0)
    {
        _Item = item;
        _goodsKey = goodsKey;

        SetNewTag(_Item.bNew);

        RefreshInfo(type);
    }

    public void SetNewTag(bool state)
    {
        if (null == _Item) return;

        _Item.bNew = state;
        _objNewTag.SetActive(_Item.bNew);

        if ( !state && MenuManager.Singleton.CurScene == ESceneType.Lobby )
        {
            bool cur = false;

            foreach ( ItemGear gear in GameManager.Singleton.invenGear )
                if ( gear.bNew ) cur = true;

            if ( !cur ) _invenPage.SetNewTag(2, false);
        }
    }

    public void RefreshInfo(EPresentType type = EPresentType.current) // bool isOrigin = false)
    {
        _maxLevel = u + _Item.nCurReinforce * u;
        _imgFrame.color = _colorFrame[_Item.nGrade];
        _imgGlow.color = _colorGlow[_Item.nGrade];
        _Icon.sprite = _Item.GetIcon();
        _imgType.sprite = ComUtil.GetGearTypeIcon(_Item.eSubType);

        for (int i = 0; i < _objGradeIndicator.Length; i++)
        {
            _objGradeIndicator[i].SetActive(i < _Item.nGrade);

            switch (type)
            {
                case EPresentType.current:
                    _objLimitbreakIndicator[i].SetActive(i < _Item.nCurLimitbreak);
                    break;
                case EPresentType.max:
                    _objLimitbreakIndicator[i].SetActive(true);
                    break;
                case EPresentType.origin:
                    _objLimitbreakIndicator[i].SetActive(false);
                    break;
            }
        }

        switch (type)
        {
            case EPresentType.current:
                PresentCurrent();
                break;
            case EPresentType.origin:
                PresentOrigin();
                break;
            case EPresentType.max:
                PresentMax();
                break;
        }
    }

    void PresentCurrent()
    {
        CheckUpgradeState();

        for (int i = 0; i < _objReinforceIndicatorBG.Length; i++)
        {
            _objReinforceIndicatorBG[i].SetActive(i < _Item.nCurLimitbreak);
            _objReinforceIndicator[i].SetActive(i < _Item.nCurReinforce);
        }

        _textUpgrade.gameObject.SetActive(true);
        _textUpgrade.text = (_ableToReInforce || _ableToLimitBreak) ? UIStringTable.GetValue("ui_max") : $"{_Item.nCurUpgrade}/{_maxLevel}";
        _textCP.text = ComUtil.ChangeNumberFormat(_Item.nCP);
        _objLock.SetActive(_Item.bIsLock);
    }

    void PresentOrigin()
    {
        for (int i = 0; i < _objReinforceIndicatorBG.Length; i++)
        {
            _objReinforceIndicatorBG[i].SetActive(false);
            _objReinforceIndicator[i].SetActive(false);
        }

        _textUpgrade.gameObject.SetActive(false);
        _textUpgrade.text = string.Empty;
        // _textCP.text = ComUtil.ChangeNumberFormat(_Item.CalcCP(_Item.nDefencePowerStandard));
        _objLock.SetActive(false);
    }

    void PresentMax()
    {
        int dp = _Item.CalcDefencePower((_Item.nGrade + 1) * 10, _Item.nGrade + 1, _Item.nGrade + 1);

        for (int i = 0; i < _objReinforceIndicatorBG.Length; i++)
        {
            _objReinforceIndicatorBG[i].SetActive(i < _Item.nGrade);
            _objReinforceIndicator[i].SetActive(i < _Item.nGrade);
        }

        _textUpgrade.gameObject.SetActive(true);
        _textUpgrade.text = UIStringTable.GetValue("ui_max");
        // _textCP.text = ComUtil.ChangeNumberFormat(_Item.CalcCP(ap));
        _objLock.SetActive(false);
    }

    void CheckUpgradeState()
    {
        _ableToLimitBreak = _ableToReInforce = _ableToUpgrade = false;

        if (_Item.nCurUpgrade == _maxLevel &&
             _Item.nCurReinforce == _Item.nCurLimitbreak &&
             _Item.nCurLimitbreak < _Item.nGrade)
            _ableToLimitBreak = ComUtil.CheckMaterial(_Item.CalcLimitbreakMaterial());
        else if (_Item.nCurUpgrade == _maxLevel &&
                  _Item.nCurReinforce < _Item.nCurLimitbreak)
            _ableToReInforce = ComUtil.CheckMaterial(_Item.CalcReinforceMaterial());
        else if (_Item.nCurUpgrade < _maxLevel)
            _ableToUpgrade = ComUtil.CheckMaterial(_Item.CalcUpgradeMaterial());

        _objLimitbreakArrow.SetActive(_ableToLimitBreak);
        _objReinforceArrow.SetActive(_ableToReInforce);
        _objUpgradeArrow.SetActive(_ableToUpgrade && (GameManager.Singleton.user.m_nGearID.Contains(_Item.id) || _Item.nCurUpgrade > 0));
    }

    public bool AbleToRecycle()
    {
        return( !( _Item.nCurUpgrade > 0 || _Item.nCurReinforce > 0 || _Item.nCurLimitbreak > 0 ) );
    }

    public void SetState(SlotState state)
    {
        _state = state;
    }

    public void OnClick()
    {
        switch (_state)
        {
            case SlotState.normal:
            case SlotState.equiped:
            case SlotState.unequiped:
                DetailPopup();
                break;
            case SlotState.reward:
                DetailPopup(true, false, true);
                break;
            case SlotState.goods:
                GoodsPopup();
                break;
            case SlotState.compareNormal:
                DetailPopup(true, false);
                break;
            case SlotState.recycleUnselected:
                ExceptionRecycleUnselected();
                break;
            case SlotState.recycleSelected:
                GameObject.Find("PopupRecycle").GetComponent<PopupRecycle>().SelectGear(_Item, gameObject, PopupRecycle.ESelectType.Unselect);
                SetSelected(SlotState.recycleUnselected);
                break;
        }
    }

    void ExceptionRecycleUnselected()
    {
        if (_Item.bIsLock)
        {
            PopupSysMessage er = MenuManager.Singleton.OpenPopup<PopupSysMessage>(EUIPopup.PopupSysMessage, true);
            er.InitializeInfo("ui_error_title", "ui_error_recycle_locked", "ui_popup_button_confirm");
        }
        else if ( GameManager.Singleton.user.IsEquipGear(_Item.id) )
        {
            PopupSysMessage er = MenuManager.Singleton.OpenPopup<PopupSysMessage>(EUIPopup.PopupSysMessage, true);
            er.InitializeInfo("ui_error_title", "ui_error_recycle_equiped", "ui_popup_button_confirm");
        }
        else if (_Item.nCurUpgrade + _Item.nCurReinforce + _Item.nCurLimitbreak > 0)
        {
            PopupSysMessage er = MenuManager.Singleton.OpenPopup<PopupSysMessage>(EUIPopup.PopupSysMessage, true);
            er.InitializeInfo("ui_error_title", "ui_error_recycle_upgraded", "ui_popup_button_confirm");
        }
        else
        {
            GameObject.Find("PopupRecycle").GetComponent<PopupRecycle>().SelectGear(_Item, gameObject, PopupRecycle.ESelectType.Select);
            SetSelected(SlotState.recycleSelected);
        }
    }

    public void SetDim(bool state)
    {
        _objDimed.SetActive(state);
        //_interactable = !state;

    }    

    public void SetSelected(SlotState state)
    {
        _objSelected.SetActive(state == SlotState.compareSelected ||
                               state == SlotState.recycleSelected);
        _state = state;
    }

    public bool IsLock()
    {
        return _Item.bIsLock;
    }

    public void DetailPopup(bool overlap = true, bool presentUpgrade = true, bool isRandom = false)
    {
        PopupGear detail = MenuManager.Singleton.OpenPopup<PopupGear>(EUIPopup.PopupGear, overlap);
        detail.InitializeInfo(_Item, presentUpgrade, isRandom);
    }

    public void GoodsPopup()
    {
        PopupItemBuy buy = MenuManager.Singleton.OpenPopup<PopupItemBuy>(EUIPopup.PopupItemBuy, true);
        buy.InitializeInfo(_goodsKey, _slotNumber);
    }

    public void SetEquipReady(bool state)
    {
        _animator.SetBool("IsReadyChange", state);
    }
}
