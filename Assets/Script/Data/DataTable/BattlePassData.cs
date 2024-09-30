using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class BattlePassTable : GameEntityData
{
    public static BattlePassTable GetData(uint key)
    {
        if (pool.ContainsKey(ENTITY_TYPE.BattlePassTable.TypeName()))
        {
            EntityContainer container = pool[ENTITY_TYPE.BattlePassTable.TypeName()];
            BattlePassTable entity = container.Find(key.ToString()) as BattlePassTable;

            if (null == entity)
            {
                string msg = $"Invalid Key.. BattlePassTable.csv == Key:{key}";
                GameManager.Log(msg, "red");
            }

            return entity;
        }

        return null;
    }

    public static List<BattlePassTable> GetList()
    {
        if (pool.ContainsKey(ENTITY_TYPE.BattlePassTable.TypeName()))
        {
            EntityContainer container = pool[ENTITY_TYPE.BattlePassTable.TypeName()];
            return container.list.ConvertAll(each => { return each as BattlePassTable; });
        }

        return new List<BattlePassTable>();
    }

    public override void OnCreateByDataBase(int fieldid, DataBase database)
    {
        base.OnCreateByDataBase(fieldid, database);
        base.SetKey(string.Format("{0}", PrimaryKey));
    }
}
