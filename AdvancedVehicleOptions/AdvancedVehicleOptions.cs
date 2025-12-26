using ICities;
using UnityEngine;

using System;
using System.Text;
using System.Collections.Generic;
using System.IO;

using ColossalFramework;
using ColossalFramework.UI;
using ColossalFramework.IO;

using AdvancedVehicleOptionsUID.Compatibility;
using AdvancedVehicleOptionsUID.GUI;

namespace AdvancedVehicleOptionsUID
{
    public class AVOMod : IUserMod

    {
    public static string ModName => "Advanced Vehicle Options";
    //public static string Version => "1.9.12 b3 20052023 1.17.0-f3";

    public static string Version => "1.9.12";
    public string Name => ModName + " " + Version;

    public AVOMod()
        {
            try
            {
                // Creating setting file
                GameSettings.AddSettingsFile(new SettingsFile[] { new SettingsFile() { fileName = AdvancedVehicleOptions.settingsFileName } });
            }
            catch (Exception e)
            {
                Logging.Error("Couldn't load/create the setting file.");
                Logging.LogException(e);
            }
        }

        public string Description
        {
            get { return (Translations.Translate("AVO_DESC")); }        // The Mod Description in the Content Manager
        }

        public void OnEnabled()
        {
            // Load the settings file.
            ModSettings.Load();
        }

