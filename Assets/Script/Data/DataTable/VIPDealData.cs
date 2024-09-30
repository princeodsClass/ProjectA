using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class VIPDealTable : GameEntityData
{
    public static VIPDealTable GetData(uint key)
    {
        if (pool.ContainsKey(ENTITY_TYPE.VIPDealTable.TypeName()))
        {
            EntityContainer container = pool[ENTITY_TYPE.VIPDealTable.TypeName()];
            VIPDealTable entity = container.Find(key.ToString()) as VIPDealTable;

            if (null == entity)
            {
                string msg = $"Invalid Key.. VIPDeal.csv == Key:{key}";
                GameManager.Log(msg, "red");
            }

            return entity;
        }

        return null;
    }

    public static bool IsContainsKey(uint nKey)
    {
        if (pool.ContainsKey(ENTITY_TYPE.VIPDealTable.TypeName()))
        {
            EntityContainer container = pool[ENTITY_TYPE.VIPDealTable.TypeName()];
            return container.ContainsKey(nKey.ToString());
        }
        return false;
    }

    public static List<VIPDealTable> GetList()
    {
        if (pool.ContainsKey(ENTITY_TYPE.VIPDealTable.TypeName()))
        {
            EntityContainer container = pool[ENTITY_TYPE.VIPDealTable.TypeName()];
            return container.list.ConvertAll(each => { return each as VIPDealTable; });
        }

        return null;
    }

    public override void OnCreateByDataBase(int fieldid, DataBase database)
    {
        base.OnCreateByDataBase(fieldid, database);
        base.SetKey(string.Format("{0}", PrimaryKey));
    }
}
