using UnityEngine;
using System;
using ColossalFramework.UI;
using ColossalFramework.Threading;

using UIUtils = SamsamTS.UIUtils;

using AdvancedVehicleOptionsUID.Compatibility;

namespace AdvancedVehicleOptionsUID.GUI
{
    public class UIOptionPanel : UIPanel
    {
        public static float maxSpeedToKmhConversionFactor = 6.25f;
        public static float mphFactor = 1.609344f;

        public static UITextField m_maxSpeed;
        private UITextField m_acceleration;
        private UITextField m_braking;
        private UITextField m_turning;
        private UITextField m_springs;
        private UITextField m_dampers;
        private UITextField m_leanMultiplier;
        private UITextField m_nodMultiplier;
        private UICheckBox m_useColors;
        private UIColorField m_color0;
        private UIColorField m_color1;
        private UIColorField m_color2;
        private UIColorField m_color3;
        private UITextField m_color0_hex;
        private UITextField m_color1_hex;
        private UITextField m_color2_hex;
        private UITextField m_color3_hex;
        private UICheckBox m_enabled;
        private UILabel m_parkpositionLabel;
        private UIDropDown m_parkposition_size;
        private UICheckBox m_addBackEngine;
        private UICheckBox m_syncTrailers;
        private UITextField m_capacity;
        private UITextField m_specialcapacity;
        private UIButton m_restore;
        private UIButton m_lineoverview;
        private UIButton m_userguidespawn;
        private UILabel m_removeLabel;
        private UIButton m_clearVehicles;
        private UIButton m_clearParked;
        private PublicTransportDetailPanel _publicTransportDetailPanel;
        private UILabel capacityLabel;
        private UILabel specialcapacityLabel;
        private UIButton m_userguidespecialcapacity;
        private UILabel bustrailerLabel;
        private UICheckBox m_isLargeVehicle;
        private UILabel m_useColorsLabel;
        internal UILabel kmhLabel;

        public VehicleOptions m_options = null;

        internal bool m_initialized = false;
        private int LineOverviewType = -1;
        private Color32 OldColorTextField;
        private Color32 OldColorText;
        private string OldCapacityTooltip;
        private string OldMaxSpeedTooltip;
        private string OldSpecialCapacityTooltip;

        public event PropertyChangedEventHandler<bool> eventEnableCheckChanged;

        public override void Start()
        {
            base.Start();
            canFocus = true;
            isInteractive = true;
            width = 390;
            height = 370;

            SetupControls();
            OldCapacityTooltip = m_capacity.tooltip;
            OldMaxSpeedTooltip = m_maxSpeed.tooltip;
            OldSpecialCapacityTooltip = m_specialcapacity.tooltip;
            OldColorTextField = m_capacity.color;
            OldColorText = m_capacity.textColor;

            m_options = new VehicleOptions();
        }

