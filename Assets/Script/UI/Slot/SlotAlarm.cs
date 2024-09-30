using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SlotAlarm : MonoBehaviour
{
    [SerializeField]
    Image _imgIcon;

    public void InitializeInfo(long id)
    {
        _imgIcon.sprite = GameManager.Singleton.invenWeapon.GetIconSprite(id);
    }
}
