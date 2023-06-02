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
		simple_components = 3
		steel = 3
		crystallite = 1
	}
	outputs = {
		advanced_components = 1
	}
	-- was 4
	perhour = 6
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

recipe_small_arms_factory_base = {
	name = "Small Arms Production"
	inputs = {
		steel = 5
	}
	outputs = {
		small_arms = 1
	}
	perhour = 1
	editable = true
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
					factor = edits.level
					factor = {
						base = edits.level
						factor = 0.25
						add = 1
					}
				}
			}
		}
	}
}