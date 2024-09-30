using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SlotRecipeItem : MonoBehaviour
{
    [SerializeField]
    Transform _tSlotRoot;

    [SerializeField]
    TextMeshProUGUI _txtTitle, _txtName, _txtOutputCount, _txtButtonCaption, _txtLimitLevel;

    [SerializeField]
    GameObject _goCover;

    PopupWorkshop _popupWorkshop;
    RecipeTable _recipe;
    MenuManager m_MenuMgr = null;

    Coroutine _coResize;

    private void Awake()
    {
        if ( null == m_MenuMgr ) m_MenuMgr = MenuManager.Singleton;
    }

    private void OnDisable()
    {
        StopCoroutine(_coResize);
    }

    public void InitializeInfo(RecipeTable recipeKey, PopupWorkshop popup)
    {
        _popupWorkshop = popup;
        _recipe = recipeKey;

        SetTitle();
        SetSymbol();
        SetButton();

        SetCover(_recipe.AvailableWorkshopLevel > GameManager.Singleton.user.m_nWSLevel);
    }

    void SetSymbol()
    {
        ComUtil.DestroyChildren(_tSlotRoot);
        SlotWorkshopSymbol sym = m_MenuMgr.LoadComponent<SlotWorkshopSymbol>(_tSlotRoot, EUIComponent.SlotWorkshopSymbol);
        sym.InitializeInfo(_popupWorkshop, _recipe, 0, _recipe.SymbolItemKey, _recipe.OutputCount, null, false, _recipe.IsRandom == 1);
        _txtOutputCount.text = $"x {_recipe.OutputCount}";

        _coResize = StartCoroutine(Resize());
    }

    IEnumerator Resize()
    {
        yield return null;
        LayoutRebuilder.ForceRebuildLayoutImmediate(_tSlotRoot.GetComponent<RectTransform>());
    }

    void SetTitle()
    {
        _txtTitle.text = NameTable.GetValue(_recipe.TypeKey);
        _txtName.text = NameTable.GetValue(_recipe.TitleKey);
    }

    void SetButton()
    {
        int index = _recipe.Type > 11 ? 1 : _recipe.Type;
        _txtButtonCaption.text = UIStringTable.GetValue($"ui_slot_recipe_item_button_caption_{index}");
    }

    public void SetCover(bool state)
    {
        _goCover.SetActive(state);
        _txtLimitLevel.text = UIStringTable.GetValue("ui_popup_workshopselect_limitlevel_title") +
                              $"<color=red>{_recipe.AvailableWorkshopLevel}</color>";
    }

    public void OnClick(bool isTutorial = false)
    {
        PopupWorkshopItem item = m_MenuMgr.OpenPopup<PopupWorkshopItem>(EUIPopup.PopupWorkshopItem, true);
        item.InitializeInfo(_recipe);

        if ( isTutorial )
        {
            GameObject pop = GameObject.Find("PopupWorkshopItem");
            GameObject slot = GameObject.Find("MaterialRoot").transform.Find("SlotWorkshopSymbol").gameObject;
            RectTransform rt = slot.GetComponent<RectTransform>();
            GameManager.Singleton.tutorial.SetFinger(slot,
                                                     slot.GetComponent<SlotWorkshopSymbol>().OnClickTutorial,
                                                     160, 180, 750);
        }
    }


    public virtual void PopupAppear()
    {
        GameAudioManager.PlaySFX("SFX/UI/sfx_popup_appear_00", 0f, false, ComType.UI_MIX);
    }
}