        public void Show(VehicleOptions options)
        {
            m_initialized = false;

            m_options = options;

            if (m_color0 == null) return;

            m_color0.relativePosition = new Vector3(35, 135);
            m_color1.relativePosition = new Vector3(35, 160);
            m_color2.relativePosition = new Vector3(182, 135);
            m_color3.relativePosition = new Vector3(182, 160);

            if (!AdvancedVehicleOptions.SpeedUnitOption)
            {
                m_maxSpeed.text = Mathf.RoundToInt(options.maxSpeed * maxSpeedToKmhConversionFactor).ToString();
                kmhLabel.text = Translations.Translate("AVO_MOD_OP01");
            }
            else
            {
                m_maxSpeed.text = Mathf.RoundToInt((options.maxSpeed / mphFactor) * maxSpeedToKmhConversionFactor).ToString();
                kmhLabel.text = Translations.Translate("AVO_MOD_OP02");
            }

            m_acceleration.text = options.acceleration.ToString();     
            m_braking.text = options.braking.ToString();
            m_turning.text = options.turning.ToString();
            m_springs.text = options.springs.ToString();
            m_dampers.text = options.dampers.ToString();

            m_leanMultiplier.text = options.leanMultiplier.ToString();
            m_nodMultiplier.text = options.nodMultiplier.ToString();
            m_useColors.isChecked = options.useColorVariations;
            m_color0.selectedColor = options.color0;
            m_color1.selectedColor = options.color1;
            m_color2.selectedColor = options.color2;
            m_color3.selectedColor = options.color3;
            m_color0_hex.text = options.color0.ToString();
            m_color1_hex.text = options.color1.ToString();
            m_color2_hex.text = options.color2.ToString();
            m_color3_hex.text = options.color3.ToString();
            m_isLargeVehicle.isChecked = options.isLargeVehicle;

            m_enabled.isChecked = options.enabled;
            m_enabled.Show();
            m_enabled.isVisible = !options.isTrailer;

            m_addBackEngine.isChecked = options.addBackEngine;
            m_addBackEngine.isVisible = options.isTrain;
            m_maxSpeed.isInteractive = true;

            m_lineoverview.Hide();
            m_isLargeVehicle.Hide();
            m_useColorsLabel.Hide();

            m_capacity.parent.isVisible = true;
            m_capacity.isInteractive = true;
            m_capacity.text = options.capacity.ToString();
            m_capacity.isVisible = options.hasCapacity;
            capacityLabel.isVisible = options.hasCapacity;
            bustrailerLabel.isVisible = false;

            m_specialcapacity.text = options.specialcapacity.ToString();
            m_specialcapacity.isVisible = options.hasSpecialCapacity;
            specialcapacityLabel.isVisible = options.hasSpecialCapacity;
            m_userguidespecialcapacity.isVisible = options.hasSpecialCapacity;

            m_capacity.color = OldColorTextField;
            m_capacity.textColor = OldColorText;
            m_capacity.tooltip = OldCapacityTooltip;
            m_specialcapacity.tooltip = OldSpecialCapacityTooltip;
            m_maxSpeed.color = OldColorTextField;
            m_maxSpeed.textColor = OldColorText;
            m_maxSpeed.tooltip = OldMaxSpeedTooltip;

            //Only display Cargo Capacity or Passenger Capacity - not any other values
            if (options.isNonPaxCargo == true && options.hasCapacity == true && AdvancedVehicleOptions.ShowMoreVehicleOptions == true)
            {
                m_capacity.parent.isVisible = true;
            }
            else
                if (options.isNonPaxCargo == true && options.hasCapacity == true && AdvancedVehicleOptions.ShowMoreVehicleOptions == false)
            {
                m_capacity.parent.isVisible = false;
            }

            //Configure SyncTrailer Settings
            if (AdvancedVehicleOptions.RememberSyncTrailerSetting)
            {
                m_syncTrailers.isChecked = AdvancedVehicleOptions.LastSyncTrailerSetting;
            }
            else
            {
                AdvancedVehicleOptions.LastSyncTrailerSetting = false;
                m_syncTrailers.isChecked = false;
            }

            m_syncTrailers.isVisible = ((options.isTrain) && (m_options.prefab.m_class.m_level == ItemClass.Level.Level1 || m_options.prefab.m_class.m_level == ItemClass.Level.Level2 || m_options.prefab.m_class.m_level == ItemClass.Level.Level4));

            //Compatibility Patch for Vehicle Color Expander - hide all controls, if mod is active. Not relating to PublicTransport only, but all vehicles
            if (VCXCompatibilityPatch.IsVCXActive())
            {
                m_useColors.enabled = false;
                m_color0.Hide();
                m_color0_hex.Hide();
                m_color1.Hide();
                m_color1_hex.Hide();
                m_color2.Hide();
                m_color2_hex.Hide();
                m_color3.Hide();
                m_color3_hex.Hide();
                m_useColorsLabel.Show();
            }

            // Compatibility Patch for IPT, TLM and Cities Skylines Vehicle Spawning, Vehicle values. Instead of Spawn Allowed buttons for the Vehicles the Line Overview Window will be shown.
            if (options.isPublicTransportGame == true)
            {
                m_enabled.Hide();
                m_userguidespawn.Show();
                if (!options.hasTrailer)
                {
                    bustrailerLabel.Show();
                    m_lineoverview.Hide();
                }
                else
                {
                    bustrailerLabel.Hide();
                    m_lineoverview.Show();
                }

                LineOverviewType = options.ReturnLineOverviewType;
            }
            else
            {
                m_enabled.Show();
                bustrailerLabel.Hide();
                m_lineoverview.Hide();
                m_userguidespawn.Hide();
            }

            if (AdvancedVehicleOptions.OverrideCompatibilityWarnings == true && options.isPublicTransport == true && !options.isNotPublicTransportMod == true)
            {
                if (IPTCompatibilityPatch.IsIPTActive() == true)
                {
                    m_maxSpeed.color = new Color32(240, 130, 130, 255);
                    m_capacity.color = new Color32(240, 130, 130, 255);
                    m_specialcapacity.color = new Color32(240, 130, 130, 255);
                    m_maxSpeed.textColor = new Color32(255, 230, 130, 255);
                    m_capacity.textColor = new Color32(255, 230, 130, 255);
                    m_specialcapacity.textColor = new Color32(255, 230, 130, 255);
                    m_maxSpeed.tooltip = m_maxSpeed.tooltip + Translations.Translate("AVO_MOD_OP03");
                    m_capacity.tooltip = m_capacity.tooltip + Translations.Translate("AVO_MOD_OP03");
                    m_specialcapacity.tooltip = m_specialcapacity.tooltip + Translations.Translate("AVO_MOD_OP03");
                }
                if (TLMCompatibilityPatch.IsTLMActive() == true)
                {
                    m_capacity.color = new Color32(240, 130, 130, 255);
                    m_capacity.textColor = new Color32(255, 230, 130, 255);
                    m_capacity.tooltip = m_capacity.tooltip + Translations.Translate("AVO_MOD_OP04");
                }
            }

            if (NoBigTruckCompatibilityPatch.IsNBTActive() == true && AdvancedVehicleOptions.ControlTruckDelivery == true && options.isDelivery == true && options.hasTrailer == true)
            {
                m_isLargeVehicle.Show();

                Logging.Message("Vehicle " + options.localizedName + " is flagged as Large Vehicle.");
            }

            // Compatibility Patch section ends

            // Flight Stand Info introduced in Airports DLC

            if (AdvancedVehicleOptions.hasAirportDLC)
            {
                if ((options.prefab.m_vehicleType == VehicleInfo.VehicleType.Plane && options.prefab.m_class.m_level != ItemClass.Level.Level4 && options.prefab.m_class.m_level != ItemClass.Level.Level5))
                {
                    if (m_options.prefab.m_class.name == "Airplane Vehicle Small")
                    {
                        m_parkposition_size.selectedIndex = 0;
                    }

                    if (m_options.prefab.m_class.name == "Airplane Vehicle")
                    {
                        m_parkposition_size.selectedIndex = 1;
                    }

                    if (m_options.prefab.m_class.name == "Airplane Vehicle Large")
                    {
                        m_parkposition_size.selectedIndex = 2;
                    }

                    if (m_options.prefab.m_class.name == "Airplane Cargo Vehicle")
                    {
                        m_parkposition_size.selectedIndex = 3;
                    }

                    m_parkpositionLabel.isVisible = true;
                    m_parkposition_size.isVisible = true;
                }
                else
                {
                    m_parkpositionLabel.isVisible = false;
                    m_parkposition_size.isVisible = false;
                }
            }      

            string name = options.localizedName;
            if (name.Length > 40) name = name.Substring(0, 38) + "...";
            m_removeLabel.text = Translations.Translate("AVO_MOD_OP05") + name;

            (parent as UIMainPanel).ChangePreviewColor(m_color0.selectedColor);

            capacityLabel.text = options.CapacityString;
            specialcapacityLabel.text = options.SpecialCapacityString;

            m_initialized = true;
        }

