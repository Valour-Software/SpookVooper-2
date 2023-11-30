novastella_free_market = {
	name = "Free Market Efficiency"
	modifiers = {
		district.provinces.buildingslotsfactor = 0.25
		district.provinces.buildingslotsexponent = 0.04
		district.provinces.overpopulationmodifierexponent = -0.01
	}
	isgood = true
	stackable = false
}

lanatia_lore = {
	name = "Safe Haven for Anti-Imperial People"
	modifiers = {
		district.provinces.populationgrowthspeedfactor = 5
	}
	isgood = true
	stackable = false
}

elysian_katonia_chips_industry = {
	name = "Booming Chip Industry"
	description = "Every factory producing chips in Elysian Katonia receives a +15% throughput modifier"
	effects = {
		-- current scope is district
		every_scope_building = {
			-- now scope is building
			if = {
				limit = {
					building.recipe = "recipes.recipe_computer_chips_factory_base"
				}
				effects = {
					add_static_modifier_if_not_already_added = {
						name = "elysian_katonia_chips_industry_building"
					}
				}
			}
			if = {
				limit = {
					AND = {
						NOT = {
							building.recipe = "recipes.recipe_computer_chips_factory_base"
						}
						hasstaticmodifier = "elysian_katonia_chips_industry_building"
					}
				}
				effects = {
					remove_static_modifier = "elysian_katonia_chips_industry_building"
				}
			}
		}
	}
	isgood = true
	stackable = false
}

old_king_steel_industry = {
	name = "Booming Steel Industry"
	description = "Every factory producing steel in Old King receives a +15% throughput modifier"
	effects = {
		-- current scope is district
		every_scope_building = {
			-- now scope is building
			if = {
				limit = {
					building.recipe = "recipes.recipe_steel_factory_base"
				}
				effects = {
					add_static_modifier_if_not_already_added = {
						name = "old_king_steel_industry_building"
					}
				}
			}
			if = {
				limit = {
					AND = {
						NOT = {
							building.recipe = "recipes.recipe_steel_factory_base"
						}
						hasstaticmodifier = "old_king_steel_industry_building"
					}
				}
				effects = {
					remove_static_modifier = "old_king_steel_industry_building"
				}
			}
		}
	}
	isgood = true
	stackable = false
}