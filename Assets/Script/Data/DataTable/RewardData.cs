using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class RewardTable : GameEntityData
{
    public static RewardTable GetData(uint key)
    {
        if (pool.ContainsKey(ENTITY_TYPE.RewardTable.TypeName()))
        {
            EntityContainer container = pool[ENTITY_TYPE.RewardTable.TypeName()];
            RewardTable entity = container.Find(key.ToString()) as RewardTable;

            if (null == entity)
            {
                string msg = $"Invalid Key.. Reward.csv == Key:{key}";
                GameManager.Log(msg, "red");
            }

            return entity;
        }

        return null;
    }

    public static bool IsContainsKey(uint nKey)
    {
        if (pool.ContainsKey(ENTITY_TYPE.RewardTable.TypeName()))
        {
            EntityContainer container = pool[ENTITY_TYPE.RewardTable.TypeName()];
            return container.ContainsKey(nKey.ToString());
        }
        return false;
    }

    public static List<RewardTable> GetList()
    {
        if (pool.ContainsKey(ENTITY_TYPE.RewardTable.TypeName()))
        {
            EntityContainer container = pool[ENTITY_TYPE.RewardTable.TypeName()];
            return container.list.ConvertAll(each => { return each as RewardTable; });
        }

        return null;
    }

    public static List<RewardTable> GetGroup(int group)
    {
        List<RewardTable> returnValue = new List<RewardTable>();

        foreach(RewardTable reward in GetList())
        {
            if (reward.Group == group)
                returnValue.Add(reward);
        }

        return returnValue;
    }

    static List<RewardTable> RandomResultByFactorInGroup(List<RewardTable> list, int count = 1)
    {
        if (list.Count <= count) return list;

        List<RewardTable> result = new List<RewardTable>();

        float totalWeight = list.Sum(i => i.SelectionFactor);
        float r = 0f;
        float temp;

        for (int i = 0; i < count && list.Count > 0; i++)
        {
            temp = 0f;
            r = Random.Range(0f, totalWeight);

            for (int j = 0; j < list.Count; j++)
            {
                temp += list[j].SelectionFactor;

                if (r < temp)
                {
                    result.Add(list[j]);
                    totalWeight -= list[j].SelectionFactor;
                    list.RemoveAt(j);
                    break;
                }
            }
        }

        return result;
    }

    public static Dictionary<uint, int> RandomResultInGroup(int group, bool isReduce = false)
    {
        List<RewardTable> add = new List<RewardTable>();
        List<RewardTable> temp = new List<RewardTable>();

        GetGroup(group).ForEach(r =>
        {
            if (r.SelectionType == 1)
                for ( int i = 0; i < r.SelectionCount; i++ )
                    add.Add(r);
            else
                temp.Add(r);
        });

        if ( temp.Count > 0 )
            add.AddRange(RandomResultByFactorInGroup(temp, temp[0].SelectionCount));

        return RewardListTable.RandomResultByFactorInGroup(add, isReduce);
    }

    public override void OnCreateByDataBase(int fieldid, DataBase database)
    {
        base.OnCreateByDataBase(fieldid, database);
        base.SetKey(string.Format("{0}", PrimaryKey));
    }
}
