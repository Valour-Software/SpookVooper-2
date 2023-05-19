-- categories
-- Production
-- Extraction
-- Engineering & Physics
-- Military
-- Civil
extraction_category = {
	name = "Extraction"
	researches = {

	}
}

production_category = {
	name = "Production"
	researches = {
		factory_throughput = {
			name = "Factory Throughput research.level"
			costs = {
				add_locals = {
					cost_increase = 1.4^research.level
				}
				production_research_points = 150 * get_local("cost_increase")
			}
			modifiers = {
				entity.factories.throughputfactor = 0.02
			}
			color = "BD5D3F"
			-- who can research this research (district, nondistrict, or anyone)
			who_can_research = anyone
		}
		factory_efficiency = {
			name = "Factory Efficiency research.level"
			costs = {
				add_locals = {
					cost_increase = 1.4^research.level
				}
				production_research_points = 200 * get_local("cost_increase")
			}
			modifiers = {
				entity.factories.efficiencyfactor = 0.01
			}
			color = "BD5D3F"
			who_can_research = anyone
		}
		factory_quantity_cap = {
			name = "Factory Quantity Cap research.level"
			costs = {
				add_locals = {
					cost_increase = 1.6^research.level
				}
				production_research_points = 300 * get_local("cost_increase")
			}
			modifiers = {
				entity.factories.quantitycapfactor = 0.015
			}
			color = "BD5D3F"
			who_can_research = anyone
		}
	}
}

civil_category = {
	name = "Civil"
	researches = {
		more_building_slots_i = {
			name = "More Building Slots I"
			costs = {
				civil_research_points = 1500
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
						civil_research_points = 5000
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
						civil_research_points = 5000
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
								civil_research_points = 12500
							}
							modifiers = {
								district.provinces.overpopulationmodifierexponent = -0.005
							}
							color = "7FB5B5"
							who_can_research = district
							isinfinite = false
							children = {
								less_overpopulation_iii = {
									name = "Less Overpopulation III"
									costs = {
										civil_research_points = 35000
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
	}
}