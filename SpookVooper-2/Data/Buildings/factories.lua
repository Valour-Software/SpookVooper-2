building_simple_factory = {
	recipes = [
		recipe_iron_smeltery_base
		recipe_copper_smeltery_base
		recipe_bauxite_smeltery_base
		recipe_steel_factory_base
		recipe_tool_factory_base
		recipe_simple_components_factory_base
		recipe_plastic_factory_base
	]
	buildingcosts = {
		steel = 10000
		simple_components = 7500
		advanced_components = 1000
	}
	upgrades = [
		simple_factory_throughput_upgrade
		simple_factory_efficiency_upgrade
	]
	type = "Factory"
}

building_advanced_factory = {
	recipes = [
		recipe_advanced_components_factory_base
		recipe_computer_chips_factory_base
		recipe_cars_factory_base
		recipe_televisions_factory_base
	]
	buildingcosts = {
		steel = 35000
		simple_components = 20000
		advanced_components = 5000
	}
	upgrades = [
		advanced_factory_throughput_upgrade
		advanced_factory_efficiency_upgrade
	]
	type = "Factory"
}