using System;
using System.IO;
using System.Linq;
using VAMP.Services;
using VAMP.Structs.Settings;

namespace VAMP.Systems;

internal static class ServerWipe
{
    public static void WipeDataDetection()
    {
        if (!Directory.Exists(FilePaths.WipeData))
        {
            Directory.CreateDirectory(FilePaths.WipeData);
            File.WriteAllText(Path.Combine(FilePaths.WipeData, "README.txt"),
            "This directory contains data related to mods maintaining persistence between server restarts. \n\n" +
            "When a server wipe occurs, VAMP will automatically delete the mod files in this folder to ensure a fresh start. \n\n" +
            "If you prefer to manually manage mod files during server wipes, you can disable the AutoWipeDetection setting in VAMP.cfg.");
        }

        if (File.Exists(VAMPSettings.StartDateFile.Value))
        {
            try
            {
                string dateContent = File.ReadAllText(VAMPSettings.StartDateFile.Value).Trim();

                if (DateTime.TryParseExact(dateContent, "MM/dd/yyyy", null, System.Globalization.DateTimeStyles.None, out DateTime fileDate))
                {
                    DateTime today = DateTime.Today;

                    if (fileDate.Date == today && PlayerService.GetAllCachedUsers().Count() == 0)
                    {
                        WipeData();
                    }
                }
                else
                {
                    Plugin.LogInstance.LogWarning($"Could not parse date from StartDateFile: '{dateContent}'");
                }
            }
            catch (Exception ex)
            {
                Plugin.LogInstance.LogError($"Error reading or parsing StartDateFile: {ex.Message}");
            }
        }
        else
        {
            WipeData();
        }
    }

    private static void WipeData()
    {
        if (VAMPSettings.AutoWipeDetection.Value)
        {
            try
            {
                var files = Directory.GetFiles(FilePaths.WipeData)
                    .Where(file => Path.GetFileName(file) != "README.txt");

                foreach (string file in files)
                {
                    File.Delete(file);
                }

                if (files.Any())
                {
                    Plugin.LogInstance.LogInfo("Server wipe detected - cleared mod persistence data");
                }

                Events.OnServerWiped?.Invoke(true);
            }
            catch (Exception ex)
            {
                Plugin.LogInstance.LogError($"Error deleting wipe data files: {ex.Message}");
            }
        }
        else
        {
            Events.OnServerWiped?.Invoke(false);
        }
    }
}