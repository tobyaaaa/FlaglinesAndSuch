local enums = require("consts.celeste_enums")

local FlagLightFade = {}

FlagLightFade.name = "FlaglinesAndSuch/FlagLightFade"
FlagLightFade.fieldInformation = {
    positionMode = {
        options = enums.trigger_position_modes,
        editable = false
    }
}
FlagLightFade.placements = {
	name = "LightFadeIfFlag",
	data = {
		LightAddFrom = 1.0,
		LightAddTo = 0.0,
		positionMode = "NoEffect",
		Flag = "",
		Flag_state = true,
		Reset_Flag =  false
	}
}
FlagLightFade.category = "visual"

return FlagLightFade