local effect = {}

effect.name = "FlaglinesAndSuch/GeometricBlackhole"
effect.canBackground = true
effect.canForeground = true

effect.fieldInformation = {
    InnerColor = {
        fieldType = "color"
    },
    OuterColor = {
        fieldType = "color"
    },
    OuterColor2 = {
        fieldType = "color"
    },
    shape_count = {
        fieldType = "integer"
    }
}

effect.defaultData = {
    InnerColor = "000000",
    OuterColor = "d60000",
    OuterColor2 = "d60000",
    speed = 2.0,
    shape_count = 20,
    Texture = "scenery/FlaglinesAndSuch/GeometricBlackhole/square",
    swirlMin = 0.0,
    swirlMax = 1.0,
    swirlMinNew = 0.1,
    swirlMaxNew = 4.0,
    rotationDifference = 2.0,
    reverse = false
}

return effect