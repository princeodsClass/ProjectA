using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemCharacter : ItemBase
{
    public struct stSkillUpgrade
    {
        public int Upgrade05;
        public int Upgrade10;
        public int Upgrade15;
        public int Upgrade20;
        public int Upgrade25;
        public int Upgrade30;
        public int Upgrade35;
        public int Upgrade40;
        public int Upgrade45;
        public int Upgrade50;
        public int Upgrade55;
        public int Upgrade60;
        public int Upgrade65;
    }

    public stSkillUpgrade _stSkillUpgrade = new stSkillUpgrade();
    public int nMoveSpeed = 0;
    public int nHP = 0;

    public ItemCharacter() { }

    public ItemCharacter(long id, uint primaryKey, int upgrade, stSkillUpgrade upInfo = default)
    {
        Initialize(id, primaryKey, upgrade, upInfo);
    }

    public void Initialize(long nID, uint primaryKey, int upgrade, stSkillUpgrade upInfo = default, bool isNew = false)
    {
        CharacterTable cTbl = CharacterTable.GetData(primaryKey);
        if (null == cTbl)
        {
            GameManager.Log("Item Init Failed.. Key:" + primaryKey, "red");
            return;
        }

        id = nID;
        nKey = cTbl.PrimaryKey;
        eType = (EItemType)cTbl.Type;
        nGrade = cTbl.Grade;
        nVolume = 1;
        nCurUpgrade = upgrade;
        strName = NameTable.GetValue(cTbl.NameKey);
        strDesc = DescTable.GetValue(cTbl.DescKey);

        _stSkillUpgrade = upInfo;
		var oType = _stSkillUpgrade.GetType();

		foreach(var oFieldInfo in oType.GetFields())
		{
			int nVal = (int)oFieldInfo.GetValue(_stSkillUpgrade);
			oFieldInfo.SetValueDirect(__makeref(_stSkillUpgrade), (nVal == 0) ? 1 : nVal);
		}

        strIcon = cTbl.Icon;
    }

    public override Sprite GetIcon()
    {
        EAtlasType eAtlas = EAtlasType.Icons;
        return GameResourceManager.Singleton.LoadSprite(eAtlas, strIcon);
    }
}