        private void SetupControls()
        {
            UIPanel panel = AddUIComponent<UIPanel>();
            panel.gameObject.AddComponent<UICustomControl>();

            panel.backgroundSprite = "UnlockingPanel";
            panel.width = width - 10;
            panel.height = height - 75;
            panel.relativePosition = new Vector3(5, 0);

            // Max Speed
            UILabel maxSpeedLabel = panel.AddUIComponent<UILabel>();
            maxSpeedLabel.text = Translations.Translate("AVO_MOD_OP06");
            maxSpeedLabel.textScale = 0.8f;
            maxSpeedLabel.relativePosition = new Vector3(15, 14);

            m_maxSpeed = UIUtils.CreateTextField(panel);
            m_maxSpeed.numericalOnly = true;
            m_maxSpeed.width = 75;
            m_maxSpeed.tooltip = Translations.Translate("AVO_MOD_OP07");
            m_maxSpeed.relativePosition = new Vector3(15, 33);

            kmhLabel = panel.AddUIComponent<UILabel>();
            kmhLabel.text = "km/h";
            kmhLabel.textScale = 0.8f;
            kmhLabel.relativePosition = new Vector3(95, 38);

            // Acceleration
            UILabel accelerationLabel = panel.AddUIComponent<UILabel>();
            accelerationLabel.text = Translations.Translate("AVO_MOD_OP08");
            accelerationLabel.textScale = 0.8f;
            accelerationLabel.relativePosition = new Vector3(160, 13);

            m_acceleration = UIUtils.CreateTextField(panel);
            m_acceleration.numericalOnly = true;
            m_acceleration.allowFloats = true;
            m_acceleration.width = 60;
            m_acceleration.tooltip = Translations.Translate("AVO_MOD_OP09");
            m_acceleration.relativePosition = new Vector3(160, 33);

            // Braking
            m_braking = UIUtils.CreateTextField(panel);
            m_braking.numericalOnly = true;
            m_braking.allowFloats = true;
            m_braking.width = 60;
            m_braking.tooltip = Translations.Translate("AVO_MOD_OP10");
            m_braking.relativePosition = new Vector3(230, 33);

            // Turning
            m_turning = UIUtils.CreateTextField(panel);
            m_turning.numericalOnly = true;
            m_turning.allowFloats = true;
            m_turning.width = 60;
            m_turning.tooltip = Translations.Translate("AVO_MOD_OP11");
            m_turning.relativePosition = new Vector3(300, 33);

            // Springs
            UILabel springsLabel = panel.AddUIComponent<UILabel>();
            springsLabel.text = Translations.Translate("AVO_MOD_OP12");
            springsLabel.textScale = 0.8f;
            springsLabel.relativePosition = new Vector3(15, 66);

            m_springs = UIUtils.CreateTextField(panel);
            m_springs.numericalOnly = true;
            m_springs.allowFloats = true;
            m_springs.width = 60;
            m_springs.tooltip = Translations.Translate("AVO_MOD_OP13");
            m_springs.relativePosition = new Vector3(15, 85);

            // Dampers
            m_dampers = UIUtils.CreateTextField(panel);
            m_dampers.numericalOnly = true;
            m_dampers.allowFloats = true;
            m_dampers.width = 60;
            m_dampers.tooltip = Translations.Translate("AVO_MOD_OP14");
            m_dampers.relativePosition = new Vector3(85, 85);

            // LeanMultiplier
            UILabel leanMultiplierLabel = panel.AddUIComponent<UILabel>();
            leanMultiplierLabel.text = Translations.Translate("AVO_MOD_OP15");
            leanMultiplierLabel.textScale = 0.8f;
            leanMultiplierLabel.relativePosition = new Vector3(160, 66);

            m_leanMultiplier = UIUtils.CreateTextField(panel);
            m_leanMultiplier.numericalOnly = true;
            m_leanMultiplier.allowFloats = true;
            m_leanMultiplier.width = 60;
            m_leanMultiplier.tooltip = Translations.Translate("AVO_MOD_OP16");
            m_leanMultiplier.relativePosition = new Vector3(160, 85);

            // NodMultiplier
            m_nodMultiplier = UIUtils.CreateTextField(panel);
            m_nodMultiplier.numericalOnly = true;
            m_nodMultiplier.allowFloats = true;
            m_nodMultiplier.width = 60;
            m_nodMultiplier.tooltip = Translations.Translate("AVO_MOD_OP17");
            m_nodMultiplier.relativePosition = new Vector3(230, 85);

            // Colors
            m_useColors = UIUtils.CreateCheckBox(panel);
            m_useColors.text = Translations.Translate("AVO_MOD_OP18");
            m_useColors.isChecked = true;
            m_useColors.width = width - 40;
            m_useColors.tooltip = Translations.Translate("AVO_MOD_OP19");
            m_useColors.relativePosition = new Vector3(15, 116);

            m_color0 = UIUtils.CreateColorField(panel);
            m_color0.name = "AVO-color0";
            m_color0.popupTopmostRoot = false;
            m_color0.relativePosition = new Vector3(35, 135);
            m_color0_hex = UIUtils.CreateTextField(panel);
            m_color0_hex.maxLength = 6;
            m_color0_hex.relativePosition = new Vector3(80, 137);

            m_color1 = UIUtils.CreateColorField(panel);
            m_color1.name = "AVO-color1";
            m_color1.popupTopmostRoot = false;
            m_color1.relativePosition = new Vector3(35, 160);
            m_color1_hex = UIUtils.CreateTextField(panel);
            m_color1_hex.maxLength = 6;
            m_color1_hex.relativePosition = new Vector3(80, 162);

            m_color2 = UIUtils.CreateColorField(panel);
            m_color2.name = "AVO-color2";
            m_color2.popupTopmostRoot = false;
            m_color2.relativePosition = new Vector3(182, 135);
            m_color2_hex = UIUtils.CreateTextField(panel);
            m_color2_hex.maxLength = 6;
            m_color2_hex.relativePosition = new Vector3(225, 137);

            m_color3 = UIUtils.CreateColorField(panel);
            m_color3.name = "AVO-color3";
            m_color3.popupTopmostRoot = false;
            m_color3.relativePosition = new Vector3(182, 160);
            m_color3_hex = UIUtils.CreateTextField(panel);
            m_color3_hex.maxLength = 6;
            m_color3_hex.relativePosition = new Vector3(225, 162);

            m_useColorsLabel = panel.AddUIComponent<UILabel>();
            m_useColorsLabel.Hide();
            m_useColorsLabel.textScale = 0.8f;
            m_useColorsLabel.text = Translations.Translate("AVO_MOD_OP20");
            m_useColorsLabel.relativePosition = new Vector3(15, 116);

            // Enable & SyncTrailer & BackEngine
            m_enabled = UIUtils.CreateCheckBox(panel);
            m_enabled.text = Translations.Translate("AVO_MOD_OP21");
            m_enabled.width = 250;
            m_enabled.isChecked = true;
            m_enabled.tooltip = Translations.Translate("AVO_MOD_OP22");
            m_enabled.relativePosition = new Vector3(15, 195);

            m_addBackEngine = UIUtils.CreateCheckBox(panel);
            m_addBackEngine.text = Translations.Translate("AVO_MOD_OP23");
            m_addBackEngine.isChecked = false;
            m_addBackEngine.width = width - 40;
            m_addBackEngine.tooltip = Translations.Translate("AVO_MOD_OP24");
            m_addBackEngine.relativePosition = new Vector3(15, 215);

            // LargeVehicle Setting for NoBigTruck Delivery Mod
            m_isLargeVehicle = UIUtils.CreateCheckBox(panel);
            m_isLargeVehicle.text = Translations.Translate("AVO_MOD_OP25");
            m_isLargeVehicle.width = width - 40;
            m_isLargeVehicle.tooltip = Translations.Translate("AVO_MOD_OP26");
            m_isLargeVehicle.relativePosition = new Vector3(15, 215);

            // Capacity
            UIPanel capacityPanel = panel.AddUIComponent<UIPanel>();
            capacityPanel.size = Vector2.zero;
            capacityPanel.relativePosition = new Vector3(15, 240);

            capacityLabel = capacityPanel.AddUIComponent<UILabel>();
            capacityLabel.text = Translations.Translate("AVO_MOD_CAPA");
            capacityLabel.textScale = 0.8f;
            capacityLabel.relativePosition = new Vector3(0, 2);

            m_capacity = UIUtils.CreateTextField(capacityPanel);
            m_capacity.numericalOnly = true;
            m_capacity.maxLength = 8;
            m_capacity.width = 100;
            m_capacity.tooltip = Translations.Translate("AVO_MOD_OP27");
            m_capacity.relativePosition = new Vector3(0, 21);

            // Sync Trailer setting
            m_syncTrailers = UIUtils.CreateCheckBox(capacityPanel);
            m_syncTrailers.text = Translations.Translate("AVO_MOD_OP48");
            m_syncTrailers.height = 30;
            m_syncTrailers.relativePosition = new Vector3(160, 15);
            m_syncTrailers.tooltip = Translations.Translate("AVO_MOD_OP49");

            // Plane Sizes and Parking Position 
            m_parkpositionLabel = capacityPanel.AddUIComponent<UILabel>();
            m_parkpositionLabel.textScale = 0.8f;
            m_parkpositionLabel.text = Translations.Translate("AVO_MOD_OP43");
            m_parkpositionLabel.relativePosition = new Vector3(170, 2);
            m_parkpositionLabel.isVisible = AdvancedVehicleOptions.hasAirportDLC;

            m_parkposition_size = UIUtils.CreateDropDown(capacityPanel);
            m_parkposition_size.width = 100;
            m_parkposition_size.tooltip = Translations.Translate("AVO_MOD_OP50");
            m_parkposition_size.AddItem(Translations.Translate("AVO_MOD_OP44"));
            m_parkposition_size.AddItem(Translations.Translate("AVO_MOD_OP45"));
            m_parkposition_size.AddItem(Translations.Translate("AVO_MOD_OP46"));
            m_parkposition_size.selectedIndex = 0;
            m_parkposition_size.relativePosition = new Vector3(170, 19);
            m_parkposition_size.isVisible = AdvancedVehicleOptions.hasAirportDLC;

            // Special Capacity			
            specialcapacityLabel = capacityPanel.AddUIComponent<UILabel>();
            specialcapacityLabel.Hide();
            specialcapacityLabel.text = Translations.Translate("AVO_MOD_OP28");
            specialcapacityLabel.textScale = 0.8f;
            specialcapacityLabel.relativePosition = new Vector3(160, 2);

            m_specialcapacity = UIUtils.CreateTextField(capacityPanel);
            m_specialcapacity.Hide();
            m_specialcapacity.numericalOnly = true;
            m_specialcapacity.maxLength = 8;
            m_specialcapacity.width = 100;
            m_specialcapacity.tooltip = Translations.Translate("AVO_MOD_OP29");
            m_specialcapacity.relativePosition = new Vector3(160, 21);

            // Userguide Special Capacity Button
            m_userguidespecialcapacity = UIUtils.CreateButton(capacityPanel);
            m_userguidespecialcapacity.Hide();
            m_userguidespecialcapacity.normalBgSprite = "EconomyMoreInfo";
            m_userguidespecialcapacity.hoveredBgSprite = "EconomyMoreInfoHovered";
            m_userguidespecialcapacity.size = new Vector2(14f, 14f);
            m_userguidespecialcapacity.tooltip = Translations.Translate("AVO_MOD_OP30");
            m_userguidespecialcapacity.relativePosition = new Vector3(265, 24);

            // Transport Line Overview Button	
            m_lineoverview = UIUtils.CreateButton(panel);
            m_lineoverview.Hide();
            m_lineoverview.textScale = 0.8f;
            m_lineoverview.height = 18;
            m_lineoverview.textVerticalAlignment = UIVerticalAlignment.Bottom;
            m_lineoverview.text = Translations.Translate("AVO_MOD_OP31");
            m_lineoverview.width = 335;
            m_lineoverview.tooltip = Translations.Translate("AVO_MOD_OP32");
            m_lineoverview.relativePosition = new Vector3(15, 194);

            // Userguide Spawn Button
            m_userguidespawn = UIUtils.CreateButton(panel);
            m_userguidespawn.Hide();
            m_userguidespawn.normalBgSprite = "EconomyMoreInfo";
            m_userguidespawn.hoveredBgSprite = "EconomyMoreInfoHovered";
            m_userguidespawn.size = new Vector2(14f, 14f);
            m_userguidespawn.tooltip = Translations.Translate("AVO_MOD_OP33");
            m_userguidespawn.relativePosition = new Vector3(355, 195);

            // Buslabel		
            bustrailerLabel = panel.AddUIComponent<UILabel>();
            bustrailerLabel.textScale = 0.8f;
            bustrailerLabel.text = Translations.Translate("AVO_MOD_OP34");
            bustrailerLabel.relativePosition = new Vector3(15, 194);

            // Restore default
            m_restore = UIUtils.CreateButton(panel);
            m_restore.text = Translations.Translate("AVO_MOD_OP35");
            m_restore.width = 120;
            m_restore.tooltip = Translations.Translate("AVO_MOD_OP36");
            m_restore.relativePosition = new Vector3(250, height - 45);

            // Remove Vehicles
            m_removeLabel = this.AddUIComponent<UILabel>();
            m_removeLabel.text = Translations.Translate("AVO_MOD_OP37");
            m_removeLabel.textScale = 0.8f;
            m_removeLabel.relativePosition = new Vector3(10, height - 65);

            m_clearVehicles = UIUtils.CreateButton(this);
            m_clearVehicles.text = Translations.Translate("AVO_MOD_OP38");
            m_clearVehicles.width = 120;
            m_clearVehicles.tooltip = Translations.Translate("AVO_MOD_OP39");
            m_clearVehicles.relativePosition = new Vector3(5, height - 45);

            m_clearParked = UIUtils.CreateButton(this);
            m_clearParked.text = Translations.Translate("AVO_MOD_OP40");
            m_clearParked.width = 120;
            m_clearParked.tooltip = Translations.Translate("AVO_MOD_OP41");
            m_clearParked.relativePosition = new Vector3(130, height - 45);

            panel.BringToFront();

            // Event handlers
            m_maxSpeed.eventTextSubmitted += OnMaxSpeedSubmitted;
            m_acceleration.eventTextSubmitted += OnAccelerationSubmitted;
            m_braking.eventTextSubmitted += OnBrakingSubmitted;
            m_turning.eventTextSubmitted += OnTurningSubmitted;
            m_springs.eventTextSubmitted += OnSpringsSubmitted;
            m_dampers.eventTextSubmitted += OnDampersSubmitted;
            m_leanMultiplier.eventTextSubmitted += OnleanMultiplierSubmitted;
            m_nodMultiplier.eventTextSubmitted += OnnodMultiplierSubmitted;

            m_useColors.eventCheckChanged += OnCheckChanged;

            MouseEventHandler mousehandler = (c, p) => { if (m_initialized) (parent as UIMainPanel).ChangePreviewColor((c as UIColorField).selectedColor); };

            m_color0.eventMouseEnter += mousehandler;
            m_color1.eventMouseEnter += mousehandler;
            m_color2.eventMouseEnter += mousehandler;
            m_color3.eventMouseEnter += mousehandler;

            m_color0_hex.eventMouseEnter += (c, p) => { if (m_initialized) (parent as UIMainPanel).ChangePreviewColor(m_color0.selectedColor); };
            m_color1_hex.eventMouseEnter += (c, p) => { if (m_initialized) (parent as UIMainPanel).ChangePreviewColor(m_color1.selectedColor); };
            m_color2_hex.eventMouseEnter += (c, p) => { if (m_initialized) (parent as UIMainPanel).ChangePreviewColor(m_color2.selectedColor); };
            m_color3_hex.eventMouseEnter += (c, p) => { if (m_initialized) (parent as UIMainPanel).ChangePreviewColor(m_color3.selectedColor); };

            m_color0.eventSelectedColorChanged += OnColorChanged;
            m_color1.eventSelectedColorChanged += OnColorChanged;
            m_color2.eventSelectedColorChanged += OnColorChanged;
            m_color3.eventSelectedColorChanged += OnColorChanged;

            m_color0_hex.eventTextSubmitted += OnColorHexSubmitted;
            m_color1_hex.eventTextSubmitted += OnColorHexSubmitted;
            m_color2_hex.eventTextSubmitted += OnColorHexSubmitted;
            m_color3_hex.eventTextSubmitted += OnColorHexSubmitted;

            m_enabled.eventCheckChanged += OnCheckChanged;
            m_syncTrailers.eventCheckChanged += OnCheckChanged;
            m_addBackEngine.eventCheckChanged += OnCheckChanged;
            m_isLargeVehicle.eventCheckChanged += OnCheckChanged;

            m_capacity.eventTextSubmitted += OnCapacitySubmitted;
            m_specialcapacity.eventTextSubmitted += OnSpecialCapacitySubmitted;

            m_restore.eventClick += (c, p) =>
            {
                m_initialized = false;
                bool isEnabled = m_options.enabled;
                DefaultOptions.Restore(m_options.prefab);
                VehicleOptions.UpdateTransfertVehicles();

                VehicleOptions.prefabUpdateEngine = m_options.prefab;
                VehicleOptions.prefabUpdateUnits = m_options.prefab;
                SimulationManager.instance.AddAction(VehicleOptions.UpdateBackEngines);
                SimulationManager.instance.AddAction(VehicleOptions.UpdateCapacityUnits);

                Show(m_options);

                if (m_options.enabled != isEnabled)
                    eventEnableCheckChanged(this, m_options.enabled);
            };

            m_parkposition_size.eventSelectedIndexChanged += (c, t) => 
            {
                if (AdvancedVehicleOptions.hasAirportDLC)
                {

                    if (!m_initialized || m_options == null) return;
                    m_initialized = false;

                    m_parkposition_size.enabled = false;
                    m_parkpositionLabel.text = Translations.Translate("AVO_MOD_OP43") + ' ' + m_parkposition_size.selectedValue;
                    m_parkposition_size.enabled = true;

                    Logging.Message("Current ItemClass: " + m_options.prefab.m_class.name);

                    if (m_parkposition_size.selectedIndex == 0)
                    {
                        m_options.prefab.m_class = ItemClassCollection.FindClass("Airplane Vehicle Small");
                        m_parkpositionLabel.text = Translations.Translate("AVO_MOD_OP43") + Translations.Translate("AVO_MOD_OP44");
                        Logging.Message("Found the required template ItemClass: " + ItemClassCollection.FindClass("Airplane Vehicle Small"));
                        VehicleOptions.UpdateTransfertVehicles();
                    }

                    if (m_parkposition_size.selectedIndex == 1)
                    {
                        m_options.prefab.m_class = ItemClassCollection.FindClass("Airplane Vehicle");
                        m_parkpositionLabel.text = Translations.Translate("AVO_MOD_OP43") + Translations.Translate("AVO_MOD_OP45");
                        Logging.Message("Found the required template ItemClass: " + ItemClassCollection.FindClass("Airplane Vehicle"));
                        VehicleOptions.UpdateTransfertVehicles();
                    }

                    if (m_parkposition_size.selectedIndex == 2)
                    {
                        m_options.prefab.m_class = ItemClassCollection.FindClass("Airplane Vehicle Large");
                        m_parkpositionLabel.text = Translations.Translate("AVO_MOD_OP43") + Translations.Translate("AVO_MOD_OP46");
                        Logging.Message("Found the required template ItemClass: " + ItemClassCollection.FindClass("Airplane Vehicle Large"));
                        VehicleOptions.UpdateTransfertVehicles();
                    }

                    Logging.Message("Active Airplane ItemClass: " + m_options.prefab.m_class.name + " for " + m_options.prefab.name + " on " + m_options.prefab.m_class.m_level);

                }

                AdvancedVehicleOptions.ExportVehicleDataConfig(m_initialized);
                m_initialized = true;
            };

            m_clearVehicles.eventClick += OnClearVehicleClicked;
            m_clearParked.eventClick += OnClearVehicleClicked;
            m_lineoverview.eventClick += OnlineoverviewClicked;
            m_userguidespawn.eventClick += OnUserGuideSpawnClicked;
            m_userguidespecialcapacity.eventClick += OnUserGuideSpecialCapacityClicked;
        }

