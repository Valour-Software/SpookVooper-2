recipe_infrastructure_roads = {
	name = "Road Infrastructure"
	inputs = {
        tools = 0.025
        simple_components = 0.075
        advanced_components = 0.01
        steel = 0.15
    }
    outputs = {
		modifiers = {
			province.buildingslotsfactor = 0.005
			province.overpopulationmodifierpopulationbase = -30000
			province.buildingslotsexponent = 0.0002
			province.migrationattraction = 0.5
		}
	}
	inputcost_scaleperlevel = false
	perhour = 1
    editable = false
}