        public void OnSettingsUI(UIHelperBase helper)
        {
            try
            {
                // Section for General Settings

                UIHelperBase Group_General = helper.AddGroup(Translations.Translate("AVO_OPT_GEN") + " - " + Name);

                // Checkbox for Autosave Config

                UICheckBox AutoSaveVehicleConfig_Box = (UICheckBox)Group_General.AddCheckbox(Translations.Translate("AVO_OPT_GEN_ASAVE"), AdvancedVehicleOptions.AutoSaveVehicleConfig, (b) =>
                {
                    AdvancedVehicleOptions.AutoSaveVehicleConfig = b;
                    ModSettings.Save();
                    AdvancedVehicleOptions.UpdateOptionPanelInfo();
                });

                AutoSaveVehicleConfig_Box.tooltip = Translations.Translate("AVO_OPT_GEN_ASAVE_TT");

                // Checkbox for validating services

                UICheckBox ValidateMissingServices_Box = (UICheckBox)Group_General.AddCheckbox(Translations.Translate("AVO_OPT_GEN_SERVICE"), AdvancedVehicleOptions.OnLoadValidateServices, (b) =>
                {
                    AdvancedVehicleOptions.OnLoadValidateServices = b;
                    ModSettings.Save();
                });

                ValidateMissingServices_Box.tooltip = Translations.Translate("AVO_OPT_GEN_SERVICE_TT");

                // Checkbox for Trailer Sync setting

                UICheckBox TrailerSync_Box = (UICheckBox)Group_General.AddCheckbox(Translations.Translate("AVO_OPT_SYNC"), AdvancedVehicleOptions.RememberSyncTrailerSetting, (b) =>
                {
                    AdvancedVehicleOptions.RememberSyncTrailerSetting = b;
                    ModSettings.Save();
                });

                TrailerSync_Box.tooltip = Translations.Translate("AVO_OPT_SYNC_TT");

                // Checkbox for Debug Setting

                UICheckBox DebugMsg_Box = (UICheckBox)Group_General.AddCheckbox(Translations.Translate("AVO_OPT_GEN_DEBUG"), Logging.detailLogging, (b) =>
                {
                    Logging.detailLogging = b;
                    ModSettings.Save();
                });

                DebugMsg_Box.tooltip = Translations.Translate("AVO_OPT_GEN_DEBUG_TT");

                Group_General.AddSpace(1);

                // Checkbox for GUI Button

                UICheckBox HideButton_Box = (UICheckBox)Group_General.AddCheckbox(Translations.Translate("AVO_OPT_GEN_GUI"), AdvancedVehicleOptions.HideGUIbutton, (b) =>
                {
                    AdvancedVehicleOptions.HideGUIbutton = b;
                    AdvancedVehicleOptions.UpdateGUI();
                    ModSettings.Save();
                });

                HideButton_Box.tooltip = Translations.Translate("AVO_OPT_GEN_GUI_TT");

                // Datafield for Hot Key

                HideButton_Box.parent.gameObject.AddComponent<OptionsKeymapping>();

                // Checkbox for Language Option

                UIDropDown Language_DropDown = (UIDropDown)Group_General.AddDropdown(Translations.Translate("TRN_CHOICE"), Translations.LanguageList, Translations.Index, (value) => 
                {
                    Translations.Index = value;
                    ModSettings.Save();
                });

                Language_DropDown.tooltip = Translations.Translate("TRN_TOOLTIP");
                Language_DropDown.autoSize = false;
                Language_DropDown.width = 350f;

                // Section for Game Balancing

                UIHelperBase Group_Balance = helper.AddGroup(Translations.Translate("AVO_OPT_BAL"));

                // Checkbox for SpeedUnitOption kmh vs mph	

                UICheckBox SpeedUnitOptions_Box = (UICheckBox)Group_Balance.AddCheckbox(Translations.Translate("AVO_OPT_BAL_UNITS"), AdvancedVehicleOptions.SpeedUnitOption, (b) =>
                {
                    AdvancedVehicleOptions.SpeedUnitOption = b;
                    ModSettings.Save();
                    AdvancedVehicleOptions.UpdateOptionPanelInfo();
                });

                SpeedUnitOptions_Box.tooltip = Translations.Translate("AVO_OPT_BAL_UNITS_TT");

                // Checkbox for Game Balancing	

                UICheckBox ExtendedValues_Box = (UICheckBox)Group_Balance.AddCheckbox(Translations.Translate("AVO_OPT_BAL_EXT"), AdvancedVehicleOptions.ShowMoreVehicleOptions, (b) =>
                {
                    AdvancedVehicleOptions.ShowMoreVehicleOptions = b;
                    ModSettings.Save();
                });

                ExtendedValues_Box.tooltip = Translations.Translate("AVO_OPT_BAL_EXT_TT");

                // Section for Compatibility

                UIHelperBase Group_Compatibility = helper.AddGroup(Translations.Translate("AVO_OPT_COMP"));

                // Checkbox for Overriding Incompability Warnings

                UICheckBox DisplayCompatibility_Box = (UICheckBox)Group_Compatibility.AddCheckbox(Translations.Translate("AVO_OPT_COMP_MODS"), AdvancedVehicleOptions.OverrideCompatibilityWarnings, (b) =>
                {
                    AdvancedVehicleOptions.OverrideCompatibilityWarnings = b;
                    ModSettings.Save();
                });

                DisplayCompatibility_Box.tooltip = Translations.Translate("AVO_OPT_COMP_MODS_TT");

                // Default = True, as AVO shall color shared mod setting values in red.

                // Checkbox for Vehicle Color Expander

                UICheckBox OverrideVCX_Box = (UICheckBox)Group_Compatibility.AddCheckbox(Translations.Translate("AVO_OPT_COMP_VCX"), AdvancedVehicleOptions.OverrideVCX, (b) =>
                {
                    AdvancedVehicleOptions.OverrideVCX = b;
                    ModSettings.Save();
                });

                OverrideVCX_Box.tooltip = Translations.Translate("AVO_OPT_COMP_VCX_TT");

                //Always True, if AVO shall not override Vehicle Color Expander / Asset Color Expander settings. As there is no Settings for Vehicle Color Expander / Asset Color Expander. AVO will show the option, but user cannot change anything as long readOnly is True.

                OverrideVCX_Box.readOnly = true;
                OverrideVCX_Box.label.textColor = Color.gray;

                if (!VCXCompatibilityPatch.IsVCXActive() | !VCXCompatibilityPatch.IsACXActive())
                {
                    OverrideVCX_Box.enabled = false;    //Do not show the option Checkbox, if Vehicle Color Expander / Asset Color Expander is not active.
                }

                // Checkbox for No Big Trucks

                UICheckBox NoBigTrucks_Box = (UICheckBox)Group_Compatibility.AddCheckbox(Translations.Translate("AVO_OPT_COMP_NBT"), AdvancedVehicleOptions.ControlTruckDelivery, (b) =>
                {
                    AdvancedVehicleOptions.ControlTruckDelivery = b;
                    ModSettings.Save();
                });

                NoBigTrucks_Box.tooltip = Translations.Translate("AVO_OPT_COMP_NBT_TT");
                //True, if AVO shall be enabled to classify Generic Industry vehicles as Large Vehicles, so No Big Trucks can suppress the dispatch to small buildings.

                if (!NoBigTruckCompatibilityPatch.IsNBTActive() | !NoBigTruckCompatibilityPatch.IsNBTBetaActive())
                {
                    NoBigTrucks_Box.enabled = false;   //Do not show the option Checkbox, if No Big Trucks is not active.
                }

 // Support Section with Wiki and Output-Log	

                UIHelperBase Group_Support = helper.AddGroup(Translations.Translate("AVO_OPT_SUP"));
				
				UIButton Wikipedia_Button = (UIButton)Group_Support.AddButton(Translations.Translate("AVO_OPT_SUP_WIKI"), () =>
                {
                    SimulationManager.instance.SimulationPaused = true;
                    Application.OpenURL("https://github.com/CityGecko/CS-AdvancedVehicleOptions/wiki");
                });
                Wikipedia_Button.textScale = 0.8f;

                UIButton OutputLog_Button = (UIButton)Group_Support.AddButton(Translations.Translate("AVO_OPT_SUP_LOG"), () =>
                {
                    Utils.OpenInFileBrowser(Application.dataPath);
                });
                OutputLog_Button.textScale = 0.8f;

                UIButton AVOLog_Button = (UIButton)Group_Support.AddButton(Translations.Translate("AVO_OPT_SUP_SET"), () =>
                {
                    // Utils.OpenInFileBrowser(Application.streamingAssetsPath);
                    Utils.OpenInFileBrowser(DataLocation.localApplicationData);
                  });
                AVOLog_Button.textScale = 0.8f;
                AVOLog_Button.tooltip = (Translations.Translate("AVO_OPT_SUP_SET_TT"));
            }	
			
            catch (Exception e)
            {
                Logging.Error("OnSettingsUI failed");
                Logging.LogException(e);
            }
        }
    }

