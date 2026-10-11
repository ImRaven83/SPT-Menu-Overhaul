using EFT.UI;
using MoxoPixel.MenuOverhaul.Infrastructure.Diagnostics;
using MoxoPixel.MenuOverhaul.Utils;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace MoxoPixel.MenuOverhaul.Infrastructure.Reflection
{
    internal static class EftReflectionMap
    {
        public const string DefaultButtonIdleMethodKey = "DefaultUIButtonAnimation.DefaultButtonIdleMethod";
        public const string DefaultButtonHighlightedMethodKey = "DefaultUIButtonAnimation.DefaultButtonHighlightedMethod";
        public const string NormalIconColorFieldKey = "DefaultUIButtonAnimation.NormalIconColorField";
        public const string NormalLabelColorFieldKey = "DefaultUIButtonAnimation.NormalLabelColorField";
        public const string NormalImageColorFieldKey = "DefaultUIButtonAnimation.NormalImageColorField";
        public const string BackgroundNormalStateAlphaFieldKey = "DefaultUIButtonAnimation.BackgroundNormalStateAlphaField";
        public const string HighlightedIconColorFieldKey = "DefaultUIButtonAnimation.HighlightedIconColorField";
        public const string HighlightedImageColorFieldKey = "DefaultUIButtonAnimation.HighlightedImageColorField";
        public const string MenuScreenPlayButtonFieldKey = "MenuScreen.PlayButtonField";
        public const string MenuScreenWarningFieldKey = "MenuScreen.WarningField";

        public static MethodInfo DefaultButtonIdleMethod { get; private set; }
        public static MethodInfo DefaultButtonHighlightedMethod { get; private set; }

        public static PropertyInfo NormalIconColorField { get; private set; }
        public static PropertyInfo NormalLabelColorField { get; private set; }
        public static PropertyInfo NormalImageColorField { get; private set; }
        public static PropertyInfo BackgroundNormalStateAlphaField { get; private set; }
        public static PropertyInfo HighlightedIconColorField { get; private set; }
        public static PropertyInfo HighlightedImageColorField { get; private set; }
        public static PropertyInfo MenuScreenPlayButtonField { get; private set; }
        public static PropertyInfo MenuScreenWarningField { get; private set; }

        private static readonly HashSet<string> WarnedMissingMembers = new HashSet<string>();

        private static readonly List<string> MissingMembers = new List<string>();
        private static readonly List<string> ResolvedMembers = new List<string>();

        public static bool IsInitialized { get; private set; }

        public static void Initialize()
        {
            if (IsInitialized)
            {
                return;
            }

            MissingMembers.Clear();
            ResolvedMembers.Clear();
            WarnedMissingMembers.Clear();

            DefaultButtonIdleMethod = ResolveMethod(
                typeof(DefaultUIButtonAnimation),
                MenuOverhaulConstants.Reflection.DefaultButtonIdleMethod,
                DefaultButtonIdleMethodKey);

            DefaultButtonHighlightedMethod = ResolveMethod(
                typeof(DefaultUIButtonAnimation),
                MenuOverhaulConstants.Reflection.DefaultButtonHighlightedMethod,
                DefaultButtonHighlightedMethodKey);

            NormalIconColorField = ResolveField(
                typeof(DefaultUIButtonAnimation),
                MenuOverhaulConstants.Reflection.NormalIconColorField,
                NormalIconColorFieldKey);

            NormalLabelColorField = ResolveField(
                typeof(DefaultUIButtonAnimation),
                MenuOverhaulConstants.Reflection.NormalLabelColorField,
                NormalLabelColorFieldKey);

            NormalImageColorField = ResolveField(
                typeof(DefaultUIButtonAnimation),
                MenuOverhaulConstants.Reflection.NormalImageColorField,
                NormalImageColorFieldKey);

            BackgroundNormalStateAlphaField = ResolveField(
                typeof(DefaultUIButtonAnimation),
                MenuOverhaulConstants.Reflection.BackgroundNormalStateAlphaField,
                BackgroundNormalStateAlphaFieldKey);

            HighlightedIconColorField = ResolveField(
                typeof(DefaultUIButtonAnimation),
                MenuOverhaulConstants.Reflection.HighlightedIconColorField,
                HighlightedIconColorFieldKey);

            HighlightedImageColorField = ResolveField(
                typeof(DefaultUIButtonAnimation),
                MenuOverhaulConstants.Reflection.HighlightedImageColorField,
                HighlightedImageColorFieldKey);

            MenuScreenPlayButtonField = ResolveField(
                typeof(MenuScreen),
                MenuOverhaulConstants.MenuScreen.PlayButtonField,
                MenuScreenPlayButtonFieldKey);

            MenuScreenWarningField = ResolveField(
                typeof(MenuScreen),
                MenuOverhaulConstants.MenuScreen.WarningField,
                MenuScreenWarningFieldKey);

            IsInitialized = true;
        }

        public static void WarnIfMissing(MemberInfo member, string memberKey)
        {
            if (member != null)
            {
                return;
            }

            if (!WarnedMissingMembers.Add(memberKey))
            {
                return;
            }

            MenuDiagnosticsLogger.Warning(LogSubsystem.Reflection, $"Missing member used at runtime: {memberKey}");
        }

        public static string GetStartupSummary()
        {
            return $"Resolved {ResolvedMembers.Count} members, missing {MissingMembers.Count}.";
        }

        public static bool HasMissingMembers()
        {
            return MissingMembers.Count > 0;
        }

        public static IReadOnlyList<string> GetMissingMembers()
        {
            return MissingMembers;
        }

        public static bool TryGetMenuScreenField(string fieldName, out PropertyInfo field, out string fieldKey)
        {
            switch (fieldName)
            {
                case MenuOverhaulConstants.MenuScreen.PlayButtonField:
                    field = MenuScreenPlayButtonField;
                    fieldKey = MenuScreenPlayButtonFieldKey;
                    return true;
                case MenuOverhaulConstants.MenuScreen.WarningField:
                    field = MenuScreenWarningField;
                    fieldKey = MenuScreenWarningFieldKey;
                    return true;
                default:
                    field = null;
                    fieldKey = null;
                    return false;
            }
        }

        private static MethodInfo ResolveMethod(Type type, string methodName, string memberKey)
        {
            MethodInfo method = type.GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public);
            TrackResolution(method, memberKey);
            return method;
        }

        // Under IL2CPP the Il2CppInterop proxy assemblies expose every native field (private ones included)
        // as a public instance property with the original field name, so "fields" are resolved as properties.
        private static PropertyInfo ResolveField(Type type, string fieldName, string memberKey)
        {
            PropertyInfo field = type.GetProperty(fieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            TrackResolution(field, memberKey);
            return field;
        }

        private static void TrackResolution(MemberInfo member, string memberKey)
        {
            if (member != null)
            {
                ResolvedMembers.Add(memberKey);
                return;
            }

            MissingMembers.Add(memberKey);
            WarnIfMissing(member, memberKey);
        }
    }
}
