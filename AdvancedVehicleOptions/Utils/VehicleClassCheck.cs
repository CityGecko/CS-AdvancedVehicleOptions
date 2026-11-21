using System.Collections.Generic;
using System.Text;

namespace AdvancedVehicleOptionsUID
{
    /// <summary>
    /// A game vehicle class (Service + SubService + Level) together with the vehicle type the game asks for.
    /// </summary>
    internal struct VehicleClassKey
    {
        internal ItemClass itemClass;
        internal VehicleInfo.VehicleType vehicleType;

        internal VehicleClassKey(ItemClass itemClass, VehicleInfo.VehicleType vehicleType)
        {
            this.itemClass = itemClass;
            this.vehicleType = vehicleType;
        }
    }

    /// <summary>
    /// Checks whether the game can still pick a vehicle of a given class and vehicle type.
    ///
    /// The game does not pick vehicles by AVO category but by class (VehicleManager.GetRandomVehicleInfo), several
    /// services additionally filter by vehicle type (e.g. a fire station asks for a Car, a helicopter depot for a
    /// Helicopter of the same class). AVO's category check (CheckServiceValidity) can therefore pass while the game
    /// gets nothing - e.g. "Citizen" still has normal cars but all electric cars are disabled, or the fire department
    /// still has its helicopter but no fire truck. Game code without a null check then throws
    /// (see Patches/TouristAIPatch.cs), services stop dispatching.
    ///
    /// Mirrors VehicleManager.RefreshTransferVehicles / GetRandomVehicleInfo: the list of a level contains every prefab
    /// of that class whose service is not None and whose placement style is Automatic (AVO disables vehicles by setting
    /// Manual). A level without any such prefab uses the list of the next lower level (same service/subService); the
    /// type filter is applied to that list afterwards.
    /// </summary>
    internal static class VehicleClassCheck
    {
        internal static bool HasUsableVehicle(ItemClass itemClass, VehicleInfo.VehicleType vehicleType)
        {
            if (itemClass == null || itemClass.m_service == ItemClass.Service.None) return true;

            List<VehicleInfo> usable = new List<VehicleInfo>();
            int count = PrefabCollection<VehicleInfo>.PrefabCount();
            for (int i = 0; i < count; i++)
            {
                VehicleInfo prefab = PrefabCollection<VehicleInfo>.GetPrefab((uint)i);
                if (prefab == null || prefab.m_class == null) continue;
                if (prefab.m_placementStyle != ItemClass.Placement.Automatic) continue;
                if (prefab.m_class.m_service != itemClass.m_service || prefab.m_class.m_subService != itemClass.m_subService) continue;
                usable.Add(prefab);
            }

            // Effective list: own level, or - if that level has no usable prefab at all - the next lower one
            if (itemClass.m_level == ItemClass.Level.None)
            {
                return HasType(usable, ItemClass.Level.None, vehicleType);
            }
            for (int level = (int)itemClass.m_level; level >= (int)ItemClass.Level.Level1; level--)
            {
                if (HasLevel(usable, (ItemClass.Level)level)) return HasType(usable, (ItemClass.Level)level, vehicleType);
            }
            return false;
        }

        /// <summary>
        /// Class/type combinations that had at least one vehicle by default but have none usable now - i.e. emptied by
        /// disabling vehicles in AVO. Combinations the game never had vehicles for are not reported.
        /// </summary>
        internal static List<VehicleClassKey> GetClassesEmptiedByAVO()
        {
            List<VehicleClassKey> result = new List<VehicleClassKey>();
            HashSet<string> seen = new HashSet<string>();

            int count = PrefabCollection<VehicleInfo>.PrefabCount();
            for (int i = 0; i < count; i++)
            {
                VehicleInfo prefab = PrefabCollection<VehicleInfo>.GetPrefab((uint)i);
                if (prefab == null || prefab.m_class == null || prefab.m_class.m_service == ItemClass.Service.None) continue;
                if (prefab.m_placementStyle == ItemClass.Placement.Automatic) continue;

                // Only vehicles AVO disabled: by default they were spawnable (Automatic)
                if (DefaultOptions.GetPlacementStyle(prefab) != ItemClass.Placement.Automatic) continue;

                if (!seen.Add(GetClassLabel(prefab.m_class) + " / " + prefab.m_vehicleType)) continue;
                if (!HasUsableVehicle(prefab.m_class, prefab.m_vehicleType)) result.Add(new VehicleClassKey(prefab.m_class, prefab.m_vehicleType));
            }
            return result;
        }

        /// <summary>Technical class name for the log, e.g. "Residential / ResidentialLowEco / Level1".</summary>
        internal static string GetClassLabel(ItemClass itemClass)
        {
            if (itemClass == null) return "?";
            return itemClass.m_service + " / " + itemClass.m_subService + " / " + itemClass.m_level;
        }

