using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class WorkshopLevelTable : GameEntityData
{
    public static WorkshopLevelTable GetData(uint key)
    {
        if (pool.ContainsKey(ENTITY_TYPE.WorkshopLevelTable.TypeName()))
        {
            EntityContainer container = pool[ENTITY_TYPE.WorkshopLevelTable.TypeName()];
            WorkshopLevelTable entity = container.Find(key.ToString()) as WorkshopLevelTable;

            if (null == entity)
            {
                string msg = $"Invalid Key.. WorkshopLevelTable.csv == Key:{key}";
                GameManager.Log(msg, "red");
            }

            return entity;
        }

        return null;
    }

    public static List<WorkshopLevelTable> GetList()
    {
        if (pool.ContainsKey(ENTITY_TYPE.WorkshopLevelTable.TypeName()))
        {
            EntityContainer container = pool[ENTITY_TYPE.WorkshopLevelTable.TypeName()];
            return container.list.ConvertAll(each => { return each as WorkshopLevelTable; });
        }

        return new List<WorkshopLevelTable>();
    }

    public override void OnCreateByDataBase(int fieldid, DataBase database)
    {
        base.OnCreateByDataBase(fieldid, database);
        base.SetKey(string.Format("{0}", PrimaryKey));
    }
}