    public class AdvancedVehicleOptionsLoader : LoadingExtensionBase
    {
        private static AdvancedVehicleOptions instance;

        #region LoadingExtensionBase overrides
        /// <summary>
        /// Called when the level (game, map editor, asset editor) is loaded
        /// </summary>
        public override void OnLevelLoaded(LoadMode mode)
        {
            try
            {
                // Is it an actual game ?
                if (mode != LoadMode.LoadGame && mode != LoadMode.NewGame && mode != LoadMode.NewGameFromScenario)
                {
                    DefaultOptions.Clear();
			        Logging.Error("AVO Incompatible GameMode " + mode);
                    return;
                }

                AdvancedVehicleOptions.isGameLoaded = true;

                if (instance != null)
                {
                    GameObject.DestroyImmediate(instance.gameObject);
                }

                instance = new GameObject("AdvancedVehicleOptions").AddComponent<AdvancedVehicleOptions>();

                try
                {
                    DefaultOptions.BuildVehicleInfoDictionary();
                    VehicleOptions.Clear();
                    Logging.Message("UIMainPanel created");
                }
                catch
                {
                    Logging.Error("Could not create UIMainPanel");

                    if (instance != null)
                        GameObject.Destroy(instance.gameObject);

                    return;
                }

                // new EnumerableActionThread(BrokenAssetsFix);
            }
            catch (Exception e)
            {
                if (instance != null)
                    GameObject.Destroy(instance.gameObject);
                Logging.LogException(e);
            }
        }

        /// <summary>
        /// Called when the level is unloaded
        /// </summary>
        public override void OnLevelUnloading()
        {
            try
            {
                Logging.Message("Restoring default values");
                DefaultOptions.RestoreAll();
                DefaultOptions.Clear();

                if (instance != null)
                    GameObject.Destroy(instance.gameObject);

                AdvancedVehicleOptions.isGameLoaded = false;
            }
            catch (Exception e)
            {
                Logging.LogException(e);
            }
        }
        #endregion
    }

