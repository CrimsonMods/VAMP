using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using HarmonyLib;

namespace VAMP.Systems;

public static class ModProfiler
{
    private static readonly string Directory = "ModProfiler";
    private static readonly Dictionary<Assembly, Dictionary<MethodBase, MethodStats>> assemblyMethodStats = new();
    private static readonly Dictionary<Assembly, CoverageStats> assemblyCoverageStats = new();
    private static readonly Harmony harmony = new Harmony("VAMP.MethodProfiler");

    private class MethodStats
    {
        public long TotalTicks { get; set; }
        public int Count { get; set; }
        public long MinTicks { get; set; } = long.MaxValue;
        public long MaxTicks { get; set; } = long.MinValue;

        public void AddTiming(long ticks)
        {
            TotalTicks += ticks;
            Count++;
            if (ticks < MinTicks) MinTicks = ticks;
            if (ticks > MaxTicks) MaxTicks = ticks;
        }

        public double AverageTicks => Count > 0 ? TotalTicks / (double)Count : 0;
    }

    private class CoverageStats
    {
        public int TotalMethods { get; set; }
        public int PatchedMethods { get; set; }
        public int SkippedMethods { get; set; }
        public int CalledMethods { get; set; }

        public double CoveragePercentage => TotalMethods > 0 ? (CalledMethods / (double)TotalMethods) * 100 : 0;
        public double PatchSuccessRate => TotalMethods > 0 ? (PatchedMethods / (double)TotalMethods) * 100 : 0;
    }

    public static void ProfileAssembly(Assembly assembly)
    {
        // Initialize stats dictionary for this assembly
        if (!assemblyMethodStats.ContainsKey(assembly))
        {
            assemblyMethodStats[assembly] = new Dictionary<MethodBase, MethodStats>();
        }

        if (!assemblyCoverageStats.ContainsKey(assembly))
        {
            assemblyCoverageStats[assembly] = new CoverageStats();
        }

        Plugin.LogInstance.LogInfo($"Starting to profile assembly: {assembly.GetName().Name}");
        int totalMethods = 0;
        int patchedMethods = 0;
        int skippedMethods = 0;

        foreach (var type in assembly.GetTypes())
        {
            // Only skip truly problematic types, be more permissive
            if (type.IsInterface || type.IsGenericTypeDefinition)
                continue;

            // Skip compiler-generated types
            if (type.GetCustomAttribute<System.Runtime.CompilerServices.CompilerGeneratedAttribute>() != null)
                continue;

            foreach (var method in type.GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly))
            {
                totalMethods++;
                
                if (!ShouldProfileMethod(method))
                {
                    skippedMethods++;
                    continue;
                }

                try
                {
                    var prefix = new HarmonyMethod(typeof(MethodWrapper).GetMethod(nameof(MethodWrapper.Prefix)));
                    var postfix = new HarmonyMethod(typeof(MethodWrapper).GetMethod(nameof(MethodWrapper.Postfix)));
                    
                    harmony.Patch(method, prefix, postfix);
                    
                    // Initialize stats for this method
                    assemblyMethodStats[assembly][method] = new MethodStats();
                    patchedMethods++;
                }
                catch (Exception e)
                {
                    Plugin.LogInstance.LogWarning($"Failed to profile method {method.DeclaringType?.FullName}.{method.Name}: {e.Message}");
                    skippedMethods++;
                }
            }
        }

        // Update coverage stats
        var coverageStats = assemblyCoverageStats[assembly];
        coverageStats.TotalMethods = totalMethods;
        coverageStats.PatchedMethods = patchedMethods;
        coverageStats.SkippedMethods = skippedMethods;

