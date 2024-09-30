using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SlotDefenceRewardsGroup : MonoBehaviour
{
    [SerializeField]
    Transform _tRootRewards;

    [SerializeField]
    TextMeshProUGUI _txtWave;

    [SerializeField]
    Color[] _colorFrame;

    [SerializeField]
    Image _imgBG, _imgIcon;

    public void InitializeInfo(MissionDefenceTable wave)
    {
        ClearGo();

        _txtWave.text = wave.Order.ToString();
        _imgBG.color = _colorFrame[wave.ColorIndex];
        _imgIcon.sprite = GameResourceManager.Singleton.LoadSprite(EAtlasType.Icons, wave.Icon);

        SetRewards(wave);
    }

    void SetRewards(MissionDefenceTable wave)
    {
        List<RewardTable> rGroup = RewardTable.GetGroup(wave.RewardGroup);
        List<RewardListTable> rewards;

        Sprite sp;

        for ( int i = 0; i < rGroup.Count; i++ )
        {
            rewards = RewardListTable.GetGroup(rGroup[i].RewardListGroup);
            sp = ComUtil.GetIcon(rewards[0].RewardKey);

            SlotAbyssRewards rSlot = MenuManager.Singleton.LoadComponent<SlotAbyssRewards>(_tRootRewards, EUIComponent.SlotAbyssRewards);
            rSlot.InitializeInfo(rewards[0].RewardKey, sp, rewards[0].RewardCountMin, rewards.Count > 1);
        }
    }

    void ClearGo()
    {
        ComUtil.DestroyChildren(_tRootRewards);
    }
}
