using UnityEngine;
namespace Blooper.MenuGenerator.Runtime
{
	[System.Serializable]
	public class MenuOption
	{
		public string optionName;
		public MenuOptionType optionType;
		
		public string scene;

		public Sprite image;
		public MenuOption(string optionName, MenuOptionType optionType)
		{
			this.optionName = optionName;
			this.optionType = optionType;
			this.scene = "";
		}

		private bool EditorShowOptionButton()
		{
			return optionType != MenuOptionType.Text;
		}

		public static MenuOption QuitGameMenuOption => new MenuOption("Quit", MenuOptionType.QuitButton);
		public static MenuOption CloseButtonMenuOption => new MenuOption("Close", MenuOptionType.ClosePanel);
		
	}
}