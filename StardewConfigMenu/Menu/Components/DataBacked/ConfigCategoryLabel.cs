﻿using StardewConfigFramework.Options;

namespace StardewConfigMenu.Components.DataBacked {

	internal class ConfigCategoryLabel: SCMCategoryLabel {
		private CategoryLabel ModData;

		public sealed override string Label => ModData.Label;

		public ConfigCategoryLabel(CategoryLabel option) : this(option, 0, 0) { }

		public ConfigCategoryLabel(CategoryLabel option, int x, int y) : base(option.Label, x, y) {
			ModData = option;
		}
	}
}
