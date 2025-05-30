local effect = {}

effect.name = "FlaglinesAndSuch/VaporwaveGrid"
effect.canBackground = true
effect.canForeground = true

effect.fieldInformation = {
    color = {
        fieldType = "color"
    },
    Horizontal_lines = {
        fieldType = "integer"
    },
    Vertical_lines = {
        fieldType = "integer"
    },
    Top_Height = {
        fieldType = "integer"
    },
    view_X = {
        fieldType = "integer"
    },
    view_Y = {
        fieldType = "integer"
    }
}

effect.defaultData = {
    color = "ffffff",
    Horizontal_lines = 10,
    Vertical_lines = 10,
    Top_Scroll = 0.2,
    Bottom_Scroll = 0.7,
    Top_Height = 60,
    Flip_Y = false,
    view_X = -100,
    view_Y = 72,
    Speed_y = 0.05
}

return effect