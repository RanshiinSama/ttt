using Dalamud.Interface;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TargetLines
{
    internal class ImGuiUtils
    {
        public static bool WrapBegin(string name, ImGuiWindowFlags flags, Action fn) {
            bool began = false;

            ImGuiHelpers.ForceNextWindowMainViewport();
            ImGui.SetNextWindowPos(ImGui.GetMainViewport().Pos);
            ImGui.SetNextWindowSize(ImGui.GetMainViewport().Size);
            if (ImGui.Begin(name, flags)) {
                began = true;
                fn();
                ImGui.End();
            }

            return began;
        }
    }
}
