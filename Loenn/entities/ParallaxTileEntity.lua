local fakeTilesHelper = require("helpers.fake_tiles")
local utils = require("utils")
local matrixLib = require("utils.matrix")
local drawableSprite = require("structs.drawable_sprite")
local connectedEntities = require("helpers.connected_entities")

local parallaxTileEntity = {}

parallaxTileEntity.name = "FlaglinesAndSuch/parallaxTileEntity"
parallaxTileEntity.warnBelowSize = {16, 16}

parallaxTileEntity.fillColor = {0.169, 0.533, 0.851}
parallaxTileEntity.borderColor = {0.267, 0.718, 1.0}

parallaxTileEntity.fieldInformation = {
    offsetX = {
        fieldType = "integer"
    },
    offsetY = {
        fieldType = "integer"
    }
}
parallaxTileEntity.placements = {
    {
        name = "ptileentitynormal",
        data = {
            width = 16,
            height = 16,
            tiletype = "3",
            scroll = 0.8,
            offsetX = 0,
            offsetY = 0
        }
    }
}


parallaxTileEntity.sprite = fakeTilesHelper.getEntitySpriteFunction("tileType", false, "tilesFg", {1.0, 1.0, 1.0, 0.7})
parallaxTileEntity.fieldInformation = fakeTilesHelper.getFieldInformation("tileType")

return parallaxTileEntity