using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Objects.Types;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Common.Component.BGCollision;
using FFXIVClientStructs.FFXIV.Common.Math;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TargetLines
{
    internal unsafe class GameObjectHelper {
        public GameObject Object;

        public GameObjectHelper(GameObject obj) {
            Object = obj;
        }

        public bool IsBattleChara() {
            return Object is BattleChara;
        }

        public bool IsPlayerCharacter() {
            return Object.ObjectKind == ObjectKind.Player;
        }

        public bool IsVisible(bool occlusion) {
            Vector3 safePos = Position;
            safePos.Y += RealObject->Height + HitboxRadius;

            if (RealObject->Scale == 0.0f) {
                return false;
            }

            return Globals.IsVisible(safePos, occlusion);
        }

        public unsafe bool IsTargetable() {
            return RealObject->GetIsTargetable();
        }

        public unsafe FFXIVClientStructs.FFXIV.Client.Game.Object.GameObject* RealObject {
            get { return (FFXIVClientStructs.FFXIV.Client.Game.Object.GameObject*)Object.Address; }
        }

        public ObjectKind Kind {
            get { return Object.ObjectKind; }
        }

        public Vector3 Position {
            get { return Object.Position; }
        }

        public float Rotation {
            get {  return Object.Rotation; }
        }

        public float HitboxRadius {
            get { return Object.HitboxRadius; }
        }

        public ulong TargetObjectId {
            get { return Object.TargetObjectId; }
        }

        public GameObject? TargetObject {
            get { return Object.TargetObject; }
        }

        public BattleChara BattleChara {
            get { return Object as BattleChara; }
        }

        public PlayerCharacter PlayerCharacter {
            get { return Object as PlayerCharacter; }
        }
    }
}
