local fakeTilesHelper = require("helpers.fake_tiles")
local utils = require("utils")
local matrixLib = require("utils.matrix")
local drawableSprite = require("structs.drawable_sprite")
local connectedEntities = require("helpers.connected_entities")

local BlueBlock = {}

BlueBlock.name = "FlaglinesAndSuch/BlueBlock"
BlueBlock.warnBelowSize = {16, 16}

BlueBlock.fillColor = {0.169, 0.533, 0.851}
BlueBlock.borderColor = {0.267, 0.718, 1.0}

BlueBlock.fieldInformation = {
    dashes = {
        fieldType = "integer"
    }
}
BlueBlock.placements = {
    {
        name = "blueblocknormal",
        data = {
            width = 16,
            height = 16,
            dashes = 0
        }
    }
}

return BlueBlock