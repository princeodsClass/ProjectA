using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ComHuntReward : MonoBehaviour
{
    [SerializeField]
    Animator _animator;

    [SerializeField]
    TextMeshProUGUI _txtTitle, _txtDesc;

    [SerializeField]
    GameObject _goFullDesc;

    private void Awake()
    {
        _txtTitle.text = UIStringTable.GetValue("ui_component_hunt_reward_title");
        _txtDesc.text = UIStringTable.GetValue("ui_component_hunt_reward_alert");

        Resize();
    }

    public void SetBoost(bool state)
    {
        string s = state == true ? "On" : "Off";
        _animator.SetTrigger(s);
    }

    public void SetAlert(bool state)
    {
        _goFullDesc.SetActive(state);
    }

    public void Resize()
    {
        LayoutRebuilder.ForceRebuildLayoutImmediate(_goFullDesc.GetComponent<RectTransform>());
    }

    public void ShowBox(int grade)
    {
        uint key = 0;

        switch ( grade )
        {
            case 1: key = ComType.KEY_BOX_G1; break;
            case 2: key = ComType.KEY_BOX_G2; break;
            case 3: key = ComType.KEY_BOX_G3; break;
            case 4: key = ComType.KEY_BOX_G4; break;
        }

        PopupBoxNormal bo = MenuManager.Singleton.OpenPopup<PopupBoxNormal>(EUIPopup.PopupBoxNormal, true);
        bo.InitializeInfo(new ItemBox(0, key, 0), false, false);
    }
}
