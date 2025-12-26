using UnityEngine;
using ColossalFramework.Threading;
using ColossalFramework.Globalization;

using System;
using System.Text;
using System.Xml.Serialization;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;

using AdvancedVehicleOptionsUID.Compatibility;

namespace AdvancedVehicleOptionsUID
{
    public class VehicleOptions : IComparable
    {
        public enum Category
        {
            None = -1,
            Citizen,
            Bicycle,
            Forestry,
            Farming,
            Ore,
            Oil,
            Fishing,
            IndustryGeneric,
            IndustryPlayer,
            Police,
            Prison,
            FireSafety,
            Disaster,
            Healthcare,
            Deathcare,
            Garbage,
            WasteTransfer,
            Maintenance,
            TransportPost,
            TransportBank,
            TransportBus,
            TransportIntercityBus,
            TrolleyBus,
            TransportTaxi,
            TransportMetro,
            Tram,
            Monorail,
            CableCar,
            CargoTrain,
            TransportTrain,
            CargoShip,
            TransportShip,
            TransportFerry,
            CargoPlane,
            TransportPlane,
            TransportBlimp,
            TransportTours,
            Monument,
            Natural
        }

        #region serialized
        [XmlAttribute("name")]
        public string name
        {
            get { return m_prefab.name; }
            set
            {
                if (value == null) return;

                VehicleInfo prefab = PrefabCollection<VehicleInfo>.FindLoaded(value);
                if (prefab == null)
                    Logging.Message("Vehicle Asset not available / deactivated or unsubscribed -  " + value);
                else
                    SetPrefab(prefab);
            }
        }
        // enabled
        public bool enabled
        {
            get
            {
                if (m_engine != null)
                    return m_engine.m_placementStyle != ItemClass.Placement.Manual;

                return m_prefab.m_placementStyle != ItemClass.Placement.Manual;
            }
            set
            {
                if (m_prefab == null || enabled == value) return;

                if (value)
                {
                    ItemClass.Placement placement = DefaultOptions.GetPlacementStyle(m_prefab);

                    m_prefab.m_placementStyle = (int)placement != -1 ? placement : m_placementStyle;

                    if (hasTrailer)
                    {
                        for (uint i = 0; i < m_prefab.m_trailers.Length; i++)
                        {
                            if (m_prefab.m_trailers[i].m_info == null) continue;

                            placement = DefaultOptions.GetPlacementStyle(m_prefab.m_trailers[i].m_info);
                            m_prefab.m_trailers[i].m_info.m_placementStyle = (int)placement != -1 ? placement : m_placementStyle;
                        }
                    }
                }
                else
                {
                    m_prefab.m_placementStyle = ItemClass.Placement.Manual;

                    if (hasTrailer)
                    {
                        for (uint i = 0; i < m_prefab.m_trailers.Length; i++)
                        {
                            if (m_prefab.m_trailers[i].m_info == null) continue;

                            m_prefab.m_trailers[i].m_info.m_placementStyle = ItemClass.Placement.Manual;
                        }
                    }
                }
            }
        }
        // addBackEngine
        public bool addBackEngine
        {
            get
            {
                if (!hasTrailer) return false;
                return m_prefab.m_trailers[m_prefab.m_trailers.Length - 1].m_info == m_prefab;
            }
            set
            {
                if (m_prefab == null || !isTrain) return;

                VehicleInfo newTrailer = value ? m_prefab : DefaultOptions.GetLastTrailer(m_prefab);
                int last = m_prefab.m_trailers.Length - 1;

                if (m_prefab.m_trailers[last].m_info == newTrailer || newTrailer == null) return;

                m_prefab.m_trailers[last].m_info = newTrailer;

                if (value)
                    m_prefab.m_trailers[last].m_invertProbability = m_prefab.m_trailers[last].m_probability;
                else
                    m_prefab.m_trailers[last].m_invertProbability = DefaultOptions.GetProbability(prefab);
            }
        }
        // maxSpeed
        public float maxSpeed
        {
            get { return m_prefab.m_maxSpeed; }
            set
            {
                if (m_prefab == null || value <= 0) return;
                m_prefab.m_maxSpeed = value;
            }
        }
        // acceleration
        public float acceleration
        {
            get { return m_prefab.m_acceleration; }
            set
            {
                if (m_prefab == null || value <= 0) return;
                m_prefab.m_acceleration = value;
            }
        }
        // braking
        public float braking
        {
            get { return m_prefab.m_braking; }
            set
            {
                if (m_prefab == null || value <= 0) return;
                m_prefab.m_braking = value;
            }
        }
        // turning
        public float turning
        {
            get { return m_prefab.m_turning; }
            set
            {
                if (m_prefab == null || value <= 0) return;
                m_prefab.m_turning = value;
            }
        }
        // springs
        public float springs
        {
            get { return m_prefab.m_springs; }
            set
            {
                if (m_prefab == null || value <= 0) return;
                m_prefab.m_springs = value;
            }
        }
        // dampers
        public float dampers
        {
            get { return m_prefab.m_dampers; }
            set
            {
                if (m_prefab == null || value <= 0) return;
                m_prefab.m_dampers = value;
            }
        }
        // leanMultiplier
        public float leanMultiplier
        {
            get { return m_prefab.m_leanMultiplier; }
            set
            {
                if (m_prefab == null) return;
                m_prefab.m_leanMultiplier = value;
            }
        }
        // nodMultiplier
        public float nodMultiplier
        {
            get { return m_prefab.m_nodMultiplier; }
            set
            {
                if (m_prefab == null) return;
                m_prefab.m_nodMultiplier = value;
            }
        }
        // useColorVariations
        public bool useColorVariations
        {
            get { return m_prefab.m_useColorVariations; }
            set
            {
                if (m_prefab == null) return;
                m_prefab.m_useColorVariations = value;
            }
        }
        // colors
        public HexaColor color0
        {
            get { return m_prefab.m_color0; }
            set
            {
                if (m_prefab == null) return;
                m_prefab.m_color0 = value;
            }
        }
        public HexaColor color1
        {
            get { return m_prefab.m_color1; }
            set
            {
                if (m_prefab == null) return;
                m_prefab.m_color1 = value;
            }
        }
        public HexaColor color2
        {
            get { return m_prefab.m_color2; }
            set
            {
                if (m_prefab == null) return;
                m_prefab.m_color2 = value;
            }
        }
        public HexaColor color3
        {
            get { return m_prefab.m_color3; }
            set
            {
                if (m_prefab == null) return;
                m_prefab.m_color3 = value;
            }
        }

        public bool isLargeVehicle
        {
            get { return m_prefab.m_isLargeVehicle; }
            set
            {
                if (m_prefab == null) return;
                m_prefab.m_isLargeVehicle = value;
            }
        }
        public string classname
        {
            get { return m_prefab.m_class.name; }
            set
            {
                if (m_prefab == null) return;
                m_prefab.m_class.name = value;
            }
        }

