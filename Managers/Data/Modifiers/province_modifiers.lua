vooperian_capital {
	name = "Capital of the Vooperian Empire"
	modifiers = {
		province.buildingslots = 0.2
		province.buildingslots_exponent = 0.03
	}
	stackable = false
}

fertile_land_river_delta_area = {
	name = "Fertile Land"
	description = "The most fertile land in all of Vooperia."
	modifiers = {
		-- these are pretty serious modifiers
		-- put together, these add up to almost ~2x increase in the max farming output
		-- combined with the increased base fertile land, means these provinces will hold a max output
		-- of more than 4x other provinces
		province.fertilelandfactor = 1
		province.farms.farmingthroughputfactor = 0.75
	}
	stackable = false
}