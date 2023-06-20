on_server_start = {
	effects = {
		district:novastella = {
			add_static_modifier_if_not_already_added = {
				name = "novastella_free_market"
			}
			province:34 = {
				add_static_modifier_if_not_already_added = {
					name = "vooperian_capital"
				}
			}
		}
		district:lanatia = {
			if = {
				limit = {
					hasstaticmodifier = "lanatia_lore"
				}
				effects = {
					remove_static_modifier = "lanatia_lore"
				}
			}
		}
		district:elysian_katonia = {
			add_static_modifier_if_not_already_added = {
				name = "elysian_katonia_chips_industry"
			}
		}
		district:old_king = {
			add_static_modifier_if_not_already_added = {
				name = "old_king_steel_industry"
			}
		}
	}
}