        Plugin.LogInstance.LogInfo($"Assembly {assembly.GetName().Name}: Total={totalMethods}, Patched={patchedMethods}, Skipped={skippedMethods}");
    }

    private static bool ShouldProfileType(Type type)
    {
        // Skip abstract types, interfaces, and generic type definitions
        if (type.IsAbstract || type.IsInterface || type.IsGenericTypeDefinition)
            return false;

        // Skip compiler-generated types
        if (type.GetCustomAttribute<System.Runtime.CompilerServices.CompilerGeneratedAttribute>() != null)
            return false;

        // Skip system types that are commonly problematic
        if (IsSystemType(type))
            return false;

        // Skip types from certain namespaces that are known to be problematic
        var typeNamespace = type.Namespace ?? "";
        if (typeNamespace.StartsWith("System.") ||
            typeNamespace.StartsWith("Microsoft.") ||
            typeNamespace.StartsWith("Unity.") ||
            typeNamespace.StartsWith("UnityEngine") ||
            typeNamespace.StartsWith("Il2Cpp"))
            return false;

        return true;
    }

    private static bool IsSystemType(Type type)
    {
        // List of system types that commonly cause issues
        var problematicTypes = new[]
        {
            typeof(object),
            typeof(System.Attribute),
            typeof(System.Exception),
            typeof(System.ValueType),
            typeof(System.Enum),
            typeof(System.Delegate),
            typeof(System.MulticastDelegate),
            typeof(System.Array),
            typeof(System.String),
            typeof(System.Type)
        };

        // Check if this type is or inherits from any problematic type
        foreach (var problematicType in problematicTypes)
        {
            if (type == problematicType || type.IsSubclassOf(problematicType))
                return true;
        }

        return false;
    }

    private static bool ShouldProfileMethod(MethodInfo method)
    {
        // Skip abstract methods
        if (method.IsAbstract)
            return false;

        // Skip generic methods
        if (method.IsGenericMethod || method.IsGenericMethodDefinition)
            return false;

        // Skip methods without a body
        if (method.GetMethodBody() == null)
            return false;

        // Skip compiler-generated methods
        if (method.GetCustomAttribute<System.Runtime.CompilerServices.CompilerGeneratedAttribute>() != null)
            return false;

        // Only skip virtual methods if they're from core system types
        if (method.IsVirtual && method.DeclaringType != null)
        {
            var declaringType = method.DeclaringType;
            
            // Skip if it's a virtual method from these specific problematic types
            if (declaringType == typeof(object) || 
                declaringType == typeof(System.Attribute) || 
                declaringType == typeof(System.Exception) ||
                declaringType == typeof(System.ValueType) ||
                declaringType == typeof(System.Enum))
            {
                return false;
            }

            // Skip specific problematic method names regardless of type
            var problematicMethodNames = new[]
            {
                "Finalize", "GetHashCode", "Equals", "ToString", "GetType",
                "MemberwiseClone", "GetBaseException", "GetObjectData",
                "InternalPreserveStackTrace", "RestoreDispatchState", "CaptureDispatchState",
                "SetCurrentStackTrace", "SetRemoteStackTrace", "get_TypeId", "Match", "IsDefaultAttribute"
            };

            if (problematicMethodNames.Contains(method.Name))
                return false;
        }

        // Skip property getters/setters and event handlers (optional - remove this if you want to profile them)
        if (method.IsSpecialName && (method.Name.StartsWith("get_") || method.Name.StartsWith("set_") || 
            method.Name.StartsWith("add_") || method.Name.StartsWith("remove_")))
            return false;

        return true;
    }

    public class MethodWrapper
    {
        [HarmonyPrefix]
        public static void Prefix(out Stopwatch __state)
        {
            __state = Stopwatch.StartNew();
        }

        [HarmonyPostfix]
        public static void Postfix(MethodBase __originalMethod, Stopwatch __state)
        {
            __state.Stop();
            var ticks = __state.ElapsedTicks;

            // Find which assembly this method belongs to
            var assembly = __originalMethod.DeclaringType?.Assembly;
            if (assembly == null) return;

            lock (assemblyMethodStats)
            {
                if (assemblyMethodStats.TryGetValue(assembly, out var methodStats) &&
                    methodStats.TryGetValue(__originalMethod, out var stats))
                {
                    stats.AddTiming(ticks);
                }
            }
        }
    }

    public static void DumpStats()
    {
        // Ensure directory exists
        if (!System.IO.Directory.Exists(Directory))
        {
            System.IO.Directory.CreateDirectory(Directory);
        }

        foreach (var assemblyKvp in assemblyMethodStats)
        {
            var assembly = assemblyKvp.Key;
            var methodStats = assemblyKvp.Value;

            // Get assembly name without extension
            var assemblyName = Path.GetFileNameWithoutExtension(assembly.Location);
            if (string.IsNullOrEmpty(assemblyName))
            {
                assemblyName = assembly.GetName().Name ?? "Unknown";
            }

            var fileName = Path.Combine(Directory, $"{assemblyName}_Performance.txt");

            using var writer = new StreamWriter(fileName);
            writer.WriteLine($"Performance Report for Assembly: {assemblyName}");
            writer.WriteLine($"Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            writer.WriteLine(new string('=', 80));
            writer.WriteLine();

            // Write Coverage Report
            WriteCoverageReport(writer, assembly, methodStats);
            writer.WriteLine();

            // Sort by average execution time (descending)
            var sortedMethods = methodStats
                .Where(kvp => kvp.Value.Count > 0)
                .OrderByDescending(kvp => kvp.Value.AverageTicks);

            foreach (var methodKvp in sortedMethods)
            {
                var method = methodKvp.Key;
                var stats = methodKvp.Value;

                writer.WriteLine($"Method: {method.DeclaringType?.FullName}.{method.Name}");
                writer.WriteLine($"  Calls: {stats.Count:N0}");
                writer.WriteLine($"  Average: {stats.AverageTicks:F2} ticks ({TicksToMilliseconds((long)stats.AverageTicks):F4} ms)");
                writer.WriteLine($"  Minimum: {stats.MinTicks:N0} ticks ({TicksToMilliseconds(stats.MinTicks):F4} ms)");
                writer.WriteLine($"  Maximum: {stats.MaxTicks:N0} ticks ({TicksToMilliseconds(stats.MaxTicks):F4} ms)");
                writer.WriteLine($"  Total:   {stats.TotalTicks:N0} ticks ({TicksToMilliseconds(stats.TotalTicks):F2} ms)");
                writer.WriteLine();
            }

            writer.WriteLine(new string('=', 80));
            writer.WriteLine($"Total Methods Profiled: {methodStats.Count(kvp => kvp.Value.Count > 0):N0}");
        }
    }

    private static void WriteCoverageReport(StreamWriter writer, Assembly assembly, Dictionary<MethodBase, MethodStats> methodStats)
    {
        writer.WriteLine("COVERAGE REPORT");
        writer.WriteLine(new string('-', 40));

        if (assemblyCoverageStats.TryGetValue(assembly, out var coverageStats))
        {
            // Update called methods count
            coverageStats.CalledMethods = methodStats.Count(kvp => kvp.Value.Count > 0);

            writer.WriteLine($"Total Methods Found:     {coverageStats.TotalMethods:N0}");
            writer.WriteLine($"Successfully Patched:    {coverageStats.PatchedMethods:N0}");
            writer.WriteLine($"Skipped (Filtered):      {coverageStats.SkippedMethods:N0}");
            writer.WriteLine($"Methods Called:          {coverageStats.CalledMethods:N0}");
            writer.WriteLine($"Methods Not Called:      {(coverageStats.PatchedMethods - coverageStats.CalledMethods):N0}");
            writer.WriteLine();
            writer.WriteLine($"Patch Success Rate:      {coverageStats.PatchSuccessRate:F1}%");
            writer.WriteLine($"Execution Coverage:      {coverageStats.CoveragePercentage:F1}%");
            
            if (coverageStats.PatchedMethods > 0)
            {
                var executionRate = (coverageStats.CalledMethods / (double)coverageStats.PatchedMethods) * 100;
                writer.WriteLine($"Called/Patched Ratio:    {executionRate:F1}%");
            }
        }
        else
        {
            writer.WriteLine("Coverage statistics not available for this assembly.");
        }

        writer.WriteLine(new string('-', 40));
    }

    private static double TicksToMilliseconds(long ticks)
    {
        return (ticks * 1000.0) / Stopwatch.Frequency;
    }

    public static void DumpStatsForAssembly(Assembly assembly)
    {
        if (!assemblyMethodStats.TryGetValue(assembly, out var methodStats))
            return;

        // Ensure directory exists
        if (!System.IO.Directory.Exists(Directory))
        {
            System.IO.Directory.CreateDirectory(Directory);
        }

        var assemblyName = Path.GetFileNameWithoutExtension(assembly.Location);
        if (string.IsNullOrEmpty(assemblyName))
        {
            assemblyName = assembly.GetName().Name ?? "Unknown";
        }

        var fileName = Path.Combine(Directory, $"{assemblyName}_Performance.txt");

        using var writer = new StreamWriter(fileName);
        writer.WriteLine($"Performance Report for Assembly: {assemblyName}");
        writer.WriteLine($"Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        writer.WriteLine(new string('=', 80));
        writer.WriteLine();

        // Write Coverage Report
        WriteCoverageReport(writer, assembly, methodStats);
        writer.WriteLine();

        var sortedMethods = methodStats
            .Where(kvp => kvp.Value.Count > 0)
            .OrderByDescending(kvp => kvp.Value.AverageTicks);

        foreach (var methodKvp in sortedMethods)
        {
            var method = methodKvp.Key;
            var stats = methodKvp.Value;

            writer.WriteLine($"Method: {method.DeclaringType?.FullName}.{method.Name}");
            writer.WriteLine($"  Calls: {stats.Count:N0}");
            writer.WriteLine($"  Average: {stats.AverageTicks:F2} ticks ({TicksToMilliseconds((long)stats.AverageTicks):F4} ms)");
            writer.WriteLine($"  Minimum: {stats.MinTicks:N0} ticks ({TicksToMilliseconds(stats.MinTicks):F4} ms)");
            writer.WriteLine($"  Maximum: {stats.MaxTicks:N0} ticks ({TicksToMilliseconds(stats.MaxTicks):F4} ms)");
            writer.WriteLine($"  Total:   {stats.TotalTicks:N0} ticks ({TicksToMilliseconds(stats.TotalTicks):F2} ms)");
            writer.WriteLine();
        }

        writer.WriteLine(new string('=', 80));
        writer.WriteLine($"Total Methods Profiled: {methodStats.Count(kvp => kvp.Value.Count > 0):N0}");
    }
}