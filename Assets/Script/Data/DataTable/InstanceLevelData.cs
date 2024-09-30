using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class InstanceLevelTable : GameEntityData
{
    public static InstanceLevelTable GetData(uint key)
    {
        if (pool.ContainsKey(ENTITY_TYPE.InstanceLevelTable.TypeName()))
        {
            EntityContainer container = pool[ENTITY_TYPE.InstanceLevelTable.TypeName()];
            InstanceLevelTable entity = container.Find(key.ToString()) as InstanceLevelTable;

            if (null == entity)
            {
                string msg = $"Invalid Key.. InstanceLevelTable.csv == Key:{key}";
                GameManager.Log(msg, "red");
            }

            return entity;
        }

        return null;
    }

    public static List<InstanceLevelTable> GetList()
    {
        if (pool.ContainsKey(ENTITY_TYPE.InstanceLevelTable.TypeName()))
        {
            EntityContainer container = pool[ENTITY_TYPE.InstanceLevelTable.TypeName()];
            return container.list.ConvertAll(each => { return each as InstanceLevelTable; });
        }

        return new List<InstanceLevelTable>();
    }

    public override void OnCreateByDataBase(int fieldid, DataBase database)
    {
        base.OnCreateByDataBase(fieldid, database);
        base.SetKey(string.Format("{0}", PrimaryKey));
    }
}
