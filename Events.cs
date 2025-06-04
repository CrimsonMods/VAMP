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
    /// Triggered when the server has been wiped.
    /// This event indicates that server data has been cleared and provides information about the wipe status.
    /// If the boolean parameter is true, files in the `_WipeData` folder were automatically deleted by VAMP.
    /// This event is always invoked before OnCoreLoaded.
    /// </summary>
    public static Action<bool> OnServerWiped;
}