    public class AdvancedVehicleOptions : MonoBehaviour
    {
        public const string settingsFileName = "AdvancedVehicleOptionsSettings";

        internal static bool AutoSaveVehicleConfig;
        internal static bool RememberSyncTrailerSetting;
        internal static bool LastSyncTrailerSetting;
        internal static bool HideGUIbutton;
        internal static bool OnLoadValidateServices;
        internal static bool SpeedUnitOption;
        internal static bool ShowMoreVehicleOptions;
        internal static bool OverrideVCX ;
        internal static bool OverrideCompatibilityWarnings;
        internal static bool ControlTruckDelivery;
        internal static bool hasAirportDLC;
        internal static bool hasFinancialDistricsDLC;
        internal static bool hasIndustrialDLC;
        internal static bool hasSunsetHarborDLC;
        internal static bool hasMassTransitDLC;
        internal static bool hasParkLifeDLC;
        internal static bool hasAfterDarkDLC;


        internal static GUI.UIMainPanel m_mainPanel;

        private static VehicleInfo m_removeInfo;
        private static VehicleInfo m_removeParkedInfo;

        private static readonly string m_OldSettingsDir = ColossalFramework.IO.DataLocation.currentDirectory;
        private static readonly string m_OldConfigFileName = "AdvancedVehicleOptionsUID.xml";

        private static readonly string m_UserSettingsDir = ColossalFramework.IO.DataLocation.localApplicationData;
        private static string m_VehicleConfigFileName = "AdvancedVehicleOptions_VehicleData.xml";
        internal static string m_VehicleSettingsFile = Path.Combine(m_UserSettingsDir, m_VehicleConfigFileName);

        public static bool isGameLoaded = false;
        public static Configuration config = new Configuration();

        public void Start()
        {
            try
            {
                //Checking for Stand Type requirement: is AirportDLC installed
                hasAirportDLC = (ColossalFramework.PlatformServices.PlatformService.IsDlcInstalled(SteamHelper.kAirportDLCAppID));
                hasFinancialDistricsDLC = (ColossalFramework.PlatformServices.PlatformService.IsDlcInstalled(SteamHelper.kFinancialDistrictsDLCAppID));
                hasIndustrialDLC = (ColossalFramework.PlatformServices.PlatformService.IsDlcInstalled(SteamHelper.kIndustryDLCAppID));
                hasSunsetHarborDLC = (ColossalFramework.PlatformServices.PlatformService.IsDlcInstalled(SteamHelper.kUrbanDLCAppID));
                hasMassTransitDLC = (ColossalFramework.PlatformServices.PlatformService.IsDlcInstalled(SteamHelper.kMotionDLCAppID));
                hasParkLifeDLC = (ColossalFramework.PlatformServices.PlatformService.IsDlcInstalled(SteamHelper.kParksDLCAppID));
                hasAfterDarkDLC = (ColossalFramework.PlatformServices.PlatformService.IsDlcInstalled(SteamHelper.kAfterDLCAppID));

                // Loading config
                AdvancedVehicleOptions.InitVehicleDataConfig();

                if (AdvancedVehicleOptions.OnLoadValidateServices)
                {
                    AdvancedVehicleOptions.CheckAllServicesValidity();
                }

                m_mainPanel = GameObject.FindObjectOfType<GUI.UIMainPanel>();
                UpdateGUI();

                UIThreading.Operating = true;
            }
            catch (Exception e)
            {
                Logging.Error("UI initialization failed.");
                Logging.LogException(e);

                GameObject.Destroy(gameObject);
            }
        }

        public static void UpdateGUI()
        {
            if (!isGameLoaded) return;

            if (!HideGUIbutton && m_mainPanel == null)
            {
                // Creating GUI
                m_mainPanel = UIView.GetAView().AddUIComponent(typeof(GUI.UIMainPanel)) as GUI.UIMainPanel;
            }
            else if (HideGUIbutton && m_mainPanel != null)
            {
                // Removing GUI
                GameObject.Destroy(m_mainPanel.gameObject);
                m_mainPanel = null;
            }
        }

        public static void UpdateOptionPanelInfo()
           
