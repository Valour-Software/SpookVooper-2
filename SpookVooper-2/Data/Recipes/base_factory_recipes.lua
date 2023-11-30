recipe_iron_smeltery_base = {
	name = "Iron Smelting"
	inputs = {
		iron_ore = 1
		tools = 0.005
	}
	outputs = {
		iron = 1
	}
	perhour = 100
    editable = false
}

recipe_copper_smeltery_base = {
	name = "Copper Smelting"
	inputs = {
		copper_ore = 1
		tools = 0.005
	}
	outputs = {
		copper = 1
	}
	perhour = 100
    editable = false
}

recipe_lead_smeltery_base = {
	name = "Lead Smelting"
	inputs = {
		lead_ore = 1
		tools = 0.005
	}
	outputs = {
		lead = 1
	}
	perhour = 50
    editable = false
}

recipe_zinc_smeltery_base = {
	name = "Zinc Smelting"
	inputs = {
		zinc_ore = 1
		tools = 0.005
	}
	outputs = {
		zinc = 1
	}
	perhour = 50
    editable = false
}

recipe_bauxite_smeltery_base = {
	name = "Bauxite Smelting"
	inputs = {
		bauxite = 1
		tools = 0.0075
	}
	outputs = {
		aluminium = 0.75
	}
	perhour = 50
    editable = false
}

recipe_brass_factory_base = {
	name = "Brass Production"
	inputs = {
		copper = 2
		zinc = 1
	}
	outputs = {
		brass = 3
	}
	perhour = 7.5
    editable = false
}

recipe_steel_factory_base = {
	name = "Steel Production"
	inputs = {
		coal = 2
		iron = 4
	}
	outputs = {
		steel = 2.5
	}

	-- was 16
	perhour = 20
    editable = false
}

recipe_tool_factory_base = {
	name = "Tool Production"
	inputs = {
		computer_chips = 0.2
		simple_components = 0.2
		steel = 2
	}
	outputs = {
		tools = 3
	}
	perhour = 8
    editable = false
}

recipe_plastic_factory_base = {
	name = "Plastic Production"
	inputs = {
		oil = 1
	}
	outputs = {
		plastic = 2.5
	}
	perhour = 20
    editable = false
}

recipe_simple_components_factory_base = {
	name = "Simple Component Production"
	inputs = {
		iron = 1
		silicon = 1
		copper = 1
	}
	outputs = {
		simple_components = 1.25
	}

	-- was 16
	perhour = 20
    editable = false
}

recipe_advanced_components_factory_base = {
	name = "Advanced Component Production"
	inputs = {
		-- was 3
		simple_components = 2.25
		-- was 3
		steel = 2.25
		crystallite = 1.25
	}
	outputs = {
		-- was 1
		advanced_components = 2
	}
	-- was 4 then 7
	perhour = 14
    editable = false
}

recipe_computer_chips_factory_base = {
	name = "Computer Chip Production"
	inputs = {
		silicon = 2
		copper = 2.5
		gold = 0.25
	}
	outputs = {
		computer_chips = 1.5
	}
	-- was 1.5
	perhour = 15
    editable = false
}

recipe_televisions_factory_base = {
	name = "Television Production"
	inputs = {
		computer_chips = 8
		steel = 2
		plastic = 20
	}
	outputs = {
		televisions = 1
	}
	perhour = 15
    editable = false
}

recipe_cars_factory_base = {
	name = "Car Production"
	inputs = {
		computer_chips = 4
		steel = 5
		plastic = 40
		aluminium = 80
		crystallite = 6
	}
	outputs = {
		cars = 1
	}
	perhour = 5
    editable = false
}

recipe_normal_ammo_factory_base = {
	name = "Normal Ammo Production"
	inputs = {
		lead = 1
		steel = 0.25
		copper = 0.25
	}
	outputs = {
		normal_ammo = 1
	}
	perhour = 50
	editable = false
}

recipe_crystallite_infused_ammo_factory_base = {
	name = "Crystallite Infused Ammo Production"
	inputs = {
		lead = 0.75
		steel = 0.25
		copper = 0.25
		crystallite = 0.25
	}
	outputs = {
		crystallite_infused_ammo = 1
	}
	perhour = 50
	editable = false
}

recipe_105mm_artillery_shell_factory_base = {
	name = "105mm Artillery Shell Production"
	inputs = {
		brass = 10
		lead = 5
		steel = 5
	}
	outputs = {
		artillery_shell_105mm = 1
	}
	perhour = 15
	editable = false
}

recipe_155mm_artillery_shell_factory_base = {
	name = "155mm Artillery Shell Production"
	inputs = {
		brass = 20
		lead = 10
		steel = 10
	}
	outputs = {
		artillery_shell_155mm = 1
	}
	perhour = 10
	editable = false
}

recipe_105mm_artillery_factory_base = {
	name = "105mm Artillery Production"
	inputs = {
		
	}
	outputs = {
		artillery_105mm = 1
	}
	perhour = 1
	editable = true
	buildingtype = "factory"
	edits = {
		attack = {
			name = "Attack"
			modifiers = {
				item.attack = {
					base = 1
					add = {
						base = 0.25
						factor = edit.level
					}
				}
			}
			costs = {
				steel = {
					base = 1
					factor = edit.level
					factor = {
						base = edit.level
						factor = 0.25
						add = 1
					}
				}
			}
		}
	}
}

recipe_tank_factory_base = {
	name = "Tank Production"
	inputs = {
		any_with_basetype = {
			id = "tank_engine"
			basetype = "tank_engine"
			required = true
			amount = 1
		}
	}
	outputs = {
		tank = 1
	}
	perhour = 1
	editable = true
	buildingtype = "factory"
	edits = {
		attack = {
			name = "Attack"

			-- these are NOT scaled to the edit's level
			modifiers = {
				item.attack = {
					base = 1
					add = {
						base = 0.25
						factor = edit.level
					}
				}
			}

			-- per level
			costs = {
				steel = {
					base = 1
					factor = edit.level
					factor = {
						base = edit.level
						factor = 0.25
						add = 1
					}
				}
			}
		}
	}
}

recipe_rifle_factory_base = {
	name = "Rifle Production"
	inputs = {
		steel = 0
	}
	outputs = {
		small_arms = 1
	}
	perhour = 1
	editable = true
	buildingtype = "factory"
	edits = {
		attack = {
			name = "Attack"

			-- these are NOT scaled to the edit's level
			modifiers = {
				item.attack = {
					base = 1
					add = {
						base = 0.25
						factor = edit.level
					}
				}
			}

			-- these are NOT scaled to the edit's level
			costs = {
				steel = {
					base = 2
					factor = 1.175 ^ edit.level
				}
			}
		}
	}
}