using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class HuntTable : GameEntityData
{
    public static HuntTable GetData(uint key)
    {
        if (pool.ContainsKey(ENTITY_TYPE.HuntTable.TypeName()))
        {
            EntityContainer container = pool[ENTITY_TYPE.HuntTable.TypeName()];
            HuntTable entity = container.Find(key.ToString()) as HuntTable;

            if (null == entity)
            {
                string msg = $"Invalid Key.. Hunt.csv == Key:{key}";
                GameManager.Log(msg, "red");
            }

            return entity;
        }

        return null;
    }

    public static bool IsContainsKey(uint nKey)
    {
        if (pool.ContainsKey(ENTITY_TYPE.HuntTable.TypeName()))
        {
            EntityContainer container = pool[ENTITY_TYPE.HuntTable.TypeName()];
            return container.ContainsKey(nKey.ToString());
        }
        return false;
    }

    public static List<HuntTable> GetList()
    {
        if (pool.ContainsKey(ENTITY_TYPE.HuntTable.TypeName()))
        {
            EntityContainer container = pool[ENTITY_TYPE.HuntTable.TypeName()];
            return container.list.ConvertAll(each => { return each as HuntTable; });
        }

        return null;
    }

    public static HuntTable GetHuntData(int level)
    {
        int ep;
        int ch;

        ep = level / 10 + 1;
        ch = (level % 10) + 1;

        string key = $"341{ep.ToString("X3")}{ch.ToString("X2")}";

        return GetData(Convert.ToUInt32(key, 16));
    }

    public override void OnCreateByDataBase(int fieldid, DataBase database)
    {
        base.OnCreateByDataBase(fieldid, database);
        base.SetKey(string.Format("{0}", PrimaryKey));
    }
}
