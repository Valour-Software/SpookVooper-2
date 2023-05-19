waste_land = {
	name = "Waste Land"
	development_value_required = 0
	modifiers = {
		province.buildingslotsfactor = -0.1
	}
}

shanty = {
	name = "Shanty"
	development_value_required = 15 -- ~65k population required
	modifiers = {
		province.buildingslotsfactor = 0
	}
}

village = {
	name = "Village"
	development_value_required = 20 -- ~100k population required
	modifiers = {
		province.buildingslotsfactor = 0.1
	}
}

town = {
	name = "Town"
	development_value_required = 35 -- ~300k population required
	modifiers = {
		province.buildingslotsfactor = 0.2
	}
}

hub = {
	name = "Hub"
	development_value_required = 70 -- ~1m population required
	modifiers = {
		province.buildingslotsfactor = 0.35
	}
}

city = {
	name = "City"
	development_value_required = 120 -- ~3m population required
	modifiers = {
		province.buildingslotsfactor = 0.6
		province.migrationattractionfactor = 0.05
	}
}

megacity = {
	name = "Megacity"
	development_value_required = 180 -- ~6m population required
	modifiers = {
		province.buildingslotsfactor = 1.25
		-- give a small bonus
		province.buildingslotsexponent = 0.005
		province.migrationattractionfactor = 0.1
	}
}