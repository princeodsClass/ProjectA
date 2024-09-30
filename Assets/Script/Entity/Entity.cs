using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class Entity {
	
	public Entity() { }

	/// <summary>
	/// entity key
	/// </summary>
	public virtual string key { set; get; }

	/// <summary>
	/// name of the agent
	/// </summary>
	public string agentClassName { get { return this.ToString() + "Agent"; } }

	/// <summary>
	/// entity active time
	/// </summary>
	public float activeTime { protected set; get; }

	/// <summary>
	/// initialize
	/// </summary>
	public virtual void OnInitialize()
	{

	}

	/// <summary>
	/// release
	/// </summary>
	public virtual void OnRelease()
	{

	}

	/// <summary>
	/// 
	/// </summary>
	/// <param name="time"></param>
	public void SetActiveTime(float time)
	{
		activeTime = time;
	}

	/// <summary>
	/// 
	/// </summary>
	/// <returns></returns>
	public new virtual string GetType()
	{
		return this.ToString();
	}

	/*
	/// <summary>
	/// 
	/// </summary>
	/// <returns></returns>
	public virtual string GetJson()
	{
		return MGR_LITJSON.SerializePasing(this);
	}
	*/

	/// <summary>
	/// create
	/// </summary>
	/// <param name="p"></param>
	public virtual void OnCreate(params object[] p)
	{

	}

	/// <summary>
	/// 
	/// </summary>
	public virtual void OnCreateByEmpty()
	{

	}

	/// <summary>
	/// 
	/// </summary>
	/// <param name="json"></param>
	public virtual void OnCreateByJson(string json)
	{

	}

	/// <summary>
	/// update
	/// </summary>
	/// <param name="deltatime"></param>
	public virtual void OnUpdate(float deltatime)
	{

	}

	/// <summary>
	/// load
	/// </summary>
	/// <param name="p"></param>
	public virtual void OnLoad(params object[] p)
	{

	}

	/// <summary>
	/// 
	/// </summary>
	/// <param name="p"></param>
	/// <returns></returns>
	protected virtual IEnumerator OnLoadToDisk(params object[] p)
	{
		yield return null;
	}

	/// <summary>
	/// 
	/// </summary>
	/// <param name="p"></param>
	protected virtual IEnumerator OnLoadToServer(params object[] p)
	{
		yield return null;
	}

	/// <summary>
	/// 
	/// </summary>
	/// <param name="p"></param>
	public virtual void OnSave(params object[] p)
	{

	}

	/// <summary>
	/// save to disk
	/// </summary>
	/// <param name="p"></param>
	/// <returns></returns>
	protected virtual IEnumerator OnSaveToDisk(params object[] p)
	{
		yield return null;
	}

	/// <summary>
	/// save to server
	/// </summary>
	/// <param name="p"></param>
	/// <returns></returns>
	protected virtual IEnumerator OnSaveToServer(params object[] p)
	{
		yield return null;
	}

	/// <summary>
	/// 
	/// </summary>
	/// <param name="key"></param>
	public void SetKey(string key)
	{
		this.key = key;
	}
}
