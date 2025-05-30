using MonoMod.ModInterop;

namespace FlaglinesAndSuch {
    /// <summary>
    /// Provides export functions for other mods to import.
    /// If you do not need to export any functions, delete this class and the corresponding call
    /// to ModInterop() in <see cref="FlaglinesAndSuchModule.Load"/>
    /// </summary>
    [ModExportName("FlaglinesAndSuch")]
    public static class FlaglinesAndSuchExports {
        // TODO: add your mod's exports, if required
    }
}
