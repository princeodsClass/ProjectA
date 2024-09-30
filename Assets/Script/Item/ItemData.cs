using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct ItemData
{
    public int nItemKey;
    public int nItemCount;
    public bool bAdd;

    public ItemData(int nKey, int nCount = 0, bool bAddReward = false)
    {
        nItemKey = nKey;
        nItemCount = nCount;
        bAdd = bAddReward;
    }
}

public struct TypeItemData
{
    public int nType;
    public int nKey;
    public int nValue;
}