        protected void OnCheckChanged(UIComponent component, bool state)
        {
            if (!m_initialized || m_options == null) return;
            m_initialized = false;

            if (component == m_enabled)
            {
                if (m_options.isTrailer)
                {
                    VehicleOptions engine = m_options.engine;

                    if (engine.enabled != state)
                    {
                        engine.enabled = state;
                        VehicleOptions.UpdateTransfertVehicles();
                        eventEnableCheckChanged(this, state);
                    }
                }
                else
                {
                    if (m_options.enabled != state)
                    {
                        m_options.enabled = state;
                        VehicleOptions.UpdateTransfertVehicles();
                        eventEnableCheckChanged(this, state);
                    }
                }

                if (!state && !AdvancedVehicleOptions.CheckServiceValidity(m_options.category))
                {
                    GUI.UIWarningModal.message = UIMainPanel.categoryList[(int)m_options.category + 1] + Translations.Translate("AVO_MOD_OP42");
                    GUI.UIWarningModal.ShowWarning();
                }
            }
            else if (component == m_addBackEngine && m_options.addBackEngine != state)
            {
                m_options.addBackEngine = state;
                if (m_options.addBackEngine == state)
                {
                    VehicleOptions.prefabUpdateEngine = m_options.prefab;
                    SimulationManager.instance.AddAction(VehicleOptions.UpdateBackEngines);
                }
            }
            else if (component == m_useColors && m_options.useColorVariations != state)
            {
                m_options.useColorVariations = state;
                (parent as UIMainPanel).ChangePreviewColor(m_color0.selectedColor);
            }
            else if (component == m_isLargeVehicle)
            {
                m_options.isLargeVehicle = state;
            }
            else if (component == m_syncTrailers)
            {
                if (m_syncTrailers.isChecked)
                {
                    SyncTrailerDataToEngine();
                }

                if (AdvancedVehicleOptions.RememberSyncTrailerSetting)
                {
                    AdvancedVehicleOptions.LastSyncTrailerSetting = state;
                    ModSettings.Save();
                }
            }

            AdvancedVehicleOptions.ExportVehicleDataConfig(m_initialized);
            m_initialized = true;
        }

