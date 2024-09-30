

/// <summary>
/// 
/// </summary>
public class GameAgent : Entity
{
	/// <summary>
	/// 
	/// </summary>
	/// <value>The entity.</value>
	public Entity entity { private set; get; }

	/// <summary>
	/// 
	/// </summary>
	/// <param name="key"></param>
	protected GameAgent() : base() 
	{
	}

	/// <summary>
	/// 
	/// </summary>
	/// <param name="entity">Entity.</param>
	public void SetEntity(Entity entity)
	{
		this.entity = entity;

		//	same entity key
		SetKey (entity.key);
	}
}