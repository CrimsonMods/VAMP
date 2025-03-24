using System.Collections.Generic;
using ProjectM.Terrain;

namespace VAMP.Data;

public static class WorldRegions
{
	public static Dictionary<WorldRegionType, string> WorldRegionToString = new()
	{
		{ WorldRegionType.CursedForest, "Cursed Forest" },
		{ WorldRegionType.SilverlightHills, "Silverlight Hills" },
		{ WorldRegionType.DunleyFarmlands, "Dunley Farmlands" },
		{ WorldRegionType.HallowedMountains, "Hallowed Mountains" },
		{ WorldRegionType.FarbaneWoods, "Farbane Woods" },
		{ WorldRegionType.Gloomrot_North, "Gloomrot North" },
		{ WorldRegionType.Gloomrot_South, "Gloomrot South" },
		{ WorldRegionType.RuinsOfMortium, "Ruins of Mortium" },
		{ WorldRegionType.StartCave, "Start Cave" },
		{ WorldRegionType.Other, "Unknown Location" },
		{ WorldRegionType.None, "Unknown Location" },
	};
}