        protected void OnMaxSpeedSubmitted(UIComponent component, string text)
        {
            if (!m_initialized || m_options == null) return;
            m_initialized = false;

            if (!AdvancedVehicleOptions.SpeedUnitOption)
            {
                m_options.maxSpeed = float.Parse(text) / maxSpeedToKmhConversionFactor;
            }
            else
                m_options.maxSpeed = (float.Parse(text) * mphFactor) / maxSpeedToKmhConversionFactor;

            AdvancedVehicleOptions.ExportVehicleDataConfig(m_initialized);
            if (m_syncTrailers.isChecked) { SyncTrailerDataToEngine(); }    
            m_initialized = true;
        }

        protected void OnAccelerationSubmitted(UIComponent component, string text)
        {
            if (!m_initialized || m_options == null) return;
            m_initialized = false;

            m_options.acceleration = float.Parse(text);

            AdvancedVehicleOptions.ExportVehicleDataConfig(m_initialized);
            if (m_syncTrailers.isChecked) { SyncTrailerDataToEngine(); }
            m_initialized = true;
        }

        protected void OnBrakingSubmitted(UIComponent component, string text)
        {
            if (!m_initialized || m_options == null) return;
            m_initialized = false;

            m_options.braking = float.Parse(text);

            AdvancedVehicleOptions.ExportVehicleDataConfig(m_initialized);
            if (m_syncTrailers.isChecked) { SyncTrailerDataToEngine(); }
            m_initialized = true;
        }
        
