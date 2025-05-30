local MusicIfFlag = {}

MusicIfFlag.name = "FlaglinesAndSuch/MusicIfFlag"
MusicIfFlag.fieldInformation = {
    Track = {
        options = songs
    }
}
MusicIfFlag.placements = {
	name = "MusicIfFlag",
	data = {
		Flag = "",
		Flag_State = false,
		Reset_Flag = false,
		Track = "",
		Progress = 0,
		Reset_On_Leave = false
	}
}
MusicIfFlag.category = "audio"

return MusicIfFlag