        //specialcapacity - some vehicles have an additional property
        [DefaultValue(-1)]
        public int specialcapacity
        {
            get
            {
                VehicleAI ai;

                ai = m_prefab.m_vehicleAI as WaterTruckAI;
                if (ai != null) return ((WaterTruckAI)ai).m_pumpingRate;

                ai = m_prefab.m_vehicleAI as TaxiAI;
                if (ai != null) return ((TaxiAI)ai).m_travelCapacity;

                ai = m_prefab.m_vehicleAI as PoliceCarAI;
                if (ai != null) return ((PoliceCarAI)ai).m_criminalCapacity;

                ai = m_prefab.m_vehicleAI as FireCopterAI;
                if (ai != null) return ((FireCopterAI)ai).m_fireFightingCapacity;

                ai = m_prefab.m_vehicleAI as AmbulanceCopterAI;
                if (ai != null) return ((AmbulanceCopterAI)ai).m_travelCapacity;

                ai = m_prefab.m_vehicleAI as FishingBoatAI;
                if (ai != null) return ((FishingBoatAI)ai).m_fishingRate;

                return -1;
            }

            set
            {
                if (m_prefab == null || capacity == -1 || value <= 0) return;

                VehicleAI ai;

                ai = m_prefab.m_vehicleAI as WaterTruckAI;
                if (ai != null) { ((WaterTruckAI)ai).m_pumpingRate = value; return; }

                ai = m_prefab.m_vehicleAI as TaxiAI;
                if (ai != null) { ((TaxiAI)ai).m_travelCapacity = value; return; }

                ai = m_prefab.m_vehicleAI as PoliceCarAI;
                if (ai != null) { ((PoliceCarAI)ai).m_criminalCapacity = value; return; }

                ai = m_prefab.m_vehicleAI as FireCopterAI;
                if (ai != null) { ((FireCopterAI)ai).m_fireFightingCapacity = value; return; }

                ai = m_prefab.m_vehicleAI as AmbulanceCopterAI;
                if (ai != null) { ((AmbulanceCopterAI)ai).m_travelCapacity = value; return; }

                ai = m_prefab.m_vehicleAI as FishingBoatAI;
                if (ai != null) { ((FishingBoatAI)ai).m_fishingRate = value; return; }
            }
        }

