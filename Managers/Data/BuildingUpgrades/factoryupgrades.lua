simple_factory_throughput_upgrade = {
    name = "Increase Simple Factory's Throughput"

	-- these costs are scaled to the building's level!
    costs = {
        add_locals = {
			cost_increase = 1.3^building.level
		}
		steel = (3000 * get_local("cost_increase")) * 2
		simple_components = 1500 * get_local("cost_increase") * 2 + 1
		simple_components = 1500 * get_local("cost_increase") * 2 + 1
		simple_components = {
			base = 1500
			factor = get_local("cost_increase")
			factor = 2
			add = 1
		}
		advanced_components = {
			base = 200
			factor = { 
				get_local = "cost_increase"
			}
		}
    }
    modifiers = {
        building.throughputfactor = 0.15
		building.efficiencyfactor = 0.02
    }
}

simple_factory_efficiency_upgrade = {
    name = "Increase Simple Factory's Efficiency"
    costs = {
        add_locals = {
			cost_increase = {
				base = 1.4
				raiseto = building.level
			}
		}
		simple_components = {
			base = 3000
			factor = {
				get_local = "cost_increase"
			}
		}
		advanced_components = {
			base = 400
			factor = { 
				get_local = "cost_increase"
			}
		}
    }
    modifiers = {
        building.efficiencyfactor = 0.075
    }
}