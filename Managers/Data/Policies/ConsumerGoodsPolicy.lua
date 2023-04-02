consumer_goods_policy = {
	name = "Consumer Goods Policy"
	type = "province"
	default = normal
	options = {
		wartime_rationing = {
			modifiers = {
				province.consumergoods.consumptionfactor = -0.5
				province.consumergoods.modifierfactor = -0.75
			}
		}
		low_rationing = {
			modifiers = {
				province.consumergoods.consumptionfactor = -0.25
				province.consumergoods.modifierfactor = -0.4
			}
		}
		normal = {
			modifiers = {

			}
		}
		baby_boom = {
			modifiers = {
				province.consumergoods.consumptionfactor = 1.5
				province.consumergoods.modifierfactor = 0.75
			}
		}
	}
}