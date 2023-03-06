building_infrastructure = {
	recipes = {
		recipe_infrastructure_roads
	}
	buildingcosts = {
		add_locals = {
			cost_increase = {
				base = province.totaloftype["building_infrastructure"]
				factor = 0.4
				add = 1
			}
		}
		steel = {
			base = 1000
			factor = { 
				get_local = "cost_increase"
			}
		}
		simple_components = {
			base = 500
			factor = { 
				get_local = "cost_increase"
			}
		}
		advanced_components = {
			base = 50
			factor = { 
				get_local = "cost_increase"
			}
		}
	}

	base_efficiency = {
		base = 1
		divide = {
			base = province.buildings.totaloftype["building_infrastructure"]
			factor = 0.4
			add = 1
		}
	}

	-- only the governor of this province (or district if governor is null) can build this building
	onlygovernorcanbuild = true
	usebuildingslots = false
	type = "Infrastructure"
}