        {
             if ((m_mainPanel != null) && (m_mainPanel.isVisible == true))
            {
                if (AdvancedVehicleOptions.AutoSaveVehicleConfig == true)
                {
                    m_mainPanel.m_autosave.isVisible = AutoSaveVehicleConfig;
                }

                if (AdvancedVehicleOptions.AutoSaveVehicleConfig == false)
                {
                    m_mainPanel.m_autosave.isVisible = AutoSaveVehicleConfig;
                }

                if (AdvancedVehicleOptions.SpeedUnitOption == false)
                {
                    UIOptionPanel.m_maxSpeed.text = Mathf.RoundToInt(m_mainPanel.m_optionPanel.m_options.maxSpeed * UIOptionPanel.maxSpeedToKmhConversionFactor).ToString();
                    m_mainPanel.m_optionPanel.kmhLabel.text = Translations.Translate("AVO_MOD_OP01");
                }   
                
                if (AdvancedVehicleOptions.SpeedUnitOption == true)
                {
                    UIOptionPanel.m_maxSpeed.text = Mathf.RoundToInt((m_mainPanel.m_optionPanel.m_options.maxSpeed / UIOptionPanel.mphFactor) * UIOptionPanel.maxSpeedToKmhConversionFactor).ToString();
                    m_mainPanel.m_optionPanel.kmhLabel.text = Translations.Translate("AVO_MOD_OP02");
                }
            }
        }

/// <summary>
/// Init the Vehicle Data Configuration (not the Mod Configuration)A
/// </summary>
public static void InitVehicleDataConfig()
        {

        // Store modded values
        DefaultOptions.StoreAllModded();

            if (config.data != null)
            {
                config.DataToOptions();

                // Remove unneeded options
                List<VehicleOptions> optionsList = new List<VehicleOptions>();

                for (uint i = 0; i < config.options.Length; i++)
                {
                    if (config.options[i] != null && config.options[i].prefab != null) optionsList.Add(config.options[i]);
                }

                config.options = optionsList.ToArray();
            }
            else if (File.Exists(m_VehicleSettingsFile))
            {
                // Import config from Standard Filename
                ImportVehicleDataConfig();
                return;
            }
            else
            {
                // If Configuration has not been found, try to find the old config file
                if (File.Exists(m_OldConfigFileName))

                {
                    Logging.Message("Old vehicle configuration file found. Importing  old values and converting to Filename: " + Path.Combine(m_OldSettingsDir, m_OldConfigFileName));

                    m_VehicleSettingsFile = m_OldConfigFileName;

                    // Import config from Old Filename and export to the new filename
                    ImportVehicleDataConfig();

                    m_VehicleSettingsFile = Path.Combine(m_UserSettingsDir, m_VehicleConfigFileName);
                    config.Serialize(m_VehicleSettingsFile);

                    Logging.Message("Old vehicle configuration file converted to new file. New Filename: " + m_VehicleSettingsFile);

                    return;
                }

                else
                {
                    // No config file has been found at all...
                    Logging.Message("No vehicle configuration found. Default values will be used.");
                }
            }

            // Checking for new vehicles
            CompileVehiclesList();

            // Checking for conflicts
            DefaultOptions.CheckForConflicts();

            // Update existing vehicles
            SimulationManager.instance.AddAction(VehicleOptions.UpdateCapacityUnits);
            SimulationManager.instance.AddAction(VehicleOptions.UpdateBackEngines);

            Logging.Message("Vehicle Configuration initialized");
            LogVehicleListSteamID();
        }


/// Import the configuration file
        public static void ImportVehicleDataConfig()
        {
            if (!File.Exists(m_VehicleSettingsFile))
            {
                Logging.Message("Vehicle Configuration file not found.");
                return;
            }

            config.Deserialize(m_VehicleSettingsFile);

            if (config.options == null)
            {
                Logging.Message("Vehicle Configuration empty. Default values will be used.");
            }
            else
            {
                // Remove unneeded options
                List<VehicleOptions> optionsList = new List<VehicleOptions>();

                for (uint i = 0; i < config.options.Length; i++)
                {
                    if (config.options[i] != null && config.options[i].prefab != null) optionsList.Add(config.options[i]);
                }

                config.options = optionsList.ToArray();
            }

            // Checking for new vehicles
            CompileVehiclesList();

            // Checking for conflicts
            DefaultOptions.CheckForConflicts();

            // Update existing vehicles
            SimulationManager.instance.AddAction(VehicleOptions.UpdateCapacityUnits);
            SimulationManager.instance.AddAction(VehicleOptions.UpdateBackEngines);

            Logging.Message("Vehicle Configuration imported from Filename: " + m_VehicleSettingsFile);
            LogVehicleListSteamID();
        }

