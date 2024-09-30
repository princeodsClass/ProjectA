using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class BoxTable : GameEntityData
{
    public static BoxTable GetData(uint key)
    {
        if (pool.ContainsKey(ENTITY_TYPE.BoxTable.TypeName()))
        {
            EntityContainer container = pool[ENTITY_TYPE.BoxTable.TypeName()];
            BoxTable entity = container.Find(key.ToString()) as BoxTable;

            if (null == entity)
            {
                string msg = $"Invalid Key.. Box.csv == Key:{key}";
                GameManager.Log(msg, "red");
            }

            return entity;
        }

        return null;
    }

    public static bool IsContainsKey(uint nKey)
    {
        if (pool.ContainsKey(ENTITY_TYPE.BoxTable.TypeName()))
        {
            EntityContainer container = pool[ENTITY_TYPE.BoxTable.TypeName()];
            return container.ContainsKey(nKey.ToString());
        }
        return false;
    }

    public static List<BoxTable> GetList()
    {
        if (pool.ContainsKey(ENTITY_TYPE.BoxTable.TypeName()))
        {
            EntityContainer container = pool[ENTITY_TYPE.BoxTable.TypeName()];
            return container.list.ConvertAll(each => { return each as BoxTable; });
        }

        return null;
    }

    public override void OnCreateByDataBase(int fieldid, DataBase database)
    {
        base.OnCreateByDataBase(fieldid, database);
        base.SetKey(string.Format("{0}", PrimaryKey));

        //GameManager.Log($"Box : {fieldid} /  {PrimaryKey}  / {NameKey} / {DescKey}", "green");
    }
}
