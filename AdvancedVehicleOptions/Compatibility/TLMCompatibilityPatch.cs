using System.Collections.Generic;
using System.Linq;
using ColossalFramework.Plugins;

namespace AdvancedVehicleOptionsUID.Compatibility
{
    public class TLMCompatibilityPatch
    {
        public static bool IsTLMActive()
        {
            return PluginManager.instance.GetPluginsInfo().Where(x => x.isEnabled)
                .Any(plugin => plugin.publishedFileID.AsUInt64 == 1312767991 || plugin.publishedFileID.AsUInt64 == 3007903394);
        }

        public static List<string> AVOFields = new List<string>
        {
            "m_maxSpeed",
            "m_enabled",
            "m_capacity"
        };
    }
}