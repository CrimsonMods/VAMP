using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using VAMP.Attributes;
using UnityEngine;

namespace VAMP.Systems
{
    public class FileWatcherSystem
    {
        private static readonly Dictionary<string, FileInfo> _trackedFiles = new Dictionary<string, FileInfo>();
        private static readonly Dictionary<string, (Type ownerType, string callbackMethod)> _fileCallbacks = new Dictionary<string, (Type, string)>();
        private const float CHECK_INTERVAL = 60f; // 1 minute in seconds

        public static void Initialize()
        {
            Plugin.LogInstance.LogInfo("Initializing FileWatcherSystem");
            
            // Start the file checking coroutine using the Core system
            Core.StartCoroutine(FileWatcherCoroutine());
        }

        private static IEnumerator FileWatcherCoroutine()
        {
            yield return new WaitForSeconds(15);

            ScanForAttributedFiles();
            
            while (true)
            {
                yield return new WaitForSeconds(CHECK_INTERVAL);
                CheckForChanges();
            }
        }

        public static void ScanForAttributedFiles()
        { 
            // Get all loaded assemblies
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            
            // Filter to only include mod assemblies and avoid game/system assemblies
            var modAssemblies = assemblies.Where(assembly => {
                string name = assembly.GetName().Name;
                
                // Skip all Unity, system, and IL2CPP related assemblies
                if (name.StartsWith("System.") || 
                    name.StartsWith("UnityEngine") ||
                    name.StartsWith("Unity.") ||
                    name.StartsWith("mscorlib") ||
                    name.StartsWith("Il2Cpp") ||
                    name.StartsWith("ProjectM") ||
                    name.StartsWith("netstandard") ||
                    name.StartsWith("Rukhanka") ||
                    name.StartsWith("Stunlock") ||
                    name == "__Generated" ||
                    name.Contains("CodeGen") ||
                    name.Contains("Cecil"))
                {
                    return false;
                }
                
                // Include BepInEx and potential mod assemblies
                if (name.StartsWith("BepInEx") || 
                    name == "VAMP" ||
                    name.Contains("Plugin") ||
                    name.Contains("Mod"))
                {
                    return true;
                }
                
                // For any other assembly, check if it references BepInEx (a good indicator it's a mod)
                try {
                    return assembly.GetReferencedAssemblies()
                        .Any(reference => reference.Name.StartsWith("BepInEx"));
                }
                catch {
                    return false;
                }
            });

            foreach (Assembly assembly in modAssemblies)
            {
                try
                {
                    Plugin.LogInstance.LogDebug($"Scanning assembly: {assembly.GetName().Name}");
                    
                    foreach (Type type in assembly.GetTypes())
                    {
                        // Skip types that are not likely to contain config paths
                        if (type.Namespace != null && 
                            (type.Namespace.StartsWith("System") || 
                             type.Namespace.StartsWith("Unity")))
                        {
                            continue;
                        }
                        
                        // Check fields
                        foreach (FieldInfo field in type.GetFields(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
                        {
                            FileReloadAttribute attribute = field.GetCustomAttribute<FileReloadAttribute>();
                            if (attribute != null && field.FieldType == typeof(string))
                            {
                                string filePath = (string)field.GetValue(null);
                                if (!string.IsNullOrEmpty(filePath))
                                {
                                    RegisterFileForTracking(filePath, type, attribute.CallbackMethodName, field.Name);
                                }
                            }
                        }

                        // Check properties
                        foreach (PropertyInfo property in type.GetProperties(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
                        {
                            FileReloadAttribute attribute = property.GetCustomAttribute<FileReloadAttribute>();
                            if (attribute != null && property.PropertyType == typeof(string) && property.GetGetMethod(true) != null)
                            {
                                string filePath = (string)property.GetGetMethod(true).Invoke(null, null);
                                if (!string.IsNullOrEmpty(filePath))
                                {
                                    RegisterFileForTracking(filePath, type, attribute.CallbackMethodName, property.Name);
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Plugin.LogInstance.LogWarning($"Error scanning assembly {assembly.GetName().Name}: {ex.Message}");
                }
            }

            Plugin.LogInstance.LogInfo($"Found {_trackedFiles.Count} files to watch for changes");
        }

        public static void RegisterFileForTracking(string filePath, Type ownerType, string callbackMethodName = null, string memberName = null)
        {
            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
            {
                Plugin.LogInstance.LogWarning($"Cannot register non-existent file for tracking: {filePath}");
                return;
            }

            try
            {
                FileInfo fileInfo = new FileInfo(filePath);
                _trackedFiles[filePath] = fileInfo;
                
                if (ownerType != null)
                {
                    if (string.IsNullOrEmpty(callbackMethodName))
                    {
                        if (!string.IsNullOrEmpty(memberName))
                        {
                            callbackMethodName = "Reload" + memberName;
                        }
                        else
                        {
                            callbackMethodName = "Reload" + Path.GetFileNameWithoutExtension(filePath);
                        }
                    }
                    
                    _fileCallbacks[filePath] = (ownerType, callbackMethodName);
                    Plugin.LogInstance.LogInfo($"Registered file for tracking: {filePath} with callback {ownerType.Name}.{callbackMethodName}");
                }
                else
                {
                    Plugin.LogInstance.LogInfo($"Registered file for tracking (no callback): {filePath}");
                }
            }
            catch (Exception ex)
            {
                Plugin.LogInstance.LogError($"Error registering file {filePath}: {ex.Message}");
            }
        }

        public static void CheckForChanges()
        {
            Plugin.LogInstance.LogDebug("Checking for file changes...");
            
            foreach (var entry in _trackedFiles.ToList())
            {
                string filePath = entry.Key;
                FileInfo oldInfo = entry.Value;
                
                if (!File.Exists(filePath))
                {
                    Plugin.LogInstance.LogWarning($"Tracked file no longer exists: {filePath}");
                    continue;
                }
                
                try
                {
                    FileInfo newInfo = new FileInfo(filePath);
                    newInfo.Refresh();
                    
                    if (newInfo.LastWriteTime != oldInfo.LastWriteTime)
                    {
                        _trackedFiles[filePath] = newInfo;
                        Plugin.LogInstance.LogInfo($"Detected change in file: {filePath}");
                        
                        if (_fileCallbacks.TryGetValue(filePath, out var callback))
                        {
                            InvokeReloadCallback(callback.ownerType, callback.callbackMethod);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Plugin.LogInstance.LogError($"Error checking file {filePath}: {ex.Message}");
                }
            }
        }

        private static void InvokeReloadCallback(Type ownerType, string methodName)
        {
            try
            {
                MethodInfo method = ownerType.GetMethod(methodName, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
                if (method != null)
                {
                    Plugin.LogInstance.LogInfo($"Invoking reload callback: {ownerType.Name}.{methodName}()");
                    method.Invoke(null, null);
                }
                else
                {
                    Plugin.LogInstance.LogError($"Reload callback method not found: {ownerType.Name}.{methodName}()");
                }
            }
            catch (Exception ex)
            {
                Plugin.LogInstance.LogError($"Error invoking reload callback {ownerType.Name}.{methodName}: {ex.Message}");
            }
        }
    }
}