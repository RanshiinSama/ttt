using Dalamud.Game.ClientState;
using Dalamud.Game.Command;
using Dalamud.Game.Gui;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin;
using System;
using ImGuiNET;
using Dalamud.Game.ClientState.Objects.Types;
using System.Collections.Generic;
using Dalamud.Logging;
using Dalamud.Game.ClientState.Conditions;
using System.IO;
using TargetLines.Attributes;

[assembly: System.Reflection.AssemblyVersion("1.0.*")]

namespace TargetLines
{
    public class Plugin : IDalamudPlugin {
        public string Name => "TargetLines";
        private DalamudPluginInterface PluginInterface;

        private const ImGuiWindowFlags OVERLAY_WINDOW_FLAGS =
              ImGuiWindowFlags.NoBackground
            | ImGuiWindowFlags.NoDecoration
            | ImGuiWindowFlags.NoFocusOnAppearing
            | ImGuiWindowFlags.NoInputs
            | ImGuiWindowFlags.NoMove
            | ImGuiWindowFlags.NoSavedSettings
            | ImGuiWindowFlags.NoNav;

        private Dictionary<uint, TargetLine> TargetLineDict;

        public Plugin(DalamudPluginInterface pluginInterface, CommandManager commandManager, ChatGui chat, ClientState clientState) {
            PluginInterface = pluginInterface;
            Globals.Chat = chat;
            Globals.ClientState = clientState;

            Globals.Config = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
            Globals.Config.Initialize(PluginInterface);

            Globals.WindowSystem = new WindowSystem(typeof(Plugin).AssemblyQualifiedName);
            Globals.WindowSystem.AddWindow(new ConfigWindow());
            PluginInterface.UiBuilder.Draw += OnDraw;
            PluginInterface.UiBuilder.OpenConfigUi += ToggleConfig;
            PluginInterface.Create<Service>();

            Globals.CommandManager = commandManager;
            Globals.PluginCommandManager = new PluginCommandManager<Plugin>(this, commandManager);
            //Commands.Initialize();

            TargetLineDict = new Dictionary<uint, TargetLine>();
            InitializeCamera();

            var texture_line_path = Path.Combine(PluginInterface.AssemblyLocation.Directory?.FullName!, "Data/TargetLine.png");
            var texture_edge_path = Path.Combine(PluginInterface.AssemblyLocation.Directory?.FullName!, "Data/TargetEdge.png");
            Globals.LineTexture = PluginInterface.UiBuilder.LoadImage(texture_line_path);
            Globals.EdgeTexture = PluginInterface.UiBuilder.LoadImage(texture_edge_path);
        }

        [Command("/ptlines")]
        [HelpMessage("Toggle configuration window")]
        private void On_pbshadows(string command, string args) {
            ToggleConfig();
        }

        [Command("/ttl")]
        [HelpMessage("Toggle target line overlay")]
        private void On_ttl(string command, string args)
        {
            string str = "on";
            Globals.Config.saved.ToggledOff = !Globals.Config.saved.ToggledOff;

            if (Globals.Config.saved.ToggledOff) {
                str = "off";
            }

            Globals.Chat.Print($"Target Lines overlay toggled {str}");
        }

        private void ToggleConfig() {
            Globals.WindowSystem.GetWindow(ConfigWindow.ConfigWindowName).IsOpen = !Globals.WindowSystem.GetWindow(ConfigWindow.ConfigWindowName).IsOpen;
        }

        private unsafe void InitializeCamera() {
            Globals.CameraManager = (CameraManager*)Service.SigScanner.GetStaticAddressFromSig("4C 8D 35 ?? ?? ?? ?? 85 D2"); // g_ControlSystem_CameraManager
            PluginLog.Warning($"Camera Pointer {((IntPtr)Globals.CameraManager->WorldCamera).ToString("X")}");
        }

        private unsafe void DrawOverlay() {
            FFXIVClientStructs.FFXIV.Client.System.Framework.Framework* framework = FFXIVClientStructs.FFXIV.Client.System.Framework.Framework.Instance(); ;

            Globals.Runtime += framework->FrameDeltaTime;

            if (Globals.ClientState.LocalPlayer == null) {
                if (TargetLineDict != null) {
                    if (TargetLineDict.Count > 0) {
                        TargetLineDict.Clear();
                    }
                }
            }

            for (int index = 0; index < Service.ObjectTable.Length; index++) {
                GameObject obj = Service.ObjectTable[index];
                uint id;
                bool should_delete = false;
                if (obj != null) {
                    id = obj.ObjectId;
                    if (!obj.IsValid()) {
                        should_delete = true;
                    }

                    // delete keys that are unused for a bit
                    if (TargetLineDict.ContainsKey(id)) {
                        if (TargetLineDict[id].DeadTime > 20.0f) {
                            should_delete = true;
                        }
                    }

                    if (should_delete) {
                        if (TargetLineDict.ContainsKey(id)) {
                            TargetLineDict.Remove(id);
                        }
                        continue;
                    }

                    GameObjectHelper gobj = new GameObjectHelper(obj);

                    if (!TargetLineDict.ContainsKey(id)) {
                        TargetLineDict.Add(id, new TargetLine(gobj));
                    }

                    bool combat_flag = Service.Condition[ConditionFlag.InCombat];
                    bool doDraw = 
                        ((combat_flag && Globals.Config.saved.OnlyInCombat) || !Globals.Config.saved.OnlyInCombat)
                        && ((Globals.Config.saved.OnlyTargetingPC && TargetLineDict[id].ThisObject.TargetObjectId == Globals.ClientState.LocalPlayer.ObjectId)
                        || !Globals.Config.saved.OnlyTargetingPC);

#if (!PROBABLY_BAD)
                    if (Globals.ClientState.IsPvP) {
                        doDraw = True;
                    }
#endif

                    if (doDraw) {
                        bool condition = TargetLineDict[id].ThisObject.Object.IsValid();
#if (!PROBABLY_BAD)
                        condition = condition && TargetLineDict[id].ThisObject.IsTargetable();
#endif
                        if (condition) {
                            TargetLineDict[id].Draw();
                        }
                    }
                }
            }
        }

        private void OnDraw() {
            Globals.WindowSystem.Draw();

            if ((Globals.Config.saved.OnlyUnsheathed && (Globals.ClientState.LocalPlayer.StatusFlags & Dalamud.Game.ClientState.Objects.Enums.StatusFlags.WeaponOut) != 0) || !Globals.Config.saved.OnlyUnsheathed) {
                if (Globals.Config.saved.ToggledOff == false) {
                    ImGuiUtils.WrapBegin("##TargetLinesOverlay", OVERLAY_WINDOW_FLAGS, DrawOverlay);
                }
            }
            else {
                if (TargetLineDict != null) {
                    if (TargetLineDict.Count > 0) {
                        TargetLineDict.Clear();
                    }
                }
            }
        }

#region IDisposable Support
        protected virtual void Dispose(bool disposing) {
            if (!disposing) return;

            //Commands.Uninitialize();
            Globals.PluginCommandManager.Dispose();

            PluginInterface.SavePluginConfig(Globals.Config);

            PluginInterface.UiBuilder.Draw -= OnDraw;
            Globals.WindowSystem.RemoveAllWindows();

            Globals.LineTexture.Dispose();
            Globals.EdgeTexture.Dispose();
        }

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
#endregion
    }
}
