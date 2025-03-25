namespace  VAMP.Data;

/// <summary>
/// Represents the alignment or allegiance of a territory.
/// </summary>
public enum TerritoryAlignment
{
	/// <summary>
	/// Territory is aligned with the player or their allies.
	/// </summary>
	Friendly,

	/// <summary>
	/// Territory is controlled by hostile forces.
	/// </summary>
	Enemy,

	/// <summary>
	/// Territory has no specific alignment to any faction.
	/// </summary>
	Neutral,

	/// <summary>
	/// Territory has not been assigned an alignment.
	/// </summary>
	None
}