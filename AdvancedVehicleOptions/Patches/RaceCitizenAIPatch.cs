using ColossalFramework;
using HarmonyLib;

namespace AdvancedVehicleOptionsUID.Patches
{
    [HarmonyPatch]
    public static class RaceCitizenAIPatch
    {
        [HarmonyPatch(typeof(RaceCitizenAI), "SimulationStep")]
        [HarmonyPrefix]
        public static void SimulationStep(ushort instanceID, ref CitizenInstance citizenData, ref CitizenInstance.Frame frameData, bool lodPhysics)
        {
            Building building = Singleton<BuildingManager>.instance.m_buildings.m_buffer[citizenData.m_sourceBuilding];
            EventData eventData = Singleton<EventManager>.instance.m_events.m_buffer[building.m_eventIndex];

            if(eventData.m_raceEventData == null) return;

            var citizen = Singleton<CitizenManager>.instance.m_citizens.m_buffer[citizenData.m_citizen];
            if (citizen.m_vehicle == 0) return;

            Vehicle race_vehicle = Singleton<VehicleManager>.instance.m_vehicles.m_buffer[citizen.m_vehicle];

            if (race_vehicle.Info == null) return;

            if ((race_vehicle.Info.vehicleCategory & VehicleInfo.VehicleCategory.Bicycle) == 0) return;

            ref RacerData racerData = ref eventData.m_raceEventData.m_racerData[citizenData.m_racerIndex];
            racerData.m_maxSpeed = race_vehicle.Info.m_maxSpeed;
            racerData.m_acceleration = race_vehicle.Info.m_acceleration;
        }
    }
}
