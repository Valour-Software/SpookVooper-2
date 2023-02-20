waste_land = {
	name = "Waste Land"
	development_value_required = 0
	modifiers = {
		provinces.buildingslotsfactor = -0.1
	}
}

shanty = {
	name = "Shanty"
	development_value_required = 15 -- ~65k population required
	modifiers = {
		provinces.buildingslotsfactor = 0
	}
}

village = {
	name = "Village"
	development_value_required = 20 -- ~100k population required
	modifiers = {
		provinces.buildingslotsfactor = 0.1
	}
}

town = {
	name = "Town"
	development_value_required = 35 -- ~300k population required
	modifiers = {
		provinces.buildingslotsfactor = 0.2
	}
}

hub = {
	name = "Hub"
	development_value_required = 70 -- ~1m population required
	modifiers = {
		provinces.buildingslotsfactor = 0.35
	}
}

city = {
	name = "City"
	development_value_required = 120 -- ~3m population required
	modifiers = {
		provinces.buildingslotsfactor = 0.6
	}
}

megacity = {
	name = "Megacity"
	development_value_required = 180 -- ~6m population required
	modifiers = {
		provinces.buildingslotsfactor = 1.25
		-- give a small bonus
		provinces.buildingslotsexponent = 0.005
	}
}