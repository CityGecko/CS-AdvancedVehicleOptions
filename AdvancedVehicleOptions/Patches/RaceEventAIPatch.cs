using ColossalFramework;
using HarmonyLib;

namespace AdvancedVehicleOptionsUID.Patches
{
    [HarmonyPatch]
    public static class RaceEventAIPatch
    {
        [HarmonyPatch(typeof(MotorRaceAI), "SpawnRacer")]
        [HarmonyPostfix]
        public static void SpawnRacerMotorPostfix(MotorRaceAI __instance, ushort eventID, ref EventData data, int index, bool __result)
        {
            if (!__result) return;

            ref var racer = ref data.m_raceEventData.m_racerData[index];

            if(racer != null && racer.m_racerID != 0)
            {
                Vehicle race_vehicle = Singleton<VehicleManager>.instance.m_vehicles.m_buffer[racer.m_racerID];

                if(race_vehicle.Info == null) return;

                racer.m_maxSpeed = race_vehicle.Info.m_maxSpeed;
                racer.m_acceleration = race_vehicle.Info.m_acceleration;
                racer.m_braking = race_vehicle.Info.m_braking;
                racer.m_turning = race_vehicle.Info.m_turning;
            }
        }

        [HarmonyPatch(typeof(BicycleRaceAI), "SpawnRacer")]
        [HarmonyPostfix]
        public static void SpawnRacerBicyclePostfix(BicycleRaceAI __instance, ushort eventID, ref EventData data, int index, bool __result)
        {
            if (!__result) return;

            ref var racer = ref data.m_raceEventData.m_racerData[index];

            if (racer != null && racer.m_racerID != 0)
            {
                var citizen_instance = Singleton<CitizenManager>.instance.m_instances.m_buffer[racer.m_racerID];

                if(citizen_instance.m_citizen == 0) return;

                var citizen = Singleton<CitizenManager>.instance.m_citizens.m_buffer[citizen_instance.m_citizen];
                if (citizen.m_vehicle == 0) return;

                Vehicle race_vehicle = Singleton<VehicleManager>.instance.m_vehicles.m_buffer[citizen.m_vehicle];

                if (race_vehicle.Info == null) return;

                racer.m_maxSpeed = race_vehicle.Info.m_maxSpeed;
                racer.m_acceleration = race_vehicle.Info.m_acceleration;
            }
        }
    }
}
