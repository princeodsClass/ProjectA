using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SlotBattlePassSet : MonoBehaviour
{
    [SerializeField]
    GameObject _goBG, _goHorizonLine, _goButtonSkip;

    [SerializeField]
    Transform[] _tSlotRoot;

    [SerializeField]
    GameObject[] _goLine;

    [SerializeField]
    TextMeshProUGUI _tStepCaption, _tConsumeItem4Skip;

    [SerializeField]
    Image _imgConsumeItem4Skip;

    PopupBattlePass _pop;
    BattlePassTable _pass;
    SlotBattlePass[] _slot;

    public SlotBattlePassSet() { }

    public void InitializeInfo(PopupBattlePass pop, BattlePassTable pass, bool[] isObtain = null)
    {
        if (null == isObtain) isObtain = new bool[] { false, false, false };

        _pop = pop;
        _pass = pass;
        _slot = new SlotBattlePass[_tSlotRoot.Length];

        SetSlot();

        _goLine[0].SetActive(GameManager.Singleton.user.m_nPassLevel >= pass.Lv);
        _goLine[1].SetActive(!_goLine[0].activeSelf);
        _goBG.SetActive(!_goLine[0].activeSelf);

        if ( GameManager.Singleton.user.m_nPassLevel + 1 == pass.Lv )
        {
            _goHorizonLine.SetActive(true);
            _imgConsumeItem4Skip.sprite = ComUtil.GetIcon(_pass.SkipItemKey);
            _tConsumeItem4Skip.text = _pass.SkipItemCount.ToString();
        }
        else
        {
            _goHorizonLine.SetActive(false);
        }

        _tStepCaption.text = pass.Lv.ToString();
    }

    void SetSlot()
    {
        bool isLock = false;
        uint key;
        int count;
        Sprite icon;

        for (int i = 0; i < _tSlotRoot.Length; i++)
        {
            ComUtil.DestroyChildren(_tSlotRoot[i]);

            if (i == 0)
            {
                isLock = !GameManager.Singleton.user.m_bIsPlus;
                key = _pass.PlusRewardItemKey;
                count = _pass.PlusRewardItemCount;
                icon = ComUtil.GetIcon(_pass.PlusRewardItemKey);
            }
            else if (i == 1)
            {
                isLock = !GameManager.Singleton.user.m_bIsElite;
                key = _pass.EliteRewardItemKey;
                count = _pass.EliteRewardItemCount;
                icon = ComUtil.GetIcon(_pass.EliteRewardItemKey);
            }
            else
            {
                isLock = false;
                key = _pass.NormalRewardItemKey;
                count = _pass.NormalRewardItemCount;
                icon = ComUtil.GetIcon(_pass.NormalRewardItemKey);
            }

            _slot[i] = MenuManager.Singleton.LoadComponent<SlotBattlePass>(_tSlotRoot[i], EUIComponent.SlotBattlePass);
            _slot[i].InitializeInfo(_pop, _pass, key, icon, count, i, GameManager.Singleton.user.m_bpPass[_pass.Lv-1][i], isLock);
        }
    }

    public void OnClickSkip()
    {
        StartCoroutine(Skip());
    }

    IEnumerator Skip()
    {
        if ( !GameManager.Singleton.invenMaterial.CheckCost(_pass.SkipItemKey, _pass.SkipItemCount) )
        {
            if (_pass.SkipItemKey == ComType.KEY_ITEM_CRYSTAL_FREE || _pass.SkipItemKey == ComType.KEY_ITEM_CRYSTAL_PAY)
                MenuManager.Singleton.OpenPopup<PopupShopCrystal>(EUIPopup.PopupShopCrystal, true);
            else if (_pass.SkipItemKey == ComType.KEY_ITEM_GOLD)
                MenuManager.Singleton.OpenPopup<PopupShopGameMoney>(EUIPopup.PopupShopGameMoney, true);

            _goButtonSkip.GetComponent<SoundButton>().interactable = true;
            yield break;
        }

        PopupWait4Response wait = MenuManager.Singleton.OpenPopup<PopupWait4Response>(EUIPopup.PopupWait4Response, true);

        yield return StartCoroutine(ConsumeCost());
        yield return StartCoroutine(GameDataManager.Singleton.SkipBattlePass(wait, _pop));

        // _pop.SetFocus();
        // _pop.InitializeInfo();
    }

    IEnumerator ConsumeCost()
    {
        if (_pass.SkipItemKey == ComType.KEY_ITEM_CRYSTAL_FREE || _pass.SkipItemKey == ComType.KEY_ITEM_CRYSTAL_PAY)
        {
            int t = GameManager.Singleton.invenMaterial.GetItemCount(ComType.KEY_ITEM_CRYSTAL_FREE) - _pass.SkipItemCount;

            if (t < 0)
            {
                yield return StartCoroutine(GameManager.Singleton.AddItemCS(ComType.KEY_ITEM_CRYSTAL_PAY, t));

                if (_pass.SkipItemCount != -t)
                    yield return StartCoroutine(GameManager.Singleton.AddItemCS(ComType.KEY_ITEM_CRYSTAL_FREE, -(_pass.SkipItemCount + t)));
            }
            else
            {
                yield return StartCoroutine(GameManager.Singleton.AddItemCS(ComType.KEY_ITEM_CRYSTAL_FREE, -_pass.SkipItemCount));
            }
        }
        else
            yield return StartCoroutine(GameManager.Singleton.AddItemCS(_pass.SkipItemKey, -_pass.SkipItemCount));
    }
}
