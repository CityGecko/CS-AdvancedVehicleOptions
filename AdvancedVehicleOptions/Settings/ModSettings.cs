using System;
using System.IO;
using System.Xml.Serialization;
using UnityEngine;
using ColossalFramework;


namespace AdvancedVehicleOptionsUID
{
    /// <summary>
    /// Global mod settings.
    /// </summary>
	[XmlRoot("AdvancedVehicleOptions")]
    public class ModSettings
    {
        // Settings file name.
        [XmlIgnore]
        private static readonly string SettingsFileName = "AdvancedVehicleOptions_Settings.xml";

        // User settings directory.
        [XmlIgnore]
        private static readonly string UserSettingsDir = ColossalFramework.IO.DataLocation.localApplicationData;

        // Full userdir settings file name.
        [XmlIgnore]
        private static readonly string SettingsFile = Path.Combine(UserSettingsDir, SettingsFileName);

        // SavedInputKey reference for communicating with UUI.
        [XmlIgnore]
        private static readonly SavedInputKey uuiSavedKey = new SavedInputKey("AdvancedVehicleOptions hotkey", "AdvancedVehicleOptions hotkey", key: KeyCode.O, control: false, shift: true, alt: true, false);

        // Language.
        [XmlElement("Language")]
        public string Language
        {
            get => Translations.CurrentLanguage;

            set => Translations.CurrentLanguage = value;
        }

        // Hotkey element.
        [XmlElement("ToggleKey")]
        public KeyBinding ToggleKey
        {
            get
            {
                return new KeyBinding
                {
                    keyCode = (int)ToggleSavedKey.Key,
                    control = ToggleSavedKey.Control,
                    shift = ToggleSavedKey.Shift,
                    alt = ToggleSavedKey.Alt
                };
            }
            set
            {
                uuiSavedKey.Key = (KeyCode)value.keyCode;
                //uuiSavedKey.Key = KeyCode.O;   The Standard Hot Key
                uuiSavedKey.Control = value.control;
                uuiSavedKey.Shift = value.shift;
                uuiSavedKey.Alt = value.alt;
            }
        }
        /// <summary>
        /// Toggle hotkey as ColossalFramework SavedInputKey.
        /// </summary>
        [XmlIgnore]
        internal static SavedInputKey ToggleSavedKey => uuiSavedKey;


        /// <summary>
        /// The current hotkey settings as ColossalFramework InputKey.
        /// </summary>
        /// </summary>
        [XmlIgnore]
        internal static InputKey CurrentHotkey
        {
            get => uuiSavedKey.value;

            set => uuiSavedKey.value = value;
        }

        // General: Debug Messages
        [XmlElement("DebugMessages")]
        public bool detailLogging
        {
            get => Logging.detailLogging;

            set => Logging.detailLogging = value;
        }

        // General: Enable Autosave for Vehicle Configuration
        [XmlElement("AutoSaveVehicleConfig")]
        public bool AutoSaveVehicleConfigFlag
        {
            get => AdvancedVehicleOptions.AutoSaveVehicleConfig;

            set => AdvancedVehicleOptions.AutoSaveVehicleConfig = value;
        }
 
       // General: Trailer Sync Settings
       [XmlElement("RememberTrailerSync")]
        public bool RememberSyncTrailerSettingFlag
        {
            get => AdvancedVehicleOptions.RememberSyncTrailerSetting;

            set => AdvancedVehicleOptions.RememberSyncTrailerSetting = value;
        }

        [XmlElement("LastTrailerSync")]
        public bool LastSyncTrailerSettingFlag
        {
            get => AdvancedVehicleOptions.LastSyncTrailerSetting;

            set => AdvancedVehicleOptions.LastSyncTrailerSetting = value;
        }

        // General: Hide the GUI Button
        [XmlElement("GUIButton")]
        public bool HideTheGUIButton
        {
            get => AdvancedVehicleOptions.HideGUIbutton;

            set => AdvancedVehicleOptions.HideGUIbutton = value;
        }

        // General: Validate Services
        [XmlElement("ValidateServices")]
        public bool ValidateServices
        {
            get => AdvancedVehicleOptions.OnLoadValidateServices;

            set => AdvancedVehicleOptions.OnLoadValidateServices = value;
        }

