using System.Collections.Generic;
using Stunlock.Core;

namespace VAMP.Data;

public static class VBloods
{
    public static Dictionary<PrefabGUID, (string Long, string Short)> PrefabToNames = new()
    {
        {Prefabs.CHAR_Bandit_Frostarrow_VBlood, ("Keely the Frost Archer", "Keely")},
        {Prefabs.CHAR_Bandit_Foreman_VBlood, ("Rufus the Foreman", "Rufus")},
        {Prefabs.CHAR_Bandit_StoneBreaker_VBlood, ("Errol the Stonebreaker", "Errol")},
        {Prefabs.CHAR_Bandit_Chaosarrow_VBlood, ("Lidia the Chaos Archer", "Lidia")},
        {Prefabs.CHAR_Undead_BishopOfDeath_VBlood, ("Goreswine the Ravager", "Goreswine")},
        {Prefabs.CHAR_Bandit_Stalker_VBlood, ("Grayson the Armourer", "Grayson")},
        {Prefabs.CHAR_Vermin_DireRat_VBlood, ("Nibbles the Putrid Rat", "Nibbles")},
        {Prefabs.CHAR_Bandit_Bomber_VBlood, ("Clive the Firestarter", "Clive")},
        {Prefabs.CHAR_Undead_Priest_VBlood, ("Nicholaus the Fallen", "Nicholaus")},
        {Prefabs.CHAR_Bandit_Tourok_VBlood, ("Quincey the Bandit King", "Quincey")},
        {Prefabs.CHAR_Villager_Tailor_VBlood, ("Beatrice the Tailor", "Beatrice")},
        {Prefabs.CHAR_Militia_Guard_VBlood, ("Vincent the Frostbringer", "Vincent")},
        {Prefabs.CHAR_VHunter_Leader_VBlood, ("Tristan the Vampire Hunter", "Tristan")},
        {Prefabs.CHAR_Undead_BishopOfShadows_VBlood, ("Leandra the Shadow Priestess", "Leandra")},
        {Prefabs.CHAR_Geomancer_Human_VBlood, ("Terah the Geomancer", "Terah")},
        {Prefabs.CHAR_Militia_Longbowman_LightArrow_Vblood, ("Meredith the Bright Archer", "Meredith")},
        {Prefabs.CHAR_Wendigo_VBlood, ("Frostmaw the Mountain Terror", "Frostmaw")},
        {Prefabs.CHAR_Militia_Leader_VBlood, ("Octavian the Militia Captain", "Octavian")},
        {Prefabs.CHAR_Militia_BishopOfDunley_VBlood, ("Raziel the Shepherd", "Raziel")},
        {Prefabs.CHAR_Spider_Queen_VBlood, ("Ungora the Spider Queen", "Ungora")},
        {Prefabs.CHAR_Cursed_ToadKing_VBlood, ("Albert the Duke of Balaton", "Albert")},
        {Prefabs.CHAR_VHunter_Jade_VBlood, ("Jade the Vampire Hunter", "Jade")},
        {Prefabs.CHAR_Undead_ZealousCultist_VBlood, ("Foulrot the Soultaker", "Foulrot")},
        {Prefabs.CHAR_WerewolfChieftain_Human, ("Willfred the Werewolf Chief", "Willfred")},
        {Prefabs.CHAR_ArchMage_VBlood, ("Mairwyn the Elementalist", "Mairwyn")},
        {Prefabs.CHAR_Winter_Yeti_VBlood, ("Terrorclaw the Ogre", "Terrorclaw")},
        {Prefabs.CHAR_Harpy_Matriarch_VBlood, ("Morian the Stormwing Matriarch", "Morian")},
        {Prefabs.CHAR_Cursed_Witch_VBlood, ("Matka the Curse Weaver", "Matka")},
        {Prefabs.CHAR_BatVampire_VBlood, ("Lord Styx the Night Champion", "Styx")},
        {Prefabs.CHAR_Cursed_MountainBeast_VBlood, ("Gorecrusher the Behemoth", "Gorecrusher")},
        {Prefabs.CHAR_Manticore_VBlood, ("Talzur the Winged Horror", "Talzur")},
        //{Prefabs.CHAR_Bandit_GraveDigger_VBlood_UNUSED, ("Boris the Gravedigger", "Boris")},
        {Prefabs.CHAR_Bandit_Leader_VBlood_UNUSED, ("Quincey the Marauder", "Quincey")},
        {Prefabs.CHAR_ChurchOfLight_Cardinal_VBlood, ("Azariel the Sunbringer", "Azariel")},
        {Prefabs.CHAR_ChurchOfLight_Overseer_VBlood, ("Sir Magnus the Overseer", "Sir Magnus")},
        {Prefabs.CHAR_ChurchOfLight_Paladin_VBlood, ("Solarus the Immaculate", "Solarus")},
        {Prefabs.CHAR_ChurchOfLight_Sommelier_VBlood, ("Baron du Bouchon the Sommelier", "Baron")},
        {Prefabs.CHAR_Forest_Bear_Dire_Vblood, ("Kodia the Ferocious Bear", "Kodia")},
        {Prefabs.CHAR_Forest_Wolf_VBlood, ("Alpha the White Wolf", "Alpha")},
        {Prefabs.CHAR_Gloomrot_Iva_VBlood, ("Ziva the Engineer", "Ziva")},
        {Prefabs.CHAR_Gloomrot_Monster_VBlood, ("Adam the Firstborn", "Adam")},
        {Prefabs.CHAR_Gloomrot_Purifier_VBlood, ("Angram the Purifier", "Angram")},
        {Prefabs.CHAR_Gloomrot_RailgunSergeant_VBlood, ("Voltatia the Power Master", "Voltatia")},
        {Prefabs.CHAR_Gloomrot_TheProfessor_VBlood, ("Henry Blackbrew the Doctor", "Henry")},
        {Prefabs.CHAR_Gloomrot_Voltage_VBlood, ("Domina the Blade Dancer", "Domina")},
        {Prefabs.CHAR_Militia_Glassblower_VBlood, ("Grethel the Glassblower", "Grethel")},
        {Prefabs.CHAR_Militia_Hound_VBlood, ("Brutus the Watcher", "Brutus")},
        {Prefabs.CHAR_Militia_HoundMaster_VBlood, ("Boyo", "Boyo")},
        {Prefabs.CHAR_Militia_Nun_VBlood, ("Christina the Sun Priestess", "Christina")},
        {Prefabs.CHAR_Militia_Scribe_VBlood, ("Maja the Dark Savant", "Maja")},
        {Prefabs.CHAR_Poloma_VBlood, ("Polora the Feywalker", "Polora")},
        {Prefabs.CHAR_Undead_CursedSmith_VBlood, ("Cyril the Cursed Smith", "Cyril")},
        {Prefabs.CHAR_Undead_Infiltrator_VBlood, ("Bane the Shadowblade", "Bane")},
        {Prefabs.CHAR_Undead_Leader_Vblood, ("Kriig the Undead General", "Kriig")},
        {Prefabs.CHAR_Villager_CursedWanderer_VBlood, ("Ben the Old Wanderer", "Ben")},
        {Prefabs.CHAR_Bandit_Fisherman_VBlood, ("Finn the Fisherman", "Finn")},
        {Prefabs.CHAR_VHunter_CastleMan, ("Simon Belmont the Vampire Hunter", "Simon")},
        {Prefabs.CHAR_Vampire_BloodKnight_VBlood, ("General Valencia the Depraved", "Valencia")},
        {Prefabs.CHAR_Vampire_Dracula_VBlood, ("Dracula the Immortal King", "Dracula")},
        {Prefabs.CHAR_Vampire_HighLord_VBlood, ("General Cassius the Betrayer", "Cassius")},
        {Prefabs.CHAR_Vampire_IceRanger_VBlood, ("General Elena the Hollow", "Elena")},
        {Prefabs.CHAR_Militia_Fabian_VBlood, ("Sir Erwin the Gallant Cavalier", "Erwin")},
        {Prefabs.CHAR_Undead_ArenaChampion_VBlood, ("Gaius The Cursed Champion", "Gaius")},
        {Prefabs.CHAR_Blackfang_Livith_VBlood, ("Jakira the Shadow Huntress", "Jakira")},
        {Prefabs.CHAR_Blackfang_CarverBoss_VBlood, ("Stavros the Carver", "Stavros")},
        {Prefabs.CHAR_Blackfang_Valyr_VBlood, ("Dantos the Forgebinder", "Dantos")},
        {Prefabs.CHAR_Blackfang_Morgana_VBlood, ("Megara the Serpent Queen", "Megara")},
        {Prefabs.CHAR_Blackfang_Lucie_VBlood, ("Lucile the Venom Alchemist", "Lucile")},
    };

