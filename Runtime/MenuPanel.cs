using System;
using UnityEngine;

namespace Blooper.MenuGenerator.Runtime
{
	public class MenuPanel : MonoBehaviour
	{
		private bool _open = true;

		private void Awake()
		{
			Close();
		}

		public virtual void Open(bool animate = false)
		{
			if (_open)
			{
				return;
			}
			
			SetChildrenActive(true);
			_open = true;
		}

		public virtual void Close(bool animate = false)
		{
			if (!_open)
			{
				return;
			}
			
			SetChildrenActive(false);
			_open = false;
		}

		private void SetChildrenActive(bool active)
		{
			foreach (Transform child in transform)
			{
				child.gameObject.SetActive(active);
			}
		}
	}
}