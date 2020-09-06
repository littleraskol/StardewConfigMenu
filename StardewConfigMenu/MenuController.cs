using System;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;
using System.Collections.Generic;
using StardewConfigFramework;
using System.IO;

namespace StardewConfigMenu {
	public delegate void ModDidAddPackage(IOptionsPackage package);

	public class MenuController: IConfigMenu {
		public static int? PageIndex = null;
		private IModHelper MyHelper;
		private IMonitor MyMonitor;
		private event ModDidAddPackage ModDidAddPackage;

		internal MenuController(IModHelper helper, IMonitor monitor) {
			MyHelper = helper;
			MyMonitor = monitor;
			helper.Events.Display.MenuChanged += MenuOpened;
			helper.Events.Display.MenuChanged += MenuClosed;
		}


	//internal SettingsPage page;
	internal MenuTab Tab;
		internal MenuPage Page;

		internal List<IOptionsPackage> OptionPackageList = new List<IOptionsPackage>();

		public void AddOptionsPackage(IOptionsPackage package) {
			// Only one per mod, remove old one
			var existingPackage = OptionPackageList.Find(x => x.ModManifest.UniqueID == package.ModManifest.UniqueID);

			if (existingPackage != null)
				OptionPackageList.Remove(existingPackage);

			OptionPackageList.Add(package);
			MyMonitor.Log($"{package.ModManifest.Name} has added its mod options");
			ModDidAddPackage?.Invoke(package);
		}

		/// <summary>
		/// Removes the delegates that handle the button click and draw method of the tab
		/// </summary>
		private void MenuClosed(object sender, MenuChangedEventArgs e) {
			//To do: merge the events into one structured with this conditional
			if (!(e.NewMenu == null)) return;

			MyHelper.Events.Display.RenderedActiveMenu -= RenderTab;
			//MyHelper.Events.Display.RenderingActiveMenu -= HandleJunimo;

			if (Tab != null) {
				Tab.RemoveListeners();
				Tab = null;
			}

			if (Page != null) {
				if (e.OldMenu is GameMenu) {
					List<IClickableMenu> pages = MyHelper.Reflection.GetField<List<IClickableMenu>>((e.OldMenu as GameMenu), "pages").GetValue();
					pages.Remove(Page);
				}

				Page.RemoveListeners(true);
				Page = null;
				PageIndex = null;
			}
		}

		/// <summary>
		/// Attaches the delegates that handle the button click and draw method of the tab
		/// </summary>
		private void MenuOpened(object sender, MenuChangedEventArgs e)
		{
			//To do: merge the events into one structured with this conditional
			if (e.NewMenu == null) return;

			// copied from MenuClosed
			MyHelper.Events.Display.RenderedActiveMenu -= RenderTab;
			//MyHelper.Events.Display.RenderingActiveMenu -= HandleJunimo;

			if (Tab != null) {
				Tab.RemoveListeners();
				Tab = null;
			}

			if (Page != null) {

				if (e.OldMenu is GameMenu) {
					List<IClickableMenu> oldpages = MyHelper.Reflection.GetField<List<IClickableMenu>>((e.OldMenu as GameMenu), "pages").GetValue();
					oldpages.Remove(Page);
				}

				Page.RemoveListeners(true);
				Page = null;
				PageIndex = null;
			}

			if (!(e.NewMenu is GameMenu)) {
				Tab = null;
				Page = null;
				PageIndex = null;
				return;
			}

			GameMenu menu = (GameMenu) e.NewMenu;
			List<IClickableMenu> pages = MyHelper.Reflection.GetField<List<IClickableMenu>>(menu, "pages").GetValue();

			var options = pages.Find(x => { return x is OptionsPage; });
			int width = options.width;

			Page = new MenuPage(OptionPackageList, menu.xPositionOnScreen, menu.yPositionOnScreen, width, menu.height);
			PageIndex = pages.Count;
			pages.Add(Page);

			bool infoSuiteInstalled = MyHelper.ModRegistry.IsLoaded("Cdaragorn.UiInfoSuite");
			int tabLocation = infoSuiteInstalled ? 9 : 11;
			Tab = new MenuTab(MyHelper, new Rectangle(menu.xPositionOnScreen + Game1.tileSize * tabLocation, menu.yPositionOnScreen + IClickableMenu.tabYPositionRelativeToMenuY + Game1.tileSize, Game1.tileSize, Game1.tileSize));

			MyHelper.Events.Display.RenderedActiveMenu -= RenderTab;
			MyHelper.Events.Display.RenderedActiveMenu += RenderTab;
			//MyHelper.Events.Display.RenderingActiveMenu -= HandleJunimo;
			//MyHelper.Events.Display.RenderingActiveMenu += HandleJunimo;
		}

		private ClickableTextureComponent junimoNoteIconStorage;

		//private void HandleJunimo(object sender, RenderingActiveMenuEventArgs e) {
		//	if (!(Game1.activeClickableMenu is GameMenu))
		//		return;
		//
		//	var gameMenu = Game1.activeClickableMenu as GameMenu;
		//
		//	// Remove Community Center Icon from Options, Exit Game, and Mod Options pages
		//	if (gameMenu.currentTab == PageIndex || gameMenu.currentTab == 6 || gameMenu.currentTab == 7) {
		//		if (gameMenu.junimoNoteIcon != null) {
		//			junimoNoteIconStorage = gameMenu.junimoNoteIcon;
		//			gameMenu.junimoNoteIcon = null;
		//		}
		//	} else if (junimoNoteIconStorage != null) {
		//		gameMenu.junimoNoteIcon = junimoNoteIconStorage;
		//		junimoNoteIconStorage = null;
		//	}
		//}


		private void RenderTab(object sender, RenderedActiveMenuEventArgs e) {

			if (!(Game1.activeClickableMenu is GameMenu))
				return;

			var gameMenu = (GameMenu) Game1.activeClickableMenu;

			if (gameMenu.currentTab == GameMenu.mapTab) { return; }

			if (Tab != null)
				Tab.draw(Game1.spriteBatch);
		}
	}
}
