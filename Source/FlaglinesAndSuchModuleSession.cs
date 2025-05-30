//using FlaglinesAndSuch;
using Celeste.Mod;
using System.Collections.Generic;

namespace FlaglinesAndSuch {
    public class FlaglinesAndSuchModuleSession : EverestModuleSession
    {
        public bool DustNoShrink;
        public HashSet<MiniTouchSwitch.persistencyVariables> MiniTouchSwitches = new HashSet<MiniTouchSwitch.persistencyVariables>();
        //public HashSet<IndexedKey.persistencyVars> IndexedKeys = new HashSet<IndexedKey.persistencyVars>();
        //private FlaglinesAndSuchSession() {
        //	MiniTouchSwitches = new HashSet<MiniTouchSwitch.persistencyVariables>();

    }
}
