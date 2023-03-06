recipe_iron_smeltery_base = {
	name = "Iron Smelting"
	inputs = {
		iron_ore = 1
		tools = 0.005
	}
	outputs = {
		iron = 1
	}
	perhour = 50
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
	perhour = 50
    editable = false
}

recipe_steel_factory_base = {
	name = "Steel Production"
	inputs = {
		coal = 2
		iron = 4
		tools = 0.1
	}
	outputs = {
		steel = 2.5
	}
	perhour = 3
    editable = false
}

recipe_simple_components_factory_base = {
	name = "Simple Components Production"
	inputs = {
		iron = 1
		silicon = 1
		copper = 1
	}
	outputs = {
		simple_components = 1
	}
	perhour = 6
    editable = false
}

recipe_advanced_components_factory_base = {
	name = "Advanced Components Production"
	inputs = {
		simple_components = 4
		steel = 4
		crystallite = 1
	}
	outputs = {
		advanced_components = 1
	}
	perhour = 1
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
			modifiers = {
				item = {
					item.attack = 1
				}
			}
		}
	}
}