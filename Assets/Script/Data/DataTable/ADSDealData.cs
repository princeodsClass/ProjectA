using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class ADSDealTable : GameEntityData
{
    public static ADSDealTable GetData(uint key)
    {
        if (pool.ContainsKey(ENTITY_TYPE.ADSDealTable.TypeName()))
        {
            EntityContainer container = pool[ENTITY_TYPE.ADSDealTable.TypeName()];
            ADSDealTable entity = container.Find(key.ToString()) as ADSDealTable;

            if (null == entity)
            {
                string msg = $"Invalid Key.. ADSDeal.csv == Key:{key}";
                GameManager.Log(msg, "red");
            }

            return entity;
        }

        return null;
    }

    public static bool IsContainsKey(uint nKey)
    {
        if (pool.ContainsKey(ENTITY_TYPE.ADSDealTable.TypeName()))
        {
            EntityContainer container = pool[ENTITY_TYPE.ADSDealTable.TypeName()];
            return container.ContainsKey(nKey.ToString());
        }
        return false;
    }

    public static List<ADSDealTable> GetList()
    {
        if (pool.ContainsKey(ENTITY_TYPE.ADSDealTable.TypeName()))
        {
            EntityContainer container = pool[ENTITY_TYPE.ADSDealTable.TypeName()];
            return container.list.ConvertAll(each => { return each as ADSDealTable; });
        }

        return null;
    }

    public static List<ADSDealTable> GetList(int level)
    {
        List<ADSDealTable> list = new List<ADSDealTable>();
        List<ADSDealTable> t = new List<ADSDealTable>();

        list = GetList();

        int tar = FindClosestValue(list, level);

        list.ForEach(each => { if (each.Level == tar) t.Add(each); });

        return t;
    }

    public static int FindClosestValue(List<ADSDealTable> values, int targetValue)
    {
        int closestValue = values[0].Level;
        int minDifference = Math.Abs(targetValue - closestValue);

        foreach (ADSDealTable item in values)
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

    public static List<ADSDealTable> GetDistinctRandomElements(List<ADSDealTable> list, int count)
    {
        List<ADSDealTable> result = new List<ADSDealTable>();

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
