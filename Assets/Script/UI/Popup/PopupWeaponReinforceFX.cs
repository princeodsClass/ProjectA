using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PopupWeaponReinforceFX : MonoBehaviour
{
    public void Disapear()
    {
        gameObject.SetActive(false);
    }

    public void Arrive()
    {
        GameAudioManager.PlaySFX("SFX/UI/sfx_ui_item_reinforce_01", 0f, false, ComType.UI_MIX);
    }
}
