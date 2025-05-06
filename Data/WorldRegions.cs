using System.Collections.Generic;
using ProjectM.Terrain;

namespace VAMP.Data;

/// <summary>
/// Contains mapping of world region types to their display strings.
/// </summary>
public static class WorldRegions
{
	/// <summary>
	/// Dictionary mapping WorldRegionType enum values to their human-readable string representations.
	/// </summary>
	public static Dictionary<WorldRegionType, (string Long, string Short)> WorldRegionToString = new()
	{
		{ WorldRegionType.CursedForest, ("Cursed Forest", "Forest") },
		{ WorldRegionType.SilverlightHills, ("Silverlight Hills", "Silverlight") },
		{ WorldRegionType.DunleyFarmlands, ("Dunley Farmlands", "Dunley") },
		{ WorldRegionType.HallowedMountains, ("Hallowed Mountains", "Hallowed") },
		{ WorldRegionType.FarbaneWoods, ("Farbane Woods", "Farbane") },
		{ WorldRegionType.Gloomrot_North, ("Gloomrot North", "N. Gloomrot") },
		{ WorldRegionType.Gloomrot_South, ("Gloomrot South", "S. Gloomrot") },
		{ WorldRegionType.RuinsOfMortium, ("Ruins of Mortium", "Mortium") },
		{ WorldRegionType.StartCave, ("Start Cave", "Start Cave") },
		{ WorldRegionType.Strongblade, ("Oakveil Woodlands", "Oakveil") },
		{ WorldRegionType.Other, ("Unknown Location", "Unknown Location") },
		{ WorldRegionType.None, ("Unknown Location", "Unknown Location") },
	};

	/// <summary>
	/// Gets the long display string for the given world region type.
	/// </summary>
	/// <param name="region">The world region type to get the string for.</param>
	/// <returns>The long display string for the region.</returns>
	public static string ToString(WorldRegionType region)
	{
		return WorldRegionToString[region].Long;
	}

	/// <summary>
	/// Gets the short display string for the given world region type.
	/// </summary>
	/// <param name="region">The world region type to get the string for.</param>
	/// <returns>The short display string for the region.</returns>
	public static string ToShortString(WorldRegionType region)
	{
		return WorldRegionToString[region].Short;
	}

	/// <summary>
	/// Extension method that gets the short display string for the world region type.
	/// </summary>
	/// <param name="region">The world region type to get the string for.</param>
	/// <returns>The short display string for the region.</returns>
	public static string ToStringShort(this WorldRegionType region)
	{
		return WorldRegionToString[region].Short;
	}

	/// <summary>
	/// Extension method that gets the long display string for the world region type.
	/// </summary>
	/// <param name="region">The world region type to get the string for.</param>
	/// <returns>The long display string for the region.</returns>
	public static string ToStringLong(this WorldRegionType region)
	{
		return WorldRegionToString[region].Long;
	}}