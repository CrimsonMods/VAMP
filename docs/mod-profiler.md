
# Mod Profiler Documentation

The Mod Profiler in VAMP provides detailed performance analysis and monitoring of mod methods during runtime.

## Overview

The mod profiler system offers:
- Method-level performance tracking
- Thread usage analysis
- Code coverage reporting
- Detailed timing statistics

## For Server Admins

### Using the Profiler

VAMP's profiler can be used to:
- Monitor mod performance impact
- Identify bottlenecks
- Track thread usage
- Generate detailed performance reports

### Performance Reports

The profiler generates reports containing:
- Method execution times
- Call frequency statistics
- Thread utilization data
- Code coverage metrics

Reports are saved in the `ModProfiler` directory with filenames following the pattern: `{ModName}_Performance.txt`

If your server is having lag issues provide the appropriate mod file to the modder so they can identify the source of the lag.

## For Modders

### Profiling Your Mod

Profile your mod's assembly:

```csharp
// Profile a specific assembly
Assembly myModAssembly = typeof(MyModClass).Assembly;
ModProfiler.ProfileAssembly(myModAssembly);

// Generate performance report
ModProfiler.DumpStats();
```

### Understanding the Output

Performance reports include:

1. Coverage Statistics
   - Total methods found
   - Successfully patched methods
   - Methods called during runtime
   - Patch success rate
   - Execution coverage percentage

2. Thread Usage
   - Main thread vs background thread calls
   - Unique thread count
   - Per-method thread distribution

3. Method Statistics
   - Average execution time
   - Minimum/maximum timings
   - Total execution time
   - Call count
   - Thread-specific metrics

### Filtered Methods

The profiler automatically filters out:
- Abstract and generic methods
- Methods without bodies
- Compiler-generated methods
- System type methods
- Property accessors
- Event handlers

## Best Practices

1. Profile in a test environment before production
2. Monitor both average and maximum execution times
3. Pay attention to methods running on multiple threads
4. Use coverage reports to ensure comprehensive testing
5. Watch for methods with unexpectedly high execution times
