using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class SlotShopGameMoney : MonoBehaviour, IPointerClickHandler, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField]
    GameObject _goCounter, _goCounterBonus;

    [SerializeField]
    Image _imgIcon, _imgCostIcon;

    [SerializeField]
    TextMeshProUGUI _txtName, _txtCost, _txtRewardCount,
                    _txtRewardBonusCountOriginal, _txtRewardBonusCountBonus,
                    _txtBonusRatio;

    Animator _animator;
    MoneyDealTable _deal;
    bool _isBonus;

    public void InitializeInfo(MoneyDealTable deal, bool isBonus = false)
    {
        _deal = deal;
        _isBonus = isBonus;
        _imgIcon.sprite = GameResourceManager.Singleton.LoadSprite(EAtlasType.Icons, deal.Icon);
        _imgCostIcon.sprite = GameResourceManager.Singleton.LoadSprite(EAtlasType.Icons, MaterialTable.GetData(deal.CostItemKey).Icon);

        _txtName.text = NameTable.GetValue(deal.NameKey);
        _txtCost.text = deal.CostItemCount.ToString();

        _animator = GetComponent<Animator>();

        SetBonus();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        PopupItemBuy buy = MenuManager.Singleton.OpenPopup<PopupItemBuy>(EUIPopup.PopupItemBuy, true);
        buy.InitializeInfo(_deal, _isBonus);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        _animator.SetTrigger("Pressed");
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        _animator.SetTrigger("Unpressed");
    }

    void SetBonus()
    {
        if ( _isBonus )
        {
            _goCounter.SetActive(false);
            _goCounterBonus.SetActive(true);

            float ratio = GlobalTable.GetData<float>("ratioGameMoneyDealBonus");

            _txtRewardBonusCountOriginal.text = _deal.RewardItemCount.ToString();
            _txtRewardBonusCountBonus.text = (_deal.RewardItemCount * ratio).ToString();
            _txtBonusRatio.text = $"X {ratio}";
        }
        else
        {
            _goCounter.SetActive(true);
            _goCounterBonus.SetActive(false);

            _txtRewardCount.text = _deal.RewardItemCount.ToString();
        }
    }
}
