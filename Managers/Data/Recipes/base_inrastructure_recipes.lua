recipe_infrastructure_roads = {
	inputs = {
        tools = 0.05
        simple_components = 0.075
        advanced_components = 0.01
        steel = 0.15
    }
    outputs = {
		modifiers = {
			province = {
				province.buildingslots = 1
				province.overpopulationmodifierpopulationbase = -50000
			}
		}
	}
	inputcost_scaleperlevel = false
	perhour = 1
    editable = false
}