        // capacity
        [DefaultValue(-1)]
        public int capacity
        {
            get
            {
                VehicleAI ai;

                ai = m_prefab.m_vehicleAI as AmbulanceAI;
                if (ai != null) return ((AmbulanceAI)ai).m_patientCapacity;

                ai = m_prefab.m_vehicleAI as AmbulanceCopterAI;
                if (ai != null) return ((AmbulanceCopterAI)ai).m_patientCapacity;

                ai = m_prefab.m_vehicleAI as BusAI;
                if (ai != null) return ((BusAI)ai).m_passengerCapacity;

                ai = m_prefab.m_vehicleAI as CargoShipAI;
                if (ai != null) return ((CargoShipAI)ai).m_cargoCapacity;

                ai = m_prefab.m_vehicleAI as CargoPlaneAI;
                if (ai != null) return ((CargoPlaneAI)ai).m_cargoCapacity;

                ai = m_prefab.m_vehicleAI as CargoTrainAI;
                if (ai != null) return ((CargoTrainAI)ai).m_cargoCapacity;

                ai = m_prefab.m_vehicleAI as CargoTruckAI;
                if (ai != null) return ((CargoTruckAI)ai).m_cargoCapacity;

                ai = m_prefab.m_vehicleAI as GarbageTruckAI;
                if (ai != null) return ((GarbageTruckAI)ai).m_cargoCapacity;

                ai = m_prefab.m_vehicleAI as FireTruckAI;
                if (ai != null) return ((FireTruckAI)ai).m_fireFightingRate;

                ai = m_prefab.m_vehicleAI as FireCopterAI;
                if (ai != null) return ((FireCopterAI)ai).m_fireFightingRate;

                ai = m_prefab.m_vehicleAI as HearseAI;
                if (ai != null) return ((HearseAI)ai).m_corpseCapacity;

                ai = m_prefab.m_vehicleAI as PassengerPlaneAI;
                if (ai != null) return ((PassengerPlaneAI)ai).m_passengerCapacity;

                ai = m_prefab.m_vehicleAI as PassengerShipAI;
                if (ai != null) return ((PassengerShipAI)ai).m_passengerCapacity;

                ai = m_prefab.m_vehicleAI as PassengerTrainAI;
                if (ai != null) return ((PassengerTrainAI)ai).m_passengerCapacity;

                ai = m_prefab.m_vehicleAI as PoliceCarAI;
                if (ai != null) return ((PoliceCarAI)ai).m_crimeCapacity;

                ai = m_prefab.m_vehicleAI as PoliceCopterAI;
                if (ai != null) return ((PoliceCopterAI)ai).m_crimeCapacity;

                ai = m_prefab.m_vehicleAI as TaxiAI;
                if (ai != null) return ((TaxiAI)ai).m_passengerCapacity;

                ai = m_prefab.m_vehicleAI as TramAI;
                if (ai != null) return ((TramAI)ai).m_passengerCapacity;

                ai = m_prefab.m_vehicleAI as MaintenanceTruckAI;
                if (ai != null) return ((MaintenanceTruckAI)ai).m_maintenanceCapacity;

                ai = m_prefab.m_vehicleAI as ParkMaintenanceVehicleAI;
                if (ai != null) return ((ParkMaintenanceVehicleAI)ai).m_maintenanceCapacity;

                ai = m_prefab.m_vehicleAI as WaterTruckAI;
                if (ai != null) return ((WaterTruckAI)ai).m_cargoCapacity;

                ai = m_prefab.m_vehicleAI as SnowTruckAI;
                if (ai != null) return ((SnowTruckAI)ai).m_cargoCapacity;

                ai = m_prefab.m_vehicleAI as CableCarAI;
                if (ai != null) return ((CableCarAI)ai).m_passengerCapacity;

                ai = m_prefab.m_vehicleAI as TrolleybusAI;
                if (ai != null) return ((TrolleybusAI)ai).m_passengerCapacity;

                ai = m_prefab.m_vehicleAI as PassengerFerryAI;
                if (ai != null) return ((PassengerFerryAI)ai).m_passengerCapacity;

                ai = m_prefab.m_vehicleAI as PassengerBlimpAI;
                if (ai != null) return ((PassengerBlimpAI)ai).m_passengerCapacity;

                ai = m_prefab.m_vehicleAI as PostVanAI;
                if (ai != null) return ((PostVanAI)ai).m_mailCapacity;

                ai = m_prefab.m_vehicleAI as PassengerHelicopterAI;
                if (ai != null) return ((PassengerHelicopterAI)ai).m_passengerCapacity;

                ai = m_prefab.m_vehicleAI as DisasterResponseCopterAI;
                if (ai != null) return ((DisasterResponseCopterAI)ai).m_efficiency;

                ai = m_prefab.m_vehicleAI as DisasterResponseVehicleAI;
                if (ai != null) return ((DisasterResponseVehicleAI)ai).m_efficiency;

                ai = m_prefab.m_vehicleAI as FishingBoatAI;
                if (ai != null) return ((FishingBoatAI)ai).m_capacity;

                ai = m_prefab.m_vehicleAI as BankVanAI;
                if (ai != null) return ((BankVanAI)ai).m_cashCapacity;


                return -1;
            }
            set
            {
                if (m_prefab == null || capacity == -1 || value <= 0) return;

                VehicleAI ai;

                ai = m_prefab.m_vehicleAI as AmbulanceAI;
                if (ai != null) { ((AmbulanceAI)ai).m_patientCapacity = value; return; }

                ai = m_prefab.m_vehicleAI as AmbulanceCopterAI;
                if (ai != null) { ((AmbulanceCopterAI)ai).m_patientCapacity = value; return; }

                ai = m_prefab.m_vehicleAI as BusAI;
                if (ai != null) { ((BusAI)ai).m_passengerCapacity = value; return; }

                ai = m_prefab.m_vehicleAI as CargoShipAI;
                if (ai != null) { ((CargoShipAI)ai).m_cargoCapacity = value; return; }

                ai = m_prefab.m_vehicleAI as CargoPlaneAI;
                if (ai != null) { ((CargoPlaneAI)ai).m_cargoCapacity = value; return; }

                ai = m_prefab.m_vehicleAI as CargoTrainAI;
                if (ai != null) { ((CargoTrainAI)ai).m_cargoCapacity = value; return; }

                ai = m_prefab.m_vehicleAI as CargoTruckAI;
                if (ai != null) { ((CargoTruckAI)ai).m_cargoCapacity = value; return; }

                ai = m_prefab.m_vehicleAI as GarbageTruckAI;
                if (ai != null) { ((GarbageTruckAI)ai).m_cargoCapacity = value; return; }

                ai = m_prefab.m_vehicleAI as FireTruckAI;
                if (ai != null) { ((FireTruckAI)ai).m_fireFightingRate = value; return; }

                ai = m_prefab.m_vehicleAI as FireCopterAI;
                if (ai != null) { ((FireCopterAI)ai).m_fireFightingRate = value; return; }

                ai = m_prefab.m_vehicleAI as HearseAI;
                if (ai != null) { ((HearseAI)ai).m_corpseCapacity = value; return; }

                ai = m_prefab.m_vehicleAI as PassengerPlaneAI;
                if (ai != null) { ((PassengerPlaneAI)ai).m_passengerCapacity = value; return; }

                ai = m_prefab.m_vehicleAI as PassengerShipAI;
                if (ai != null) { ((PassengerShipAI)ai).m_passengerCapacity = value; return; }

                ai = m_prefab.m_vehicleAI as PassengerTrainAI;
                if (ai != null) { ((PassengerTrainAI)ai).m_passengerCapacity = value; return; }

                ai = m_prefab.m_vehicleAI as PoliceCarAI;
                if (ai != null) { ((PoliceCarAI)ai).m_crimeCapacity = value; return; }

                ai = m_prefab.m_vehicleAI as PoliceCopterAI;
                if (ai != null) { ((PoliceCopterAI)ai).m_crimeCapacity = value; return; }

                ai = m_prefab.m_vehicleAI as TaxiAI;
                if (ai != null) { ((TaxiAI)ai).m_passengerCapacity = value; return; }

                ai = m_prefab.m_vehicleAI as TramAI;
                if (ai != null) { ((TramAI)ai).m_passengerCapacity = value; return; }

                ai = m_prefab.m_vehicleAI as MaintenanceTruckAI;
                if (ai != null) { ((MaintenanceTruckAI)ai).m_maintenanceCapacity = value; return; }

                ai = m_prefab.m_vehicleAI as ParkMaintenanceVehicleAI;
                if (ai != null) { ((ParkMaintenanceVehicleAI)ai).m_maintenanceCapacity = value; return; }

                ai = m_prefab.m_vehicleAI as WaterTruckAI;
                if (ai != null) { ((WaterTruckAI)ai).m_cargoCapacity = value; return; }

                ai = m_prefab.m_vehicleAI as SnowTruckAI;
                if (ai != null) { ((SnowTruckAI)ai).m_cargoCapacity = value; return; }

                ai = m_prefab.m_vehicleAI as CableCarAI;
                if (ai != null) { ((CableCarAI)ai).m_passengerCapacity = value; return; }

                ai = m_prefab.m_vehicleAI as TrolleybusAI;
                if (ai != null) { ((TrolleybusAI)ai).m_passengerCapacity = value; return; }

                ai = m_prefab.m_vehicleAI as PassengerFerryAI;
                if (ai != null) { ((PassengerFerryAI)ai).m_passengerCapacity = value; return; }

                ai = m_prefab.m_vehicleAI as PassengerBlimpAI;
                if (ai != null) { ((PassengerBlimpAI)ai).m_passengerCapacity = value; return; }

                ai = m_prefab.m_vehicleAI as PostVanAI;
                if (ai != null) { ((PostVanAI)ai).m_mailCapacity = value; return; }

                ai = m_prefab.m_vehicleAI as DisasterResponseCopterAI;
                if (ai != null) { ((DisasterResponseCopterAI)ai).m_efficiency = value; return; }

                ai = m_prefab.m_vehicleAI as DisasterResponseVehicleAI;
                if (ai != null) { ((DisasterResponseVehicleAI)ai).m_efficiency = value; return; }

                ai = m_prefab.m_vehicleAI as PassengerHelicopterAI;
                if (ai != null) { ((PassengerHelicopterAI)ai).m_passengerCapacity = value; return; }

                ai = m_prefab.m_vehicleAI as FishingBoatAI;
                if (ai != null) { ((FishingBoatAI)ai).m_capacity = value; return; }

                ai = m_prefab.m_vehicleAI as BankVanAI;
                if (ai != null) { ((BankVanAI)ai).m_cashCapacity = value; return; }
            }
        }

        #endregion

        public static VehicleInfo prefabUpdateUnits = null;
        public static VehicleInfo prefabUpdateEngine = null;

        private VehicleInfo m_prefab = null;
        private VehicleInfo m_engine = null;
        private ItemClass.Placement m_placementStyle;
        private string m_localizedName;
        private bool m_hasCapacity = false;
        private bool m_hasSpecialCapacity = false;
        private string m_steamID;

        public VehicleOptions() { }

        public VehicleOptions(VehicleInfo prefab)
        {
            SetPrefab(prefab);
        }

        public VehicleInfo prefab
        {
            get { return m_prefab; }
        }

        public VehicleOptions engine
        {
            get { return new VehicleOptions(m_engine); }
        }

        public ItemClass.Placement placementStyle
        {
            get { return m_placementStyle; }
        }

        public bool hasCapacity
        {
            get { return m_hasCapacity; }
        }

        public bool hasSpecialCapacity
        {
            get { return m_hasSpecialCapacity; }
        }

        public bool hasTrailer
        {
            get { return m_prefab.m_trailers != null && m_prefab.m_trailers.Length > 0; }
        }

