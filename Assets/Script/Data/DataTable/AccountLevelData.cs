using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class AccountLevelTable : GameEntityData
{
    public static AccountLevelTable GetData(uint key)
    {
        if (pool.ContainsKey(ENTITY_TYPE.AccountLevelTable.TypeName()))
        {
            EntityContainer container = pool[ENTITY_TYPE.AccountLevelTable.TypeName()];
            AccountLevelTable entity = container.Find(key.ToString()) as AccountLevelTable;

            if (null == entity)
            {
                string msg = $"Invalid Key.. AccountLevelTable.csv == Key:{key}";
                GameManager.Log(msg, "red");
            }

            return entity;
        }

        return null;
    }

    public static List<AccountLevelTable> GetList()
    {
        if (pool.ContainsKey(ENTITY_TYPE.AccountLevelTable.TypeName()))
        {
            EntityContainer container = pool[ENTITY_TYPE.AccountLevelTable.TypeName()];
            return container.list.ConvertAll(each => { return each as AccountLevelTable; });
        }

        return new List<AccountLevelTable>();
    }

    public override void OnCreateByDataBase(int fieldid, DataBase database)
    {
        base.OnCreateByDataBase(fieldid, database);
        base.SetKey(string.Format("{0}", PrimaryKey));
    }
}
