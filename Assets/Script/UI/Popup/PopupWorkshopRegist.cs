using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PopupWorkshopRegist : UIDialog
{
    [SerializeField]
    Transform _tMaterialRoot;

    [SerializeField]
    Slider _slSlider;

    [SerializeField]
    GameObject _goButtonRegist;

    [SerializeField]
    TextMeshProUGUI _txtTitle,
                    _txtSliderTitle,
                    _txtButtonCaption;

    
    PopupWorkshopItem _popupWorkshopItem;
    RecipeTable _recipe;

    uint _pk;
    int _curCount, _maxCount;
    long _id;

    public void InitializeInfo(long id, uint pk, int count, RecipeTable recipe)
    {
        _popupWorkshopItem = GameObject.Find("PopupWorkshopItem").GetComponent<PopupWorkshopItem>();

        _id = id;
        _pk = pk;
        _recipe = recipe;

        int t = _popupWorkshopItem.GetCurCount() % _recipe.NeedCount;
        int tCount = t == 0 ? _recipe.NeedCount : (_recipe.NeedCount - t);
        _maxCount = count > tCount ? tCount : count;

        SetMaterial();
        InitializeText();
    }

    void SetMaterial()
    {
        SlotMaterial m = MenuManager.Singleton.LoadComponent<SlotMaterial>(_tMaterialRoot, EUIComponent.SlotMaterial);
        m.Initialize(_pk, _maxCount, false, false, false);
        m.SetRandom(false);
        m.SetState(SlotMaterial.SlotState.none);

        SetSlider();
    }

    public void ChangeValue()
    {
        _curCount = (int)(_maxCount * _slSlider.value);
        _slSlider.value = (float)_curCount / _maxCount;
        _txtSliderTitle.text = _curCount.ToString();
    }

    void SetSlider()
    {
        _slSlider.value = 1;
        _txtSliderTitle.text = _maxCount.ToString();

        ChangeValue();
    }

    void InitializeText()
    {
        _txtTitle.text = UIStringTable.GetValue("ui_popup_workshopregist_top_title");
        _txtButtonCaption.text = UIStringTable.GetValue("ui_popup_button_confirm");

    }

    public void OnClickRegist()
    {
        _popupWorkshopItem.Regist(_id, _pk, _curCount);
        Close();
    }

    public void OnClickRegistTutorial(bool isTutorial)
    {
        OnClickRegist();

        GameObject pop = GameObject.Find("PopupWorkshopItem");
        GameObject button = pop.transform.Find("Base").Find("Product").gameObject;
        RectTransform rt = button.GetComponent<RectTransform>();
        GameManager.Singleton.tutorial.SetFinger(button,
                                                 pop.GetComponent<PopupWorkshopItem>().OnClickProductTutorial,
                                                 rt.rect.width, rt.rect.height, 750);
    }

    private void Awake()
    {
        Initialize();
    }

    private void OnEnable()
    {
        ComUtil.DestroyChildren(_tMaterialRoot);
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
