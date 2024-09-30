using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class MoneyDealTable : GameEntityData
{
    public static MoneyDealTable GetData(uint key)
    {
        if (pool.ContainsKey(ENTITY_TYPE.MoneyDealTable.TypeName()))
        {
            EntityContainer container = pool[ENTITY_TYPE.MoneyDealTable.TypeName()];
            MoneyDealTable entity = container.Find(key.ToString()) as MoneyDealTable;

            if (null == entity)
            {
                string msg = $"Invalid Key.. MoneyDeal.csv == Key:{key}";
                GameManager.Log(msg, "red");
            }

            return entity;
        }

        return null;
    }

    public static bool IsContainsKey(uint nKey)
    {
        if (pool.ContainsKey(ENTITY_TYPE.MoneyDealTable.TypeName()))
        {
            EntityContainer container = pool[ENTITY_TYPE.MoneyDealTable.TypeName()];
            return container.ContainsKey(nKey.ToString());
        }
        return false;
    }

    public static List<MoneyDealTable> GetList()
    {
        if (pool.ContainsKey(ENTITY_TYPE.MoneyDealTable.TypeName()))
        {
            EntityContainer container = pool[ENTITY_TYPE.MoneyDealTable.TypeName()];
            return container.list.ConvertAll(each => { return each as MoneyDealTable; });
        }

        return null;
    }

    public override void OnCreateByDataBase(int fieldid, DataBase database)
    {
        base.OnCreateByDataBase(fieldid, database);
        base.SetKey(string.Format("{0}", PrimaryKey));
    }
}
