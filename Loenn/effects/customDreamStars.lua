local effect = {}

effect.name = "FlaglinesAndSuch/customDreamStars"
effect.canBackground = true
effect.canForeground = true

effect.fieldInformation = {
    shape = {
        options = { "HollowRect","FilledRect","Circle","Diamond" },
        editable = false
    },
    Color = {
        fieldType = "color"
    },
    count = {
        fieldType = "integer"
    },
    AngleX = {
        fieldType = "integer"
    },
    AngleY = {
        fieldType = "integer"
    }
}

effect.defaultData = {
    count = 50,
    minSpeed = 24.0,
    maxSpeed = 48.0,
    minSize = 2.0,
    maxSize = 8.0,
    Color = "008080",
    AngleX = -2,
    AngleY = -7,
    Scroll = 0.5,
    shape = "HollowRect"
}

return effect