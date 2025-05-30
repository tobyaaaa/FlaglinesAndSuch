local effect = {}

effect.name = "FlaglinesAndSuch/PolygonEffect"
effect.canBackground = true
effect.canForeground = true

--effect.fieldInformation = {
--    color = {
--        fieldType = "color"
--    }
--}

effect.defaultData = {
    colors = "ff0000",
    positions = "0,0,1.0; 0,10,1.0; 10,10,1.0",
    VisibleSide = false,
    OffsetX = 0,
    OffsetY = 0
}

return effect