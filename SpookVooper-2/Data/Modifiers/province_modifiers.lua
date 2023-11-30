vooperian_capital = {
	name = "Capital of the Vooperian Empire"
	modifiers = {
		province.buildingslotsfactor = 0.2
		province.buildingslotsexponent = 0.03
		province.overpopulationmodifierexponent = -0.015
		province.migrationattractionfactor = 0.2
	}
	isgood = true
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
		province.farms.farmingthroughputfactor = 1.5
		province.overpopulationmodifierexponent = -0.0125
	}
	isgood = true
	stackable = false
}

mass_migration = {
	name = "Mass Migration"
	description = "Large number of people are moving to this province"
	modifiers = {
		province.migrationattractionfactor = 1.5
	}
	isgood = true
	stackable = false
}