        public bool isTrain
        {
            get { return hasTrailer && (m_prefab.m_vehicleType == VehicleInfo.VehicleType.Train || m_prefab.m_vehicleType == VehicleInfo.VehicleType.Tram || m_prefab.m_vehicleType == VehicleInfo.VehicleType.Metro || m_prefab.m_vehicleType == VehicleInfo.VehicleType.Monorail); }
        }

        // Define all vehicles, which have no passengers or cargo capacities
        public bool isNonPaxCargo
        {
            get {
                return prefab.m_class.m_service == ItemClass.Service.FireDepartment
                                                  || prefab.m_class.m_service == ItemClass.Service.PoliceDepartment && prefab.m_class.m_subService == ItemClass.SubService.None
                                                  || prefab.m_class.m_service == ItemClass.Service.HealthCare
                                                  || prefab.m_class.m_service == ItemClass.Service.Disaster && prefab.m_class.m_level == ItemClass.Level.Level2
                                                  || prefab.m_class.m_service == ItemClass.Service.Water
                                                  || prefab.m_class.m_service == ItemClass.Service.Road
                                                  || prefab.m_class.m_subService == ItemClass.SubService.BeautificationParks; }
        }

        // Check if vehicle is in the Industry Generic group for delivery
        public bool isDelivery
        {
            get { return prefab.m_class.m_subService == ItemClass.SubService.IndustrialGeneric; }
        }

        // Check if vehicle is in the Public Transport Group, used for Compability Patch IPT, TLM
        public bool isPublicTransport
        {
            get { return prefab.m_class.m_service == ItemClass.Service.PublicTransport; }
        }


        // Define all vehicles, where AVO cannot longer control the spawning (Bus, Biofuel Bus, Trolley Bus, Tour Bus, Tram, Metro, Helicopter, Ferry, Blimp, Monorail)
        public bool isPublicTransportGame
        {
            get { return prefab.m_class.m_subService == ItemClass.SubService.PublicTransportBus && prefab.m_class.m_level == ItemClass.Level.Level1
                                                     || prefab.m_class.m_subService == ItemClass.SubService.PublicTransportBus && prefab.m_class.m_level == ItemClass.Level.Level2
                                                     || prefab.m_class.m_subService == ItemClass.SubService.PublicTransportTrolleybus
                                                     || prefab.m_class.m_subService == ItemClass.SubService.PublicTransportTours && prefab.m_class.m_level == ItemClass.Level.Level3
                                                     || prefab.m_class.m_subService == ItemClass.SubService.PublicTransportTram
                                                     || prefab.m_class.m_subService == ItemClass.SubService.PublicTransportMetro
                                                     || prefab.m_class.m_subService == ItemClass.SubService.PublicTransportPlane && prefab.m_vehicleType == VehicleInfo.VehicleType.Blimp
                                                     || prefab.m_class.m_subService == ItemClass.SubService.PublicTransportShip && prefab.m_class.m_level == ItemClass.Level.Level2
                                                     || prefab.m_class.m_subService == ItemClass.SubService.PublicTransportPlane && prefab.m_vehicleType == VehicleInfo.VehicleType.Helicopter
                                                     || prefab.m_class.m_subService == ItemClass.SubService.PublicTransportMonorail; }
        }


        // Class Intercity Bus, Cargo Train, Cargo Plane, Cargo Ship, Postal Service und Tours to be excluded as not in scope of IPT and TLM
        public bool isNotPublicTransportMod
        {
            get {
                return prefab.m_class.m_subService == ItemClass.SubService.PublicTransportBus && prefab.m_class.m_level == ItemClass.Level.Level3
                                                     || prefab.m_class.m_subService == ItemClass.SubService.PublicTransportTours
                                                     || prefab.m_class.m_subService == ItemClass.SubService.PublicTransportShip && prefab.m_class.m_level == ItemClass.Level.Level4
                                                     || prefab.m_class.m_subService == ItemClass.SubService.PublicTransportTrain && prefab.m_class.m_level == ItemClass.Level.Level4
                                                     || prefab.m_class.m_subService == ItemClass.SubService.PublicTransportPlane && prefab.m_class.m_level == ItemClass.Level.Level4
                                                     || prefab.m_class.m_subService == ItemClass.SubService.PublicTransportPost; }
        }

        public bool isTrailer
        {
            get { return m_engine != null; }
        }

        public string localizedName
        {
            get { return m_localizedName; }
        }

        public int ReturnLineOverviewType
        {
            get
            {
                switch (prefab.m_class.m_subService)
                {
                    case ItemClass.SubService.PublicTransportBus:
                        return 0;
                    case ItemClass.SubService.PublicTransportTrolleybus:
                        return 1;
                    case ItemClass.SubService.PublicTransportTram:
                        return 2;
                    case ItemClass.SubService.PublicTransportMetro:
                        return 3;
                    case ItemClass.SubService.PublicTransportShip:
                        return 5;
                    case ItemClass.SubService.PublicTransportPlane:
                        return 6;
                    case ItemClass.SubService.PublicTransportMonorail:
                        return 7;
                    case ItemClass.SubService.PublicTransportTours:
                        return 10;
                }

                return -1;
            }
        }

        public string SpecialCapacityString
        {
            get
            {
                switch (m_prefab.m_vehicleAI)
                { case FireCopterAI _:
                        return Translations.Translate("AVO_MOD_VO01");
                    case PoliceCarAI _:
                        return Translations.Translate("AVO_MOD_VO02");
                    case WaterTruckAI _:
                        return Translations.Translate("AVO_MOD_VO03");
                    case TaxiAI _:
                        return Translations.Translate("AVO_MOD_VO04");
                    case AmbulanceCopterAI _:
                        return Translations.Translate("AVO_MOD_VO04");
                    case FishingBoatAI _:
                        return Translations.Translate("AVO_MOD_VO05");
                }
                return Translations.Translate("AVO_MOD_VO06");
            }
        }

        public string CapacityString
        {
            get
            {
                switch (m_prefab.m_vehicleAI)
                { case PassengerPlaneAI _:
                    case PassengerBlimpAI _:
                    case BusAI _:
                    case TrolleybusAI _:
                    case TaxiAI _:
                    case PassengerHelicopterAI _:
                    case PassengerFerryAI _:
                    case PassengerShipAI _:
                    case PassengerTrainAI _:
                    case TramAI _:
                    case CableCarAI _:
                        return Translations.Translate("AVO_MOD_VO07");

                    case CargoPlaneAI _:
                    case CargoTruckAI _:
                    case GarbageTruckAI _:
                    case SnowTruckAI _:
                    case WaterTruckAI _:
                    case CargoShipAI _:
                    case CargoTrainAI _:
                    case FishingBoatAI _:
                        return Translations.Translate("AVO_MOD_VO08");

                    case PostVanAI _:
                        return Translations.Translate("AVO_MOD_VO09");

                    case MaintenanceTruckAI _:
                    case ParkMaintenanceVehicleAI _:
                        return Translations.Translate("AVO_MOD_VO10");

                    case AmbulanceAI _:
                    case AmbulanceCopterAI _:
                        return Translations.Translate("AVO_MOD_VO11");

                    case HearseAI _:
                        return Translations.Translate("AVO_MOD_VO12");

                    case PoliceCarAI _:
                    case PoliceCopterAI _:
                        return Translations.Translate("AVO_MOD_VO13");

                    case FireTruckAI _:
                    case FireCopterAI _:
                        return Translations.Translate("AVO_MOD_VO14");

                    case DisasterResponseVehicleAI _:
                    case DisasterResponseCopterAI _:
                        return Translations.Translate("AVO_MOD_VO15");

                    case BankVanAI _:
                        return Translations.Translate("AVO_MOD_VO16");
                                        }
                return Translations.Translate("AVO_MOD_CAPA");
            }
        }

