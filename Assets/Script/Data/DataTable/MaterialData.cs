using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class MaterialTable : GameEntityData
{
    public static MaterialTable GetData(uint key)
    {
        if (pool.ContainsKey(ENTITY_TYPE.MaterialTable.TypeName()))
        {
            EntityContainer container = pool[ENTITY_TYPE.MaterialTable.TypeName()];
            MaterialTable entity = container.Find(key.ToString()) as MaterialTable;

            if (null == entity)
            {
                string msg = $"Invalid Key.. Material.csv == Key:{key}";
                GameManager.Log(msg, "red");
            }

            return entity;
        }

        return null;
    }

    public static MaterialTable GetData(int order)
    {
        List<MaterialTable> list = GetList();
        return list[order];
    }

    public static bool IsContainsKey(uint nKey)
    {
        if (pool.ContainsKey(ENTITY_TYPE.MaterialTable.TypeName()))
        {
            EntityContainer container = pool[ENTITY_TYPE.MaterialTable.TypeName()];
            return container.ContainsKey(nKey.ToString());
        }
        return false;
    }

    public static List<MaterialTable> GetList()
    {
        if (pool.ContainsKey(ENTITY_TYPE.MaterialTable.TypeName()))
        {
            EntityContainer container = pool[ENTITY_TYPE.MaterialTable.TypeName()];
            return container.list.ConvertAll(each => { return each as MaterialTable; });
        }

        return null;
    }

    public override void OnCreateByDataBase(int fieldid, DataBase database)
    {
        base.OnCreateByDataBase(fieldid, database);
        base.SetKey(string.Format("{0}", PrimaryKey));
    }
}
