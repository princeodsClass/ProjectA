using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SlotAbyssGroup : MonoBehaviour
{
    [SerializeField]
    Transform _tFloorRoot, _tSkillRoot;

    [SerializeField]
    TextMeshProUGUI _txtTitle, _txtSkillTitle;

    [SerializeField]
    RectTransform[] _rt4Resize;

    PopupAbyss _popup;
    int _nGroup;

    public void InitializeInfo(int group, PopupAbyss pop)
    {
        ClearFloorRoot();
        ClearSkillRoot();

        _popup = pop;
        _nGroup = group;
        StartCoroutine(SetFloor());

        SetTitle();
    }

    IEnumerator SetFloor()
    {
        WaitForSecondsRealtime waitTime = new WaitForSecondsRealtime(0.1f);

        for (int i = 5; i >= 1; i--)
        {
            SlotAbyss floor = MenuManager.Singleton.LoadComponent<SlotAbyss>(_tFloorRoot, EUIComponent.SlotAbyss);
            floor.InitializeInfo(_nGroup * 5 + i, this, _popup);

            yield return waitTime;
        }

        SetSkill();
    }

    void SetSkill()
    {
        ComUtil.DestroyChildren(_tSkillRoot);

        AbyssTable pt = AbyssTable.GetData((uint)(0x37800000 + _nGroup * 5 + 1));
        List<EffectGroupTable> eGroup = EffectGroupTable.GetGroup(pt.BonusBuffGroup);

        eGroup.ForEach(abyss =>
        {
            EffectTable ef = EffectTable.GetData(abyss.EffectKey);
            SlotSkill skill = MenuManager.Singleton.LoadComponent<SlotSkill>(_tSkillRoot, EUIComponent.SlotSkill);
            skill.InitializeInfo(EffectTable.GetData(abyss.EffectKey));
        });

        Array.ForEach(_rt4Resize, rt => LayoutRebuilder.ForceRebuildLayoutImmediate(rt));
    }

    public void SetTitle()
    {
        _txtTitle.text = $"{UIStringTable.GetValue("ui_slot_abyss_group")} : {( _nGroup + 1 )}";
    }

    void InitializeText()
    {
        _txtSkillTitle.text = UIStringTable.GetValue("ui_slot_abyssgroup_skilltitle");
    }

    void ClearFloorRoot()
    {
        ComUtil.DestroyChildren(_tFloorRoot);
    }

    void ClearSkillRoot()
    {
        ComUtil.DestroyChildren(_tSkillRoot);
    }

    private void Awake()
    {
        InitializeText();
    }

    private void OnEnable()
    {

    }
}