    public Category category
        {
	        get
            {
                if (prefab == null) return Category.None;

                // Re-Categorizing the Trailer logic, now validating engine service class against trailer service class, then replacing.

                if (hasTrailer)
                {
                    for (uint i = 0; i < m_prefab.m_trailers.Length; i++)
                    {
                        if (m_prefab.m_trailers[i].m_info == null) continue;

                        if (m_prefab.m_class.m_service != m_prefab.m_trailers[i].m_info.m_class.m_service)
                        {
                            Logging.Message("Service Change required / Engine: " + m_prefab.name + " is " + m_prefab.m_class.m_service + " / Trailer : " + m_prefab.m_trailers[i].m_info.name + " is " + m_prefab.m_trailers[i].m_info.m_class.m_service);                            
                            m_prefab.m_trailers[i].m_info.m_class = m_prefab.m_class;
                            Logging.Message("Service Change successful / Trailer : " + m_prefab.m_trailers[i].m_info.name + " reclassified to " + m_prefab.m_trailers[i].m_info.m_class.m_service);
                        }
                    }
                }

                switch (prefab.m_class.m_service)
                {
                    case ItemClass.Service.PoliceDepartment:
                        if (prefab.m_class.m_level == ItemClass.Level.Level4)
                            return Category.Prison;
                        else
                        if (prefab.m_class.m_subService == ItemClass.SubService.PoliceDepartmentBank)
                            return Category.TransportBank;
                        else
                            return Category.Police;
						
                    case ItemClass.Service.FireDepartment:
                        return Category.FireSafety;
						
                    case ItemClass.Service.HealthCare:
                        if (prefab.m_class.m_level == ItemClass.Level.Level1 || prefab.m_class.m_level == ItemClass.Level.Level3)
                            return Category.Healthcare;
                        else
						    return Category.Deathcare;
						
                    case ItemClass.Service.Garbage:
                        if (prefab.m_class.m_level == ItemClass.Level.Level3 || prefab.m_class.m_level == ItemClass.Level.Level4)		
						    return Category.WasteTransfer;
						else
                            return Category.Garbage;
						
                    case ItemClass.Service.Water:
                    case ItemClass.Service.Road:
                        return Category.Maintenance;
						
                    case ItemClass.Service.Disaster:
                        return Category.Disaster;
						
                    case ItemClass.Service.Monument:
                        if (prefab.m_class.m_level == ItemClass.Level.Level5) // Aviation Club Light Aircraft L5
                            return Category.TransportBlimp;
                        else
							return Category.Monument;
						
                    case ItemClass.Service.Natural:
                        return Category.Natural;
						
					case ItemClass.Service.PlayerIndustry:
     					return Category.IndustryPlayer;
						
					case ItemClass.Service.Fishing:
						return Category.Fishing;				
                }

                switch (prefab.m_class.m_subService)
                {
                    case ItemClass.SubService.PublicTransportBus:
					    if (prefab.m_class.m_level == ItemClass.Level.Level3)
                            return Category.TransportIntercityBus;
                        else
					     	return Category.TransportBus;
						
                    case ItemClass.SubService.PublicTransportMetro:
                        return Category.TransportMetro;
						
                    case ItemClass.SubService.PublicTransportTrain:
                        if (prefab.m_class.m_level == ItemClass.Level.Level1)
                            return Category.TransportTrain;
                        else
                        if (prefab.m_class.m_level == ItemClass.Level.Level2)
                            return Category.TransportTrain;
                        else
                            return Category.CargoTrain;
						
                    case ItemClass.SubService.PublicTransportShip:
                        if (prefab.m_class.m_level == ItemClass.Level.Level4)
                            return Category.CargoShip;
						else
						if (prefab.m_class.m_level == ItemClass.Level.Level1)
                            return Category.TransportShip;
						else
                            return Category.TransportFerry;
						
                    case ItemClass.SubService.PublicTransportTaxi:
                        return Category.TransportTaxi;
						
                    case ItemClass.SubService.PublicTransportTours:
                        return Category.TransportTours;
						
                    case ItemClass.SubService.PublicTransportPlane:
                        if (prefab.m_class.m_level == ItemClass.Level.Level4)
                            return Category.CargoPlane;
                        if (prefab.m_vehicleType == VehicleInfo.VehicleType.Helicopter)
                            return Category.TransportBlimp;
                        if (prefab.m_vehicleType == VehicleInfo.VehicleType.Blimp)
                            return Category.TransportBlimp;
                        else
                            return Category.TransportPlane;
							
                    case ItemClass.SubService.IndustrialForestry:
                        return Category.Forestry;
						
                    case ItemClass.SubService.IndustrialFarming:
                        return Category.Farming;
						
                    case ItemClass.SubService.IndustrialOre:
                        return Category.Ore;
						
                    case ItemClass.SubService.IndustrialOil:
                        return Category.Oil;
						
                    case ItemClass.SubService.IndustrialGeneric:
                        return Category.IndustryGeneric;
						
                    case ItemClass.SubService.PublicTransportTram:
                        return Category.Tram;
						
                    case ItemClass.SubService.PublicTransportMonorail:
                        return Category.Monorail;
						
                    case ItemClass.SubService.PublicTransportCableCar:
                        return Category.CableCar;
						
                    case ItemClass.SubService.PublicTransportTrolleybus:
                        return Category.TrolleyBus;
						
                    case ItemClass.SubService.ResidentialHigh:
                        return Category.Bicycle;
						
                    case ItemClass.SubService.BeautificationParks:
                        return Category.Maintenance;
						
					case ItemClass.SubService.PublicTransportPost:
                        return Category.TransportPost;
                }
		
                return Category.Citizen;
            }
        }

        public string steamID
        {
            get
            {
                if (m_steamID != null) return m_steamID;

                if (name.Contains("."))
                {
                    m_steamID = name.Substring(0, name.IndexOf("."));

                    ulong result;
                    if (!ulong.TryParse(m_steamID, out result) || result == 0)
                        m_steamID = null;
                }

                return m_steamID;
            }
        }

