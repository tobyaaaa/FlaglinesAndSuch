local FlagIfFlag = {}

FlagIfFlag.name = "FlaglinesAndSuch/FlagIfFlag"
FlagIfFlag.placements = {
	name = "FlagIfFlag",
	data = {
		if_flag = "flag A",
		if_state = false,
		set_flag = "flag B",
		set_state = false,
		reset_if_flag = false,
		only_once = false
	}
}

return FlagIfFlag