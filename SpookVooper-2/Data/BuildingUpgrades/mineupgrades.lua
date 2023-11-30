mine_throughput_upgrade = {
    name = "Increase Throughput"
    costs = {
        add_locals = {
			cost_increase = 1.3^upgrade.level
		}
		steel = 1000 * get_local("cost_increase")
		simple_components = 400 * get_local("cost_increase")
		advanced_components = 50 * get_local("cost_increase")
    }
    modifiers = {
        building.throughputfactor = 0.175
		building.efficiencyfactor = -0.05
    }
}

mine_efficiency_upgrade = {
    name = "Increase Efficiency"
    costs = {
        add_locals = {
			cost_increase = 1.4^upgrade.level
		}
		simple_components = 1000 * get_local("cost_increase")
		advanced_components = 100 * get_local("cost_increase")
    }
    modifiers = {
        building.efficiencyfactor = 0.075
    }
}