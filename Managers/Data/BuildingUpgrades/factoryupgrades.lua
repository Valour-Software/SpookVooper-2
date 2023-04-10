simple_factory_throughput_upgrade = {
    name = "Increase Simple Factory's Throughput"
    costs = {
        add_locals = {
			cost_increase = 1.3^building.level
		}
		steel = 3000 * get_local("cost_increase")
		simple_components = 1500 * get_local("cost_increase")
		advanced_components = 200 * get_local("cost_increase")
    }
    modifiers = {
        building.throughputfactor = 0.15
		building.efficiencyfactor = -0.03
    }
}

simple_factory_efficiency_upgrade = {
    name = "Increase Simple Factory's Efficiency"
    costs = {
        add_locals = {
			cost_increase = 1.4^building.level
		}
		simple_components = 2500 * get_local("cost_increase")
		advanced_components = 450 * get_local("cost_increase")
    }
    modifiers = {
        building.efficiencyfactor = 0.075
    }
}

advanced_factory_throughput_upgrade = {
    name = "Increase Advanced Factory's Throughput"
    costs = {
        add_locals = {
			cost_increase = 1.3^building.level
		}
		steel = 10000 * get_local("cost_increase")
		simple_components = 4000 * get_local("cost_increase")
		advanced_components = 700 * get_local("cost_increase")
    }
    modifiers = {
        building.throughputfactor = 0.2
		building.efficiencyfactor = -0.03
    }
}

advanced_factory_efficiency_upgrade = {
    name = "Increase Advanced Factory's Efficiency"
    costs = {
        add_locals = {
			cost_increase = 1.4^building.level
		}
		simple_components = 8000 * get_local("cost_increase")
		advanced_components = 1500 * get_local("cost_increase")
    }
    modifiers = {
        building.efficiencyfactor = 0.08
    }
}