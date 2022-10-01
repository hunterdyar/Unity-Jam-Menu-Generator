using UnityEngine;
using UnityEngine.SceneManagement;

namespace Blooper.MenuGenerator.Runtime
{
	public class MenuManager : MonoBehaviour
	{
		[SerializeField] private MenuPanel[] _allSubPanels;
		[SerializeField] private MenuPanel _defaultPanel;
		private MenuPanel _currentlyOpenPanel;
		void Awake()
		{
			//if overrideInjection = true.
			_allSubPanels = GetComponentsInChildren<MenuPanel>();
		}

		private void Start()
		{
			foreach (var panel in _allSubPanels)
			{
				//Panels stay active and disable their own children instead. They should be empty.
				//But it's convenient to disable them in the inspector.
				//We handle that gracefully. Turn them back on, let them turn their own children off.
				panel.gameObject.SetActive(true);
			}
			ResetPanels();
		}

		public void GoToScene(string sceneName)
		{
			SceneManager.LoadScene(sceneName);
		}

		public void ResetPanels()
		{
			foreach (var menuPanel in _allSubPanels)
			{
				menuPanel.Close();
			}
			_defaultPanel.Open();
			_currentlyOpenPanel = _defaultPanel;
		}

		public void OpenPanel(MenuPanel panel)
		{
			foreach (var menuPanel in _allSubPanels)
			{
				if (menuPanel == panel)
				{
					menuPanel.Open(true);
					_currentlyOpenPanel = menuPanel;
				}
				else
				{
					menuPanel.Close();
				}
			}
		}

		public void ClosePanel(MenuPanel panel)
		{
			//do we need to loop through other panels?
			panel.Close(true);
			_defaultPanel.Open();
			_currentlyOpenPanel = _defaultPanel;
		}

		public void SetDefaultPanel(MenuPanel panel)
		{
			_defaultPanel = panel;
		}
		public void SetAllSubPanels(MenuPanel[] allPanels)
		{
			_allSubPanels = allPanels;
		}
	}
}