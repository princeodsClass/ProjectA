using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PopupWorkshopSelect : UIDialog
{
    [SerializeField]
    GameObject _goGuide, _goGuideB;

    [SerializeField]
    Transform[] _tWeaponSlotRoot, _tGearSlotRoot;

    [SerializeField]
    GameObject _goButtonMerge, _goButtonDimmed;

    [SerializeField]
    Image _imgButtonCostIcon;

    [SerializeField]
    TextMeshProUGUI _txtTitle,
                    _txtDescTitle,
                    _txtGuide, _txtGuideB,
                    _txtDButtonCaption, _txtButtonCaption, _txtButtonCost;

    [SerializeField]
    GameObject[] _goWeaponTab, _goGearTab,
                 _goWeaponTabSelectBG, _goWeaponCounter,
                 _goGearTabSelectBG, _goGearCounter;

    [SerializeField]
    TextMeshProUGUI[] _txtWeaponCounter, _txtGearCounter;

    [SerializeField]
    RectTransform[] _rtForResize;

    PopupWorkshopItem _popupWorkshopItem;
    RecipeTable _recipe;

    Dictionary<uint, int> _result;

    int _selectableCount = 0;
    int[] _curSelectCount;

    int _curType;

    public void InitializeInfo(RecipeTable recipe, int count = 1)
    {
        _goGuide.SetActive(count == 1);
        _goGuideB.SetActive(count > 1);

        _recipe = recipe;
        _selectableCount = count;

        _txtTitle.text = $"{UIStringTable.GetValue("ui_popup_workshopselect_top_title")} : {count}";
        _txtButtonCaption.text = _txtDButtonCaption.text = UIStringTable.GetValue("ui_slot_recipe_item_button_caption_3");

        _txtButtonCost.text = (_recipe.CostItemCount * count).ToString();
        _imgButtonCostIcon.sprite = GameResourceManager.Singleton.LoadSprite(EAtlasType.Icons,
                                                                             MaterialTable.GetData(_recipe.CostItemKey).Icon);

        SetTab();
    }

    void SetTab()
    {
        switch ( _recipe.Type )
        {
            case 3:
                SetWeaponTab();
                InitialWeaponPanel();
                break;
            case 4:
                SetGearTab();
                InitialGearPanel();
                break;
        }
    }

    void SetWeaponTab()
    {
        SetWeaponCount();

        Array.ForEach(_goWeaponTab, tab => tab.SetActive(true));
        Array.ForEach(_goGearTab, tab => tab.SetActive(false));
        Array.ForEach(_tGearSlotRoot, pannel => pannel.gameObject.SetActive(false));

        for (int i = 0; i < _goWeaponTabSelectBG.Length; i++)
        {
            _goWeaponTabSelectBG[i].SetActive(false);
            _goWeaponCounter[i].SetActive(false);

            _tWeaponSlotRoot[i].gameObject.SetActive(false);
        }
    }

    void SetGearTab()
    {
        SetGearCount();

        Array.ForEach(_goGearTab, tab => tab.SetActive(true));
        Array.ForEach(_goWeaponTab, tab => tab.SetActive(false));
        Array.ForEach(_tWeaponSlotRoot, pannel => pannel.gameObject.SetActive(false));

        for (int i = 0; i < _goGearTabSelectBG.Length; i++)
        {
            _goGearTabSelectBG[i].SetActive(false);
            _goGearCounter[i].SetActive(false);

            _tGearSlotRoot[i].gameObject.SetActive(false);
        }
    }

    void SetWeaponCount()
    {
        _txtDescTitle.text = $"{UIStringTable.GetValue("ui_popup_workshopselect_desc_title")}" +
                             $" : {_selectableCount - _curSelectCount.Sum()}";

        for ( int i = 0; i < _goWeaponCounter.Length; i++ )
        {
            _goWeaponCounter[i].SetActive(_curSelectCount[i] > 0);
            _txtWeaponCounter[i].text = _curSelectCount[i].ToString();
        }

        _goButtonMerge.SetActive(_selectableCount == _curSelectCount.Sum());
        _goButtonDimmed.SetActive(_selectableCount != _curSelectCount.Sum());
    }

    void SetGearCount()
    {
        _txtDescTitle.text = $"{UIStringTable.GetValue("ui_popup_workshopselect_desc_title")}" +
                             $" : {_selectableCount - _curSelectCount.Sum()}";

        for (int i = 0; i < _goGearCounter.Length; i++)
        {
            _goGearCounter[i].SetActive(_curSelectCount[i] > 0);
            _txtGearCounter[i].text = _curSelectCount[i].ToString();
        }

        _goButtonMerge.SetActive(_selectableCount == _curSelectCount.Sum());
        _goButtonDimmed.SetActive(_selectableCount != _curSelectCount.Sum());
    }

    public void OnClickWeaponTab(int type)
    {
        if (_curType == type) return;

        _curType = type;

        for (int i = 0; i < _goWeaponTabSelectBG.Length; i++)
        {
            _goWeaponTabSelectBG[i].SetActive(type == i + 1);
            _tWeaponSlotRoot[i].gameObject.SetActive(type == i + 1);
        }

        _goGuide.SetActive(false);
        _goGuideB.SetActive(false);

        Resize();
    }

    public void OnClickGearTab(int type)
    {
        if (_curType == type) return;

        _curType = type;

        for (int i = 0; i < _goGearTabSelectBG.Length; i++)
        {
            _goGearTabSelectBG[i].SetActive(type == i + 1);
            _tGearSlotRoot[i].gameObject.SetActive(type == i + 1);
        }

        _goGuide.SetActive(false);
        _goGuideB.SetActive(false);

        Resize();
    }

    public void OnClickButtonMerge()
    {
        _popupWorkshopItem.Product(_result);

        Close();
    }

    void InitialWeaponPanel()
    {
        List<MaterialTable> m = MaterialTable.GetList();

        for ( int i = 0; i < _tWeaponSlotRoot.Length; i++ )
        {
            for (int j = 0; j < m.Count; j++)
            {
                if (m[j].Type == _recipe.TypeHigh &&
                     m[j].SubType == i + 1 &&
                     m[j].Grade == _recipe.TypeGrade)
                {
                    SlotWorkshopItem item = m_MenuMgr.LoadComponent<SlotWorkshopItem>(_tWeaponSlotRoot[i], EUIComponent.SlotWorkshopItem);
                    item.Initialize(m[j].PrimaryKey, m_InvenMaterial.GetItemCount(m[j].PrimaryKey), _selectableCount > 1);
                    item.Resize();
                }
            }
        }

        Resize();
    }

    void InitialGearPanel()
    {
        List<MaterialTable> m = MaterialTable.GetList();

        for (int i = 0; i < _tGearSlotRoot.Length; i++)
        {
            for (int j = 0; j < m.Count; j++)
            {
                if (m[j].Type == _recipe.TypeHigh &&
                     m[j].SubType == i + 1 &&
                     m[j].Grade == _recipe.TypeGrade)
                {
                    SlotWorkshopItem item = m_MenuMgr.LoadComponent<SlotWorkshopItem>(_tGearSlotRoot[i], EUIComponent.SlotWorkshopItem);
                    item.Initialize(m[j].PrimaryKey, m_InvenMaterial.GetItemCount(m[j].PrimaryKey), _selectableCount > 1);
                    item.Resize();
                }
            }
        }

        Resize();
    }

    void Resize()
    {
        _rtForResize.ToList().ForEach(rt => LayoutRebuilder.ForceRebuildLayoutImmediate(rt));
    }

    public void SetResult(uint pk, int count)
    {
        if (_result.ContainsKey(pk))
        {
            _result[pk] += count;
        }
        else
        {
            _result.Add(pk, count);
        }

        _curSelectCount[MaterialTable.GetData(pk).SubType - 1] += count;

        SetWeaponCount();
        SetGearCount();
    }

    public int RemainSelectCount()
    {
        return _selectableCount - _curSelectCount.Sum();
    }

    private void Awake()
    {
        _popupWorkshopItem = GameObject.Find("PopupWorkshopItem").GetComponent<PopupWorkshopItem>();

        _txtGuide.text = _txtGuideB.text = UIStringTable.GetValue("ui_popup_workshopselect_guide_title");

        Initialize();
    }

    private void OnEnable()
    {
        for (int i = 0; i < _tWeaponSlotRoot.Length; i++)
            ComUtil.DestroyChildren(_tWeaponSlotRoot[i], false);

        for (int i = 0; i < _tGearSlotRoot.Length; i++)
            ComUtil.DestroyChildren(_tGearSlotRoot[i], false);

        _goButtonMerge.SetActive(false);

        _curSelectCount = new int[7];
        _result = new Dictionary<uint, int>();

        _selectableCount = 0;
        _curSelectCount.ToList().ForEach(a => a = 0);

        _curType = -1;
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
