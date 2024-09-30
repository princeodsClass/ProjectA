using UnityEngine;
using System.Collections;
using System.Collections.Generic;

#if (NET_4_6 || !NET_STANDARD_2_0)
using System.Runtime.Remoting;
#endif

/// <summary>
/// game entity pool
/// </summary>
public partial class Entity
{	

#region pool
	/// <summary>
	/// static entity pool
	/// key : entity type [ type ]
	/// value : key : entity unique key [ key ]
	/// 		value : entity instance [ instance ]
	/// </summary>
	static protected Dictionary<string, EntityContainer> pool = new Dictionary<string, EntityContainer>();

	/// <summary>
	/// 
	/// </summary>
	/// <returns>The agent.</returns>
	static protected GameAgent CreateAgent(Entity entity)
	{
		GameAgent en = null;
		
		try
		{
#if (NET_4_6 || !NET_STANDARD_2_0)
			ObjectHandle handle = System.Activator.CreateInstance(null, entity.agentClassName);
			en = handle.Unwrap() as GameAgent;
			en.SetEntity(entity);

			Regist(en);
#endif
		}
		catch (System.Exception e)
		{
			GameManager.Log(e.ToString(), "red");
		}
		
		return en;
	}

	/// <summary>
	/// 
	/// </summary>
	/// <returns>The by object.</returns>
	static protected Entity CreateByObject(string type, object p)
	{
		Entity en = null;
		
		try
		{
#if (NET_4_6 || !NET_STANDARD_2_0)
			ObjectHandle handle = System.Activator.CreateInstance(null, type);
			en = handle.Unwrap() as Entity;
			en.OnCreate(p);
			//en.SetKey(GetEmptyKey(type));
			
			Regist(en);
#endif
		}
		catch (System.Exception e)
		{
			GameManager.Log(e.ToString(), "red");
		}
		
		return en;
	}

	/// <summary>
	/// create entity : using new entity / edit mode
	/// </summary>
	static protected Entity CreateByEmpty(string type)
	{
		Entity en = null;

		try
		{
#if (NET_4_6 || !NET_STANDARD_2_0)
			ObjectHandle handle = System.Activator.CreateInstance(null, type);
			en = handle.Unwrap() as Entity;
			en.OnCreateByEmpty();
			//en.SetKey(GetEmptyKey(type));

			Regist(en);
#endif
		}
		catch (System.Exception e)
		{
			GameManager.Log(e.ToString(), "red");
		}

		return en;
	}

	/// <summary>
	/// 
	/// </summary>
	/// <returns>The data base.</returns>
	static public void LoadDataBase(string fileName, string type)
	{
		//	
		if (DataBase.IsExistDB (type) != null)
			return; 


		// load
		DataBase database = DataBase.GetDB (type);
		DataBaseReader.LoadStringToDataTable (fileName, database);

		///*
		GameEntityData data = null;

		for(int i=0,ii=database.row; ii>i; ++i) 
		{
			if(database.table[i,0] == null )
				break;
			if(string.IsNullOrEmpty(database.table[i,0]))
				break;

			try
			{
#if (NET_4_6 || !NET_STANDARD_2_0)
				ObjectHandle handle = System.Activator.CreateInstance(null, type);
				data = handle.Unwrap() as GameEntityData;
				data.OnCreateByDataBase(i, database);

				Regist(data);
#endif
			}
			catch (System.Exception e)
			{
				GameManager.Log(e.ToString(), "red");
			}
		}

		//GameResourceManager.Singleton.LoadedToDataBase++;
	}

	/// <summary>
	/// create entity : using load / ingame
	/// </summary>
	/// <returns>The by json.</returns>
	/// <param name="type">Type.</param>
	/// <param name="json">Json.</param>
	static protected Entity CreateByJson(string type, string json)
	{
		Entity en = null;
		
		try
		{
#if (NET_4_6 || !NET_STANDARD_2_0)
			ObjectHandle handle = System.Activator.CreateInstance(null, type);
			en = handle.Unwrap() as Entity;
			en.OnCreateByJson(json);
	

			Regist(en);
#endif
		}
		catch (System.Exception e)
		{
			GameManager.Log(e.ToString(), "red");
		}
		
		return en;
	}

	/// <summary>
	/// static regist game entity
	/// </summary>
	/// <param name="en"></param>
	static protected void Regist(Entity en)
	{
		string type = en.GetType ();

		if (pool.ContainsKey(type))
		{
			pool[type].Regist(en);
		}
		else
		{
			pool.Add(type, new EntityContainer(type));
			pool[type].Regist(en);
		}

		//Debug.Log(string.Format("regist entity type : {0}, key : {1}", en.GetType(), en.key));
	}
	
	/// <summary>
	/// static remove game entity
	/// </summary>
	/// <param name="en"></param>
	static protected void Remove(Entity en)
	{
		if (pool.ContainsKey(en.GetType()))
		{
			GameManager.Log(string.Format("remove entity type : {0}, key : {1}", en.GetType(), en.key));

			pool[en.GetType()].Pop(en.key);
			en.OnRelease();
			en = null;
		}
	}

	/// <summary>
	/// 
	/// </summary>
	/// <param name="key">Key.</param>
	static protected void Remove(string key)
	{
		Entity en = null;

		using (Dictionary<string, EntityContainer>.Enumerator e = pool.GetEnumerator()) {

			while(e.MoveNext())
			{
				EntityContainer container =  e.Current.Value;
				en = container.Find(key);

				if(en != null){

					container.Pop(en.key);

					en.OnRelease();
					en = null;

					break;
				}
			}
		}
	}

	/// <summary>
	/// 
	/// </summary>
	/// <param name="type">Type.</param>
	static protected EntityContainer GetContainer(string type)
	{
		if (pool.ContainsKey(type))
		{
			return pool[type];
		}
		
		return null;
	}

	/// <summary>
	/// 
	/// </summary>
	static protected Entity Get(string type, string key)
	{
		if (pool.ContainsKey(type))
		{
			return pool[type][key];
		}
		
		return null;
	}

	/// <summary>
	/// 
	/// </summary>
	/// <param name="key">Key.</param>
	static protected Entity Get(string key)
	{
		using (Dictionary<string, EntityContainer>.Enumerator e = pool.GetEnumerator()) {
			
			while (e.MoveNext()) {

				EntityContainer container =  e.Current.Value;
				Entity en = container.Find(key);
				if(en != null)
				{
					return en;
				}
			}
		}

		return null;
	}

	/// <summary>
	/// 
	/// </summary>
	/// <returns>The count.</returns>
	/// <param name="type">Type.</param>
	static protected int GetCount(string type)
	{
		if (pool.ContainsKey (type))
			return pool[type].count;

		return 0;
	}
	#endregion
}

