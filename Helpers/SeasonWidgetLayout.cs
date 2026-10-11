using EFT.UI;
using MoxoPixel.MenuOverhaul.Infrastructure.Diagnostics;
using MoxoPixel.MenuOverhaul.Utils;
using UnityEngine;

namespace MoxoPixel.MenuOverhaul.Helpers
{
    /// <summary>
    /// Places the EFT 1.x season widget (MenuScreen._seasonWidget), which otherwise overlaps the custom button column.
    /// The widget is moved under the full-screen MenuScreen so "top right" is the screen corner; its original
    /// placement is remembered so the Default mode can restore it.
    /// </summary>
    internal static class SeasonWidgetLayout
    {
        public const string ModeTopRight = "Top Right (Compact)";
        public const string ModeDefault = "Default";
        public const string ModeHidden = "Hidden";

        private static readonly Vector2 TopRightMargin = new Vector2(40f, 40f);

        private static MenuScreen _menuScreen;
        private static RectTransform _widgetRect;
        private static Transform _originalParent;
        private static int _originalSiblingIndex;
        private static Vector2 _originalAnchorMin;
        private static Vector2 _originalAnchorMax;
        private static Vector2 _originalPivot;
        private static Vector2 _originalAnchoredPosition;
        private static Vector3 _originalScale;
        private static bool _hiddenByMod;

        public static void Apply(MenuScreen menuScreen)
        {
            if (menuScreen != null)
            {
                _menuScreen = menuScreen;
            }

            if (_menuScreen == null || _menuScreen._seasonWidget == null)
            {
                return;
            }

            RectTransform widgetRect = _menuScreen._seasonWidget.transform.TryCast<RectTransform>();
            if (widgetRect == null)
            {
                MenuDiagnosticsLogger.WarningOnce(LogSubsystem.Layout, "Layout.SeasonWidgetRect", "Season widget has no RectTransform; leaving it in place.");
                return;
            }

            CaptureOriginal(widgetRect);

            string mode = Settings.SeasonWidgetMode.Value;
            if (mode == ModeHidden)
            {
                if (widgetRect.gameObject.activeSelf)
                {
                    widgetRect.gameObject.SetActive(false);
                    _hiddenByMod = true;
                }
                return;
            }

            if (_hiddenByMod)
            {
                widgetRect.gameObject.SetActive(true);
                _hiddenByMod = false;
            }

            if (mode == ModeDefault)
            {
                RestoreOriginal();
                return;
            }

            float scale = Mathf.Clamp(Settings.SeasonWidgetScale.Value, 0.3f, 1f);
            widgetRect.SetParent(_menuScreen.transform, false);
            widgetRect.SetAsLastSibling();
            widgetRect.anchorMin = Vector2.one;
            widgetRect.anchorMax = Vector2.one;
            widgetRect.pivot = Vector2.one;
            widgetRect.anchoredPosition = -TopRightMargin;
            widgetRect.localScale = new Vector3(scale, scale, 1f);
        }

        private static void CaptureOriginal(RectTransform widgetRect)
        {
            if (_widgetRect == widgetRect)
            {
                return;
            }

            _widgetRect = widgetRect;
            _originalParent = widgetRect.parent;
            _originalSiblingIndex = widgetRect.GetSiblingIndex();
            _originalAnchorMin = widgetRect.anchorMin;
            _originalAnchorMax = widgetRect.anchorMax;
            _originalPivot = widgetRect.pivot;
            _originalAnchoredPosition = widgetRect.anchoredPosition;
            _originalScale = widgetRect.localScale;
        }

        private static void RestoreOriginal()
        {
            if (_widgetRect == null)
            {
                return;
            }

            if (_originalParent != null && _widgetRect.parent != _originalParent)
            {
                _widgetRect.SetParent(_originalParent, false);
                _widgetRect.SetSiblingIndex(_originalSiblingIndex);
            }

            _widgetRect.anchorMin = _originalAnchorMin;
            _widgetRect.anchorMax = _originalAnchorMax;
            _widgetRect.pivot = _originalPivot;
            _widgetRect.anchoredPosition = _originalAnchoredPosition;
            _widgetRect.localScale = _originalScale;
        }
    }
}