        private static int GetUnitsCapacity(VehicleAI vehicleAI)
        {
            VehicleAI ai;

            ai = vehicleAI as AmbulanceAI;
            if (ai != null) return ((AmbulanceAI)ai).m_patientCapacity + ((AmbulanceAI)ai).m_paramedicCount;

            ai = vehicleAI as BusAI;
            if (ai != null) return ((BusAI)ai).m_passengerCapacity;

            ai = vehicleAI as HearseAI;
            if (ai != null) return ((HearseAI)ai).m_corpseCapacity + ((HearseAI)ai).m_driverCount;

            ai = vehicleAI as PassengerPlaneAI;
            if (ai != null) return ((PassengerPlaneAI)ai).m_passengerCapacity;

            ai = vehicleAI as PassengerShipAI;
            if (ai != null) return ((PassengerShipAI)ai).m_passengerCapacity;

            ai = vehicleAI as PassengerTrainAI;
            if (ai != null) return ((PassengerTrainAI)ai).m_passengerCapacity;

            ai = vehicleAI as TramAI;
            if (ai != null) return ((TramAI)ai).m_passengerCapacity;

            ai = vehicleAI as CableCarAI;
            if (ai != null) return ((CableCarAI)ai).m_passengerCapacity;
			
			ai = vehicleAI as TrolleybusAI;
            if (ai != null) return ((TrolleybusAI)ai).m_passengerCapacity;

            ai = vehicleAI as PassengerFerryAI;
            if (ai != null) return ((PassengerFerryAI)ai).m_passengerCapacity;

            ai = vehicleAI as PassengerBlimpAI;
            if (ai != null) return ((PassengerBlimpAI)ai).m_passengerCapacity;
			
			ai = vehicleAI as PassengerHelicopterAI;
            if (ai != null) return ((PassengerHelicopterAI)ai).m_passengerCapacity;

            return -1;
        }

        private static int GetTotalUnitGroups(uint unitID)
        {
            int count = 0;
            while (unitID != 0)
            {
                CitizenUnit unit = CitizenManager.instance.m_units.m_buffer[unitID];
                unitID = unit.m_nextUnit;
                count++;
            }
            return count;
        }

        public static void UpdateCapacityUnits()
        {
            int count = 0;
            Array16<Vehicle> vehicles = VehicleManager.instance.m_vehicles;
            for (uint i = 0; i < vehicles.m_buffer.Length; i++)
            {
                if ((vehicles.m_buffer[i].m_flags & Vehicle.Flags.Spawned) == Vehicle.Flags.Spawned)
                {
                    if (prefabUpdateUnits == null || vehicles.m_buffer[i].Info == prefabUpdateUnits)
                    {
                        int capacity = GetUnitsCapacity(vehicles.m_buffer[i].Info.m_vehicleAI);

                        if (capacity != -1)
                        {
                            CitizenUnit[] units = CitizenManager.instance.m_units.m_buffer;
                            uint unit = vehicles.m_buffer[i].m_citizenUnits;

                            int currentUnitCount = GetTotalUnitGroups(unit);
                            int newUnitCount = Mathf.CeilToInt(capacity / 5f);

                            // Capacity reduced
                            if (newUnitCount < currentUnitCount)
                            {
                                // Get the first unit to remove
                                uint n = unit;
                                for (int j = 1; j < newUnitCount; j++)
                                    n = units[n].m_nextUnit;
                                // Releasing units excess
                                CitizenManager.instance.ReleaseUnits(units[n].m_nextUnit);
                                units[n].m_nextUnit = 0;

                                count++;
                            }
                            // Capacity increased
                            else if (newUnitCount > currentUnitCount)
                            {
                                // Get the last unit
                                uint n = unit;
                                while (units[n].m_nextUnit != 0)
                                    n = units[n].m_nextUnit;

                                // Creating missing units
                                int newCapacity = capacity - currentUnitCount * 5;
                                CitizenManager.instance.CreateUnits(out units[n].m_nextUnit, ref SimulationManager.instance.m_randomizer, 0, (ushort)i, 0, 0, 0, newCapacity, 0);

                                count++;
                            }
                        }
                    }
                }
            }
            prefabUpdateUnits = null;

            if (count > 0)
            {
                Logging.Message("Modified capacity of " + count + " vehicle(s). Total unit count: " + CitizenManager.instance.m_unitCount + "/" + CitizenManager.MAX_UNIT_COUNT);
            }
        }

        public static void UpdateBackEngines()
        {
            Array16<Vehicle> vehicles = VehicleManager.instance.m_vehicles;
            for (uint i = 0; i < vehicles.m_buffer.Length; i++)
            {
                try
                {
                    VehicleInfo prefab = vehicles.m_buffer[i].Info;
                    if (prefab != null)
                    {
                        bool isTrain = prefab.m_vehicleType == VehicleInfo.VehicleType.Train || prefab.m_vehicleType == VehicleInfo.VehicleType.Tram || prefab.m_vehicleType == VehicleInfo.VehicleType.Metro || prefab.m_vehicleType == VehicleInfo.VehicleType.Monorail;
                        bool isLeading = vehicles.m_buffer[i].m_leadingVehicle == 0 && prefab.m_trailers != null && prefab.m_trailers.Length > 0;
                        if ((prefabUpdateEngine == null || prefab == prefabUpdateEngine) && isTrain && isLeading && prefab.m_trailers[prefab.m_trailers.Length - 1].m_info != null)
                        {
                            ushort last = vehicles.m_buffer[i].GetLastVehicle((ushort)i);
                            ushort oldPrefabID = vehicles.m_buffer[last].m_infoIndex;
                            ushort newPrefabID = (ushort)prefab.m_trailers[prefab.m_trailers.Length - 1].m_info.m_prefabDataIndex;
                            if (oldPrefabID != newPrefabID)
                            {
                                vehicles.m_buffer[last].m_infoIndex = newPrefabID;
                                vehicles.m_buffer[last].m_flags = vehicles.m_buffer[vehicles.m_buffer[last].m_leadingVehicle].m_flags;

                                if (prefab.m_trailers[prefab.m_trailers.Length - 1].m_info == prefab)
                                    vehicles.m_buffer[last].m_flags |= Vehicle.Flags.Inverted;
                            }
                        }
                    }

                }
                catch (Exception e)
                {
                    Logging.Error("Couldn't update back engine :");
                    Logging.LogException(e);
                }
            }

            prefabUpdateEngine = null;
        }

