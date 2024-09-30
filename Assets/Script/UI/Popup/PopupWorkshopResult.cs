using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PopupWorkshopResult : UIDialog
{
    [SerializeField]
    TextMeshProUGUI _txtTitle, _txtDesc;

    [SerializeField]
    Transform _tSlotRoot;

    bool _isGrade;
    uint _key;
    int _grade;
    string _volume;
    Sprite _icon;

    List<GameObject> _goReward;

    public void InitializeInfo(Dictionary<uint, int> result)
    {
        InitializeText();

        _goReward = new List<GameObject>();

        foreach (KeyValuePair<uint, int> reward in result)
        {
            _key = reward.Key;
            _volume = reward.Value.ToString();

            string type = _key.ToString("X").Substring(0, 2);

            switch (type)
            {
                case "20":
                    _icon = m_ResourceMgr.LoadSprite(EAtlasType.Icons, WeaponTable.GetData(_key).Icon);
                    _volume = NameTable.GetValue(WeaponTable.GetData(_key).NameKey);
                    _isGrade = true;
                    _grade = WeaponTable.GetData(_key).Grade;

                    for (int i = 0; i < reward.Value; i++)
                        SetSlot();

                    break;
                case "22":
                    _icon = m_ResourceMgr.LoadSprite(EAtlasType.Icons, MaterialTable.GetData(_key).Icon);
                    _isGrade = false;
                    _grade = MaterialTable.GetData(_key).Grade;
                    SetSlot();
                    break;
                case "23":
                    _icon = m_ResourceMgr.LoadSprite(EAtlasType.Icons, GearTable.GetData(_key).Icon);
                    _volume = NameTable.GetValue(GearTable.GetData(_key).NameKey);
                    _isGrade = true;
                    _grade = GearTable.GetData(_key).Grade;

                    for (int i = 0; i < reward.Value; i++)
                        SetSlot();

                    break;
                case "11":
                    break;
            }
        }

        StartCoroutine(ShowAll());
    }
    
    void InitializeText()
    {
        _txtTitle.text = UIStringTable.GetValue("ui_popup_workshopresult_result_title");
        _txtDesc.text = UIStringTable.GetValue("ui_continue");
    }

    void SetSlot()
    {
        SlotSimpleItemReward _slot = m_MenuMgr.LoadComponent<SlotSimpleItemReward>(_tSlotRoot, EUIComponent.SlotSimpleItemReward);

        if (_isGrade)
        {
            string name = $"";
            _slot.Initialize(_icon, _volume, name, _grade, _isGrade);
        }
        else
        {
            string name = NameTable.GetValue(MaterialTable.GetData(_key).NameKey); ;
            _slot.Initialize(_icon, _volume, name, _grade);
        }

        _goReward.Add(_slot.gameObject);

        _slot.gameObject.SetActive(false);
    }

    IEnumerator ShowAll()
    {
        if (_goReward.Count > 0)
        {
            foreach (GameObject obj in _goReward)
            {
                obj.SetActive(true);
                obj.GetComponent<SlotSimpleItemReward>().SetRestart();

                yield return new WaitForSecondsRealtime(GlobalTable.GetData<float>("timeRewardShowGap") / 1000);
            }
        }
    }

    private void Awake()
    {
        Initialize();
    }

    private void OnEnable()
    {
        ComUtil.DestroyChildren(_tSlotRoot);
    }

    public override void Initialize()
    {
        base.Initialize();
    }

    public override void Open()
    {
        base.Open();

        m_MenuMgr.ShowPopupDimmed(true);
    }

    public override void Close()
    {
        base.Close();

        m_MenuMgr.ShowPopupDimmed(false);
    }
}