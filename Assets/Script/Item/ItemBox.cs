using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemBox : ItemBase
{
    public int nSubType = 0;
    public int nSlotNumber = 0;
    public int nLevel = 0;
    public int nOpenDatetime = 0;
    public uint nOpenMaterialKey = 0;
    public int nOpenMaterialCount = 0;
    public int nRewardGroup = 0;

    public string strPrefeb = string.Empty;

    public DateTime _openStartDatetime;

    public ItemBox() { }
    public ItemBox(long id, uint primaryKey, int slotNumber, DateTime openStartDatetime = default, bool isNew = false)
    {
        Initialize(id, primaryKey, slotNumber, openStartDatetime);
    }

    public void Initialize(long nID, uint primaryKey, int slotNumber, DateTime openStartDatetime = default, bool isNew = false)
    {
        BoxTable cTbl = BoxTable.GetData(primaryKey);

        if (null == cTbl)
        {
            GameManager.Log("Item Init Failed.. Key:" + primaryKey, "red");
            return;
        }

        id = nID;
        nKey = cTbl.PrimaryKey;
        eType = (EItemType)cTbl.Type;
        nSubType = cTbl.SubType;
        nGrade = cTbl.Grade;
        nLevel = cTbl.Level;
        nVolume = 1;
        nSlotNumber = slotNumber;
        nOpenDatetime = cTbl.OpenTime;
        nOpenMaterialKey = cTbl.OpenMaterialKey;
        nOpenMaterialCount = cTbl.OpenMaterialCount;
        strName = NameTable.GetValue(cTbl.NameKey);
        strDesc = DescTable.GetValue(cTbl.DescKey);
        nRewardGroup = cTbl.RewardGroup;

        strPrefeb = cTbl.Prefeb;
        strIcon = cTbl.Icon;

        _openStartDatetime = openStartDatetime;
    }

    /*
    public override Sprite GetIcon()
    {
        EAtlasType eAtlas = EAtlasType.Icons;
        return GameResourceManager.Singleton.LoadSprite(eAtlas, strIcon);
    }
    */
}