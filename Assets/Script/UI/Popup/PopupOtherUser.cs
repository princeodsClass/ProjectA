using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Newtonsoft.Json.Linq;
using TMPro;

public class PopupOtherUser : UIDialog
{
    [SerializeField]
    TextMeshProUGUI _txtTitle;

    [SerializeField]
    Transform[] _tEquipWeapon, _tEquipGear;

    [SerializeField]
    Transform _tEquipCharacter;

    GameObject _goCharacter = null;
    OtherUser otherUser;

    SlotWeapon[] _sWeapon;
    SlotGear[] _sGear;

    public void InitializeInfo(string result)
    {
        JObject userinfo = JObject.Parse(result);
        JArray iteminfo = JArray.Parse(userinfo["equipments"].ToString());

        long auid = (long)userinfo.GetValue("auid");

        if ( GameManager.Singleton.otherUser.ContainsKey(auid) )
        {
            otherUser = GameManager.Singleton.otherUser[auid];
        }
        else
        {
            otherUser.uID = auid;
            otherUser.nickname = (string)userinfo.GetValue("nickname");

            foreach ( JToken jt in iteminfo )
            {
                JProperty targetProperty = userinfo.Properties().FirstOrDefault(ca => ca.Value.ToString() == jt["id"].ToString());
                string target = targetProperty.Name;

                switch ( ((int)jt["primaryKey"]).ToString("X").Substring(0, 2) )
                {
                    case "20":
                        AddWeapon(jt, target);
                        break;
                    case "23":
                        AddGear(jt, target);
                        break;
                    case "11":
                        AddCharacter(jt, target);
                        break;
                }
            }

            GameManager.Singleton.otherUser.Add(auid, otherUser);
        }

        _txtTitle.text = otherUser.nickname;

        SetItem();
        SetCharacter();
    }

    IEnumerable<JProperty> FindKeyByValue(JObject jObject, string target)
    {
        return jObject.Properties().Where(prop => prop.Value.ToString() == target);
    }

    void AddWeapon(JToken jt, string target)
    {
        long id = (long)jt["id"];
        uint primaryKey = (uint)jt["primaryKey"];
        int count = 1;
        int upgrade = (int)jt["upgrade"];
        int reinforce = (int)jt["reinforce"];
        int limitbreak = (int)jt["limitbreak"];
        bool islock = false;
        bool isNew = false;
        uint[] effectkey = { (uint)jt["effect00"], (uint)jt["effect01"], (uint)jt["effect02"],
                             (uint)jt["effect03"], (uint)jt["effect04"], (uint)jt["effect05"] };
        float[] effectvalue = { (float)jt["effectValue00"], (float)jt["effectValue01"], (float)jt["effectValue02"],
                                (float)jt["effectValue03"], (float)jt["effectValue04"], (float)jt["effectValue05"] };

        int slot = int.Parse(target.Replace("equipWeapon", ""));

        ItemWeapon temp = new ItemWeapon(id, primaryKey, count, upgrade, reinforce, limitbreak,
                                         islock, isNew, effectkey, effectvalue);
        otherUser.equipWeapon[slot] = temp;
    }

    void AddGear(JToken jt, string target)
    {
        long id = (long)jt["id"];
        uint primaryKey = (uint)jt["primaryKey"];
        int count = 1;
        int upgrade = (int)jt["upgrade"];
        int reinforce = (int)jt["reinforce"];
        int limitbreak = (int)jt["limitbreak"];
        bool islock = false;
        bool isNew = false;
        uint[] effectkey = { (uint)jt["effect00"], (uint)jt["effect01"], (uint)jt["effect02"],
                             (uint)jt["effect03"], (uint)jt["effect04"], (uint)jt["effect05"] };
        float[] effectvalue = { (float)jt["effectValue00"], (float)jt["effectValue01"], (float)jt["effectValue02"],
                                (float)jt["effectValue03"], (float)jt["effectValue04"], (float)jt["effectValue05"] };

        int slot = int.Parse(target.Replace("equipGear", ""));

        ItemGear temp = new ItemGear(id, primaryKey, count, upgrade, reinforce, limitbreak,
                                     islock, isNew, effectkey, effectvalue);

        otherUser.equipGear[slot] = temp;
    }

    void AddCharacter(JToken jt, string key)
    {
        long id = (long)jt["id"];
        uint primaryKey = (uint)jt["primaryKey"];

        ItemCharacter temp = new ItemCharacter(id, primaryKey, 0);

        otherUser.equipcharacter = temp;
    }

    void SetItem()
    {
        for ( int i = 0; i < 4; i++ )
        {
            if ( null != otherUser.equipWeapon[i] )
            {
                _sWeapon[i] = m_MenuMgr.LoadComponent<SlotWeapon>(_tEquipWeapon[i], EUIComponent.SlotWeapon);
                _sWeapon[i].Initialize(otherUser.equipWeapon[i]);
                _sWeapon[i].SetState(SlotWeapon.SlotState.compareNormal);
            }
                
            if ( null != otherUser.equipGear[i] )
            {
                _sGear[i] = m_MenuMgr.LoadComponent<SlotGear>(_tEquipGear[i], EUIComponent.SlotGear);
                _sGear[i].Initialize(otherUser.equipGear[i]);
                _sGear[i].SetState(SlotGear.SlotState.compareNormal);
            }                
        }
    }

    void SetCharacter()
    {
        string cName = string.Empty;

        if (null != otherUser.equipcharacter )
        {
            cName = CharacterTable.GetData(otherUser.equipcharacter.nKey).Prefab;
            _goCharacter = m_ResourceMgr.CreateObject(EResourceType.Character, cName, _tEquipCharacter);
        }
    }

    private void OnEnable()
    {
        Array.ForEach(_tEquipWeapon, w => ComUtil.DestroyChildren(w));
        Array.ForEach(_tEquipGear, g => ComUtil.DestroyChildren(g));
        Destroy(_goCharacter);

        _goCharacter = null;
        otherUser = new OtherUser();
        _sWeapon = new SlotWeapon[4];
        _sGear = new SlotGear[4];
    }

    private void Awake()
    {
        Initialize();
    }

    public override void Initialize()
    {
        base.Initialize();
    }

    public override void Open()
    {
        base.Open();
    }

    public override void Close()
    {
        base.Close();
    }

    public override void Escape()
    {
        base.Escape();
    }
}
