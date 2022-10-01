using System;
using UnityEngine;
using UnityEngine.UI;

namespace Blooper.MenuGenerator.Runtime
{
	[RequireComponent(typeof(Button))]
	public class MenuButton : MonoBehaviour
	{
		public Action OnButtonPressed;
		private MenuManager _manager;
		public MenuOptionType optionType;
		private MenuPanel panel;
		private string sceneName;
		
		private Button _button;
		private void Awake()
		{
			_button = GetComponent<Button>();
			if (_button != null)
			{
				_button.onClick.AddListener(OnButtonClick);
			}
			else
			{
				//don't need to throw a warning in else because of [RequireComponent] attribute.
			}
			//
			if (_manager == null)
			{
				//this should be set by generation OR by inspector, but if it somehow still isnt
				_manager = GetComponentInParent<MenuManager>();//we try our best, useful for quickly testing.
				Debug.LogWarning("Menu Button should have Menu Manager assigned",this);
			}
		}

		public MenuButton SetPanel(MenuPanel panel)
		{
			this.panel = panel;
			return this;
		}

		public MenuButton SetFromOption(MenuOption option)
		{
			sceneName = option.scene;
			optionType = option.optionType;
			//option.optionName;
			
			return this;//for chaining
		}
		public MenuButton SetMenuManager(MenuManager manager)
		{
			_manager = manager;
			return this;
		}
		private void OnButtonClick()
		{
			switch (optionType)
			{
				case MenuOptionType.SubPanel:
					_manager.OpenPanel(panel);
					break;
				case MenuOptionType.LoadSceneButton:
					_manager.GoToScene(sceneName);
					break;
				case MenuOptionType.ClosePanel:
					_manager.ClosePanel(panel);
					break;
				case MenuOptionType.QuitButton:
					Application.Quit();
					break;
				//case MenuOptionType.JustAButton://handled by the button component or our action!
				//case MenuOptionType.Text, .image, etc;//not our problem!
			}

			OnButtonPressed?.Invoke();
		}
	}
}