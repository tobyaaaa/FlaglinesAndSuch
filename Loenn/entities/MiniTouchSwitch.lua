local utils = require("utils")

local MiniTouchSwitch = {}

MiniTouchSwitch.name = "FlaglinesAndSuch/MiniTouchSwitch"
MiniTouchSwitch.depth = -1000000
MiniTouchSwitch.texture = "objects/FlaglinesAndSuch/MiniTouchSwitch/normal00"
MiniTouchSwitch.fieldInformation = {
    Color = {
        fieldType = "color"
    },
    spritepath = {
        options = { "normal", "circle", "cross", "diamond", "double", "drop", "heart", "hourglass", "split", "square", "star", "tall", "triangle", "triple", "wide"},
        editable = true
    }
}
MiniTouchSwitch.placements = {
    {
        name = "minitouchswitchnormal",
        data = {
            Flag = "",
            flagState = true,
            toggleFlag = false,
            Color = "ffffff",
            spritepath = "normal",
            OverrideSpritepath = ""
        }
    }
}


function MiniTouchSwitch.selection(room, entity)
    local x, y = entity.x or 0, entity.y or 0
    return utils.rectangle(x - 8, y - 8, 16, 16)
end


return MiniTouchSwitch