using ColossalFramework;
using HarmonyLib;

namespace AdvancedVehicleOptionsUID.Patches
{
    [HarmonyPatch]
    public static class RaceCarAIPatch
    {
        [HarmonyPatch(typeof(RaceCarAI), "SimulationStep", [typeof(ushort), typeof(Vehicle), typeof(Vehicle.Frame), typeof(ushort), typeof(Vehicle), typeof(int)], 
            [ArgumentType.Normal, ArgumentType.Ref, ArgumentType.Ref, ArgumentType.Normal, ArgumentType.Ref, ArgumentType.Normal])]
        [HarmonyPrefix]
        public static void SimulationStep(ushort vehicleID, ref Vehicle vehicleData, ref Vehicle.Frame frameData, ushort leaderID, ref Vehicle leaderData, int lodPhysics)
        {
            Building building = Singleton<BuildingManager>.instance.m_buildings.m_buffer[vehicleData.m_sourceBuilding];
            EventData eventData = Singleton<EventManager>.instance.m_events.m_buffer[building.m_eventIndex];
            ref RacerData racerData = ref eventData.m_raceEventData.m_racerData[vehicleData.m_racerIndex];
            racerData.m_maxSpeed = vehicleData.Info.m_maxSpeed;
            racerData.m_acceleration = vehicleData.Info.m_acceleration;
            racerData.m_braking = vehicleData.Info.m_braking;
            racerData.m_turning = vehicleData.Info.m_turning;
        }
    }
}
