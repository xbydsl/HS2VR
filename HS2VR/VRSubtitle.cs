using Manager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using VRGIN.Core;
using Resources = UnityEngine.Resources;
using Scene = UnityEngine.SceneManagement.Scene;

namespace HS2VR
{
    // Code 'borrowed' from the KK Plugins Subtitles project...just needed enough differences for the VR plugin to make a fork needed

    public class VRSubtitle
    {
        internal static GameObject Pane;
        internal static Scene ActiveScene => SceneManager.GetActiveScene();

        internal const float WorldScale = 10f;

        private static void InitGUI()
        {
            if (Pane != null)
                return;


            Pane = new GameObject("KK_Subtitles_Caption");

            var cscl = Pane.GetOrAddComponent<CanvasScaler>();
            cscl.uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize;
      //      cscl.referenceResolution = new Vector2(Screen.width, Screen.height);

            var canvas = Pane.GetOrAddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 500;
            Pane.GetOrAddComponent<CanvasGroup>().blocksRaycasts = false;

            var vlg = Pane.GetOrAddComponent<VerticalLayoutGroup>();
            vlg.childForceExpandHeight = false;
            vlg.childForceExpandWidth = false;
            vlg.childAlignment = TextAnchor.LowerCenter;
            vlg.padding = new RectOffset(10, 10, 10, 10);

            UpdateScene(ActiveScene);
        }

        public static void DisplayVRSubtitle(GameObject voice, string text)
        {
            DisplayVRSubtitle(text, Color.magenta, new Color(0, 0, 0, 1f), onDestroy => voice.OnDestroyAsObservable().Subscribe(obj => onDestroy()));
        }

        public static void DisplayVRSubtitle(string text, Color textColor, Color outlineColor, Action<Action> onDestroy)
        {
            if (text.IsNullOrWhiteSpace()) return;
            InitGUI();

            Font fontFace = (Font)Resources.GetBuiltinResource(typeof(Font), "Arial.ttf");
            int fsize = -5;
            fsize = (int)(fsize < 0 ? (fsize * (Screen.height) / -100.0) : fsize);

            GameObject subtitle = new GameObject("SubtitleText");
            subtitle.transform.SetParent(Pane.transform, false);

            var rect = subtitle.GetOrAddComponent<RectTransform>();
            rect.pivot = new Vector2(0.5f, 0);
            rect.sizeDelta = new Vector2(Screen.width * .5f * 0.990f, fsize + (fsize * 0.05f));

            var subtitleText = subtitle.GetOrAddComponent<Text>();
            subtitleText.font = fontFace;
            subtitleText.fontSize = fsize;
            subtitleText.fontStyle = fontFace.dynamic ? UnityEngine.FontStyle.Bold : UnityEngine.FontStyle.Normal;
            subtitleText.alignment = TextAnchor.LowerCenter;
            subtitleText.horizontalOverflow = HorizontalWrapMode.Wrap;
            subtitleText.verticalOverflow = VerticalWrapMode.Overflow;
            subtitleText.color = textColor;

            var effectDistance = new Vector2(1.5f, -1.5f);
            var subOutline = subtitle.GetOrAddComponent<Outline>();
            subOutline.effectColor = outlineColor;
            subOutline.effectDistance = effectDistance;
            var subShadow = subtitle.GetOrAddComponent<Shadow>();
            subShadow.effectColor = outlineColor;
            subShadow.effectDistance = effectDistance;

            subtitleText.text = text;
            VRLog.Info(text);

            onDestroy(() => VRPlugin.Destroy(subtitle));
        }

        internal static void UpdateScene(Scene newScene)
        {
            if (newScene == null)
                return;
            
            SceneManager.MoveGameObjectToScene(Pane, newScene);
        }
    }
}
