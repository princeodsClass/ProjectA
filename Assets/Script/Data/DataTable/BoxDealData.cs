using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class BoxDealTable : GameEntityData
{
    public static BoxDealTable GetData(uint key)
    {
        if (pool.ContainsKey(ENTITY_TYPE.BoxDealTable.TypeName()))
        {
            EntityContainer container = pool[ENTITY_TYPE.BoxDealTable.TypeName()];
            BoxDealTable entity = container.Find(key.ToString()) as BoxDealTable;

            if (null == entity)
            {
                string msg = $"Invalid Key.. BoxDeal.csv == Key:{key}";
                GameManager.Log(msg, "red");
            }

            return entity;
        }

        return null;
    }

    public static bool IsContainsKey(uint nKey)
    {
        if (pool.ContainsKey(ENTITY_TYPE.BoxDealTable.TypeName()))
        {
            EntityContainer container = pool[ENTITY_TYPE.BoxDealTable.TypeName()];
            return container.ContainsKey(nKey.ToString());
        }
        return false;
    }

    public static List<BoxDealTable> GetList()
    {
        if (pool.ContainsKey(ENTITY_TYPE.BoxDealTable.TypeName()))
        {
            EntityContainer container = pool[ENTITY_TYPE.BoxDealTable.TypeName()];
            return container.list.ConvertAll(each => { return each as BoxDealTable; });
        }

        return null;
    }

    public static float[] GetPercentage(uint pk)
    {
        BoxDealTable b = GetData(pk);

        float[] percentage =
            {
                b.Percentage0,
                b.Percentage1,
                b.Percentage2,
                b.Percentage3,
                b.Percentage4,
                b.Percentage5,
                b.Percentage6,
            };

        return percentage;
    }

    public static List<BoxDealTable> GetSymbolDataPerGroup(int type)
    {
        List<BoxDealTable> returnValue = new List<BoxDealTable>();

        foreach (BoxDealTable boxDeal in GetList())
        {
            bool hasSymbol = returnValue.Any(d => d.Group == boxDeal.Group);
            if ( false == hasSymbol && boxDeal.Type == type)
                returnValue.Add(boxDeal);
        }

        return returnValue;
    }

    public static List<BoxDealTable> GetGroup(int group)
    {
        List<BoxDealTable> returnValue = new List<BoxDealTable>();

        foreach (BoxDealTable reward in GetList())
        {
            if (reward.Group == group)
                returnValue.Add(reward);
        }

        return returnValue;
    }

    public static uint RandomIndexByFactorInGroup(int group, uint preKey = 00)
    {
        float totalWeight = 0f;

        List<BoxDealTable> list = GetGroup(group);
        List<BoxDealTable> newList = new List<BoxDealTable>();

        if (preKey == 0)
        {
            newList = list;
            totalWeight = list.Sum(item => item.SelectionFactor);
        }
        else
        {
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].PrimaryKey != preKey)
                {
                    newList.Add(list[i]);
                    totalWeight += list[i].SelectionFactor;
                }
            }
        }

        float randomValue = UnityEngine.Random.Range(0f, totalWeight);

        for (int i = 0; i < newList.Count; i++)
        {
            if (randomValue < newList[i].SelectionFactor)
            {
                return newList[i].PrimaryKey;
            }
            randomValue -= newList[i].SelectionFactor;
        }

        return newList[newList.Count - 1].PrimaryKey;
    }

    public override void OnCreateByDataBase(int fieldid, DataBase database)
    {
        base.OnCreateByDataBase(fieldid, database);
        base.SetKey(string.Format("{0}", PrimaryKey));
    }
}
