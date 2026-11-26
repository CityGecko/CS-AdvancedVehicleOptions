using System;
using System.Collections.Generic;
using System.Linq;
using ColossalFramework.Plugins;

namespace AdvancedVehicleOptionsUID.Compatibility
{
    public class NoBigTruckCompatibilityPatch
    {
        // Workshop IDs of No Big Truck: original (2069057130) and beta (2513186434).
        // No Big Truck Revisited gets a new ID after its upload - add it here when known.
        private static readonly ulong[] NBTWorkshopIDs = { 2069057130UL, 2513186434UL };

        // All No Big Truck variants (original, beta, Revisited, local copies without Workshop ID) ship NoBigTruck.dll
        private const string NBTAssemblyName = "NoBigTruck";

        // DO NOT RENAME: No Big Truck patches this method (Harmony prefix -> true).
        public static bool IsNBTActive()
        {
            return PluginManager.instance.GetPluginsInfo().Where(x => x.isEnabled)
                .Any(plugin => NBTWorkshopIDs.Contains(plugin.publishedFileID.AsUInt64) || HasNBTAssembly(plugin));
        }

        public static bool IsNBTBetaActive()
        {
            return PluginManager.instance.GetPluginsInfo().Where(x => x.isEnabled)
                .Any(plugin => plugin.publishedFileID.AsUInt64 == 2513186434);
        }

        private static bool HasNBTAssembly(PluginManager.PluginInfo plugin)
        {
            try
            {
                return plugin.GetAssemblies().Any(a => a != null && a.GetName().Name == NBTAssemblyName);
            }
            catch (Exception)
            {
                return false;   // a broken plugin must not stop the options page from opening
            }
        }

        public static List<string> AVOFields = new List<string>
        {
            "m_isLargeVehicle",
        };
    }
}
