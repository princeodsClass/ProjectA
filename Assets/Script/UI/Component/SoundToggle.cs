using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;

public class SoundToggle : Toggle
{
    public UnityEvent onClick = new UnityEvent();
    public EButtonSoundType eButtonSoundType;

    public override void OnPointerClick(PointerEventData eventData)
    {
        switch (eButtonSoundType)
        {
            case EButtonSoundType.none_sound:
                GameAudioManager.PlaySFX("SFX/UI/sfx_ui_click_normal", 0f, false, ComType.UI_MIX);
                break;
            case EButtonSoundType.critical_sound:
                GameAudioManager.PlaySFX("SFX/UI/sfx_ui_click_critical", 0f, false, ComType.UI_MIX);
                break;
        }

        if (!interactable) return;
        else isOn = !isOn;

        onClick?.Invoke();
    }

    protected override void OnDestroy()
    {
        onClick.RemoveAllListeners();
    }
}
