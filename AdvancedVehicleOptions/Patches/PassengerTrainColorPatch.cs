using System;
using System.Collections.Generic;
using ColossalFramework.Math;
using HarmonyLib;
using UnityEngine;
using AdvancedVehicleOptionsUID.Compatibility;

namespace AdvancedVehicleOptionsUID.Patches
{
    /// <summary>
    /// Makes AVO's vehicle colours work again for passenger trains without a transport line (e.g. trains of outside
    /// connections).
    ///
    /// The game's PassengerTrainAI.GetColor never uses the prefab colours m_color0..3 that AVO edits: in the normal view
    /// it returns the line colour, or - for trains without a line - the global default train colour
    /// (TransportManager.m_properties.m_transportColors[Train]). Trailers ask the engine. Metro trains inherit this.
    /// Cargo trains are not affected (TrainAI -> VehicleAI.GetColor already uses the prefab colours).
    ///
    /// The postfix only changes the result if
    ///   - the view is the normal one (InfoMode.None),
    ///   - the train (its first vehicle) has no transport line - player lines keep their line colour,
    ///   - the colours of this vehicle's prefab were changed in AVO (differ from the game defaults), so players who never
    ///     touched train colours see no difference,
    ///   - neither Vehicle Color Expander nor Asset Color Expander is active (they manage colours themselves).
    /// The colour is chosen like VehicleAI.GetColor does it: one of color0..3 per vehicle if colour variations are on,
    /// otherwise color0. A trailer with changed colours uses its own colours, otherwise it keeps the engine's colour.
    /// </summary>
    [HarmonyPatch(typeof(PassengerTrainAI), "GetColor",
        new Type[] { typeof(ushort), typeof(Vehicle), typeof(InfoManager.InfoMode), typeof(InfoManager.SubInfoMode) },
        new ArgumentType[] { ArgumentType.Normal, ArgumentType.Ref, ArgumentType.Normal, ArgumentType.Normal })]
    public static class PassengerTrainColorPatch
    {
        // Prefab -> "colours changed in AVO". GetColor runs every frame for every visible train, so this is cached and
        // cleared whenever AVO changes a colour (VehicleOptions setters) or reloads the defaults.
        private static readonly Dictionary<VehicleInfo, bool> s_customColors = new Dictionary<VehicleInfo, bool>();
        private static readonly object s_lock = new object();
        private static int s_colorModActive = -1;   // -1 = not checked yet

        internal static void InvalidateCache()
        {
            lock (s_lock) s_customColors.Clear();
        }

        [HarmonyPostfix]
        public static void Postfix(ushort vehicleID, ref Vehicle data, InfoManager.InfoMode infoMode, ref Color __result)
        {
            try
            {
                if (infoMode != InfoManager.InfoMode.None) return;
                if (IsColorModActive()) return;

                VehicleInfo info = data.Info;
                if (info == null) return;

                ushort first = data.m_leadingVehicle == 0 ? vehicleID : data.GetFirstVehicle(vehicleID);
                if (VehicleManager.instance.m_vehicles.m_buffer[first].m_transportLine != 0) return;

                if (!HasCustomColors(info)) return;

                if (info.m_useColorVariations)
                {
                    switch (new Randomizer(vehicleID).Int32(4u))
                    {
                        case 1: __result = info.m_color1; return;
                        case 2: __result = info.m_color2; return;
                        case 3: __result = info.m_color3; return;
                        default: __result = info.m_color0; return;
                    }
                }
                __result = info.m_color0;
            }
            catch
            {
                // Never break rendering - keep the game's colour
            }
        }

        private static bool HasCustomColors(VehicleInfo info)
        {
            lock (s_lock)
            {
                if (!s_customColors.TryGetValue(info, out bool custom))
                {
                    custom = DefaultOptions.HasCustomColors(info);
                    s_customColors[info] = custom;
                }
                return custom;
            }
        }

        private static bool IsColorModActive()
        {
            if (s_colorModActive == -1)
            {
                bool active = false;
                try { active = VCXCompatibilityPatch.IsVCXActive() || VCXCompatibilityPatch.IsACXActive(); }
                catch { }
                s_colorModActive = active ? 1 : 0;
                if (active) Logging.KeyMessage("Vehicle/Asset Color Expander active - AVO train colours for trains without a line are left to that mod");
            }
            return s_colorModActive == 1;
        }
    }
}
