using Dalamud.Game.ClientState;
using Dalamud.Game.Command;
using Dalamud.Game.Gui;
using Dalamud.Interface.Windowing;
using FFXIVClientStructs.FFXIV.Common.Component.BGCollision;
using System;
using System.Numerics;

namespace TargetLines
{
    internal class Globals
    {
        public static Configuration Config;
        public static ChatGui Chat;
        public static ClientState ClientState;
        public static CommandManager CommandManager;
        public static PluginCommandManager<Plugin> PluginCommandManager;
        public static WindowSystem WindowSystem;
        public static unsafe CameraManager* CameraManager;
        public static double Runtime = 0.0;
        public static ImGuiScene.TextureWrap LineTexture;
        public static ImGuiScene.TextureWrap EdgeTexture;

        public static unsafe Vector3 WorldCamera_GetPos() {
            if (CameraManager == null) {
                return new Vector3(0, 0, 0);
            }
            if (CameraManager->WorldCamera == null) {
                return new Vector3(0, 0, 0);
            }

            return new Vector3(CameraManager->WorldCamera->X, CameraManager->WorldCamera->Z, CameraManager->WorldCamera->Y);
        }

        public static unsafe Vector3 WorldCamera_GetLookAtPos() {
            if (CameraManager == null)
            {
                return new Vector3(0, 0, 0);
            }
            if (CameraManager->WorldCamera == null)
            {
                return new Vector3(0, 0, 0);
            }

            return new Vector3(CameraManager->WorldCamera->LookAtX, CameraManager->WorldCamera->LookAtZ, CameraManager->WorldCamera->LookAtY);
        }

        public static unsafe Vector3 WorldCamera_GetForward() {
            if (CameraManager == null) {
                return new Vector3(0, 0, 1);
            }
            if (CameraManager->WorldCamera == null) {
                return new Vector3(0, 0, 1);
            }



            return Vector3.Normalize(WorldCamera_GetPos() - WorldCamera_GetLookAtPos());
        }

        public static unsafe bool IsVisible(FFXIVClientStructs.FFXIV.Common.Math.Vector3 position, bool occlusion) {
            FFXIVClientStructs.FFXIV.Common.Math.Vector3 cam = WorldCamera_GetPos();
            FFXIVClientStructs.FFXIV.Common.Math.Vector3 forward = WorldCamera_GetForward();
            FFXIVClientStructs.FFXIV.Common.Math.Vector3 to_camera = cam - position;
            FFXIVClientStructs.FFXIV.Client.System.Framework.Framework* framework = FFXIVClientStructs.FFXIV.Client.System.Framework.Framework.Instance();

            // behind camera
            if (Vector3.Dot(to_camera, forward) < 0) {
                return false;
            }

            if (occlusion) {
                var direction = position - cam;
                var length = direction.Magnitude;

                if (length != 0) {
                    var flags = stackalloc int[] { 0x4000, 0x4000 }; // should probably figure out what these mean
                    var hit = stackalloc RaycastHit[1];
                    direction = direction.Normalized;
                    var result = framework->BGCollisionModule->RaycastEx(hit, cam, direction, length, 1, flags);
                    return result == false;
                }

                return false;
            }
            else {
                return true;
            }
        }
    }
}