        // Gameplay: Use different Speed Units
        [XmlElement("SpeedUnitOption")]
        public bool SelectSpeedUnitOption
        {
            get => AdvancedVehicleOptions.SpeedUnitOption;

            set => AdvancedVehicleOptions.SpeedUnitOption = value;
        }

        // Gameplay: Show more vehicle properties
        [XmlElement("ShowMoreVehicleOptions")]
        public bool SelectShowMoreVehicleOptions
        {
            get => AdvancedVehicleOptions.ShowMoreVehicleOptions;

            set => AdvancedVehicleOptions.ShowMoreVehicleOptions = value;
        }

        // Compatibility: Do not show colored Mod Compatibility Warnings 
        [XmlElement("OverrideCompatibilityWarnings")]
        public bool SelectOverrideCompatibilityWarnings
        {
            get => AdvancedVehicleOptions.OverrideCompatibilityWarnings;

            set => AdvancedVehicleOptions.OverrideCompatibilityWarnings = value;
        }

        // Compatibility: Use No Big Trucks setting 
        [XmlElement("ControlTruckDelivery")]
        public bool NoBigTrucksDelivery
        {
            get => AdvancedVehicleOptions.ControlTruckDelivery;

            set => AdvancedVehicleOptions.ControlTruckDelivery = value;
        }

        /// <summary>
        /// Load settings from XML file.
        /// </summary>
        internal static void Load()
        {
            try
            {
                // Attempt to read new settings file (in user settings directory).
                string fileName = SettingsFile;
                if (!File.Exists(fileName))
                {
                    // No settings file in user directory; use application directory instead. If still no settings file, set default values.
                    fileName = SettingsFileName;

                    if (!File.Exists(fileName))
                    {
                        Logging.Message("no settings file found");

                        AdvancedVehicleOptions.AutoSaveVehicleConfig = true;
                        AdvancedVehicleOptions.RememberSyncTrailerSetting = false;
                        AdvancedVehicleOptions.LastSyncTrailerSetting = false;
                        AdvancedVehicleOptions.HideGUIbutton = false;
                        AdvancedVehicleOptions.OnLoadValidateServices = true;
                        AdvancedVehicleOptions.SpeedUnitOption = false;
                        AdvancedVehicleOptions.ShowMoreVehicleOptions = false;
                        AdvancedVehicleOptions.OverrideVCX = false;
                        AdvancedVehicleOptions.OverrideCompatibilityWarnings = true;
                        AdvancedVehicleOptions.ControlTruckDelivery = true;

                        return;
                    }
                }

                // Read settings file.
                using (StreamReader reader = new StreamReader(fileName))
                {
                    XmlSerializer xmlSerializer = new XmlSerializer(typeof(ModSettings));
                    if (!(xmlSerializer.Deserialize(reader) is ModSettings settingsFile))
                    {
                        Logging.Error("couldn't deserialize settings file");
                    }

                    Logging.Message("User Setting Configuration successful loaded.");
                }
            }
            catch (Exception e)
            {
                Logging.LogException(e, "exception reading XML settings file");
            }
                }

        /// <summary>
        /// Save settings to XML file.
        /// </summary>
        internal static void Save()
        {
            try
            {
                // Pretty straightforward.
                using (StreamWriter writer = new StreamWriter(SettingsFile))
                {
                    XmlSerializer xmlSerializer = new XmlSerializer(typeof(ModSettings));
                    xmlSerializer.Serialize(writer, new ModSettings());
                }

                // Cleaning up after ourselves - delete any old config file in the application direcotry.
                if (File.Exists(SettingsFileName))
                {
                    File.Delete(SettingsFileName);
                }

                Logging.Message("User Setting Configuration successful saved.");
            }
            catch (Exception e)
            {
                Logging.LogException(e, "exception saving XML settings file");
            }
        }
    }

        /// <summary>
    /// Basic keybinding class - code and modifiers.
    /// </summary>
    public class KeyBinding
    {
        [XmlAttribute("KeyCode")]
        public int keyCode;

        [XmlAttribute("Control")]
        public bool control;

        [XmlAttribute("Shift")]
        public bool shift;

        [XmlAttribute("Alt")]
        public bool alt;
    }
}