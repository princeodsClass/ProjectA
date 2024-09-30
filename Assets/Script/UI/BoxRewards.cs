using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoxRewards : MonoBehaviour
{
    PopupBoxReward _popupBoxReward;
    PopupMultyReward _popupMultyReward;

    private void OnEnable()
    {
        _popupBoxReward = GameObject.Find("PopupBoxReward")?.GetComponent<PopupBoxReward>();
        _popupMultyReward = GameObject.Find("PopupMultyReward")?.GetComponent<PopupMultyReward>();
    }

    public void Pop()
    {
        _popupBoxReward?.PopCard();
        _popupMultyReward?.PopCard();
    }

    public virtual void BoxLanding()
    {
        GameAudioManager.PlaySFX("SFX/UI/sfx_box_landing_00", 0f, false, ComType.UI_MIX);
    }
}
