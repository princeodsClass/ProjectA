using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class ChapterTable : GameEntityData
{
    public static ChapterTable GetData(uint key)
    {
        if (pool.ContainsKey(ENTITY_TYPE.ChapterTable.TypeName()))
        {
            EntityContainer container = pool[ENTITY_TYPE.ChapterTable.TypeName()];
            ChapterTable entity = container.Find(key.ToString()) as ChapterTable;

            if (null == entity)
            {
                string msg = $"Invalid Key.. Chapter.csv == Key:{key}";
                GameManager.Log(msg, "red");
            }

            return entity;
        }

        return null;
    }

    public static bool IsContainsKey(uint nKey)
    {
        if (pool.ContainsKey(ENTITY_TYPE.ChapterTable.TypeName()))
        {
            EntityContainer container = pool[ENTITY_TYPE.ChapterTable.TypeName()];
            return container.ContainsKey(nKey.ToString());
        }
        return false;
    }

    public static List<ChapterTable> GetList()
    {
        if (pool.ContainsKey(ENTITY_TYPE.ChapterTable.TypeName()))
        {
            EntityContainer container = pool[ENTITY_TYPE.ChapterTable.TypeName()];
            return container.list.ConvertAll(each => { return each as ChapterTable; });
        }

        return null;
    }

    public static List<ChapterTable> GetEpisodeGroup(int episode)
    {
        List<ChapterTable> returnValue = new List<ChapterTable>();

        foreach (ChapterTable chapter in GetList())
        {
            if (chapter.Episode == episode)
                returnValue.Add(chapter);
        }

        return returnValue;
    }

    public static List<ChapterTable> GetEpisodeRewards(int episode)
    {
        List<ChapterTable> returnValue = new List<ChapterTable>();

        foreach (ChapterTable chapter in GetList())
        {
            if (( chapter.Episode == episode && chapter.Order < GetMax(episode) ) ||
                ( chapter.Episode == episode - 1 && chapter.Order == GetMax(episode - 1) ))
                returnValue.Add(chapter);
        }

        return returnValue;
    }

    public static int GetMax(int episode = 0)
    {
        List<ChapterTable> chl = episode == 0 ? GetList() : GetEpisodeGroup(episode);
        return chl.Max(ch => ch.Order);
    }

    public override void OnCreateByDataBase(int fieldid, DataBase database)
    {
        base.OnCreateByDataBase(fieldid, database);
        base.SetKey(string.Format("{0}", PrimaryKey));
    }
}
