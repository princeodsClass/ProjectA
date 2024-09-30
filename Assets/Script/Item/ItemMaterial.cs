using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemMaterial : ItemBase
{
    public int nSubType = 0;

    public uint[] nContents4GetKey = new uint[10];
    public uint[] nContents4UseKey = new uint[10];

    public ItemMaterial() { }
    public ItemMaterial(long id, uint primaryKey, int count)
    {
        Initialize(id, primaryKey, count);
    }

    public void Initialize(long nID, uint primaryKey, int count, bool isNew = false)
    {
        MaterialTable cTbl = MaterialTable.GetData(primaryKey);
        if (null == cTbl)
        {
            GameManager.Log("Item Init Failed.. Key:" + primaryKey, "red");
            return;
        }

        id = nID;
        bNew = isNew;
        nKey = cTbl.PrimaryKey;
        eType = (EItemType)cTbl.Type;
        nSubType = cTbl.SubType;
        nGrade = cTbl.Grade;
        nVolume = count;
        strName = NameTable.GetValue(cTbl.NameKey);
        strDesc = DescTable.GetValue(cTbl.DescKey);
        nMaxStackCount = cTbl.MaxStackCount;

        strIcon = cTbl.Icon;

        nContents4GetKey[0] = cTbl.ContentsForGetKey00;
        nContents4GetKey[1] = cTbl.ContentsForGetKey01;
        nContents4GetKey[2] = cTbl.ContentsForGetKey02;
        nContents4GetKey[3] = cTbl.ContentsForGetKey03;
        nContents4GetKey[4] = cTbl.ContentsForGetKey04;
        nContents4GetKey[5] = cTbl.ContentsForGetKey05;
        nContents4GetKey[6] = cTbl.ContentsForGetKey06;
        nContents4GetKey[7] = cTbl.ContentsForGetKey07;
        nContents4GetKey[8] = cTbl.ContentsForGetKey08;
        nContents4GetKey[9] = cTbl.ContentsForGetKey09;

        nContents4UseKey[0] = cTbl.ContentsForUseKey00;
        nContents4UseKey[1] = cTbl.ContentsForUseKey01;
        nContents4UseKey[2] = cTbl.ContentsForUseKey02;
        nContents4UseKey[3] = cTbl.ContentsForUseKey03;
        nContents4UseKey[4] = cTbl.ContentsForUseKey04;
        nContents4UseKey[5] = cTbl.ContentsForUseKey05;
        nContents4UseKey[6] = cTbl.ContentsForUseKey06;
        nContents4UseKey[7] = cTbl.ContentsForUseKey07;
        nContents4UseKey[8] = cTbl.ContentsForUseKey08;
        nContents4UseKey[9] = cTbl.ContentsForUseKey09;
    }

    public override Sprite GetIcon()
    {
        EAtlasType eAtlas = EAtlasType.Icons;
        return GameResourceManager.Singleton.LoadSprite(eAtlas, strIcon);
    }
}