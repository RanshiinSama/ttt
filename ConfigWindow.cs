using Dalamud.Interface.Windowing;
using ImGuiNET;
using System;
using System.Numerics;

namespace TargetLines
{
    internal class ConfigWindow : Window, IDisposable
    {
        public static string ConfigWindowName = "Target Lines Config";

        private Vector4 color_pp;
        private Vector4 color_pe;
        private Vector4 color_ep;
        private Vector4 color_ee;
        private Vector4 color_o;
        private Vector4 color_ol;

        public ConfigWindow() : base(ConfigWindowName) { }

        public override void Draw() {
            bool should_save = false;

            should_save |= ImGui.Checkbox("Only show target lines during combat", ref Globals.Config.saved.OnlyInCombat);
            should_save |= ImGui.Checkbox("Only show target lines when unsheathed", ref Globals.Config.saved.OnlyUnsheathed);
            should_save |= ImGui.Checkbox("Only show target lines when target is you", ref Globals.Config.saved.OnlyTargetingPC);
            should_save |= ImGui.Checkbox("Occlusion Culling (Always Enabled for enemies!)", ref Globals.Config.saved.OcclusionCulling);
            should_save |= ImGui.Checkbox("Use solid color instead of texture", ref Globals.Config.saved.SolidColor);
            if (Globals.Config.saved.SolidColor == false) {
                should_save |= ImGui.Checkbox("Fade line as it approaches target", ref Globals.Config.saved.FadeToEnd);
                if (Globals.Config.saved.FadeToEnd) {
                    should_save |=  ImGui.SliderFloat("End point opacity %", ref Globals.Config.saved.FadeToEndScalar, 0.0f, 1.0f);
                }

                should_save |= ImGui.SliderInt("Texture Smoothness Steps", ref Globals.Config.saved.TextureCurveSampleCount, 2, 512);
            }
            else {
                ImGui.Spacing(); ImGui.Spacing();
            }

            should_save |= ImGui.SliderFloat("Arc Scale", ref Globals.Config.saved.ArcHeightScalar, 0.0f, 2.0f);
            should_save |= ImGui.SliderFloat("Line Thickness", ref Globals.Config.saved.LineThickness, 0.0f, 64.0f);
            if (Globals.Config.saved.SolidColor == true) {
                should_save |= ImGui.SliderFloat("Outline Thickness", ref Globals.Config.saved.OutlineThickness, 0.0f, 72.0f);
            }
            else {
                ImGui.Spacing();
            }
            should_save |= ImGui.SliderFloat("New Target Easing Time", ref Globals.Config.saved.NewTargetEaseTime, 0.0f, 5.0f);
            should_save |= ImGui.SliderFloat("No Target Fading Time", ref Globals.Config.saved.NoTargetFadeTime, 0.0f, 5.0f);
            should_save |= ImGui.SliderFloat("Player Arc Height Bump", ref Globals.Config.saved.PlayerHeightBump, 0.0f, 10.0f);
            should_save |= ImGui.SliderFloat("Enemy Arc Height Bump", ref Globals.Config.saved.EnemyHeightBump, 0.0f, 10.0f);

            should_save |= ImGui.SliderFloat("Alpha Fade Amplitude", ref Globals.Config.saved.WaveAmplitudeOffset, 0.0f, 0.5f);
            should_save |= ImGui.SliderFloat("Alpha Frequency", ref Globals.Config.saved.WaveFrequencyScalar, 0.0f, 10.0f);

            color_pp = Globals.Config.saved.PlayerPlayerLineColor.Color;
            if (ImGui.ColorEdit4("Player -> Player Line Color", ref color_pp)) {
                Globals.Config.saved.PlayerPlayerLineColor.Color = color_pp;
                should_save = true;
            }

            color_pe = Globals.Config.saved.PlayerEnemyLineColor.Color;
            if (ImGui.ColorEdit4("Player -> Enemy Line Color", ref color_pe)) {
                Globals.Config.saved.PlayerEnemyLineColor.Color = color_pe;
                should_save = true;
            }

            color_ep = Globals.Config.saved.EnemyPlayerLineColor.Color;
            if (ImGui.ColorEdit4("Enemy -> Player Line Color", ref color_ep)) {
                Globals.Config.saved.EnemyPlayerLineColor.Color = color_ep;
                should_save = true;
            }

            color_ee = Globals.Config.saved.EnemyEnemyLineColor.Color;
            if (ImGui.ColorEdit4("Enemy -> Enemy Line Color", ref color_ee)) {
                Globals.Config.saved.EnemyEnemyLineColor.Color = color_ee;
                should_save = true;
            }

            color_o = Globals.Config.saved.OtherLineColor.Color;
            if (ImGui.ColorEdit4("Other Line Color", ref color_o)) {
                Globals.Config.saved.OtherLineColor.Color = color_o;
                should_save = true;
            }

            color_ol = Globals.Config.saved.OutlineColor.Color;
            if (ImGui.ColorEdit4("Outline Color", ref color_ol)) {
                Globals.Config.saved.OutlineColor.Color = color_ol;
                should_save = true;
            }

            if (ImGui.Button("Reset To Default")) {
                Globals.Config.saved = new SavedConfig();
                should_save = true;
            }

            if (should_save) {
                Globals.Config.Save();
            }
        }

        public void Dispose() { }
    }
}
