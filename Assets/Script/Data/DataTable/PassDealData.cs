using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class PassDealTable : GameEntityData
{
    public static PassDealTable GetData(uint key)
    {
        if (pool.ContainsKey(ENTITY_TYPE.PassDealTable.TypeName()))
        {
            EntityContainer container = pool[ENTITY_TYPE.PassDealTable.TypeName()];
            PassDealTable entity = container.Find(key.ToString()) as PassDealTable;

            if (null == entity)
            {
                string msg = $"Invalid Key.. PassDeal.csv == Key:{key}";
                GameManager.Log(msg, "red");
            }

            return entity;
        }

        return null;
    }

    public static bool IsContainsKey(uint nKey)
    {
        if (pool.ContainsKey(ENTITY_TYPE.PassDealTable.TypeName()))
        {
            EntityContainer container = pool[ENTITY_TYPE.PassDealTable.TypeName()];
            return container.ContainsKey(nKey.ToString());
        }
        return false;
    }

    public static List<PassDealTable> GetList()
    {
        if (pool.ContainsKey(ENTITY_TYPE.PassDealTable.TypeName()))
        {
            EntityContainer container = pool[ENTITY_TYPE.PassDealTable.TypeName()];
            return container.list.ConvertAll(each => { return each as PassDealTable; });
        }

        return null;
    }

    public override void OnCreateByDataBase(int fieldid, DataBase database)
    {
        base.OnCreateByDataBase(fieldid, database);
        base.SetKey(string.Format("{0}", PrimaryKey));
    }
}
