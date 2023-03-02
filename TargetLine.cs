using ImGuiNET;
using System;
using System.Numerics;

namespace TargetLines
{
    internal class TargetLine
    {
        public GameObjectHelper ThisObject;
        public Vector3 LastTargetPosition = new Vector3();
        public ulong LastTargetId = 0;
        public bool Switching = false;
        public bool had_target = false;
        public float LivingTime = 0.0f;
        public float DyingTime = 0.0f;
        public float DeadTime = 0.0f;

        private float LastHeight = 0.0f;
        private float LastYOffset = 0.0f;

        public TargetLine(GameObjectHelper obj) {
            ThisObject = obj;
            if (ThisObject.TargetObject != null) {
                LastTargetId = ThisObject.TargetObject.TargetObjectId;
                LastTargetPosition = ThisObject.TargetObject.Position;
            }
            else {
                LastTargetPosition = ThisObject.Position;
            }
        }

        Vector2 EvaluateCubic(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, float t) {
            float t2 = t * t;
            float t3 = t2 * t;
            float mt = 1 - t;
            float mt2 = mt * mt;
            float mt3 = mt2 * mt;
            Vector2 point =
                mt3 * p0 +
                3 * mt2 * t * p1 +
                3 * mt * t2 * p2 +
                t3 * p3;
            return point;
        }

        Vector2 EvaluateQuadratic(Vector2 p0, Vector2 p1, Vector2 p2, float t) {
            float t2 = t * t;
            float mt = 1 - t;
            float mt2 = mt * mt;
            Vector2 point =
                mt2 * p0 +
                2 * mt * t * p1 +
                t2 * p2;
            return point;
        }