        protected void OnTurningSubmitted(UIComponent component, string text)
        {
            if (!m_initialized || m_options == null) return;
            m_initialized = false;

            m_options.turning = float.Parse(text);

            AdvancedVehicleOptions.ExportVehicleDataConfig(m_initialized);
            if (m_syncTrailers.isChecked) { SyncTrailerDataToEngine(); }
            m_initialized = true;
        }

        protected void OnSpringsSubmitted(UIComponent component, string text)
        {
            if (!m_initialized || m_options == null) return;
            m_initialized = false;

            m_options.springs = float.Parse(text);

            AdvancedVehicleOptions.ExportVehicleDataConfig(m_initialized);
            if (m_syncTrailers.isChecked) { SyncTrailerDataToEngine(); }
            m_initialized = true;
        }

        protected void OnDampersSubmitted(UIComponent component, string text)
        {
            if (!m_initialized || m_options == null) return;
            m_initialized = false;

            m_options.dampers = float.Parse(text);

            AdvancedVehicleOptions.ExportVehicleDataConfig(m_initialized);
            if (m_syncTrailers.isChecked) { SyncTrailerDataToEngine(); }
            m_initialized = true;
        }

        protected void OnleanMultiplierSubmitted(UIComponent component, string text)
        {
            if (!m_initialized || m_options == null) return;
            m_initialized = false;

            m_options.leanMultiplier = float.Parse(text);

            AdvancedVehicleOptions.ExportVehicleDataConfig(m_initialized);
            if (m_syncTrailers.isChecked) { SyncTrailerDataToEngine(); }
            m_initialized = true;
        }

