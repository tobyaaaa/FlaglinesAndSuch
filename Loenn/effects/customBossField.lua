local effect = {}

effect.name = "FlaglinesAndSuch/customBossField"
effect.canBackground = true
effect.canForeground = true

effect.fieldInformation = {
    bg_color = {
        fieldType = "color"
    },
    count = {
        fieldType = "integer"
    },
    pos_x_min = {
        fieldType = "integer"
    },
    pos_x_max = {
        fieldType = "integer"
    },
    pos_y_min = {
        fieldType = "integer"
    },
    pos_y_max = {
        fieldType = "integer"
    }
}

effect.defaultData = {
    bg_color = "000000",
    alpha = 1.0,
    count = 200,
    particle_colors = "030c1b,0b031b,1b0319,0f0301",
    speed_min = 500.0,
    speed_max = 1200.0,
    pos_x_min = 0,
    pos_x_max = 384,
    pos_y_min = 9,
    pos_y_max = 244,
    dir_x = -1.0,
    dir_y = 0.0,
    dir_x_alt = 0.0,
    dir_y_alt = -1.0,
    scroll_x = 0.9,
    scroll_y = 0.9,
    stretch_multiplier = 1.0
}

return effect