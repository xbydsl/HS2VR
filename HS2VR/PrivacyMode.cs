using UnityEngine;
using UnityEngine.UI;

namespace HS2VR
{
	public static class PrivacyMode
	{
		private static GameObject privacyObject;
		private static Canvas canvas;

		public static void Enable()
		{
			privacyObject = new GameObject("PrivacyMode");
			
			canvas = privacyObject.AddComponent<Canvas>();
			canvas.renderMode = RenderMode.ScreenSpaceOverlay;
			canvas.sortingOrder = 30000;

			var overlayObject = new GameObject("Overlay");
			overlayObject.transform.SetParent(privacyObject.transform);

			var image = overlayObject.AddComponent<Image>();
			image.rectTransform.sizeDelta = new Vector2(Screen.width * 4, Screen.height * 4);
			image.color = Color.black;
			
			Object.DontDestroyOnLoad(privacyObject);
		}
		
		public static bool Check(Canvas other)
		{
			return canvas != null && canvas == other;
		}
	}
}