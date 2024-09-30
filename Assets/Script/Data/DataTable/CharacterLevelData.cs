using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class CharacterLevelTable : GameEntityData
{
    public static CharacterLevelTable GetData(uint key)
    {
        if (pool.ContainsKey(ENTITY_TYPE.CharacterLevelTable.TypeName()))
        {
            EntityContainer container = pool[ENTITY_TYPE.CharacterLevelTable.TypeName()];
            CharacterLevelTable entity = container.Find(key.ToString()) as CharacterLevelTable;

            if (null == entity)
            {
                string msg = $"Invalid Key.. CharacterLevel.csv == Key:{key}";
                GameManager.Log(msg, "red");
            }

            return entity;
        }

        return null;
    }

    public static bool IsContainsKey(uint nKey)
    {
        if (pool.ContainsKey(ENTITY_TYPE.CharacterLevelTable.TypeName()))
        {
            EntityContainer container = pool[ENTITY_TYPE.CharacterLevelTable.TypeName()];
            return container.ContainsKey(nKey.ToString());
        }
        return false;
    }

    public static List<CharacterLevelTable> GetList()
    {
        if (pool.ContainsKey(ENTITY_TYPE.CharacterLevelTable.TypeName()))
        {
            EntityContainer container = pool[ENTITY_TYPE.CharacterLevelTable.TypeName()];
            return container.list.ConvertAll(each => { return each as CharacterLevelTable; });
        }

        return null;
    }

    /// <summary>
    /// 캐릭터 키를 그룹으로 한 그룹
    /// </summary>
    /// <param name="characterKey"></param>
    /// <returns></returns>
    public static List<CharacterLevelTable> GetGroup(uint characterKey)
    {
        List<CharacterLevelTable> returnValue = new List<CharacterLevelTable>();

        foreach (CharacterLevelTable charcter in GetList())
        {
            if (charcter.CharacterKey == characterKey)
                returnValue.Add(charcter);
        }

        return returnValue;
    }

    public override void OnCreateByDataBase(int fieldid, DataBase database)
    {
        base.OnCreateByDataBase(fieldid, database);
        base.SetKey(string.Format("{0}", PrimaryKey));
    }

	#region 추가
	/** 테이블을 반환한다 */
	public static CharacterLevelTable GetTable(uint a_nKey, int a_nLevel)
	{
		var oLevelTableList = GetGroup(a_nKey);
		return oLevelTableList.ExIsValidIdx(a_nLevel) ? oLevelTableList[a_nLevel] : null;
	}
	#endregion // 추가
}
