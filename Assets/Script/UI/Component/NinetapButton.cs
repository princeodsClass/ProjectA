using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;

public class NinetapButton : UIBehaviour, IPointerClickHandler
{
    public UnityEvent onClick = new UnityEvent();
    public EButtonSoundType eButtonSoundType;

    public void OnPointerClick(PointerEventData eventData)
    {
        switch (eButtonSoundType)
        {
            case EButtonSoundType.normal_sound:
                GameAudioManager.PlaySFX("SFX/UI/sfx_ui_click_normal", 0f, false, ComType.UI_MIX);
                break;
            case EButtonSoundType.critical_sound:
                GameAudioManager.PlaySFX("SFX/UI/sfx_ui_click_critical", 0f, false, ComType.UI_MIX);
                break;
        }

        onClick?.Invoke();
    }

    protected override void OnDestroy()
    {
        onClick.RemoveAllListeners();
    }

}