        protected void OnnodMultiplierSubmitted(UIComponent component, string text)
        {
            if (!m_initialized || m_options == null) return;
            m_initialized = false;

            m_options.nodMultiplier = float.Parse(text);

            AdvancedVehicleOptions.ExportVehicleDataConfig(m_initialized);
            if (m_syncTrailers.isChecked) { SyncTrailerDataToEngine(); }
            m_initialized = true;
        }

        protected void OnCapacitySubmitted(UIComponent component, string text)
        {
            if (!m_initialized || m_options == null) return;
            m_initialized = false;

            m_options.capacity = int.Parse(text);
            VehicleOptions.prefabUpdateUnits = m_options.prefab;
            SimulationManager.instance.AddAction(VehicleOptions.UpdateCapacityUnits);

            AdvancedVehicleOptions.ExportVehicleDataConfig(m_initialized);
            m_initialized = true;
        }

        protected void OnSpecialCapacitySubmitted(UIComponent component, string text)
        {
            if (!m_initialized || m_options == null) return;
            m_initialized = false;

            m_options.specialcapacity = int.Parse(text);
            VehicleOptions.prefabUpdateUnits = m_options.prefab;
            SimulationManager.instance.AddAction(VehicleOptions.UpdateCapacityUnits);

            AdvancedVehicleOptions.ExportVehicleDataConfig(m_initialized);
            m_initialized = true;
        }

