using Dalamud.Configuration;
using Dalamud.Plugin;
using Dalamud.Logging;
using System;
using System.Collections.Generic;
using FFXIVClientStructs.FFXIV.Common.Math;

namespace TargetLines
{
    public class ABGR {
        public byte a;
        public byte b;
        public byte g;
        public byte r;

        public ABGR(byte A, byte B, byte G, byte R) {
            a = A;
            b = B;
            g = G;
            r = R;
        }

        public Vector4 Color {
            get {
                return new Vector4((float)r / 255.0f, (float)g / 255.0f, (float)b / 255.0f, (float)a / 255.0f);
            }
            set {
                a = (byte)(value.W * 255.0f);
                b = (byte)(value.Z * 255.0f);
                g = (byte)(value.Y * 255.0f);
                r = (byte)(value.X * 255.0f);
            }
        }

        public void CopyValues(ABGR from) {
            a = from.a;
            b = from.b;
            g = from.g;
            r = from.r;
        }

        public uint GetRaw() {
            return (uint)((a << 24) | (b << 16) | (g << 8) | r);
        }
    }

    public class SavedConfig {
        public float ArcHeightScalar = 0.4f;
        public float PlayerHeightBump = 0.0f;
        public float EnemyHeightBump = 0.0f;
        public float LineThickness = 16.0f;
        public float OutlineThickness = 20.0f;
        public float NewTargetEaseTime = 0.25f;
        public float NoTargetFadeTime = 0.25f;
        public float WaveAmplitudeOffset = 0.175f;
        public float WaveFrequencyScalar = 3.0f;
        public float FadeToEndScalar = 0.2f;
        public ABGR PlayerPlayerLineColor = new ABGR(0xC0, 0x00, 0xFF, 0x00);
        public ABGR PlayerEnemyLineColor = new ABGR(0x80, 0x40, 0x00, 0xC0);
        public ABGR EnemyPlayerLineColor = new ABGR(0xC0, 0x00, 0x00, 0xFF);
        public ABGR EnemyEnemyLineColor = new ABGR(0x80, 0x00, 0xC0, 0x80);
        public ABGR OtherLineColor = new ABGR(0x80, 0x00, 0xBF, 0xFF);
        public ABGR OutlineColor = new ABGR(0x80, 0x00, 0x00, 0x00);
        public int TextureCurveSampleCount = 48;
        public bool OnlyInCombat = false;
        public bool OnlyUnsheathed = false;
        public bool OnlyTargetingPC = false;
        public bool SolidColor = false;
        public bool FadeToEnd = true;
        public bool ToggledOff = false;
        public bool OcclusionCulling = false;
    }

    public class Configuration : IPluginConfiguration {
        int IPluginConfiguration.Version { get; set; }

        #region Saved configuration values
        public SavedConfig saved = new SavedConfig();
        #endregion

        private DalamudPluginInterface PluginInterface;

        public void Initialize(DalamudPluginInterface pluginInterface) {
            PluginInterface = pluginInterface;
        }

        public void Save() {
            PluginInterface.SavePluginConfig(this);
        }
    }
}
