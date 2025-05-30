local DialogIfFlag = {}

DialogIfFlag.name = "FlaglinesAndSuch/DialogIfFlag"
DialogIfFlag.placements = {
	name = "DialogIfFlag",
	data = {
		dialogID = "",
		Flag = "",
		Flag_State = true,
		Reset_Flag =  false,
		onlyOnce = true,
		endLevel = false,
		deathCount = -1
	}
}

return DialogIfFlag