    /// <summary>
    /// Converts a PrefabGUID to its full V Blood name
    /// </summary>
    /// <param name="prefab">The PrefabGUID to convert</param>
    /// <returns>The full name of the V Blood</returns>
    public static string ToString(PrefabGUID prefab)
    {
        if (PrefabToNames.TryGetValue(prefab, out var names))
            return names.Long;
        return "VBlood Unknown";
    }

    /// <summary>
    /// Converts a GUID integer to its full V Blood name
    /// </summary>
    /// <param name="guid">The GUID integer to convert</param>
    /// <returns>The full name of the V Blood</returns>
    public static string ToString(int guid)
    {
        return ToString(new PrefabGUID(guid));
    }

    /// <summary>
    /// Converts a PrefabGUID to its short V Blood name
    /// </summary>
    /// <param name="prefab">The PrefabGUID to convert</param>
    /// <returns>The short name of the V Blood</returns>
    public static string ToShortString(PrefabGUID prefab)
    {
        if (PrefabToNames.TryGetValue(prefab, out var names))
            return names.Short;
        return "VBlood Unknown";
    }

    /// <summary>
    /// Converts a GUID integer to its short V Blood name
    /// </summary>
    /// <param name="guid">The GUID integer to convert</param>
    /// <returns>The short name of the V Blood</returns>
    public static string ToShortString(int guid)
    {
        return ToShortString(new PrefabGUID(guid));
    }}