using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class SkillTable : GameEntityData
{
    public static SkillTable GetData(uint key)
    {
        if (pool.ContainsKey(ENTITY_TYPE.SkillTable.TypeName()))
        {
            EntityContainer container = pool[ENTITY_TYPE.SkillTable.TypeName()];
            SkillTable entity = container.Find(key.ToString()) as SkillTable;

            if (null == entity)
            {
                string msg = $"Invalid Key.. Skill.csv == Key:{key}";
                GameManager.Log(msg, "red");
            }

            return entity;
        }

        return null;
    }

    public static bool IsContainsKey(uint nKey)
    {
        if (pool.ContainsKey(ENTITY_TYPE.SkillTable.TypeName()))
        {
            EntityContainer container = pool[ENTITY_TYPE.SkillTable.TypeName()];
            return container.ContainsKey(nKey.ToString());
        }
        return false;
    }

    public static List<SkillTable> GetList()
    {
        if (pool.ContainsKey(ENTITY_TYPE.SkillTable.TypeName()))
        {
            EntityContainer container = pool[ENTITY_TYPE.SkillTable.TypeName()];
            return container.list.ConvertAll(each => { return each as SkillTable; });
        }

        return null;
    }

    public override void OnCreateByDataBase(int fieldid, DataBase database)
    {
        base.OnCreateByDataBase(fieldid, database);
        base.SetKey(string.Format("{0}", PrimaryKey));
    }
}
