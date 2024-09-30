using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class DailyDealTable : GameEntityData
{
    public static DailyDealTable GetData(uint key)
    {
        if (pool.ContainsKey(ENTITY_TYPE.DailyDealTable.TypeName()))
        {
            EntityContainer container = pool[ENTITY_TYPE.DailyDealTable.TypeName()];
            DailyDealTable entity = container.Find(key.ToString()) as DailyDealTable;

            if (null == entity)
            {
                string msg = $"Invalid Key.. DailyDeal.csv == Key:{key}";
                GameManager.Log(msg, "red");
            }

            return entity;
        }

        return null;
    }

    public static bool IsContainsKey(uint nKey)
    {
        if (pool.ContainsKey(ENTITY_TYPE.DailyDealTable.TypeName()))
        {
            EntityContainer container = pool[ENTITY_TYPE.DailyDealTable.TypeName()];
            return container.ContainsKey(nKey.ToString());
        }
        return false;
    }

    public static List<DailyDealTable> GetList()
    {
        if (pool.ContainsKey(ENTITY_TYPE.DailyDealTable.TypeName()))
        {
            EntityContainer container = pool[ENTITY_TYPE.DailyDealTable.TypeName()];
            return container.list.ConvertAll(each => { return each as DailyDealTable; });
        }

        return null;
    }

    public static List<DailyDealTable> GetList(int level)
    {
        List<DailyDealTable> list = new List<DailyDealTable>();
        List<DailyDealTable> t = new List<DailyDealTable>();

        list = GetList();

        int tar = FindClosestValue(list, level);

        list.ForEach(each => { if (each.Level == tar) t.Add(each); });

        return t;
    }

    public static int FindClosestValue(List<DailyDealTable> values, int targetValue)
    {
        int closestValue = values[0].Level;
        int minDifference = Math.Abs(targetValue - closestValue);

        foreach (DailyDealTable item in values)
        {
            int difference = Math.Abs(targetValue - item.Level);
            if (difference < minDifference)
            {
                minDifference = difference;
                closestValue = item.Level;
            }
        }

        return closestValue;
    }

    public override void OnCreateByDataBase(int fieldid, DataBase database)
    {
        base.OnCreateByDataBase(fieldid, database);
        base.SetKey(string.Format("{0}", PrimaryKey));
    }

    public static List<DailyDealTable> GetDistinctRandomElements(List<DailyDealTable> list, int count)
    {
        List<DailyDealTable> result = new List<DailyDealTable>();

        float totalWeight = list.Sum(item => item.SelectionFactor);

        while (result.Count < count && list.Count > 0)
        {
            float randomValue = UnityEngine.Random.Range(0f, totalWeight);

            foreach (var item in list)
            {
                float selectionProbability = item.SelectionFactor / totalWeight;

                if (randomValue < selectionProbability)
                {
                    result.Add(item);
                    totalWeight -= item.SelectionFactor;
                    list.Remove(item);
                    break;
                }

                randomValue -= selectionProbability;
            }
        }

        return result;
    }
}
