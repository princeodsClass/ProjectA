using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class QuestTable : GameEntityData
{
    public static QuestTable GetData(uint key)
    {
        if (pool.ContainsKey(ENTITY_TYPE.QuestTable.TypeName()))
        {
            EntityContainer container = pool[ENTITY_TYPE.QuestTable.TypeName()];
            QuestTable entity = container.Find(key.ToString()) as QuestTable;

            if (null == entity)
            {
                string msg = $"Invalid Key.. Quest.csv == Key:{key}";
                GameManager.Log(msg, "red");
            }

            return entity;
        }

        return null;
    }

    public static bool IsContainsKey(uint nKey)
    {
        if (pool.ContainsKey(ENTITY_TYPE.QuestTable.TypeName()))
        {
            EntityContainer container = pool[ENTITY_TYPE.QuestTable.TypeName()];
            return container.ContainsKey(nKey.ToString());
        }
        return false;
    }

    public static List<QuestTable> GetList()
    {
        if (pool.ContainsKey(ENTITY_TYPE.QuestTable.TypeName()))
        {
            EntityContainer container = pool[ENTITY_TYPE.QuestTable.TypeName()];
            return container.list.ConvertAll(each => { return each as QuestTable; });
        }

        return null;
    }

    public static List<QuestTable> GetList(int level, int count)
    {
        List<QuestTable> list = GetList();

        int tar = FindClosestValue(list, level);
        List<QuestTable> filteredList = list.Where(quest => quest.Level == tar).ToList();

        return filteredList.OrderBy(x => Guid.NewGuid()).Take(count).ToList();
    }

    public static int FindClosestValue(List<QuestTable> values, int targetValue)
    {
        int closestValue = values[0].Level;
        int minDifference = Math.Abs(targetValue - closestValue);

        foreach (QuestTable item in values)
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

    public static List<QuestTable> GetDistinctRandomElements(List<QuestTable> list, int count)
    {
        List<QuestTable> result = new List<QuestTable>();

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