        protected void OnColorChanged(UIComponent component, Color color)
        {
            if (!m_initialized || m_options == null) return;
            m_initialized = false;

            (parent as UIMainPanel).ChangePreviewColor(color);

            m_options.color0 = m_color0.selectedColor;
            m_options.color1 = m_color1.selectedColor;
            m_options.color2 = m_color2.selectedColor;
            m_options.color3 = m_color3.selectedColor;

            m_color0_hex.text = m_options.color0.ToString();
            m_color1_hex.text = m_options.color1.ToString();
            m_color2_hex.text = m_options.color2.ToString();
            m_color3_hex.text = m_options.color3.ToString();

            AdvancedVehicleOptions.ExportVehicleDataConfig(m_initialized);
            m_initialized = true;
        }

        protected void OnColorHexSubmitted(UIComponent component, string text)
        {
            if (!m_initialized || m_options == null) return;
            m_initialized = false;

            // Is text a valid color?
            if(text != "000000" && new HexaColor(text).ToString() == "000000")
            {
                m_color0_hex.text = m_options.color0.ToString();
                m_color1_hex.text = m_options.color1.ToString();
                m_color2_hex.text = m_options.color2.ToString();
                m_color3_hex.text = m_options.color3.ToString();

                m_initialized = true;
                return;
            }

            m_options.color0 = new HexaColor(m_color0_hex.text);
            m_options.color1 = new HexaColor(m_color1_hex.text);
            m_options.color2 = new HexaColor(m_color2_hex.text);
            m_options.color3 = new HexaColor(m_color3_hex.text);

            m_color0_hex.text = m_options.color0.ToString();
            m_color1_hex.text = m_options.color1.ToString();
            m_color2_hex.text = m_options.color2.ToString();
            m_color3_hex.text = m_options.color3.ToString();

            m_color0.selectedColor = m_options.color0;
            m_color1.selectedColor = m_options.color1;
            m_color2.selectedColor = m_options.color2;
            m_color3.selectedColor = m_options.color3;

            (parent as UIMainPanel).ChangePreviewColor(color);

            AdvancedVehicleOptions.ExportVehicleDataConfig(m_initialized);
            m_initialized = true;
        }

        protected void OnClearVehicleClicked(UIComponent component, UIMouseEventParameter p)
        {
            if (m_options == null) return;

            if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
                AdvancedVehicleOptions.ClearVehicles(null, component == m_clearParked);
            else
                AdvancedVehicleOptions.ClearVehicles(m_options, component == m_clearParked);
        }

        protected void OnlineoverviewClicked(UIComponent component, UIMouseEventParameter p)
		{
            this._publicTransportDetailPanel = GameObject.Find("(Library) PublicTransportDetailPanel").GetComponent<PublicTransportDetailPanel>();
		
			if (this._publicTransportDetailPanel.component.isVisible == true) 
			{
				UIView.library.Hide("PublicTransportDetailPanel");
			}
			else
			{
				PublicTransportDetailPanel publicTransportDetailPanel = UIView.library.Show<PublicTransportDetailPanel>("PublicTransportDetailPanel", bringToFront: true, onlyWhenInvisible: true);
					if (LineOverviewType != -1)
					{
                    publicTransportDetailPanel.SetActiveTab(LineOverviewType);
					}
			}
        }

        protected void OnUserGuideSpawnClicked(UIComponent component, UIMouseEventParameter p)
        {
            SimulationManager.instance.SimulationPaused = true;
            Application.OpenURL("https://github.com/CityGecko/CS-AdvancedVehicleOptions/wiki/02.06-Vehicle-Spawning");
        }
        protected void OnUserGuideSpecialCapacityClicked(UIComponent component, UIMouseEventParameter p)
        {
            SimulationManager.instance.SimulationPaused = true;
            Application.OpenURL("https://github.com/CityGecko/CS-AdvancedVehicleOptions/wiki/02.05-Vehicle-Settings");
        }

        private void SyncTrailerDataToEngine()
        {
            if ((m_options.prefab.m_vehicleType == VehicleInfo.VehicleType.Train) && (m_options.prefab.m_class.m_level == ItemClass.Level.Level1 || m_options.prefab.m_class.m_level == ItemClass.Level.Level2 || m_options.prefab.m_class.m_level == ItemClass.Level.Level4))
            { 
                    if ((m_options.engine != null) && (m_options.hasTrailer))
                    {
                    for (uint i = 0; i < m_options.prefab.m_trailers.Length; i++)
                    {
                        if (m_options.prefab.m_trailers[i].m_info == null) continue;
                        m_options.prefab.m_trailers[i].m_info.m_maxSpeed = m_options.prefab.m_maxSpeed;
                        m_options.prefab.m_trailers[i].m_info.m_acceleration = m_options.prefab.m_acceleration;
                        m_options.prefab.m_trailers[i].m_info.m_braking = m_options.prefab.m_braking;
                        m_options.prefab.m_trailers[i].m_info.m_turning = m_options.prefab.m_turning;
                        m_options.prefab.m_trailers[i].m_info.m_springs = m_options.prefab.m_springs;
                        m_options.prefab.m_trailers[i].m_info.m_dampers = m_options.prefab.m_dampers;
                        m_options.prefab.m_trailers[i].m_info.m_leanMultiplier = m_options.prefab.m_leanMultiplier;
                        m_options.prefab.m_trailers[i].m_info.m_nodMultiplier = m_options.prefab.m_nodMultiplier;
                    }
                }
            }

           // hier der code fürs trailer syncen übernehmen
        }
    }

}
