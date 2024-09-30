using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// entity container
/// </summary>
public class EntityContainer : Entity
{
	/// <summary>
	/// 
	/// </summary>
	Dictionary<string, Entity> dictionary = new Dictionary<string, Entity>();

	/// <summary>
	/// 
	/// </summary>
	CallbackCoroutineMethod updateMethod;

	/// <summary>
	/// 
	/// </summary>
	public List<Entity> list { private set; get; }

	/// <summary>
	/// 
	/// </summary>
	/// <param name="key">Key.</param>
	public Entity this [string key] {
		get { return Find (key); }
	}

	/// <summary>
	/// 
	/// </summary>
	/// <param name="key">Key.</param>
	public Entity this [int index] {
		get
		{
			return list[index]; 
		}
	}


	/// <summary>
	/// 
	/// </summary>
	/// <value>The count.</value>
	public int count { 
		get 
		{
			return list.Count;
		}
	}

	/// <summary>
	/// 
	/// </summary>
	bool run = false;

	/// <summary>
	/// 
	/// </summary>
	/// <value><c>true</c> if update; otherwise, <c>false</c>.</value>
	public bool update
	{
		set 
		{
			if(updateMethod != null)
			{
				if(value) {
					//StartCoroutine(updateMethod());
				}
				else {
					//StartCoroutine(updateMethod());
				}
			}

			run = value;
		}
	}

	/// <summary>
	/// 
	/// </summary>
	float delay = 0f;

	/// <summary>
	/// 
	/// </summary>
	/// <param name="type">Type.</param>
	public EntityContainer(string key, CallbackCoroutineMethod method = null, float delay = 0f) : base()
	{
		this.key = key;
		this.update = false;
		this.delay = delay;
		this.list = new List<Entity> ();

		//	set default
		if (method == null) {
			if (this.delay == 0f) {
				this.updateMethod = OnUpdateToTick;
			}
			else {
				this.updateMethod = OnUpdateToWaitForSeconds;
			}
		} else {
			this.updateMethod = method;
		}
	}

	/// <summary>
	/// 
	/// </summary>
	public override void OnRelease()
	{
		base.OnRelease ();
	}

	/// <summary>
	/// 
	/// </summary>
	IEnumerator OnUpdateToTick(params object[] p)
	{
		float delta = 0f;

		while (run) {

			yield return null;

			OnUpdate(delta);
		}
	}

	/// <summary>
	/// 
	/// </summary>
	IEnumerator OnUpdateToWaitForSeconds(params object[] p)
	{
		float delta = 0f;

		while (run) {

			yield return YieldInstructionCache.WaitForSeconds(delay);
			
			OnUpdate(delta);
		}

		yield return null;
	}

	/// <summary>
	/// 
	/// </summary>
	public override void OnUpdate(float delta)
	{
		base.OnUpdate(delta);
		for (int i=0,ii=list.Count; ii>i; ++i) {
			list[i].OnUpdate(delta);
		}
	}

	/// <summary>
	/// 
	/// </summary>
	/// <param name="key">Key.</param>
	public Entity Find(string key)
	{
		if (dictionary.ContainsKey (key))
			return dictionary [key];

		return null;
	}


	public bool ContainsKey(string key)
	{
		return dictionary.ContainsKey(key);
	}

	/// <summary>
	/// 
	/// </summary>
	/// <param name="key">Enumerator.</param>
	public Dictionary<string, Entity>.Enumerator GetEnumerator()
	{
		return dictionary.GetEnumerator();
	}

	/// <summary>
	/// 
	/// </summary>
	/// <returns>The to condition.</returns>
	public Entity FindToCondition()
	{
		return null;
	}

	/// <summary>
	/// 
	/// </summary>
	public Entity Pop(string key)
	{
		Entity en = Find (key);
		if (en != null) {

			Remove (en);

			return en;
		}

		return null;
	}

	/// <summary>
	/// 
	/// </summary>
	/// <param name="entity">Entity.</param>
	public new void Regist(Entity entity)
	{
		if (dictionary.ContainsKey (entity.key))
			return;

		dictionary.Add (entity.key, entity);
		list.Add (entity);
	}

	/// <summary>
	/// 
	/// </summary>
	public void RemoveAll()
	{
		dictionary.Clear();
		list.Clear();
	}
	
	/// <summary>
	/// 
	/// </summary>
	/// <param name="key">Key.</param>
	public new void Remove(string key)
	{
		if (dictionary.ContainsKey (key)) {

			Entity en = dictionary[key];
			list.Remove(en);
			dictionary.Remove(key);
		}
	}

	/// <summary>
	/// 
	/// </summary>
	/// <param name="entity">Entity.</param>
	private new void Remove(Entity entity)
	{
		Remove (entity.key);
	}
}