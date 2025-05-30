local effect = {}

effect.name = "FlaglinesAndSuch/SummitBoostBackground"
effect.canBackground = true
effect.canForeground = true

effect.fieldInformation = {
    background_color = {
        fieldType = "color"
    },
    bar_color = {
        fieldType = "color"
    },
    cloud_color = {
        fieldType = "color"
    },
    streak_count = {
        fieldType = "integer"
    },
    cloud_count = {
        fieldType = "integer"
    }
}

effect.defaultData = {
    draw_background = true,
    draw_bars = true,
    draw_streaks = true,
    draw_clouds = true,
    background_color = "75a0ab",
    bar_color = "ffffff",
    streak_count = 80,
    streak_speed_min = 600.0,
    streak_speed_max = 2000.0,
    streak_colors = "ffffff,e69ecb",
    streak_alpha = 1.0,
    cloud_count = 10,
    cloud_speed_min = 400.0,
    cloud_speed_max = 800.0,
    cloud_color = "b64a86",
    cloud_alpha = 1.0
}

return effect