        /// <summary>
        /// The game's class name as players see it in the wiki, e.g. "Low Residential Eco - Level1". Only if vehicles of
        /// several types share the same class and class name, the type is added, e.g. "... - Car" / "... - Helicopter".
        /// </summary>
        internal static string GetClassName(VehicleClassKey key)
        {
            string name = string.IsNullOrEmpty(key.itemClass?.name) ? GetClassLabel(key.itemClass) : key.itemClass.name;
            return HasSeveralTypes(key.itemClass) ? name + " - " + key.vehicleType : name;
        }

        /// <summary>
        /// The vehicles of a class and type, as shown in the AVO list (trailers left out), e.g.
        /// "Electric Car Edison Model 5, ..., Personal Electric Transport Ride". Players recognise the vehicles.
        /// </summary>
        internal static string GetVehicleNames(VehicleClassKey key)
        {
            List<string> names = new List<string>();
            VehicleOptions[] options = AdvancedVehicleOptions.config?.options;
            if (options != null)
            {
                foreach (VehicleOptions o in options)
                {
                    if (o == null || o.isTrailer) continue;
                    VehicleInfo prefab = o.prefab;
                    ItemClass c = prefab?.m_class;
                    if (c == null || !SameClass(c, key.itemClass) || prefab.m_vehicleType != key.vehicleType) continue;
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
        internal static string GetEmptyClassMessage(IList<VehicleClassKey> classes)
        {
            StringBuilder message = new StringBuilder();
            int number = 0;

            foreach (bool citizen in new[] { true, false })
            {
                List<VehicleClassKey> group = new List<VehicleClassKey>();
                foreach (VehicleClassKey c in classes) if (IsCitizenClass(c.itemClass) == citizen) group.Add(c);
                if (group.Count == 0) continue;

                if (message.Length > 0) message.Append("\n\n");
                message.Append(Translations.Translate(citizen ? "AVO_MOD_OP52" : "AVO_MOD_OP53"));
                message.Append("\n\n").Append(Translations.Translate("AVO_MOD_OP56")).Append("\n");

                foreach (VehicleClassKey c in group)
                {
                    number++;
                    message.Append("\n").Append(number).Append(" - (").Append(GetClassName(c)).Append(")\n");
                    message.Append(GetVehicleNames(c)).Append("\n");
                }
            }

            return message.ToString().TrimEnd('\n');
        }

        internal static string GetEmptyClassMessage(VehicleClassKey key)
        {
            return GetEmptyClassMessage(new List<VehicleClassKey> { key });
        }

        /// <summary>Citizen and tourist vehicles (cars, bicycles, campers) belong to the Residential service.</summary>
        internal static bool IsCitizenClass(ItemClass itemClass)
        {
            return itemClass != null && itemClass.m_service == ItemClass.Service.Residential;
        }

        private static bool SameClass(ItemClass a, ItemClass b)
        {
            return a.m_service == b.m_service && a.m_subService == b.m_subService && a.m_level == b.m_level;
        }

        private static bool HasLevel(List<VehicleInfo> prefabs, ItemClass.Level level)
        {
            foreach (VehicleInfo p in prefabs) if (p.m_class.m_level == level) return true;
            return false;
        }

        private static bool HasType(List<VehicleInfo> prefabs, ItemClass.Level level, VehicleInfo.VehicleType type)
        {
            foreach (VehicleInfo p in prefabs) if (p.m_class.m_level == level && p.m_vehicleType == type) return true;
            return false;
        }

        // Whether the type has to be added to the class name to tell two entries apart: only if vehicles of more than one
        // type share the same class AND the same class name (e.g. fire trucks "Fire Department Vehicle" and fire
        // helicopters "Fire Helicopter" share service/sub-service/level but already have different names - no suffix).
        // Trailers are ignored: they are not spawned on their own (not Automatic by default).
        private static bool HasSeveralTypes(ItemClass itemClass)
        {
            if (itemClass == null) return false;
            bool found = false;
            VehicleInfo.VehicleType first = VehicleInfo.VehicleType.None;
            int count = PrefabCollection<VehicleInfo>.PrefabCount();
            for (int i = 0; i < count; i++)
            {
                VehicleInfo prefab = PrefabCollection<VehicleInfo>.GetPrefab((uint)i);
                if (prefab == null || prefab.m_class == null || !SameClass(prefab.m_class, itemClass)) continue;
                if (prefab.m_class.name != itemClass.name) continue;

                ItemClass.Placement defaultPlacement = DefaultOptions.GetPlacementStyle(prefab);
                ItemClass.Placement placement = (int)defaultPlacement != -1 ? defaultPlacement : prefab.m_placementStyle;
                if (placement != ItemClass.Placement.Automatic) continue;

                if (!found) { found = true; first = prefab.m_vehicleType; }
                else if (prefab.m_vehicleType != first) return true;
            }
            return false;
        }
    }
}