        public static void ResetVehicleDataConfig()
        {  
            // Checking for conflicts
            DefaultOptions.CheckForConflicts();

            // Update existing vehicles
            SimulationManager.instance.AddAction(VehicleOptions.UpdateCapacityUnits);
            SimulationManager.instance.AddAction(VehicleOptions.UpdateBackEngines);

            Logging.Message("Vehicle Configuration reset");
        }

        /// <summary>
        /// Export the configuration file
        /// </summary>
        public static void ExportVehicleDataConfig(bool isManual)
        {
            // display a message for the user in the panel, if Autosave is not active and button was pressed manually

            if ((AutoSaveVehicleConfig == true) && (isManual == false))
            {
                Logging.Message("Vehicle Configuration will be auto-exported to Filename: " + m_VehicleSettingsFile);
                config.Serialize(m_VehicleSettingsFile);
            }

            if (isManual == true)
            {
                Logging.Message("Vehicle Configuration will be exported to Filename: " + m_VehicleSettingsFile);
                config.Serialize(m_VehicleSettingsFile);
                ExceptionPanel panel = UIView.library.Show<ExceptionPanel>("ExceptionPanel");
                panel.SetMessage("Advanced Vehicle Options", Translations.Translate("AVO_MOD_AV01") + m_VehicleSettingsFile, false);
            }
        }

        public static void CheckAllServicesValidity()
        {
            string warning = "";

            for (int i = 0; i < (int)VehicleOptions.Category.Natural; i++)
                if (!CheckServiceValidity((VehicleOptions.Category)i)) warning += "> " + GUI.UIMainPanel.categoryList[i + 1] + "\n";

            if(warning != "")
            {
                 //GUI.UIWarningModal.message = "\n" + Translations.Translate("AVO_MOD_AV02") + "\n" + "\n" + warning + "\n";
                 //GUI.UIWarningModal.ShowWarning();

                ExceptionPanel panel = UIView.library.Show<ExceptionPanel>("ExceptionPanel");
                panel.SetMessage("Advanced Vehicle Options", "\n" + Translations.Translate("AVO_MOD_AV02") + "\n" + "\n" + warning + "\n", false);
            }
        }

        public static bool CheckServiceValidity(VehicleOptions.Category service)
        {
            if (config == null || config.options == null) return true;

            int count = 0;

            for (int i = 0; i < config.options.Length; i++)
            {
                if (config.options[i].category == service)
                {
                    if(config.options[i].enabled) return true;
                    count++;
                }
            }

            return count == 0;
        }

        public static void ClearVehicles(VehicleOptions options, bool parked)
        {
            if (parked)
            {
                if(options == null)
                {
                    SimulationManager.instance.AddAction(ActionRemoveParkedAll);
                    return;
                }
                
                m_removeParkedInfo = options.prefab;
                SimulationManager.instance.AddAction(ActionRemoveParked);
            }
            else
            {
                if (options == null)
                {
                    SimulationManager.instance.AddAction(ActionRemoveExistingAll);
                    return;
                }

                m_removeInfo = options.prefab;
                SimulationManager.instance.AddAction(ActionRemoveExisting);
            }
        }

        public static void ActionRemoveExisting()
        {
            VehicleInfo info = m_removeInfo;
            Array16<Vehicle> vehicles = VehicleManager.instance.m_vehicles;

            for (int i = 0; i < vehicles.m_buffer.Length; i++)
            {
                if (vehicles.m_buffer[i].Info != null)
                {
                    if (info == vehicles.m_buffer[i].Info)
                        VehicleManager.instance.ReleaseVehicle((ushort)i);
                }
            }
        }

