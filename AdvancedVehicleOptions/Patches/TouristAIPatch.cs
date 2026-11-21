using System;
using HarmonyLib;

namespace AdvancedVehicleOptionsUID.Patches
{
    /// <summary>
    /// Safety net for a missing null check in the game's TouristAI.GetVehicleInfo.
    ///
    /// In the camper branch (tourist arriving by car, camper roll positive) the game asks for a Level2 vehicle of the
    /// car class; if none exists it falls back to Level1 and then reads randomVehicleInfo.m_generatedInfo WITHOUT a
    /// null check. When every vehicle of that class is disabled (e.g. all four "Electric Car" assets plus the
    /// "Personal Electric Transport Ride", class Residential / ResidentialLowEco / Level1), VehicleManager returns null
    /// and the game throws a NullReferenceException. Reproduced with the AVO-NullProbe test mod: 426 of 8761 tourists.
    ///
    /// The finalizer turns exactly this exception into "no vehicle" (result and trailer null) - all callers of
    /// GetVehicleInfo handle null, the tourist then walks or uses public transport. It is logged once per session with
    /// the stack trace, so player logs show the cause instead of an unexplained NullReferenceException.
    /// </summary>
    [HarmonyPatch(typeof(TouristAI), "GetVehicleInfo",
        new Type[] { typeof(ushort), typeof(CitizenInstance), typeof(bool), typeof(VehicleInfo) },
        new ArgumentType[] { ArgumentType.Normal, ArgumentType.Ref, ArgumentType.Normal, ArgumentType.Out })]
    public static class TouristAIGetVehicleInfoPatch
    {
        private static bool s_reported;

        [HarmonyFinalizer]
        public static Exception Finalizer(Exception __exception, ref VehicleInfo __result, ref VehicleInfo trailer)
        {
            if (!(__exception is NullReferenceException)) return __exception;

            __result = null;
            trailer = null;

            if (!s_reported)
            {
                s_reported = true;
                Logging.Error("A tourist could not get a vehicle because a vehicle class has no enabled vehicle " +
                              "(typically all electric cars incl. \"Personal Electric Transport Ride\" disabled). " +
                              "The game's NullReferenceException in TouristAI.GetVehicleInfo was suppressed; the tourist travels without a car. " +
                              "Reported once per session.");
                Logging.LogException(__exception, "suppressed exception (for reference)");
            }

            return null;
        }
    }
}