        public static void UpdateTransfertVehicles()
        {
            SimulationManager.instance.AddAction(() => {
                try
                {
                    typeof(VehicleManager).GetField("m_vehiclesRefreshed", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(VehicleManager.instance, false);
                    VehicleManager.instance.RefreshTransferVehicles();
                }
                catch(Exception e)
                {
                    Logging.Error("Couldn't update transfer vehicles :");
                    Logging.LogException(e);
                }
            });
        }

        public void SetPrefab(VehicleInfo prefab)
        {
            if (prefab == null) return;

            m_prefab = prefab;
            m_placementStyle = prefab.m_placementStyle;

            m_engine = GetEngine();
            if (m_engine != null)
            {
                m_localizedName = Locale.GetUnchecked("VEHICLE_TITLE", m_engine.name) + " (Trailer)";
            }
            else
            {
                m_localizedName = Locale.GetUnchecked("VEHICLE_TITLE", prefab.name);
                if (m_localizedName.StartsWith("VEHICLE_TITLE"))
                {
                    m_localizedName = prefab.name;
                    // Removes the steam ID and trailing _Data from the name
                    m_localizedName = m_localizedName.Substring(m_localizedName.IndexOf('.') + 1).Replace("_Data", "");
                }
            }

            m_hasCapacity = capacity != -1;
            m_hasSpecialCapacity = specialcapacity != -1;
        }

        public int CompareTo(object o)
        {
            VehicleOptions options = o as VehicleOptions;
            if (options == null) return 1;


            int delta = category - options.category;
            if (delta == 0)
            {
                if (steamID != null && options.steamID == null)
                    delta = 1;
                else if (steamID == null && options.steamID != null)
                    delta = -1;
            }
            if (delta == 0) return localizedName.CompareTo(options.localizedName);

            return delta;
        }

        private static Dictionary<VehicleInfo, VehicleInfo> _trailerEngines = null;
        public static void Clear()
        {
            if (_trailerEngines != null)
            {
                _trailerEngines.Clear();
                _trailerEngines = null;
            }
        }

        private VehicleInfo GetEngine()
        {
            if (_trailerEngines == null)
            {
                _trailerEngines = new Dictionary<VehicleInfo, VehicleInfo>();

                for (uint i = 0; i < PrefabCollection<VehicleInfo>.PrefabCount(); i++)
                {
                    try
                    {
                        VehicleInfo prefab = PrefabCollection<VehicleInfo>.GetPrefab(i);

                        if (prefab == null || prefab.m_trailers == null || prefab.m_trailers.Length == 0) continue;

                        for (int j = 0; j < prefab.m_trailers.Length; j++)
                        {
                            if (prefab.m_trailers[j].m_info != null && prefab.m_trailers[j].m_info != prefab && !_trailerEngines.ContainsKey(prefab.m_trailers[j].m_info))
                                _trailerEngines.Add(prefab.m_trailers[j].m_info, prefab);
                        }
                    }
                    catch (Exception e)
                    {
                        Logging.LogException(e);
                    }
                }
            }

            if (_trailerEngines.ContainsKey(m_prefab))
                return _trailerEngines[m_prefab];

            return null;
        }
    }

    public struct HexaColor : IXmlSerializable
    {
        private float r, g, b;

        public string Value
        {
            get
            {
                return ToString();
            }

            set
            {
                value = value.Trim().Replace("#", "");

                if (value.Length != 6) return;

                try
                {
                    r = int.Parse(value.Substring(0, 2), System.Globalization.NumberStyles.HexNumber) / 255f;
                    g = int.Parse(value.Substring(2, 2), System.Globalization.NumberStyles.HexNumber) / 255f;
                    b = int.Parse(value.Substring(4, 2), System.Globalization.NumberStyles.HexNumber) / 255f;
                }
                catch
                {
                    r = g = b = 0;
                }
            }
        }

        public HexaColor(string value)
        {
            try
            {
                r = int.Parse(value.Substring(0, 2), System.Globalization.NumberStyles.HexNumber) / 255f;
                g = int.Parse(value.Substring(2, 2), System.Globalization.NumberStyles.HexNumber) / 255f;
                b = int.Parse(value.Substring(4, 2), System.Globalization.NumberStyles.HexNumber) / 255f;
            }
            catch
            {
                r = g = b = 0;
            }
        }

        public System.Xml.Schema.XmlSchema GetSchema()
        {
            return null;
        }

        public void ReadXml(System.Xml.XmlReader reader)
        {
            Value = reader.ReadString();
        }

        public void WriteXml(System.Xml.XmlWriter writer)
        {
            writer.WriteString(Value);
        }

        public override string ToString()
        {
            StringBuilder s = new StringBuilder();

            s.Append(((int)(255 * r)).ToString("X2"));
            s.Append(((int)(255 * g)).ToString("X2"));
            s.Append(((int)(255 * b)).ToString("X2"));

            return s.ToString();
        }

        public static implicit operator HexaColor(Color c)
        {
            HexaColor temp = new HexaColor();

            temp.r = c.r;
            temp.g = c.g;
            temp.b = c.b;

            return temp;
        }

        public static implicit operator Color(HexaColor c)
        {
            return new Color(c.r, c.g, c.b, 1f);
        }
    }

    public class DefaultOptions
    {
        private static Dictionary<string, VehicleInfo> m_prefabs = new Dictionary<string, VehicleInfo>();
        private static Dictionary<string, DefaultOptions> m_default = new Dictionary<string, DefaultOptions>();
        private static Dictionary<string, DefaultOptions> m_modded = new Dictionary<string, DefaultOptions>();

        public static ItemClass.Placement GetPlacementStyle(VehicleInfo prefab)
        {
            if (m_default.ContainsKey(prefab.name))
                return m_default[prefab.name].m_placementStyle;
            return (ItemClass.Placement)(-1);
        }

        public static VehicleInfo GetLastTrailer(VehicleInfo prefab)
        {
            if (m_default.ContainsKey(prefab.name))
                return m_default[prefab.name].m_lastTrailer;
            return null;
        }

        public static int GetProbability(VehicleInfo prefab)
        {
            if (m_default.ContainsKey(prefab.name))
                return m_default[prefab.name].m_probability;
            return 0;
        }

        public static void Store(VehicleInfo prefab)
        {
            if (prefab != null && !m_default.ContainsKey(prefab.name))
            {
                m_default.Add(prefab.name, new DefaultOptions(prefab));
            }
        }

        public static void StoreAll()
        {
            DefaultOptions.Clear();
            for (uint i = 0; i < PrefabCollection<VehicleInfo>.PrefabCount(); i++)
                DefaultOptions.Store(PrefabCollection<VehicleInfo>.GetPrefab(i));

            Logging.Message("Default values stored");
        }

        public static void StoreAllModded()
        {
            if (m_modded.Count > 0) return;

            for (uint i = 0; i < PrefabCollection<VehicleInfo>.PrefabCount(); i++)
            {
                VehicleInfo prefab = PrefabCollection<VehicleInfo>.GetPrefab(i);

                if (prefab != null && !m_modded.ContainsKey(prefab.name))
                    m_modded.Add(prefab.name, new DefaultOptions(prefab));
            }
        }

        public static void BuildVehicleInfoDictionary()
        {
            m_prefabs.Clear();

            for (uint i = 0; i < PrefabCollection<VehicleInfo>.PrefabCount(); i++)
            {
                VehicleInfo prefab = PrefabCollection<VehicleInfo>.GetPrefab(i);

                if (prefab != null)
                    m_prefabs[prefab.name] = prefab;
            }
        }

