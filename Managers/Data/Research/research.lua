-- categories
-- Production
-- Extraction
-- Engineering & Physics
-- Military
-- Civil
production_category = {
	name = "Production"

	researches = {
		factory_throughput = {
			name = "Factory Throughput research.level"
			costs = {
				add_locals = {
					cost_increase = 1.5^research.level
				}
				production_research_points = 100 * get_local("cost_increase")
			}
			modifiers = {
				entity.factories.throughputfactor = 0.1
			}
			color = "BD5D3F"
			-- who can research this research (district or nondistrict)
			who_can_research = nondistrict
		}
		factory_efficiency = {
			name = "Factory Efficiency research.level"
			costs = {
				add_locals = {
					cost_increase = 1.5^research.level
				}
				production_research_points = 100 * get_local("cost_increase")
			}
			modifiers = {
				entity.factories.efficiencyfactor = 0.05
			}
			color = "BD5D3F"
			who_can_research = nondistrict
		}
		factory_quantity_cap = {
			name = "Factory Quantity Cap research.level"
			costs = {
				add_locals = {
					cost_increase = 1.75^research.level
				}
				production_research_points = 200 * get_local("cost_increase")
			}
			modifiers = {
				entity.factories.quantitycapfactor = 0.075
			}
			color = "BD5D3F"
			who_can_research = nondistrict
		}
	}
}

civil_category = {
	name = "Civil"
	researches = {
		more_building_slots_i = {
			name = "More Building Slots I"
			costs = {
				civil_research_points = 1000
			}
			modifiers = {
				district.provinces.buildingslotsfactor = 0.05
			}
			color = "7FB5B5"
			isinfinite = false
			who_can_research = district
			children = {
				more_building_slots_ii = {
					name = "More Building Slots II"
					costs = {
						civil_research_points = 2500
					}
					modifiers = {
						district.provinces.buildingslotsfactor = 0.05
					}
					color = "7FB5B5"
					who_can_research = district
					isinfinite = false
				}
				less_overpopulation_i = {
					name = "Less Overpopulation I"
					costs = {
						civil_research_points = 2500
					}
					modifiers = {
						district.provinces.overpopulationmodifierexponent = -0.005
					}
					color = "7FB5B5"
					who_can_research = district
					isinfinite = false
					children = {
						less_overpopulation_ii = {
							name = "Less Overpopulation II"
							costs = {
								civil_research_points = 5000
							}
							modifiers = {
								district.provinces.overpopulationmodifierexponent = -0.005
							}
							color = "7FB5B5"
							who_can_research = district
							isinfinite = false
						}
					}
				}
			}
		}
	}
}