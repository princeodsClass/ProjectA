using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NSubstitute.Core;

public partial class StageTable : GameEntityData
{
    public static StageTable GetData(uint key)
    {
        if (pool.ContainsKey(ENTITY_TYPE.StageTable.TypeName()))
        {
            EntityContainer container = pool[ENTITY_TYPE.StageTable.TypeName()];
            StageTable entity = container.Find(key.ToString()) as StageTable;

            if (null == entity)
            {
                string msg = $"Invalid Key.. Chapter.csv == Key:{key}";
                GameManager.Log(msg, "red");
            }

            return entity;
        }

        return null;
    }

    public static StageTable GetData(int episode, int chapter, int stage)
    {
        foreach (StageTable tempStage in GetList())
            if (tempStage.Episode == episode && tempStage.Chapter == chapter && tempStage.Order == stage)
                return tempStage;

        return null;
    }

    public static bool IsContainsKey(uint nKey)
    {
        if (pool.ContainsKey(ENTITY_TYPE.StageTable.TypeName()))
        {
            EntityContainer container = pool[ENTITY_TYPE.StageTable.TypeName()];
            return container.ContainsKey(nKey.ToString());
        }
        return false;
    }

    public static List<StageTable> GetList()
    {
        if (pool.ContainsKey(ENTITY_TYPE.StageTable.TypeName()))
        {
            EntityContainer container = pool[ENTITY_TYPE.StageTable.TypeName()];
            return container.list.ConvertAll(each => { return each as StageTable; });
        }

        return null;
    }

    public static List<StageTable> GetStageGroup(int episode, int chapter)
    {
        List<StageTable> returnValue = new List<StageTable>();

        foreach (StageTable stage in GetList())
        {
            if (stage.Episode == episode && stage.Chapter == chapter)
                returnValue.Add(stage);
        }

        return returnValue;
    }

    public static int GetMax(int episode, int chapter)
    {
        List<StageTable> stage = GetStageGroup(episode, chapter);
        return stage.Max(ch => ch.Order);
    }

    /// <summary>
    /// returnValue[0] : exp
    /// returnValue[1] : passExp
    /// </summary>
    /// <param name="episode"></param>
    /// <param name="chapter"></param>
    /// <param name="stage"></param>
    /// <returns></returns>
    public static int[] GetGainExp(int episode, int chapter, int stage)
    {
        List<StageTable> tar = GetStageGroup(episode, chapter);

        int returnExp = 0;
        int returnPassExp = 0;
        int[] returnValue = new int[2];

        tar.ForEach(st =>
        {
            if (st.Order <= stage)
            {
                returnExp += st.RewardExp;
                returnPassExp += st.RewardPassExp;
            }
        });

        returnValue[0] = returnExp;
        returnValue[1] = returnPassExp;

        return returnValue;
    }

    public override void OnCreateByDataBase(int fieldid, DataBase database)
    {
        base.OnCreateByDataBase(fieldid, database);
        base.SetKey(string.Format("{0}", PrimaryKey));
    }
}
