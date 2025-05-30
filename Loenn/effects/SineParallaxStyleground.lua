local effect = {}

effect.name = "FlaglinesAndSuch/SineParallaxStyleground"
effect.canBackground = true
effect.canForeground = true

effect.fieldInformation = {
    BlendMode = {
        options = {
            "Additive",
            "Alphablend"
        },
        editable = false
    },
    Color = {
        fieldType = "color"
    }
}

effect.defaultData = {
    Texture = "bgs/04/bgCloudLoop",
    posX = 0.0,
    posY = 0.0,
    speedX = 0.0,
    speedY = 0.0,
    scrollX = 0.0,
    scrollY = 0.0,
    loopX = false,
    loopY = false,
    amplitude = 10.0,
    frequency = 1.0,
    offset = 0.0,
    SineVertically = true,
    Alpha = 1.0,
    --BlendMode = "Alphablend",
    Color = "ffffff",
    InstantIn = true,
    InstantOut = false,
    FlipX = false,
    FlipY = false
}

return effect