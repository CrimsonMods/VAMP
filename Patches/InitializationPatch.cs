using HarmonyLib;
using ProjectM;
using ProjectM.Network;
using Unity.Entities;

namespace VAMP.Patches;

[HarmonyPatch(typeof(SpawnTeamSystem_OnPersistenceLoad), nameof(SpawnTeamSystem_OnPersistenceLoad.OnUpdate))]
public static class InitializationPatch
{
    [HarmonyPostfix]
    public static void OneShot_AfterLoad_InitializationPatch()
    {
        Core.Initialize();
        Plugin.Harmony.Unpatch(typeof(SpawnTeamSystem_OnPersistenceLoad).GetMethod("OnUpdate"), typeof(InitializationPatch).GetMethod("OneShot_AfterLoad_InitializationPatch"));
    }
}

[HarmonyBefore("gg.deca.VampireCommandFramework")]
[HarmonyPatch(typeof(ServerBootstrapSystem), nameof(ServerBootstrapSystem.SendRevealedMapData))]
public static class RevealedMapDataPatch
{
	public static void Prefix(ServerBootstrapSystem __instance, Entity userEntity, User user)
	{
		if (!Core.hasInitialized) Core.Initialize();
	}
}