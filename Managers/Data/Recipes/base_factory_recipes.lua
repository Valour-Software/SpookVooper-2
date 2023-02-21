recipe_iron_smeltery_base = {
	inputs = {
		iron_ore = 1
		tools = 0.005
	}
	outputs = {
		iron = 1
	}
	perhour = 50
    editable = false
    simple = true
}

recipe_steel_factory_base = {
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
	simple = true
}

recipe_simple_components_factory_base = {
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
	simple = true
}

recipe_advanced_components_factory_base = {
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
	simple = false
}