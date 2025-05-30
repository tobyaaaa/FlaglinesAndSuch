local enums = require("consts.celeste_enums")

local MiniTextboxIfFlag = {}

MiniTextboxIfFlag.name = "FlaglinesAndSuch/MiniTextboxIfFlag"
MiniTextboxIfFlag.fieldInformation = {
    death_count = {
        fieldType = "integer",
    },
    mode = {
        options = enums.mini_textbox_trigger_modes,
        editable = false
    }
}
MiniTextboxIfFlag.placements = {
	name = "MiniTextboxIfFlag",
	data = {
		Flag = "",
		Flag_State = false,
		Reset_Flag = false,
		mode = "",
		dialog_id = "",
		death_count = -1,
		only_once = true
	}
}

return MiniTextboxIfFlag