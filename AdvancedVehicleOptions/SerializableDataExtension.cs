using System.IO;
using System.Linq;
using ColossalFramework.IO;
using ICities;

namespace AdvancedVehicleOptionsUID
{
    public class SerializableDataExtension : SerializableDataExtensionBase
    {
        private const string ID = "AVO";
        private const int VERSION = 4;

        public override void OnLoadData()
        {
            if (ToolManager.instance.m_properties.m_mode != ItemClass.Availability.Game)
            {
                return;
            }

            // Storing default values ASAP (before any mods have the time to change values)
            DefaultOptions.StoreAll();

            if (!serializableDataManager.EnumerateData().Contains(ID))
            {
                return;
            }

            var data = serializableDataManager.LoadData(ID);
            try
            {
                using (var ms = new MemoryStream(data))
                {
                    AdvancedVehicleOptions.config.data = DataSerializer.Deserialize<Configuration>(ms, DataSerializer.Mode.Memory).data;
                }
            }
            catch (System.Exception e)
            {
                // A failed config load must not take the whole level down with a ModException.
                // Known cause: two copies of this mod enabled at once (e.g. workshop + local
                // build) - the save stores assembly-qualified type names and the deserializer
                // then resolves Configuration in the wrong assembly, which surfaces as an
                // InvalidCastException. Falling back to defaults keeps the game playable.
                Logging.Error("Failed to deserialize AVO save data; continuing with default vehicle options. " +
                              "If this repeats, check that only one copy of Advanced Vehicle Options is enabled.");
                Logging.LogException(e);
            }
        }

        public override void OnSaveData()
        {
            if (ToolManager.instance.m_properties.m_mode != ItemClass.Availability.Game ||
                AdvancedVehicleOptions.config == null)
            {
                return;
            }
            using (var ms = new MemoryStream())
            {
                DataSerializer.Serialize(ms, DataSerializer.Mode.Memory, VERSION, AdvancedVehicleOptions.config);
                var data = ms.ToArray();
                serializableDataManager.SaveData(ID, data);
            }
        }
    }
}