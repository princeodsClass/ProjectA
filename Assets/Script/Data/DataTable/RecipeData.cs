using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class RecipeTable : GameEntityData
{
    public static RecipeTable GetData(uint key)
    {
        if (pool.ContainsKey(ENTITY_TYPE.RecipeTable.TypeName()))
        {
            EntityContainer container = pool[ENTITY_TYPE.RecipeTable.TypeName()];
            RecipeTable entity = container.Find(key.ToString()) as RecipeTable;

            if (null == entity)
            {
                string msg = $"Invalid Key.. Recipe.csv == Key:{key}";
                GameManager.Log(msg, "red");
            }

            return entity;
        }

        return null;
    }

    public static bool IsContainsKey(uint nKey)
    {
        if (pool.ContainsKey(ENTITY_TYPE.RecipeTable.TypeName()))
        {
            EntityContainer container = pool[ENTITY_TYPE.RecipeTable.TypeName()];
            return container.ContainsKey(nKey.ToString());
        }
        return false;
    }

    public static List<RecipeTable> GetList()
    {
        if (pool.ContainsKey(ENTITY_TYPE.RecipeTable.TypeName()))
        {
            EntityContainer container = pool[ENTITY_TYPE.RecipeTable.TypeName()];
            return container.list.ConvertAll(each => { return each as RecipeTable; });
        }

        return null;
    }

    public static List<RecipeTable> GetCategory(int category)
    {
        List<RecipeTable> returnValue = new List<RecipeTable>();

        foreach (RecipeTable recipe in GetList())
        {
            if (recipe.Category == category)
                returnValue.Add(recipe);
        }

        return returnValue;
    }

    public static List<RecipeTable> GetType(int category, int type)
    {
        List<RecipeTable> returnValue = new List<RecipeTable>();

        foreach (RecipeTable recipe in GetCategory(category))
        {
            if (recipe.Type == type)
                returnValue.Add(recipe);
        }

        return returnValue;
    }

    public override void OnCreateByDataBase(int fieldid, DataBase database)
    {
        base.OnCreateByDataBase(fieldid, database);
        base.SetKey(string.Format("{0}", PrimaryKey));

        //GameManager.Log($"Box : {fieldid} /  {PrimaryKey}  / {NameKey} / {DescKey}", "green");
    }
}
