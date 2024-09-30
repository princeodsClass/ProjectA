using System;
using System.Collections.Generic;

public partial class MissionDefenceTable : GameEntityData
{
    public static MissionDefenceTable GetData(uint key)
    {
        if (pool.ContainsKey(ENTITY_TYPE.MissionDefenceTable.TypeName()))
        {
            EntityContainer container = pool[ENTITY_TYPE.MissionDefenceTable.TypeName()];
            MissionDefenceTable entity = container.Find(key.ToString()) as MissionDefenceTable;

            if (null == entity)
            {
                string msg = $"Invalid Key.. MissionDefence.csv == Key:{key}";
                GameManager.Log(msg, "red");
            }

            return entity;
        }

        return null;
    }

    public static bool IsContainsKey(uint nKey)
    {
        if (pool.ContainsKey(ENTITY_TYPE.MissionDefenceTable.TypeName()))
        {
            EntityContainer container = pool[ENTITY_TYPE.MissionDefenceTable.TypeName()];
            return container.ContainsKey(nKey.ToString());
        }
        return false;
    }

    public static List<MissionDefenceTable> GetList()
    {
        if (pool.ContainsKey(ENTITY_TYPE.MissionDefenceTable.TypeName()))
        {
            EntityContainer container = pool[ENTITY_TYPE.MissionDefenceTable.TypeName()];
            return container.list.ConvertAll(each => { return each as MissionDefenceTable; });
        }

        return null;
    }

	public static List<MissionDefenceTable> GetDifficulty(int difficulty) 
	{
		List< MissionDefenceTable> list = MissionDefenceTable.GetList();
        List<MissionDefenceTable> returnList = new List<MissionDefenceTable>();

		for(int i = 0; i < list.Count; ++i) {
			if(list[i].Difficulty == difficulty) returnList.Add(list[i]);
		}

		return returnList;
	}

    public override void OnCreateByDataBase(int fieldid, DataBase database)
    {
        base.OnCreateByDataBase(fieldid, database);
        base.SetKey(string.Format("{0}", PrimaryKey));
    }
}
