using Dalamud.Game.ClientState.Objects.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TargetLines
{
    internal class ObjectTableHelper {
        public static GameObjectHelper GetObjectByID(uint id) {
            GameObjectHelper gobj = null;
            foreach (GameObject obj in Service.ObjectTable) {
                if (obj.ObjectId == id) {
                    gobj = new GameObjectHelper(obj);
                    return gobj;
                    break;
                }
            }

            return gobj;
        }
    }
}
