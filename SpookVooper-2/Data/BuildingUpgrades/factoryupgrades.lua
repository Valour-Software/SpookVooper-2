simple_factory_throughput_upgrade = {
    name = "Increase Throughput"
    costs = {
        add_locals = {
			cost_increase = 1.3^upgrade.level
		}
		steel = 3000 * get_local("cost_increase")
		simple_components = 1500 * get_local("cost_increase")
		advanced_components = 200 * get_local("cost_increase")
    }
    modifiers = {
        building.throughputfactor = 0.175
		building.efficiencyfactor = -0.03
    }
}

simple_factory_efficiency_upgrade = {
    name = "Increase Efficiency"
    costs = {
        add_locals = {
			cost_increase = 1.4^upgrade.level
		}
		simple_components = 2500 * get_local("cost_increase")
		advanced_components = 450 * get_local("cost_increase")
    }
    modifiers = {
        building.efficiencyfactor = 0.075
    }
}

advanced_factory_throughput_upgrade = {
    name = "Increase Throughput"
    costs = {
        add_locals = {
			cost_increase = 1.3^upgrade.level
		}
		steel = 9000 * get_local("cost_increase")
		simple_components = 5000 * get_local("cost_increase")
		advanced_components = 800 * get_local("cost_increase")
    }
    modifiers = {
        building.throughputfactor = 0.25
		building.efficiencyfactor = -0.04
    }
}

advanced_factory_efficiency_upgrade = {
    name = "Increase Efficiency"
    costs = {
        add_locals = {
			cost_increase = 1.4^upgrade.level
		}
		simple_components = 8000 * get_local("cost_increase")
		advanced_components = 1500 * get_local("cost_increase")
    }
    modifiers = {
        building.efficiencyfactor = 0.08
    }
}