        public unsafe void Draw() {
            FFXIVClientStructs.FFXIV.Client.System.Framework.Framework* framework = FFXIVClientStructs.FFXIV.Client.System.Framework.Framework.Instance(); ;
            ImDrawListPtr drawlist = ImGui.GetWindowDrawList();
            GameObjectHelper target = null;
            Vector3 mypos = ThisObject.Position;
            Vector3 tpos = new Vector3();
            Vector3 midpos = new Vector3();
            ABGR linecolor = new ABGR(0, 0, 0, 0);
            ABGR outlinecolor = new ABGR(0, 0, 0, 0);
            float height = ThisObject.HitboxRadius; // for midpoint
            float alpha = (1.0f - Globals.Config.saved.WaveAmplitudeOffset) + (float)Math.Cos(Globals.Runtime * Globals.Config.saved.WaveFrequencyScalar) * Globals.Config.saved.WaveAmplitudeOffset;
            bool usequad = false;
            bool easing = false;
            bool has_target = ThisObject.TargetObject != null;

            if (has_target) {
                if (ThisObject.TargetObject.IsValid() == false) {
                    has_target = false;
                }
            }

            linecolor.CopyValues(Globals.Config.saved.OtherLineColor);
            outlinecolor.CopyValues(Globals.Config.saved.OutlineColor);

            if (!has_target) {
                DeadTime += framework->FrameDeltaTime;
                if (DyingTime > Globals.Config.saved.NoTargetFadeTime) {
                    LastTargetId = 0;
                    return;
                }

                alpha = 1.0f - (DyingTime / Globals.Config.saved.NoTargetFadeTime);
                if (alpha < 0.0f) {
                    alpha = 0.0f;
                }

                tpos = Vector3.Lerp(LastTargetPosition, ThisObject.Position, 1.0f - alpha);
                easing = true;
                DyingTime += framework->FrameDeltaTime;
                LivingTime = 0.0f;
            }
            else if (has_target) {
                target = new GameObjectHelper(ThisObject.TargetObject);
                tpos = target.Object.Position;
                if (ThisObject.TargetObjectId != LastTargetId && had_target) {
                    Switching = true;
                    LivingTime = 0.0f;
                }

                if (Switching && LivingTime < Globals.Config.saved.NewTargetEaseTime) {
                    tpos = LastTargetPosition;
                }
                if (Switching && LivingTime >= Globals.Config.saved.NewTargetEaseTime) {
                    Switching = false;
                }

                height += target.Object.HitboxRadius;
                LivingTime += framework->FrameDeltaTime;
                DyingTime = 0.0f;
                DeadTime = 0.0f;
                LastTargetId = target.Object.ObjectId;
                LastYOffset = target.RealObject->Height + target.HitboxRadius;
            }

            had_target = has_target;

            if (has_target && LivingTime < Globals.Config.saved.NewTargetEaseTime) {
                easing = true;
                alpha = LivingTime / Globals.Config.saved.NewTargetEaseTime;
                if (alpha > 1.0f) {
                    alpha = 1.0f;
                }
                if (Switching) {
                    tpos = Vector3.Lerp(tpos, target.Object.Position, alpha);
                }
                else {
                    tpos = Vector3.Lerp(mypos, tpos, alpha);
                }
                
            }
            midpos = (mypos + tpos) * 0.5f;

            height *= 0.5f;
            height = (float)Math.Sqrt(height * height);
            if (easing) {
                float start = alpha;
                float end = height;

                if (Switching) {
                    start = LastHeight;
                }

                if (!has_target) {
                    end = 0.0f;
                }

                height = MathUtils.Lerpf(start, height, alpha);
            }

            if (!easing) {
                LastTargetPosition = tpos;
                LastHeight = height;
            }

            if (ThisObject.IsPlayerCharacter()) {
                if (target != null && target.IsPlayerCharacter()) {
                    linecolor.CopyValues(Globals.Config.saved.PlayerPlayerLineColor);
                }
                else if (target != null && target.IsBattleChara()) {
                    linecolor.CopyValues(Globals.Config.saved.PlayerEnemyLineColor);
                }
                midpos.Y += Globals.Config.saved.PlayerHeightBump;
            }
            else if (ThisObject.IsBattleChara()) {
                if (target != null && target.IsPlayerCharacter()) {
                    linecolor.CopyValues(Globals.Config.saved.EnemyPlayerLineColor);
                }
                else if (target != null && target.IsBattleChara()) {
                    linecolor.CopyValues(Globals.Config.saved.EnemyEnemyLineColor);
                }
                midpos.Y += Globals.Config.saved.EnemyHeightBump;
                usequad = true;
            }

            if (Switching) {
                alpha = 1.0f;
            }

            linecolor.a = (byte)((float)linecolor.a * alpha);
            if (easing) {
                outlinecolor.a = (byte)((float)outlinecolor.a * alpha);
            }

            Vector3 camera_pos = Globals.WorldCamera_GetPos();
            Vector3 forward = Globals.WorldCamera_GetForward();
            Vector3 point_to_target = Vector3.Normalize(tpos - camera_pos);
            float angle = (float)Math.Acos(Vector3.Dot(forward, point_to_target));
            float deg90 = 90.0f * MathUtils.DEG2RAD;

            height *= MathUtils.Lerpf(0.0f, 1.0f, angle / deg90);
            midpos.Y += (height * Globals.Config.saved.ArcHeightScalar);

            if (has_target && target.IsBattleChara() && !target.IsPlayerCharacter()) {
#if (PROBABLY_BAD)
                if (!target.IsVisible(Globals.Config.saved.OcclusionCulling)) return;
#else
                if (!target.IsVisible(true)) return;
#endif
            }

            if (!ThisObject.IsVisible(Globals.Config.saved.OcclusionCulling) && !Globals.IsVisible(tpos + new Vector3(0.0f, LastYOffset, 0.0f), Globals.Config.saved.OcclusionCulling)) {
                return;
            }

            Service.Gui.WorldToScreen(mypos, out Vector2 my_screen_pos);
            Service.Gui.WorldToScreen(tpos, out Vector2 t_screen_pos);
            Service.Gui.WorldToScreen(midpos, out Vector2 mid_screen_pos);

            if (Globals.Config.saved.SolidColor) {
                if (usequad) {
                    if (Globals.Config.saved.OutlineThickness > 0) {
                        drawlist.AddBezierQuadratic(my_screen_pos, mid_screen_pos, t_screen_pos, outlinecolor.GetRaw(), Globals.Config.saved.OutlineThickness);
                    }
                    if (Globals.Config.saved.LineThickness> 0) {
                        drawlist.AddBezierQuadratic(my_screen_pos, mid_screen_pos, t_screen_pos, linecolor.GetRaw(), Globals.Config.saved.LineThickness);
                    }
                }
                else {
                    if (Globals.Config.saved.OutlineThickness > 0) {
                        drawlist.AddBezierCubic(my_screen_pos, mid_screen_pos, mid_screen_pos, t_screen_pos, outlinecolor.GetRaw(), Globals.Config.saved.OutlineThickness);
                    }
                    if (Globals.Config.saved.LineThickness > 0) {
                        drawlist.AddBezierCubic(my_screen_pos, mid_screen_pos, mid_screen_pos, t_screen_pos, linecolor.GetRaw(), Globals.Config.saved.LineThickness);
                    }
                }
            }
            else {
                Vector2[] points = new Vector2[Globals.Config.saved.TextureCurveSampleCount];
                Vector2 uv1;
                Vector2 uv2;

                for (int index = 0; index < Globals.Config.saved.TextureCurveSampleCount; index++) {
                    float t = (float)index / (Globals.Config.saved.TextureCurveSampleCount - 1);
                    if (usequad) {
                        points[index] = EvaluateQuadratic(my_screen_pos, mid_screen_pos, t_screen_pos, t);
                    }
                    else {
                        points[index] = EvaluateCubic(my_screen_pos, mid_screen_pos, mid_screen_pos, t_screen_pos, t);
                    }
                }

                for (int index = 0; index < Globals.Config.saved.TextureCurveSampleCount - 1; index++) {
                    ABGR linecolor_index = new ABGR(0, 0, 0, 0);
                    Vector2 p1 = points[index];
                    Vector2 p2 = points[index + 1];
                    uv1 = new Vector2(0, 0);
                    uv2 = new Vector2(1.0f, 1.0f);

                    Vector2 dir = Vector2.Normalize(p2 - p1);
                    Vector2 perp = new Vector2(-dir.Y, dir.X);
                    Vector2 p1_perp = p1 + perp * Globals.Config.saved.LineThickness * 2.0f;
                    Vector2 p2_perp = p2 + perp * Globals.Config.saved.LineThickness * 2.0f;
                    Vector2 p1_perp_inv = p1 - perp * Globals.Config.saved.LineThickness * 2.0f;
                    Vector2 p2_perp_inv = p2 - perp * Globals.Config.saved.LineThickness * 2.0f;

                    linecolor_index.CopyValues(linecolor);

                    if (Globals.Config.saved.FadeToEnd) {
                        linecolor_index.a = (byte)MathUtils.Lerpf((float)linecolor_index.a,
                            (float)(linecolor_index.a * Globals.Config.saved.FadeToEndScalar),
                            (float)index / (float)(Globals.Config.saved.TextureCurveSampleCount - 1)
                        );
                    }

                    drawlist.AddImageQuad(Globals.LineTexture.ImGuiHandle, p1_perp_inv, p2_perp_inv, p2_perp, p1_perp, uv1, new Vector2(uv1.X, uv2.Y), uv2, new Vector2(uv2.X, uv1.Y), linecolor_index.GetRaw());
                }

                Vector2 start_dir = Vector2.Normalize(points[1] - points[0]);
                Vector2 end_dir = Vector2.Normalize(points[Globals.Config.saved.TextureCurveSampleCount - 1] - points[Globals.Config.saved.TextureCurveSampleCount - 2]);
                Vector2 start_perp = new Vector2(-start_dir.Y, start_dir.X) * Globals.Config.saved.LineThickness * 2.0f;
                Vector2 end_perp = new Vector2(-end_dir.Y, end_dir.X) * Globals.Config.saved.LineThickness * 2.0f;
                uv1 = new Vector2(0, 0);
                uv2 = new Vector2(1.0f, 1.0f);

                Vector2 start_p1 = points[0] - start_perp;
                Vector2 start_p2 = points[0] + start_perp;
                Vector2 end_p1 = points[Globals.Config.saved.TextureCurveSampleCount - 1] - end_perp;
                Vector2 end_p2 = points[Globals.Config.saved.TextureCurveSampleCount - 1] + end_perp;

                ABGR linecolor_end = new ABGR(0, 0, 0, 0);
                linecolor_end.CopyValues(linecolor);
                if (Globals.Config.saved.FadeToEnd) {
                    linecolor_end.a = (byte)(linecolor_end.a * Globals.Config.saved.FadeToEndScalar);
                }

                drawlist.AddImage(Globals.EdgeTexture.ImGuiHandle, start_p1, start_p2, uv1, uv2, linecolor.GetRaw());
                drawlist.AddImage(Globals.EdgeTexture.ImGuiHandle, end_p1, end_p2, uv1, uv2, linecolor.GetRaw());
            }
        }
    }
}