        public static void CheckForConflicts()
        {
            StringBuilder conflicts = new StringBuilder();

            foreach (string name in m_default.Keys)
            {
                VehicleOptions options = new VehicleOptions();
                options.SetPrefab(m_prefabs[name]);

                DefaultOptions modded = m_modded[name];
                DefaultOptions stored = m_default[name];

                StringBuilder details = new StringBuilder();

                if (modded.m_enabled != stored.m_enabled && options.enabled == stored.m_enabled)
                {
                    options.enabled = modded.m_enabled;
                    details.Append("enabled, ");
                }
                if (modded.m_addBackEngine != stored.m_addBackEngine && options.addBackEngine == stored.m_addBackEngine)
                {
                    options.addBackEngine = modded.m_addBackEngine;
                    details.Append("back engine, ");
                }
                if (modded.m_maxSpeed != stored.m_maxSpeed && options.maxSpeed == stored.m_maxSpeed)
                {
                    options.maxSpeed = modded.m_maxSpeed;
                    details.Append("max speed, ");
                }
                if (modded.m_acceleration != stored.m_acceleration && options.acceleration == stored.m_acceleration)
                {
                    options.acceleration = modded.m_acceleration;
                    details.Append("acceleration, ");
                }
                if (modded.m_braking != stored.m_braking && options.braking == stored.m_braking)
                {
                    options.braking = modded.m_braking;
                    details.Append("braking, ");
                }
			    if (modded.m_turning != stored.m_turning && options.turning == stored.m_turning)
                {
                    options.turning = modded.m_turning;
                    details.Append("turning, ");
                }
                if (modded.m_springs != stored.m_springs && options.springs == stored.m_springs)
                {
                    options.springs = modded.m_springs;
                    details.Append("springs, ");
                }
                if (modded.m_dampers != stored.m_dampers && options.dampers == stored.m_dampers)
                {
                    options.dampers = modded.m_dampers;
                    details.Append("dampers, ");
                }
                if (modded.m_leanMultiplier != stored.m_leanMultiplier && options.leanMultiplier == stored.m_leanMultiplier)
                {
                    options.leanMultiplier = modded.m_leanMultiplier;
                    details.Append("leanMultiplier, ");
                }
                if (modded.m_nodMultiplier != stored.m_nodMultiplier && options.nodMultiplier == stored.m_nodMultiplier)
                {
                    options.nodMultiplier = modded.m_nodMultiplier;
                    details.Append("nodMultiplier, ");
                }
                if (modded.m_capacity != stored.m_capacity && options.capacity == stored.m_capacity)
                {
                    options.capacity = modded.m_capacity;
                    details.Append("capacity, ");
                }

                if (modded.m_specialcapacity != stored.m_specialcapacity && options.specialcapacity == stored.m_specialcapacity)
                {
                    options.specialcapacity = modded.m_specialcapacity;
                    details.Append("specialcapacity, ");
                }

                if (modded.m_isLargeVehicle != stored.m_isLargeVehicle && options.isLargeVehicle == stored.m_isLargeVehicle)
                {
                    options.isLargeVehicle = modded.m_isLargeVehicle;
                    details.Append("isLargeVehicle, ");
                }

                if (modded.m_classname != stored.m_classname && options.classname == stored.m_classname)
                {
                    options.classname = modded.m_classname;
                    details.Append("ItemClass Name, ");
                }

                if (details.Length > 0)
                {
                    details.Length -= 2;
                    conflicts.AppendLine(options.name + ": " + details);
                }
            }

            if (conflicts.Length > 0)
            {
                VehicleOptions.UpdateTransfertVehicles();
                Logging.Message("Conflicts detected (this message is harmless):" + Environment.NewLine + conflicts);
            }
        }

        public static void Restore(VehicleInfo prefab)
        {
            if (prefab == null) return;

            VehicleOptions options = new VehicleOptions();
            options.SetPrefab(prefab);

            DefaultOptions stored = m_default[prefab.name];
            if (stored == null) return;

            options.enabled = stored.m_enabled;
            options.addBackEngine = stored.m_addBackEngine;
            options.maxSpeed = stored.m_maxSpeed;
            options.acceleration = stored.m_acceleration;
            options.braking = stored.m_braking;
            options.turning = stored.m_turning;
            options.springs = stored.m_springs;
            options.dampers = stored.m_dampers;
            options.leanMultiplier = stored.m_leanMultiplier;
            options.nodMultiplier = stored.m_nodMultiplier;
            options.useColorVariations = stored.m_useColorVariations;
            options.color0 = stored.m_color0;
            options.color1 = stored.m_color1;
            options.color2 = stored.m_color2;
            options.color3 = stored.m_color3;
            options.capacity = stored.m_capacity;
            options.specialcapacity = stored.m_specialcapacity;
            options.isLargeVehicle = stored.m_isLargeVehicle;
            prefab.m_placementStyle = stored.m_placementStyle;
            prefab.m_class.name = stored.m_classname;
        }

        public static void RestoreAll()
        {
            foreach (string name in m_default.Keys)
            {
                Restore(m_prefabs[name]);
            }
            VehicleOptions.UpdateTransfertVehicles();
        }

        public static void Clear()
        {
            m_default.Clear();
            m_modded.Clear();
        }

        private DefaultOptions(VehicleInfo prefab)
        {
            VehicleOptions options = new VehicleOptions();
            options.SetPrefab(prefab);

            m_enabled = options.enabled;
            m_addBackEngine = options.addBackEngine;
            m_maxSpeed = options.maxSpeed;
            m_acceleration = options.acceleration;
            m_braking = options.braking;
            m_turning = options.turning;
            m_springs = options.springs;
            m_dampers = options.dampers;
            m_leanMultiplier = options.leanMultiplier;
            m_nodMultiplier = options.nodMultiplier;
            m_useColorVariations = options.useColorVariations;
            m_color0 = options.color0;
            m_color1 = options.color1;
            m_color2 = options.color2;
            m_color3 = options.color3;
            m_capacity = options.capacity;
            m_specialcapacity = options.specialcapacity;  
            m_placementStyle = options.placementStyle;
            m_isLargeVehicle = options.isLargeVehicle;
            m_classname = prefab.m_class.name;

            if (prefab.m_trailers != null && prefab.m_trailers.Length > 0)
            {
                m_lastTrailer = prefab.m_trailers[prefab.m_trailers.Length - 1].m_info;
                m_probability = prefab.m_trailers[prefab.m_trailers.Length - 1].m_invertProbability;
            }
        }

        private bool m_enabled;
        private bool m_addBackEngine;
        private float m_maxSpeed;
        private float m_acceleration;
        private float m_braking;
	    private float m_turning;
        private float m_springs;
        private float m_dampers;
        private float m_leanMultiplier;
        private float m_nodMultiplier;
        private bool m_useColorVariations;
        private HexaColor m_color0;
        private HexaColor m_color1;
        private HexaColor m_color2;
        private HexaColor m_color3;
        private int m_capacity;
        private int m_specialcapacity;             
        private ItemClass.Placement m_placementStyle;
        private VehicleInfo m_lastTrailer;
        private int m_probability;
        private bool m_isLargeVehicle;
        private string m_classname;
    }
}
