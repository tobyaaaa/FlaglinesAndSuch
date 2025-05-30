using Microsoft.Xna.Framework.Input;
using Celeste.Mod;

namespace FlaglinesAndSuch {
    public class FlaglinesAndSuchModuleSettings : EverestModuleSettings {
        [SettingName("Settings_nailAlwaysActive")]
        public bool PlayerAlwaysHasNail { get; set; } = false;


        [SettingName("Settings_nailBind")]
        [DefaultButtonBinding(Buttons.X, Keys.A)]
        public ButtonBinding NailHit { get; set; }
    }
}
