using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemBase
{
    public long id = 0;
    public uint nKey = 0;
    public int nGrade = 0;
    public int nSubGrade = 0;
    public int nVolume = 0;
    public EItemType eType;
    public string strName = string.Empty;
    public string strDesc = string.Empty;
    public string strIcon = string.Empty;
    public bool bNew = false;

    public int nMaxStackCount = 1;

    public int nCurUpgrade = 0;
    public int nCurReinforce = 0;
    public int nCurLimitbreak = 0;

    public uint[] nRecycleMaterialKey = new uint[5];
    public int[] RecycleMaterialCount = new int[5];

    public int nCP = 0;

    public bool bIsLock = false;

    //public virtual void Initialize(long id, int nTableKey, int nCount, int upgrade = 0, int reinforce = 0, int limitbreak = 0, DateTime openDatetime = default, bool islock = false, bool isNew = false) { }
    public virtual Sprite GetIcon() { return GameResourceManager.Singleton.LoadSprite(EAtlasType.Icons, strIcon); }
}