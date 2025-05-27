using System;
using VAMP.Systems;

namespace VAMP;

public class Events
{
    /// <summary>
    /// Triggered once the server has loaded and VAMP.Core has initialized.
    /// </summary>
    public static Action OnCoreLoaded;
    /// <summary>
    /// Triggered if the server has been wiped. 
    /// This can be used to clear any data that should not persist between server restarts.
    /// NOT READY FOR USE
    /// </summary>
    public static Action OnServerWiped;
}
