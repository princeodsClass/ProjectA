using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PopupWorkshopItem : UIDialog
{
    [SerializeField]
    Transform _tResultRoot, _tRegistRoot, _tMaterialRoot;

    [SerializeField]
    GameObject _goButtonProduce, _goButtonDimmed;

    [SerializeField]
    TextMeshProUGUI _txtTitle, _txtName, _txtResultExp, _txtCount,
                    _txtPButtonCaption, _txtPButtonCost,
                    _txtCButtonCaption,
                    _txtCurRegist;

    [SerializeField]
    Image _imgPIcon;

    [Space(20)]
    [Header("--- 여기부터 진행 창 ------------")]
    [SerializeField]
    GameObject _goProcessing;

    [SerializeField]
    GameObject _goProcessingImage;

    [SerializeField]
    TextMeshProUGUI _txtProcessingTitle;

    [SerializeField]
    Slider _slProcessing;

    // --------------------------------------------------------------------------------------------------------

    PopupWorkshop _popup;

    RecipeTable _recipe;
    ScrollRect _scrollRect;

    Dictionary<long, SlotWorkshopSymbol> _dicMaterial = new Dictionary<long, SlotWorkshopSymbol>();
    Dictionary<long, SlotWorkshopSymbol> _dicRegist = new Dictionary<long, SlotWorkshopSymbol>();
    Dictionary<uint, int> _output;

    bool _isProcessing = false;

    int _countRegist, _countProduce;
    int _cost, _cur;

    string cPre, cSuf;

    // --------------------------------------------------------------------------------------------------------

    public void InitializeInfo(RecipeTable recipe)
    {
        _goButtonProduce.SetActive(false);
        _goButtonDimmed.SetActive(true);

        _cost = 0;
        _recipe = recipe;
        _txtCurRegist.text = $"0 / {_recipe.NeedCount}\n{UIStringTable.GetValue("ui_popup_workshopitem_regist_counter")}";
        _txtCButtonCaption.text = UIStringTable.GetValue("ui_common_close");

        _popup = GameObject.Find("PopupWorkshop").GetComponent<PopupWorkshop>();

        InitializeRegist();

        SetTop();
        SetList();
    }

    public int GetCurCount()
    {
        int c = 0;
        _dicRegist.ToList().ForEach(i => c += i.Value._volume);

        return c;
    }

    public void Regist(long id, uint pk, int count = 1)
    {
        SlotWorkshopSymbol.SlotState state = _recipe.isMassiveMaterial == 1 ?
                                             SlotWorkshopSymbol.SlotState.workshoppop :
                                             SlotWorkshopSymbol.SlotState.workshop;
        SlotWorkshopSymbol.SlotState rState = SlotWorkshopSymbol.SlotState.workshopregist;

        // _dicMaterial 에서 빼고
        AddDic(_dicMaterial, id, -count, state);

        if ( _dicRegist.ContainsKey(id) )
        {
            // _dicRegist 에 더하고
            AddDic(_dicRegist, id, count, rState);
        }
        else
        {
            // _dicRegist 에 만들고
            SlotWorkshopSymbol item = m_MenuMgr.LoadComponent<SlotWorkshopSymbol>(_tRegistRoot, EUIComponent.SlotWorkshopSymbol);
            item.InitializeInfo(_popup, _recipe, id, pk, count, this, true, false, rState);

            _dicRegist.Add(id, item);
        }

        SetCounter();
    }

    public void UnRegist(long id, uint pk, int count = 1)
    {
        SlotWorkshopSymbol.SlotState state;

        if (_recipe.isMassiveMaterial == 1)
        {
            state = SlotWorkshopSymbol.SlotState.workshoppop;
            count = _dicRegist[id]._volume;
        }
        else
        {
            state = SlotWorkshopSymbol.SlotState.workshop;
        }

        // _dicRegist 에서 빼고
        AddDic(_dicRegist, id, -count, SlotWorkshopSymbol.SlotState.workshopregist);

        if ( _dicMaterial.ContainsKey(id) )
        {
            // _dicMaterial 에 더하고
            AddDic(_dicMaterial, id, count, state);
        }
        else
        {
            // _dicMaterial 에 만들고
            SlotWorkshopSymbol item = m_MenuMgr.LoadComponent<SlotWorkshopSymbol>(_tMaterialRoot, EUIComponent.SlotWorkshopSymbol);
            item.InitializeInfo(_popup, _recipe, id, pk, count, this, true, false, state);

            _dicMaterial.Add(id, item);
        }

        SetCounter();
    }

    void SetCounter()
    {
        _countRegist = 0;
        _countProduce = 0;

        _dicRegist.ToList().ForEach(i => _countRegist += i.Value._volume);
        _countProduce = _countRegist / _recipe.NeedCount;

        _txtCurRegist.text = $"{_countRegist} / {_recipe.NeedCount * (_countProduce + 1)}" +
                             $" {UIStringTable.GetValue("ui_popup_workshopitem_regist_counter")}";

        _goButtonProduce.SetActive(_countRegist % _recipe.NeedCount == 0 && _countRegist > 0);
        _goButtonDimmed.SetActive(!_goButtonProduce.activeSelf);
        _txtPButtonCaption.text = UIStringTable.GetValue($"ui_slot_recipe_item_button_caption_{_recipe.Type}");
        _imgPIcon.sprite = m_ResourceMgr.LoadSprite(EAtlasType.Icons, MaterialTable.GetData(_recipe.CostItemKey).Icon);

        _cost = _recipe.CostItemCount * _countProduce;
        _cur = m_InvenMaterial.GetItemCount(_recipe.CostItemKey);

        cPre = _cost > _cur ? "<color=red>" : "";
        cSuf = _cost > _cur ? "</color>" : "";

        _txtPButtonCost.text = $"{cPre}{_cost}{cSuf}";
        _txtCount.text = $"x {Mathf.Max(_recipe.OutputCount, _recipe.OutputCount * _countProduce)}";

        SetScroll();
    }

    void SetScroll()
    {
        _scrollRect = GameObject.Find("Regist").GetComponent<ScrollRect>();

        _scrollRect.horizontalNormalizedPosition = 1f;
    }

    void AddDic(Dictionary<long, SlotWorkshopSymbol> tar, long id, int count, SlotWorkshopSymbol.SlotState state)
    {
        if (tar.ContainsKey(id))
        {
            int t = tar[id]._volume + count;

            if (t < 1)
            {
                Destroy(tar[id].gameObject);
                tar.Remove(id);
            }
            else
            {
                tar[id].InitializeInfo(_popup, _recipe, id, tar[id]._key, tar[id]._volume+ count, this, true, false, state);
            }
        }
    }

    void SetTop()
    {
        ComUtil.DestroyChildren(_tResultRoot);

        _txtTitle.text = NameTable.GetValue(_recipe.TypeKey);
        _txtName.text = NameTable.GetValue(_recipe.TitleKey);

        _txtResultExp.text = $"{UIStringTable.GetValue("ui_popup_workshopitem_reward_title")} +{_recipe.RewardWorkshopExp}";
        _txtCount.text = $"x {_recipe.OutputCount}";

        switch ( _recipe.SymbolItemKey.ToString("X").Substring(0, 2) )
        {
            case "22":
                SlotMaterial item = m_MenuMgr.LoadComponent<SlotMaterial>(_tResultRoot, EUIComponent.SlotMaterial);
                item.Initialize(_recipe.SymbolItemKey, _recipe.OutputCount, false, false, false, SlotMaterial.EVolumeType.value, SlotMaterial.SlotState.none);
                item.SetRandom(_recipe.IsRandom == 1);
                break;
            case "23":
                SlotWorkshopSymbol gear = m_MenuMgr.LoadComponent<SlotWorkshopSymbol>(_tResultRoot, EUIComponent.SlotWorkshopSymbol);
                ItemGear g = new ItemGear(0, _recipe.SymbolItemKey, 1);
                gear.InitializeInfo(_popup, _recipe, 0, _recipe.SymbolItemKey, _recipe.OutputCount, this, false, _recipe.IsRandom == 1, SlotWorkshopSymbol.SlotState.none);
                break;
        }        
    }

    void InitializeRegist()
    {
        ComUtil.DestroyChildren(_tRegistRoot);
        _dicRegist.Clear();
    }

    void InitializeMaterial()
    {
        ComUtil.DestroyChildren(_tMaterialRoot);
        _dicMaterial.Clear();
    }

    void SetList()
    {
        InitializeMaterial();

        switch (_recipe.TypeHigh)
        {
            case 1:
                break;
            case 2:
                SetGear();
                break;
            default:
                SetMaterial();
                break;
        }
    }

    void SetGear()
    {
        foreach ( ItemGear gear in m_InvenGear )
        {
            int index = Array.IndexOf(m_Account.m_nGearID, gear.id);

            if ( index < 0 && gear.nCurUpgrade < 1 && gear.nCurReinforce < 1 && gear.nCurLimitbreak < 1 )
            {
                if (_recipe.TypeLow == 0)
                {
                    if (_recipe.TypeGrade == gear.nGrade)
                        AddGear(gear);
                }
                else
                {
                    if (_recipe.TypeLow == (int)gear.eSubType && _recipe.TypeGrade == gear.nGrade)
                        AddGear(gear);
                }
            }
        }
    }

    void SetMaterial()
    {
        foreach ( ItemMaterial item in m_InvenMaterial )
        {
            if ( (int)item.eType == _recipe.TypeHigh )
                if ( _recipe.TypeLow == 0 )
                {
                    if ( _recipe.TypeGrade == item.nGrade )
                        AddMaterial(item);
                }
                else
                {
                    if ( _recipe.TypeLow == item.nSubType && _recipe.TypeGrade == item.nGrade )
                        AddMaterial(item);
                }
        }
    }

    void AddGear(ItemGear item)
    {
        if (item.nVolume < 1) return;

        SlotWorkshopSymbol material = m_MenuMgr.LoadComponent<SlotWorkshopSymbol>(_tMaterialRoot, EUIComponent.SlotWorkshopSymbol);
        material.InitializeInfo(_popup, _recipe, item.id, item.nKey, item.nVolume, this, true, false,
                                _recipe.isMassiveMaterial == 1 ?
                                    SlotWorkshopSymbol.SlotState.workshoppop :
                                    SlotWorkshopSymbol.SlotState.workshop);

        _dicMaterial.Add(item.id, material);
    }

    void AddMaterial(ItemMaterial item)
    {
        if ( item.nVolume < 1 ) return;

        SlotWorkshopSymbol material = m_MenuMgr.LoadComponent<SlotWorkshopSymbol>(_tMaterialRoot, EUIComponent.SlotWorkshopSymbol);
        material.InitializeInfo(_popup, _recipe, item.id, item.nKey, item.nVolume, this, true, false,
                                _recipe.isMassiveMaterial == 1 ?
                                    SlotWorkshopSymbol.SlotState.workshoppop :
                                    SlotWorkshopSymbol.SlotState.workshop);

        _dicMaterial.Add(item.id, material);
    }

    public void OnClickProduct()
    {
        if (m_GameMgr.invenMaterial.GetItemCount(_recipe.CostItemKey) >= _cost)
        {
            switch (_recipe.Type)
            {
                case 3:
                case 4:
                    PopupWorkshopSelect select = m_MenuMgr.OpenPopup<PopupWorkshopSelect>(EUIPopup.PopupWorkshopSelect, true);
                    select.InitializeInfo(_recipe, _countProduce);
                    break;
                default:
                        Product();
                        break;
            }
        }
        else
        {
            MenuManager.Singleton.OpenPopup<PopupShopGameMoney>(EUIPopup.PopupShopGameMoney, true);
        }
    }

    public void OnClickProductTutorial(bool isTutorial)
    {
        OnClickProduct();
        GameManager.Singleton.tutorial.Activate(false);
    }

    public void Product(Dictionary<uint, int> other = null)
    {
        if (m_InvenMaterial.GetItemCount(_recipe.CostItemCount) > _cost) return;

        _goButtonProduce.GetComponent<SoundButton>().interactable = false;
        _goButtonProduce.SetActive(false);
        _goButtonDimmed.SetActive(true);
        _goProcessing.SetActive(true);
        _txtProcessingTitle.text = UIStringTable.GetValue("ui_popup_workshopitem_processing_title");

        StartCoroutine(ConsumeMaterial());
        StartCoroutine(SetProcessingSlider());

        AddOutput(other);
    }

    IEnumerator ConsumeMaterial()
    {
        Dictionary<long, int> con = new Dictionary<long, int>();

        int volume = 0;

        foreach ( KeyValuePair<long, SlotWorkshopSymbol> r in _dicRegist )
        {
            // long id = m_InvenMaterial.GetItemID(r.Key);
            // int volume = m_InvenMaterial.GetItemCount(id) - r.Value._volume;

            switch (r.Value._key.ToString("X").Substring(0, 2))
            {
                case "20":
                    volume = m_InvenWeapon.GetItemCount(r.Key) - r.Value._volume;
                    m_InvenWeapon.ModifyItem(r.Key, InventoryData<ItemWeapon>.EItemModifyType.Volume, volume);
                    break;
                case "23":
                    volume = m_InvenGear.GetItemCount(r.Key) - r.Value._volume;
                    m_InvenGear.ModifyItem(r.Key, InventoryData<ItemGear>.EItemModifyType.Volume, volume);
                    break;
                case "22":
                    volume = m_InvenMaterial.GetItemCount(r.Key) - r.Value._volume;
                    m_InvenMaterial.ModifyItem(r.Key, InventoryData<ItemMaterial>.EItemModifyType.Volume, volume);
                    break;
            }

            con.Add(r.Key, volume);
        }

        con.Add(m_InvenMaterial.GetItemID(_recipe.CostItemKey), m_InvenMaterial.GetItemCount(_recipe.CostItemKey) - _cost);
        m_InvenMaterial.ModifyItem(m_InvenMaterial.GetItemID(_recipe.CostItemKey),
                                   InventoryData<ItemMaterial>.EItemModifyType.Volume,
                                   m_InvenMaterial.GetItemCount(_recipe.CostItemKey) - _cost);

        yield return StartCoroutine(m_DataMgr.ConsumnMaterial(con));

        SetList();
    }

    void AddOutput(Dictionary<uint, int> other = null)
    {
        _output = new Dictionary<uint, int>();

        if (null == other)
        {
            Dictionary<uint, int> t = new Dictionary<uint, int>();

            for (int i = 0; i < _countProduce; i++)
            {
                t = RewardTable.RandomResultInGroup(_recipe.OutputGroup);
                _output = ComUtil.MergeDictionaries(t, _output);
            }
        }
        else
        {
            _output = other;
        }

        // _output.ToList().ForEach(o => StartCoroutine(m_GameMgr.AddItemCS(o.Key, o.Value)));
        StartCoroutine(AddItem());
    }

    IEnumerator AddItem()
    {
        foreach (KeyValuePair<uint, int> item in _output)
            yield return m_GameMgr.AddItemCS(item.Key, item.Value);

        m_Account.RefreshQuestCount(EQuestActionType.CompleteCraft, _output.Count);
    }

    IEnumerator SetProcessingSlider()
    {
        _isProcessing = true;
        _slProcessing.value = 0f;

        WaitForSecondsRealtime wTime = new WaitForSecondsRealtime(0.01f);

        while ( _slProcessing.value < 1f )
        {
            _slProcessing.value += 0.01f;
            yield return wTime;
        }

        InitializeInfo(_recipe);
        _goProcessing.SetActive(false);
        _isProcessing = false;
        _goButtonProduce.GetComponent<SoundButton>().interactable = true;

        PopupWorkshopResult result = m_MenuMgr.OpenPopup<PopupWorkshopResult>(EUIPopup.PopupWorkshopResult, true);
        result.InitializeInfo(_output);

        StartCoroutine(_popup.GainWorkshopExp(_recipe.RewardWorkshopExp * _countProduce));
        _popup.SetCurrency();
        m_GameMgr.RefreshInventory(GameManager.EInvenType.All);
    }

    void Resize()
    {
        //LayoutRebuilder.ForceRebuildLayoutImmediate(_tGroupRoot.GetComponent<RectTransform>());
    }

    public void RefreshPopup()
    {
        if (_isProcessing) return;
        _popup.InitializeInfo(_recipe.Category);
        Close();
    }

    private void OnEnable()
    {
        ComUtil.DestroyChildren(_tResultRoot);
        ComUtil.DestroyChildren(_tRegistRoot);
        ComUtil.DestroyChildren(_tMaterialRoot);

        _goProcessing.SetActive(false);
    }

    void Awake()
    {
        Initialize();
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
        StopAllCoroutines();
        base.Close();
    }

    public override void Escape()
    {
        if ( _isProcessing ) return;
        RefreshPopup();
    }
}
