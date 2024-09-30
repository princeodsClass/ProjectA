using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SlotRecipeItemGroup : MonoBehaviour
{
    [SerializeField]
    Transform _tSlotRoot;

    [SerializeField]
    TextMeshProUGUI _txtTitle;

    List<RecipeTable> _recipe;
    PopupWorkshop _pop;
    MenuManager m_MenuMgr = null;

    float _presentGap;

    private void Awake()
    {
        if ( null == m_MenuMgr ) m_MenuMgr = MenuManager.Singleton;

        _presentGap = GlobalTable.GetData<int>("timeWorkshopListPresentGap") / 1000f;
    }

    private void OnDisable()
    {
        StopAllCoroutines();
        ComUtil.DestroyChildren(_tSlotRoot);
    }

    public IEnumerator InitializeInfo(List<RecipeTable> recipe, PopupWorkshop pop)
    {
        ComUtil.DestroyChildren(_tSlotRoot);

        _recipe = recipe;
        _pop = pop;

        SetTitle();
        yield return StartCoroutine(SetOutputSlot());
    }

    IEnumerator SetOutputSlot()
    {
        for ( int i = 0; i < _recipe.Count; i++ )
        {
            SlotRecipeItem item = m_MenuMgr.LoadComponent<SlotRecipeItem>(_tSlotRoot, EUIComponent.SlotRecipeItem);
            item.InitializeInfo(_recipe[i], _pop);

            yield return new WaitForSecondsRealtime(_presentGap);
        }

        Resize();
    }

    void SetTitle()
    {
        _txtTitle.text = NameTable.GetValue(_recipe[0].TypeKey);
    }

    void Resize()
    {
        LayoutRebuilder.ForceRebuildLayoutImmediate(_tSlotRoot.GetComponent<RectTransform>());
        LayoutRebuilder.ForceRebuildLayoutImmediate(_tSlotRoot.parent.GetComponent<RectTransform>());
        LayoutRebuilder.ForceRebuildLayoutImmediate(_tSlotRoot.parent.parent.GetComponent<RectTransform>());
    }

    public void OnClick()
    {

    }

    private void OnEnable()
    {
        _recipe = new List<RecipeTable>();
        StopAllCoroutines();
    }
}
