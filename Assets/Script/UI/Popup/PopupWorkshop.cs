using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PopupWorkshop : UIDialog
{
    [SerializeField]
    Transform _tGroupRoot;

    [SerializeField]
    GameObject[] _goTabButton,
                 _goTabButtonIndicator;

    [SerializeField]
    TextMeshProUGUI[] _txtTabButtonCaption;

    [SerializeField]
    GameObject _goTitle;

    [SerializeField]
    TextMeshProUGUI _txtTitle, _txtLevel, _txtLevelDesc, _txtCurCrystal, _txtCurMoney,
                    _txtCButtonCaption;

    [SerializeField]
    Slider _slExp;

    PageLobbyTop _pageLobbyTop;

    public void InitializeInfo(int page = 0)
    {
        ComUtil.DestroyChildren(_tGroupRoot);

        _txtCButtonCaption.text = UIStringTable.GetValue("ui_common_close");

        SetTop();
        SelectTabButton(page);
        Resize();
    }

    public void SetTop()
    {
        _txtTitle.text = UIStringTable.GetValue("ui_popup_workshop_top_title");
        _txtTabButtonCaption[0].text = UIStringTable.GetValue("ui_page_lobby_inventory_tab_gear");
        _txtTabButtonCaption[1].text = UIStringTable.GetValue("ui_page_lobby_inventory_tab_material");

        SetLevel();
        SetCurrency();
    }

    public void SetLevel()
    {
        int t = m_Account.GetWorkshopTargetExp(m_Account.m_nWSLevel);

        _txtLevel.text = m_Account.m_nWSLevel.ToString();
        _txtLevelDesc.text = m_Account.m_nWSLevel == GlobalTable.GetData<int>("levelMaxWorkshop") ?
                             UIStringTable.GetValue("ui_max") :  $"{m_Account.m_nWSExp} / {t}";
        _slExp.value = (float)m_Account.m_nWSExp / t;
    }

    public IEnumerator GainWorkshopExp(int exp)
    {
        yield return GameManager.Singleton.StartCoroutine(m_Account.GainWorkshopExp(exp));

        SetLevel();
    }

    public void SetCurrency()
    {
        _txtCurCrystal.text = ComUtil.ChangeNumberFormat(m_InvenMaterial.CalcTotalCrystal());
        _txtCurMoney.text = ComUtil.ChangeNumberFormat(m_InvenMaterial.CalcTotalMoney());

        _pageLobbyTop.InitializeCurrency();
    }

    public void SelectTabButton(int page = 0)
    {
        ComUtil.DestroyChildren(_tGroupRoot);

        for ( int i = 0; i < _goTabButtonIndicator.Length; i++ )
        {
            _goTabButtonIndicator[i].SetActive(i == page);
            _goTabButton[i].GetComponent<SoundButton>().interactable = i != page;
        }

        GameManager.Singleton.StartCoroutine(SetList(page));
        Resize();
    }

    IEnumerator SetList(int page = 0)
    {
        if ( null == RecipeTable.GetCategory(page) )
            yield break;

        for ( int i = 0; i < (int)ERecipeType.END; i++ )
        {
            List<RecipeTable> recipe = RecipeTable.GetType(page, i);

            if (recipe.Count > 0)
            {
                SlotRecipeItemGroup group = m_MenuMgr.LoadComponent<SlotRecipeItemGroup>(_tGroupRoot, EUIComponent.SlotRecipeItemGroup);
                yield return GameManager.Singleton.StartCoroutine(group.InitializeInfo(recipe, this));
            }
        }
    }

    public void OnClickAddGameMoney()
    {
        PopupShopGameMoney ga = MenuManager.Singleton.OpenPopup<PopupShopGameMoney>(EUIPopup.PopupShopGameMoney, true);
    }

    public void OnClickAddCrystal()
    {
        PopupShopCrystal ct = MenuManager.Singleton.OpenPopup<PopupShopCrystal>(EUIPopup.PopupShopCrystal, true);
    }

    void Resize()
    {
        LayoutRebuilder.ForceRebuildLayoutImmediate(_tGroupRoot.GetComponent<RectTransform>());
        LayoutRebuilder.ForceRebuildLayoutImmediate(_goTitle.transform.GetComponent<RectTransform>());
    }

    private void Awake()
    {
        _pageLobbyTop = GameObject.Find("LobbyTop").GetComponent<PageLobbyTop>();
        Initialize();
    }


    private void OnEnable()
    {
        ComUtil.DestroyChildren(_tGroupRoot);
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
        base.Escape();
    }
}
