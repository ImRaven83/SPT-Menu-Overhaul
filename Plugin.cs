using BepInEx.Logging;
using BepInEx;
using BepInEx.Unity.IL2CPP;
using MoxoPixel.MenuOverhaul.Patches;
using MoxoPixel.MenuOverhaul.Utils;
using MoxoPixel.MenuOverhaul.Helpers;
using MoxoPixel.MenuOverhaul.Infrastructure.Reflection;
using MoxoPixel.MenuOverhaul.Infrastructure.Lifecycle;
using MoxoPixel.MenuOverhaul.Infrastructure.Diagnostics;
using System.Collections.Generic;
using System;
using SPTushonka.Reflection.Il2Cpp;
using SPTushonka.Reflection.Patching;

namespace MoxoPixel.MenuOverhaul
{
    [BepInPlugin(MenuOverhaulConstants.Plugin.Guid, MenuOverhaulConstants.Plugin.Name, MenuOverhaulConstants.Plugin.Version)]
    public class Plugin : BasePlugin
    {
        public static ManualLogSource LogSource { get; private set; }

        private readonly List<ModulePatch> _patches = new List<ModulePatch>();

        public override void Load()
        {
            LogSource = Log;
            Settings.Init(Config);

            // Async continuations (await) must resume on the Unity main thread under IL2CPP.
            // Install is a no-op when SPT has already installed the dispatcher.
            try
            {
                MainThread.Install(this);
            }
            catch (Exception ex)
            {
                LogSource.LogWarning($"Could not install main-thread dispatcher: {ex.Message}");
            }

            // Managed MonoBehaviours (e.g. PlayerModelDragRotator) must be registered with IL2CPP before AddComponent.
            Il2CppInjection.RegisterAll(typeof(Plugin).Assembly, LogSource);

            EftReflectionMap.Initialize();
            MenuDiagnosticsLogger.Info(LogSubsystem.Reflection, EftReflectionMap.GetStartupSummary());
            if (EftReflectionMap.HasMissingMembers())
            {
                foreach (string missingMember in EftReflectionMap.GetMissingMembers())
                {
                    MenuDiagnosticsLogger.WarningOnce(
                        LogSubsystem.Reflection,
                        "Reflection.StartupMissing." + missingMember,
                        "Missing reflection member at startup: " + missingMember);
                }
            }
            else
            {
                MenuDiagnosticsLogger.Info(LogSubsystem.Reflection, "Integrity check passed: all mapped members resolved.");
            }

            InitializeAndEnablePatches();

            LogSource.LogInfo($"Plugin {MenuOverhaulConstants.Plugin.Name} version {MenuOverhaulConstants.Plugin.Version} loaded.");
        }

        private void InitializeAndEnablePatches()
        {
            _patches.Add(new MainMenuLayoutPatchAdapter());
            _patches.Add(new PlayerProfileViewPatchAdapter());
            _patches.Add(new DefaultUIButtonIdlePatchAdapter());
            _patches.Add(new DefaultUIButtonHoverPatchAdapter());
            _patches.Add(new OnGameStartedPatch());
            _patches.Add(new OnGameEndedPatch());

            foreach (var patch in _patches)
            {
                try
                {
                    patch.Enable();
                }
                catch (Exception ex)
                {
                    LogSource.LogError($"Failed to enable patch {patch.GetType().Name}: {ex}");
                }
            }
        }
        
        public override bool Unload()
        {
            CleanupResources();
            return true;
        }
        
        private void CleanupResources()
        {
            try
            {
                MenuLifecycleCoordinator.CleanupOnUnload();
                MainMenuLayoutRuntime.DisposeResources();
                MainMenuLightingService.Cleanup();
                GameStateUtility.ResetGameState();
                
                LogSource.LogDebug("MenuOverhaul plugin resources cleaned up.");
            }
            catch (Exception ex)
            {
                LogSource.LogError($"Error during plugin cleanup: {ex}");
            }
        }
    }
}