mass_migration.1 = {
	type = "District"
	title = "A large number of people are moving to {scope:random_province}!"

	check_feq = hourly

	chance = {
		-- about once per 30 days
		base = 0.0013

		-- if new country, double chance
		factor = {
			if = {
				limit = {
					nation.age < 30
				}
				base = 2
			}
		}
	}

	immediate = {
		random_scope_province = {
			effects = {
				add_modifier = { 
					name = "mass_migration"
					decay = true
					duration = 168
				}
			}
		}
	}
}