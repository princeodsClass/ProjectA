using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class CrystalDealTable : GameEntityData
{
    public static CrystalDealTable GetData(uint key)
    {
        if (pool.ContainsKey(ENTITY_TYPE.CrystalDealTable.TypeName()))
        {
            EntityContainer container = pool[ENTITY_TYPE.CrystalDealTable.TypeName()];
            CrystalDealTable entity = container.Find(key.ToString()) as CrystalDealTable;

            if (null == entity)
            {
                string msg = $"Invalid Key.. CrystalDeal.csv == Key:{key}";
                GameManager.Log(msg, "red");
            }

            return entity;
        }

        return null;
    }

    public static bool IsContainsKey(uint nKey)
    {
        if (pool.ContainsKey(ENTITY_TYPE.CrystalDealTable.TypeName()))
        {
            EntityContainer container = pool[ENTITY_TYPE.CrystalDealTable.TypeName()];
            return container.ContainsKey(nKey.ToString());
        }
        return false;
    }

    public static List<CrystalDealTable> GetList()
    {
        if (pool.ContainsKey(ENTITY_TYPE.CrystalDealTable.TypeName()))
        {
            EntityContainer container = pool[ENTITY_TYPE.CrystalDealTable.TypeName()];
            return container.list.ConvertAll(each => { return each as CrystalDealTable; });
        }

        return null;
    }

    public override void OnCreateByDataBase(int fieldid, DataBase database)
    {
        base.OnCreateByDataBase(fieldid, database);
        base.SetKey(string.Format("{0}", PrimaryKey));
    }
}
