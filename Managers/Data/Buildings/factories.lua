building_simple_factory = {
	recipes = {
		recipe_iron_smeltery_base
		recipe_steel_factory_base
		recipe_simple_components_factory_base
	}
	buildingcosts = {
		steel = 10000,
		simple_components = 7500,
		advanced_components = 1000
	}
	type = "Factory"
}

building_advanced_factory = {
	recipes = {
		recipe_advanced_components_factory_base,
		recipe_computer_chips_factory_base
	}
	buildingcosts = {
		steel = 35000,
		simple_components = 20000,
		advanced_components = 5000
	}
	type = "Factory"
}