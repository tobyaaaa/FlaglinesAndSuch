local spikeHelper = require("helpers.spikes")

local spikeOptions = {
    triggerSpike = true,
    directionNames = {
        up = "FlaglinesAndSuch/DustSpikesUp",
        down = "FlaglinesAndSuch/DustSpikesDown",
        left = "FlaglinesAndSuch/DustSpikesLeft",
        right = "FlaglinesAndSuch/DustSpikesRight"
    }
}

return spikeHelper.createEntityHandlers(spikeOptions)