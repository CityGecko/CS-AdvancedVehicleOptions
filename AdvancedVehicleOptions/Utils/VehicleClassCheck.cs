using System.Collections.Generic;
using System.Text;

namespace AdvancedVehicleOptionsUID
{
    /// <summary>
    /// Checks whether a game vehicle class (Service + SubService + Level) still has a vehicle the game can pick.
    ///
    /// The game does not pick vehicles by AVO category but by this class (VehicleManager.GetRandomVehicleInfo).
    /// AVO's category check (CheckServiceValidity) can therefore pass while a single class is empty - e.g. the
    /// "Citizen" category still has normal cars, but all electric cars (Residential / ResidentialLowEco / Level1) are
    /// disabled. Game code that does not check for null then throws (see Patches/TouristAIPatch.cs).
    ///
    /// Mirrors VehicleManager.RefreshTransferVehicles: a vehicle counts if its class service is not None and its
    /// placement style is Automatic (AVO disables vehicles by setting Manual). A level without own vehicles falls back
    /// to the next lower level of the same service/subService, so a class is empty only if no level up to and including
    /// its own has a usable vehicle.
    /// </summary>
    internal static class VehicleClassCheck
    {
        internal static bool HasUsableVehicle(ItemClass itemClass)
        {
            if (itemClass == null || itemClass.m_service == ItemClass.Service.None) return true;

            int count = PrefabCollection<VehicleInfo>.PrefabCount();
            for (int i = 0; i < count; i++)
            {
                VehicleInfo prefab = PrefabCollection<VehicleInfo>.GetPrefab((uint)i);
                if (prefab == null || prefab.m_class == null) continue;
                if (prefab.m_placementStyle != ItemClass.Placement.Automatic) continue;
                if (IsSameOrLowerLevel(prefab.m_class, itemClass)) return true;
            }
            return false;
        }

        /// <summary>
        /// Classes that had at least one vehicle by default but have none usable now - i.e. emptied by disabling
        /// vehicles in AVO. Classes the game never had vehicles for are not reported.
        /// </summary>
        internal static List<ItemClass> GetClassesEmptiedByAVO()
        {
            List<ItemClass> result = new List<ItemClass>();
            HashSet<string> seen = new HashSet<string>();

            int count = PrefabCollection<VehicleInfo>.PrefabCount();
            for (int i = 0; i < count; i++)
            {
                VehicleInfo prefab = PrefabCollection<VehicleInfo>.GetPrefab((uint)i);
                if (prefab == null || prefab.m_class == null || prefab.m_class.m_service == ItemClass.Service.None) continue;
                if (prefab.m_placementStyle == ItemClass.Placement.Automatic) continue;

                // Only vehicles AVO disabled: by default they were spawnable (Automatic)
                if (DefaultOptions.GetPlacementStyle(prefab) != ItemClass.Placement.Automatic) continue;

                if (!seen.Add(GetClassLabel(prefab.m_class))) continue;
                if (!HasUsableVehicle(prefab.m_class)) result.Add(prefab.m_class);
            }
            return result;
        }

        /// <summary>Technical class name for the log, e.g. "Residential / ResidentialLowEco / Level1".</summary>
        internal static string GetClassLabel(ItemClass itemClass)
        {
            if (itemClass == null) return "?";
            return itemClass.m_service + " / " + itemClass.m_subService + " / " + itemClass.m_level;
        }

        /// <summary>The game's class name as players see it in the wiki, e.g. "Low Residential Eco - Level1".</summary>
        internal static string GetClassName(ItemClass itemClass)
        {
            return string.IsNullOrEmpty(itemClass?.name) ? GetClassLabel(itemClass) : itemClass.name;
        }

        /// <summary>
        /// The vehicles that belong to a class, as shown in the AVO list, e.g.
        /// "Electric Car Edison Model 5, ..., Personal Electric Transport Ride". Players recognise the vehicles.
        /// </summary>
        internal static string GetVehicleNames(ItemClass itemClass)
        {
            List<string> names = new List<string>();
            VehicleOptions[] options = AdvancedVehicleOptions.config?.options;
            if (options != null)
            {
                foreach (VehicleOptions o in options)
                {
                    ItemClass c = o?.prefab?.m_class;
                    if (c == null || c.m_service != itemClass.m_service || c.m_subService != itemClass.m_subService || c.m_level != itemClass.m_level) continue;
                    string name = o.localizedName;
                    if (!string.IsNullOrEmpty(name) && !names.Contains(name)) names.Add(name);
                }
            }

            const int maxNames = 8;
            string list = names.Count == 0 ? "?" : string.Join(", ", names.GetRange(0, System.Math.Min(maxNames, names.Count)).ToArray());
            if (names.Count > maxNames) list += ", ... (+" + (names.Count - maxNames) + ")";
            return list;
        }

        /// <summary>
        /// The same message for disabling the last vehicle of a class and for the check on game load:
        ///
        ///   [consequence text: citizen/tourist vehicles (OP52) or service vehicles (OP53)]
        ///
        ///   Affected vehicles:
        ///
        ///   1 - (Low Residential Eco - Level1)
        ///   Electric Car Edison Model 5, ..., Personal Electric Transport Ride
        ///
        /// Several classes are numbered; citizen and service classes each get their own consequence text.
        /// The title ("Warning: No vehicle of a vehicle class active", OP55) is shown by the caller.
        /// </summary>
        internal static string GetEmptyClassMessage(IList<ItemClass> classes)
        {
            StringBuilder message = new StringBuilder();
            int number = 0;

            foreach (bool citizen in new[] { true, false })
            {
                List<ItemClass> group = new List<ItemClass>();
                foreach (ItemClass c in classes) if (IsCitizenClass(c) == citizen) group.Add(c);
                if (group.Count == 0) continue;

                if (message.Length > 0) message.Append("\n\n");
                message.Append(Translations.Translate(citizen ? "AVO_MOD_OP52" : "AVO_MOD_OP53"));
                message.Append("\n\n").Append(Translations.Translate("AVO_MOD_OP56")).Append("\n");

                foreach (ItemClass c in group)
                {
                    number++;
                    message.Append("\n").Append(number).Append(" - (").Append(GetClassName(c)).Append(")\n");
                    message.Append(GetVehicleNames(c)).Append("\n");
                }
            }

            return message.ToString().TrimEnd('\n');
        }

        internal static string GetEmptyClassMessage(ItemClass itemClass)
        {
            return GetEmptyClassMessage(new List<ItemClass> { itemClass });
        }

        /// <summary>Citizen and tourist vehicles (cars, bicycles, campers) belong to the Residential service.</summary>
        internal static bool IsCitizenClass(ItemClass itemClass)
        {
            return itemClass != null && itemClass.m_service == ItemClass.Service.Residential;
        }

        private static bool IsSameOrLowerLevel(ItemClass candidate, ItemClass target)
        {
            if (candidate.m_service != target.m_service || candidate.m_subService != target.m_subService) return false;
            if (target.m_level == ItemClass.Level.None || candidate.m_level == ItemClass.Level.None) return candidate.m_level == target.m_level;
            return candidate.m_level <= target.m_level;
        }
    }
}
