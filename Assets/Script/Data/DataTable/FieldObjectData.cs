using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class FieldObjectTable : GameEntityData
{
    public static FieldObjectTable GetData(uint key)
    {
        if (pool.ContainsKey(ENTITY_TYPE.FieldObjectTable.TypeName()))
        {
            EntityContainer container = pool[ENTITY_TYPE.FieldObjectTable.TypeName()];
            FieldObjectTable entity = container.Find(key.ToString()) as FieldObjectTable;

            if (null == entity)
            {
                string msg = $"Invalid Key.. FieldObject.csv == Key:{key}";
                GameManager.Log(msg, "red");
            }

            return entity;
        }

        return null;
    }

    public static bool IsContainsKey(uint nKey)
    {
        if (pool.ContainsKey(ENTITY_TYPE.FieldObjectTable.TypeName()))
        {
            EntityContainer container = pool[ENTITY_TYPE.FieldObjectTable.TypeName()];
            return container.ContainsKey(nKey.ToString());
        }
        return false;
    }

    public static List<FieldObjectTable> GetList()
    {
        if (pool.ContainsKey(ENTITY_TYPE.FieldObjectTable.TypeName()))
        {
            EntityContainer container = pool[ENTITY_TYPE.FieldObjectTable.TypeName()];
            return container.list.ConvertAll(each => { return each as FieldObjectTable; });
        }

        return null;
    }

    public override void OnCreateByDataBase(int fieldid, DataBase database)
    {
        base.OnCreateByDataBase(fieldid, database);
        base.SetKey(string.Format("{0}", PrimaryKey));
    }
}
