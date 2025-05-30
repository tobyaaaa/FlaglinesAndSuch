local effect = {}

effect.name = "FlaglinesAndSuch/customGodrays"
effect.canBackground = true
effect.canForeground = true

effect.fieldInformation = {
    ray_color = {
        fieldType = "color"
    },
    min_width = {
        fieldType = "integer"
    },
    max_width = {
        fieldType = "integer"
    },
    min_length = {
        fieldType = "integer"
    },
    max_length = {
        fieldType = "integer"
    },
    ray_count = {
        fieldType = "integer"
    }
}

effect.defaultData = {
    min_width = 8,
    max_width = 16,
    min_length = 20,
    max_length = 40,
    duration_base = 4.0,
    duration_variance = 8.0,
    ray_color_alpha = 0.5,
    ray_color = "f52b63",
    fade_to_color = "",
    scroll_x = 0.9,
    scroll_y = 0.9,
    speed_x = 0.0,
    speed_y = 8.0,
    ray_count = 6,
    angle_x = -1.67079639,
    angle_y = 1.0
}

return effect