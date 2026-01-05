local BonfireLight = {}

BonfireLight.name = "FlaglinesAndSuch/BonfireLight"
BonfireLight.depth = 0
BonfireLight.texture = "ahorn/FlaglinesAndSuch/bonfireIcon"
BonfireLight.justification = {0.0, 0.0}
BonfireLight.fieldInformation = {
    lightColor = {
        fieldType = "color"
    },
    lightFadeStart = {
        fieldType = "integer"
    },
    lightFadeEnd = {
        fieldType = "integer"
    },
    bloomRadius = {
        fieldType = "integer"
    }
}
BonfireLight.placements = {
    {
        name = "normal",
        data = {
            lightColor = "DB7093",
            lightFadeStart = 32,
            lightFadeEnd = 64,
            bloomRadius = 32,
            baseBrightness = 0.5,
            brightnessVariance = 0.5,
            flashFrequency = 0.25,
            wigglerDuration = 4.0,
            wigglerFrequency = 0.2,
            photosensitivityConcern = false,
            fadeIn = false
        }
    }
}

return BonfireLight