        public static void ActionRemoveParked()
        {
            VehicleInfo info = m_removeParkedInfo;
            Array16<VehicleParked> parkedVehicles = VehicleManager.instance.m_parkedVehicles;

            for (int i = 0; i < parkedVehicles.m_buffer.Length; i++)
            {
                if (parkedVehicles.m_buffer[i].Info != null)
                {
                    if (info == parkedVehicles.m_buffer[i].Info)
                        VehicleManager.instance.ReleaseParkedVehicle((ushort)i);
                }
            }
        }

        public static void ActionRemoveExistingAll()
        {
            for (int i = 0; i < VehicleManager.instance.m_vehicles.m_buffer.Length; i++)
            {
                VehicleManager.instance.ReleaseVehicle((ushort)i);
            }
        }

        public static void ActionRemoveParkedAll()
        {
            for (int i = 0; i < VehicleManager.instance.m_parkedVehicles.m_buffer.Length; i++)
            {
                VehicleManager.instance.ReleaseParkedVehicle((ushort)i);
            }
        }

        private static int ParseVersion(string version)
        {
            if (version.IsNullOrWhiteSpace()) return 0;

			int a;
            int v = 0;
            string[] t = version.Split('.');

            for (int i = 0; i < t.Length; i++)
            {
                v *= 100;
                if (int.TryParse(t[i], out a))
                    v += a;
            }

            return v;
        }

        // <summary>
        // Check if there are new vehicles and add them to the options list
        // </summary>
        private static void CompileVehiclesList()
        {
            List<VehicleOptions> optionsList = new List<VehicleOptions>();
            if (config.options != null) optionsList.AddRange(config.options);

            for (uint i = 0; i < PrefabCollection<VehicleInfo>.PrefabCount(); i++)
            {
                VehicleInfo prefab = PrefabCollection<VehicleInfo>.GetPrefab(i);

                if (prefab == null || ContainsPrefab(prefab)) continue;

                // New vehicle
                VehicleOptions options = new VehicleOptions();
                options.SetPrefab(prefab);

                optionsList.Add(options);
            }

            if (config.options != null)
                Logging.KeyMessage("Found " + (optionsList.Count - config.options.Length) + " new vehicle(s)");
            else
                Logging.KeyMessage("Found " + optionsList.Count + " new vehicle(s)");

            config.options = optionsList.ToArray();
        }

        private static bool ContainsPrefab(VehicleInfo prefab)
        {
            if (config.options == null) return false;
            for (int i = 0; i < config.options.Length; i++)
            {
                if (config.options[i].prefab == prefab) return true;
            }
            return false;
        }

        private static void LogVehicleListSteamID()
        {
            StringBuilder steamIDs = new StringBuilder("Vehicle Steam IDs : ");

            for (int i = 0; i < config.options.Length; i++)
            {
                if (config.options[i] != null && config.options[i].name.Contains("."))
                {
                    steamIDs.Append(config.options[i].name.Substring(0, config.options[i].name.IndexOf(".")));
                    steamIDs.Append(",");
                }
            }
            steamIDs.Length--;

            Logging.KeyMessage(steamIDs.ToString());
        }

        private static bool IsAICustom(VehicleAI ai)
        {
            Type type = ai.GetType();
            return (type != typeof(AmbulanceAI) ||
                type != typeof(BusAI) ||
                type != typeof(CargoTruckAI) ||
                type != typeof(FireTruckAI) ||
                type != typeof(GarbageTruckAI) ||
                type != typeof(HearseAI) ||
                type != typeof(PassengerCarAI) ||
                type != typeof(PoliceCarAI)) ||
				type != typeof(TaxiAI) ||
				type != typeof(TramAI) ||
				type != typeof(MaintenanceTruckAI) ||
				type != typeof(WaterTruckAI) ||
				type != typeof(ParkMaintenanceVehicleAI) ||
				type != typeof(SnowTruckAI) ||
				type != typeof(CableCarAI) ||
				type != typeof(TrolleybusAI) ||
				type != typeof(PassengerFerryAI) ||
				type != typeof(PassengerBlimpAI) ||
				type != typeof(PostVanAI) ||
                type != typeof(PassengerHelicopterAI) ||
                type != typeof(BankVanAI) ;

        }
    }
}
