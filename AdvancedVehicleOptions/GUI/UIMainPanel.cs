using UnityEngine;
using ColossalFramework.Globalization;
using ColossalFramework.UI;

using System;
using System.Reflection;

using UIUtils = SamsamTS.UIUtils;

namespace AdvancedVehicleOptionsUID.GUI
{
    public class UIMainPanel : UIPanel
    {
        internal UITitleBar m_title;
        internal UIDropDown m_category;
        internal UITextField m_search;
        internal UIFastList m_fastList;
        internal UIButton m_import;
        internal UIButton m_export;
        internal UILabel m_autosave;
        internal UIButton m_resetall;
        internal UITextureSprite m_preview;
        internal UISprite m_followVehicle;
        internal UISprite m_removeVehicle;
        internal UIOptionPanel m_optionPanel;

        internal UIButton m_button;
        public ushort FollowVehicleInstanceID;

        internal VehicleOptions[] m_optionsList;
        internal PreviewRenderer m_previewRenderer;
        internal Color m_previewColor;
        internal CameraController m_cameraController;
        internal uint m_seekStart = 0;

        private const int HEIGHT = 710;
        private const int WIDTHLEFT = 470;
        private const int WIDTHRIGHT = 390;

        public static readonly string[] categoryList = { Translations.Translate("AVO_MOD_MP00"), Translations.Translate("AVO_MOD_MP01"), Translations.Translate("AVO_MOD_MP02"), Translations.Translate("AVO_MOD_MP03"), 
                                                         Translations.Translate("AVO_MOD_MP04"), Translations.Translate("AVO_MOD_MP05"), Translations.Translate("AVO_MOD_MP06"), Translations.Translate("AVO_MOD_MP07"),
                                                         Translations.Translate("AVO_MOD_MP08"), Translations.Translate("AVO_MOD_MP09"), Translations.Translate("AVO_MOD_MP10"), Translations.Translate("AVO_MOD_MP11"), 
                                                         Translations.Translate("AVO_MOD_MP12"), Translations.Translate("AVO_MOD_MP13"), Translations.Translate("AVO_MOD_MP14"), Translations.Translate("AVO_MOD_MP15"), 
                                                         Translations.Translate("AVO_MOD_MP16"), Translations.Translate("AVO_MOD_MP17"), Translations.Translate("AVO_MOD_MP18"), Translations.Translate("AVO_MOD_MP19"), Translations.Translate("AVO_MOD_MP57"),
                                                         Translations.Translate("AVO_MOD_MP20"), Translations.Translate("AVO_MOD_MP21"), Translations.Translate("AVO_MOD_MP22"), Translations.Translate("AVO_MOD_MP23"),
                                                         Translations.Translate("AVO_MOD_MP24"), Translations.Translate("AVO_MOD_MP25"), Translations.Translate("AVO_MOD_MP26"), Translations.Translate("AVO_MOD_MP27"),
                                                         Translations.Translate("AVO_MOD_MP28"), Translations.Translate("AVO_MOD_MP29"), Translations.Translate("AVO_MOD_MP30"), Translations.Translate("AVO_MOD_MP31"),
                                                         Translations.Translate("AVO_MOD_MP32"), Translations.Translate("AVO_MOD_MP33"), Translations.Translate("AVO_MOD_MP34"), Translations.Translate("AVO_MOD_MP35"), 
                                                         Translations.Translate("AVO_MOD_MP36"), Translations.Translate("AVO_MOD_MP37"), Translations.Translate("AVO_MOD_MP38") };

        public static readonly string[] vehicleIconList = { "IconCitizenVehicle", "IconCitizenBicycleVehicle",
              "IconPolicyForest", "IconPolicyFarming", "IconPolicyOre", "IconPolicyOil", "SubBarIndustryFishing", "IconPolicyNone", "SubBarIndustryUniqueFactory",
              "ToolbarIconPolice", "IconPolicyDoubleSentences", "InfoIconFireSafety", "ToolbarIconFireDepartmentHovered",
              "ToolbarIconHealthcare", "ToolbarIconHealthcareHovered", "InfoIconGarbage", "InfoIconGarbage", "InfoIconMaintenance", "SubBarPublicTransportPost", "SubBarPoliceBank",
              "SubBarPublicTransportBus", "SubBarPublicTransportBus", "SubBarPublicTransportTrolleybus", "SubBarPublicTransportTaxi", "SubBarPublicTransportMetro", "SubBarPublicTransportTram", "SubBarPublicTransportMonorail", "SubBarPublicTransportCableCar", 
              "SubBarPublicTransportTrain", "SubBarPublicTransportTrain",
              "IconCargoShip", "SubBarPublicTransportShip",  "IconPolicyPreferFerries", "SubBarPublicTransportPlane", "SubBarPublicTransportPlane", "IconPolicyEducationalBlimps", "SubBarPublicTransportTours",
              "ToolbarIconMonuments", "SubBarFireDepartmentDisaster"};

        public UIOptionPanel optionPanel
        {
            get { return m_optionPanel; }
        }

        public VehicleOptions[] optionList
        {
            get { return m_optionsList; }
            set
            {
                m_optionsList = value;
                Array.Sort(m_optionsList);

                PopulateList();
            }
        }

        public override void Start()
        {
            try
            {
                UIView view = GetUIView();

                name = "AdvancedVehicleOptions";
                backgroundSprite = "UnlockingPanel2";
                isVisible = false;
                canFocus = true;
                isInteractive = true;
                width = WIDTHLEFT + WIDTHRIGHT;
                height = HEIGHT;
                relativePosition = new Vector3(Mathf.Floor((view.fixedWidth - width) / 2), Mathf.Floor((view.fixedHeight - height) / 2));

                // Get camera controller
                GameObject go = GameObject.FindGameObjectWithTag("MainCamera");
                if (go != null)
                {
                    m_cameraController = go.GetComponent<CameraController>();
                }

                // Setting up UI
                SetupControls();

                // Adding main button
                UITabstrip toolStrip = view.FindUIComponent<UITabstrip>("MainToolstrip");
                m_button = AddUIComponent<UIButton>();

                m_button.normalBgSprite = "InfoIconTrafficCongestion";
                m_button.focusedFgSprite = "ToolbarIconGroup6Focused";
                m_button.hoveredFgSprite = "ToolbarIconGroup6Hovered";

                m_button.size = new Vector2(43f, 47f);
			    m_button.name = AVOMod.ModName;
                m_button.tooltip = "Modify various vehicle properties";
                m_button.relativePosition = new Vector3(0, 5);
			   
                // GUI Button is pressed in game
                m_button.eventButtonStateChanged += (c, s) =>
                {
                    if (s == UIButton.ButtonState.Focused)
                    {
                        if (!isVisible)
                        {
                            isVisible = true;
                            m_fastList.DisplayAt(m_fastList.listPosition);
                            m_optionPanel.Show(m_fastList.rowsData[m_fastList.selectedIndex] as VehicleOptions);
                            m_followVehicle.isVisible = m_preview.parent.isVisible = true;
                            AdvancedVehicleOptions.UpdateOptionPanelInfo();
                        }
                    }
                    else
                    {
                        isVisible = false;
                        m_button.Unfocus();                   
                    }
                };

                toolStrip.AddTab("Advanced Vehicle Options", m_button.gameObject, null, null);

                FieldInfo m_ObjectIndex = typeof(MainToolbar).GetField("m_ObjectIndex", BindingFlags.Instance | BindingFlags.NonPublic);
                m_ObjectIndex.SetValue(ToolsModifierControl.mainToolbar, (int)m_ObjectIndex.GetValue(ToolsModifierControl.mainToolbar) + 1);

                m_title.closeButton.eventClick += (component, param) =>
                {
                    toolStrip.closeButton.SimulateClick();
                };

                Locale locale = (Locale)typeof(LocaleManager).GetField("m_Locale", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(LocaleManager.instance);
                Locale.Key key = new Locale.Key
                {
                    m_Identifier = "TUTORIAL_ADVISER_TITLE",
                    m_Key = m_button.name
                };
                if (!locale.Exists(key))
                {
                    locale.AddLocalizedString(key, m_button.name);
                }
                key = new Locale.Key
                {
                    m_Identifier = "TUTORIAL_ADVISER",
                    m_Key = m_button.name
                };
                if (!locale.Exists(key))
                {
                    locale.AddLocalizedString(key, "");
                }

                view.FindUIComponent<UITabContainer>("TSContainer").AddUIComponent<UIPanel>().color = new Color32(0, 0, 0, 0);

                optionList = AdvancedVehicleOptions.config.options;
                Logging.Message("UI initialized.");
            }
            catch (Exception e)
            {
                Logging.Error("UI initialization failed.");
                Logging.LogException(e);

                if (m_button != null) Destroy(m_button.gameObject);

                Destroy(gameObject);
            }
        }

		// Hide the GUI Button from Mainpanel
        public override void OnDestroy()
        {
            base.OnDestroy();

            Logging.Message("Destroying UIMainPanel");

            if (m_button != null) GameObject.Destroy(m_button.gameObject);
            GameObject.Destroy(m_optionPanel.gameObject);
        }

        internal void SetupControls()
        {
            float offset = 40f;

            // Title Bar
            m_title = AddUIComponent<UITitleBar>();
            m_title.iconSprite = "InfoIconTrafficCongestion";
            m_title.title = AVOMod.ModName + " " + AVOMod.Version;

            // Category DropDown
            UILabel label = AddUIComponent<UILabel>();
            label.textScale = 0.8f;
            label.padding = new RectOffset(0, 0, 8, 0);
            label.relativePosition = new Vector3(10f, offset);
            label.text = Translations.Translate("AVO_MOD_MP56");

            m_category = UIUtils.CreateDropDown(this);
            m_category.width = 175;

            for (int i = 0; i < categoryList.Length; i++)
                m_category.AddItem(categoryList[i]);

            m_category.selectedIndex = 0;
            m_category.tooltip = Translations.Translate("AVO_MOD_MP39");
            m_category.relativePosition = label.relativePosition + new Vector3(75f, 0f);

            m_category.eventSelectedIndexChanged += (c, t) =>
            {
                m_category.enabled = false;
                PopulateList();
                m_category.enabled = true;
            };

            // Search
            m_search = UIUtils.CreateTextField(this);
            m_search.width = 145f;
            m_search.height = 30f;
            m_search.padding = new RectOffset(6, 6, 6, 6);
            m_search.tooltip = Translations.Translate("AVO_MOD_MP40");
            m_search.relativePosition = new Vector3(WIDTHLEFT - m_search.width, offset);

            m_search.eventTextChanged += (c, t) => PopulateList();

            label = AddUIComponent<UILabel>();
            label.textScale = 0.8f;
            label.padding = new RectOffset(0, 0, 8, 0);
            label.relativePosition = m_search.relativePosition - new Vector3(60f, 0f);
            label.text = Translations.Translate("AVO_MOD_MP55");

            // FastList
            m_fastList = UIFastList.Create<UIVehicleItem>(this);
            m_fastList.backgroundSprite = "UnlockingPanel";
            m_fastList.width = WIDTHLEFT - 5;
            m_fastList.height = height - offset - 110;
            m_fastList.canSelect = true;
            m_fastList.relativePosition = new Vector3(5, offset + 35);

            // Configuration file buttons
            UILabel configLabel = this.AddUIComponent<UILabel>();
            configLabel.text = Translations.Translate("AVO_MOD_MP41");
            configLabel.textScale = 0.8f;
            configLabel.relativePosition = new Vector3(16, height - 65);

            m_import = UIUtils.CreateButton(this);
            m_import.text = Translations.Translate("AVO_MOD_MP42");
            m_import.width = 80;
            m_import.tooltip = Translations.Translate("AVO_MOD_MP43");
            m_import.relativePosition = new Vector3(10, height - 45);

            m_export = UIUtils.CreateButton(this);
            m_export.text = Translations.Translate("AVO_MOD_MP44");
            m_export.width = 80;
            m_export.tooltip = Translations.Translate("AVO_MOD_MP45");
            m_export.relativePosition = new Vector3(95, height - 45);

            m_resetall = UIUtils.CreateButton(this);
            m_resetall.text = Translations.Translate("AVO_MOD_MP46");
            m_resetall.width = 80;
            m_resetall.tooltip = Translations.Translate("AVO_MOD_MP47");
            m_resetall.relativePosition = new Vector3(180, height - 45);

            m_autosave = AddUIComponent<UILabel>();
            m_autosave.textScale = 0.6f;
            m_autosave.text = Translations.Translate("AVO_MOD_MP48");
            m_autosave.relativePosition = new Vector3(275, height - 40);
            m_autosave.autoSize = true;
            m_autosave.textAlignment = UIHorizontalAlignment.Center;
            m_autosave.textColor = Color.green;
            m_autosave.tooltip = Translations.Translate("AVO_MOD_MP49");
            m_autosave.isVisible = AdvancedVehicleOptions.AutoSaveVehicleConfig;

            // Preview
            UIPanel panel = AddUIComponent<UIPanel>();
            panel.backgroundSprite = "GenericPanel";
            panel.width = WIDTHRIGHT - 10;
            panel.height = HEIGHT - 420;
            panel.relativePosition = new Vector3(WIDTHLEFT + 5, offset);

            m_preview = panel.AddUIComponent<UITextureSprite>();
            m_preview.size = panel.size;
            m_preview.relativePosition = Vector3.zero;

            m_previewRenderer = gameObject.AddComponent<PreviewRenderer>();
            m_previewRenderer.size = m_preview.size * 2; // Twice the size for anti-aliasing

            m_preview.texture = m_previewRenderer.texture;

            // Follow a vehicle
            if (m_cameraController != null)
            {
                m_followVehicle = AddUIComponent<UISprite>();
                m_followVehicle.spriteName = "LocationMarkerFocused";
                m_followVehicle.width = m_followVehicle.spriteInfo.width;
                m_followVehicle.height = m_followVehicle.spriteInfo.height;
                m_followVehicle.tooltip = Translations.Translate("AVO_MOD_MP50");
                m_followVehicle.relativePosition = new Vector3(panel.relativePosition.x + panel.width - m_followVehicle.width - 5, panel.relativePosition.y + 5);

                m_followVehicle.eventClick += (c, p) => FollowNextVehicle();
            }
			
			//Remove the followed vehicle
			{
				m_removeVehicle = AddUIComponent<UISprite>();
                m_removeVehicle.Hide();
                m_removeVehicle.spriteName = "IconPolicyOldTown";
                m_removeVehicle.width = m_removeVehicle.spriteInfo.width -12;
                m_removeVehicle.height = m_removeVehicle.spriteInfo.height -12;
                m_removeVehicle.tooltip = Translations.Translate("AVO_MOD_MP51");
                m_removeVehicle.relativePosition = new Vector3(panel.relativePosition.x + panel.width - m_removeVehicle.width - 33, panel.relativePosition.y + 7);

                m_removeVehicle.eventClick += (c, p) => RemoveThisVehicle();
            }
			
            // Option panel
            m_optionPanel = AddUIComponent<UIOptionPanel>();
            m_optionPanel.relativePosition = new Vector3(WIDTHLEFT, height - 370);

            // Event handlers
            m_fastList.eventSelectedIndexChanged += OnSelectedItemChanged;
            m_optionPanel.eventEnableCheckChanged += OnEnableStateChanged;

            m_import.eventClick += (c, t) =>
            {
                ConfirmPanel.ShowModal(Translations.Translate("AVO_MOD_MP52"), Translations.Translate("AVO_MOD_MP58"), (comp, ret) =>
                {
                    if (ret != 1)
                        return;

                    DefaultOptions.RestoreAll();
                    AdvancedVehicleOptions.ImportVehicleDataConfig();
                    optionList = AdvancedVehicleOptions.config.options;
                });
            };

            m_export.eventClick += (c, t) => AdvancedVehicleOptions.ExportVehicleDataConfig(true);

            m_resetall.eventClick += (c, t) =>
            {
                   ConfirmPanel.ShowModal(Translations.Translate("AVO_MOD_MP52"), Translations.Translate("AVO_MOD_MP53"), (comp, ret) =>
                   {
                    if (ret != 1)
                        return;

                    DefaultOptions.RestoreAll();
                    AdvancedVehicleOptions.ResetVehicleDataConfig();
                    optionList = AdvancedVehicleOptions.config.options;

                    ExceptionPanel resetpanel = UIView.library.ShowModal<ExceptionPanel>("ExceptionPanel");
                    resetpanel.SetMessage("Advanced Vehicle Options", Translations.Translate("AVO_MOD_MP54"), false);
                });

            };

            panel.eventMouseDown += (c, p) =>
            {
                eventMouseMove += RotateCamera;
                if (m_optionPanel.m_options != null && m_optionPanel.m_options.useColorVariations)
                    m_previewRenderer.RenderVehicle(m_optionPanel.m_options.prefab, m_previewColor);
                else
                    m_previewRenderer.RenderVehicle(m_optionPanel.m_options.prefab);

            };

            panel.eventMouseUp += (c, p) =>
            {
                eventMouseMove -= RotateCamera;
                if (m_optionPanel.m_options != null && m_optionPanel.m_options.useColorVariations)
                    m_previewRenderer.RenderVehicle(m_optionPanel.m_options.prefab, m_previewColor);
                else
                    m_previewRenderer.RenderVehicle(m_optionPanel.m_options.prefab);
            };

            panel.eventMouseWheel += (c, p) =>
            {
                m_previewRenderer.zoom -= Mathf.Sign(p.wheelDelta) * 0.25f;
                if (m_optionPanel.m_options != null && m_optionPanel.m_options.useColorVariations)
                    m_previewRenderer.RenderVehicle(m_optionPanel.m_options.prefab, m_previewColor);
                else
                    m_previewRenderer.RenderVehicle(m_optionPanel.m_options.prefab);
            };
        }

        private void RotateCamera(UIComponent c, UIMouseEventParameter p)
        {
            m_previewRenderer.cameraRotation -= p.moveDelta.x / m_preview.width * 360f;
            if (m_optionPanel.m_options != null && m_optionPanel.m_options.useColorVariations)
                m_previewRenderer.RenderVehicle(m_optionPanel.m_options.prefab, m_previewColor);
            else
                m_previewRenderer.RenderVehicle(m_optionPanel.m_options.prefab);
        }

        private void PopulateList()
        {
            m_fastList.rowsData.Clear();
            m_fastList.selectedIndex = -1;
            for (int i = 0; i < m_optionsList.Length; i++)
            {
                if (m_optionsList[i] != null &&
                    (m_category.selectedIndex == 0 || (int)m_optionsList[i].category == m_category.selectedIndex - 1) &&
                    (String.IsNullOrEmpty(m_search.text.Trim()) || m_optionsList[i].localizedName.ToLower().Contains(m_search.text.Trim().ToLower())))
                {
                    m_fastList.rowsData.Add(m_optionsList[i]);
                }
            }

            m_fastList.rowHeight = 40f;
            m_fastList.DisplayAt(0);
            m_fastList.selectedIndex = 0;

            m_optionPanel.isVisible = m_fastList.rowsData.m_size > 0;
            m_followVehicle.isVisible = m_preview.parent.isVisible = m_optionPanel.isVisible;
            Logging.Message("Populating List");
            //AdvancedVehicleOptions.UpdateOptionPanelInfo();
        }

        private void RemoveThisVehicle()
        {
            Logging.Message("Current vehicle instance [for Removing]: " + FollowVehicleInstanceID);

            if (FollowVehicleInstanceID != 0)
            {
                SimulationManager.instance.AddAction(() => VehicleManager.instance.ReleaseVehicle(FollowVehicleInstanceID));
                m_removeVehicle.Hide();
            }
        }

        private void FollowNextVehicle()
        {
            Array16<Vehicle> vehicles = VehicleManager.instance.m_vehicles;
            FollowVehicleInstanceID = 0;
            m_removeVehicle.Hide();
            // VehicleOptions options = m_optionPanel.m_options;    (commenting out as never used)

            for (uint i = (m_seekStart + 1) % vehicles.m_size; i != m_seekStart; i = (i + 1) % vehicles.m_size)
            {
                if (vehicles.m_buffer[i].Info == m_optionPanel.m_options.prefab)
                {
                    bool isSpawned = (vehicles.m_buffer[i].m_flags & Vehicle.Flags.Spawned) == Vehicle.Flags.Spawned;

                    InstanceID instanceID = default(InstanceID);
                    instanceID.Vehicle = (ushort)i;

                    if (!isSpawned || instanceID.IsEmpty || !InstanceManager.IsValid(instanceID)) continue;

                    Vector3 targetPosition;
                    Quaternion quaternion;
                    Vector3 vector;

                    if (!InstanceManager.GetPosition(instanceID, out targetPosition, out quaternion, out vector)) continue;

                    Vector3 pos = targetPosition;
                    GameAreaManager.instance.ClampPoint(ref targetPosition);
                    if (targetPosition != pos) continue;

                    m_cameraController.SetTarget(instanceID, ToolsModifierControl.cameraController.transform.position, Input.GetKey(KeyCode.LeftShift) | Input.GetKey(KeyCode.RightShift));

                    FollowVehicleInstanceID = instanceID.Vehicle;
                   
                    Logging.Message("Identified last vehicle instance [to Follow]: " + FollowVehicleInstanceID);
                    
                    m_removeVehicle.Show();

                    m_seekStart = (i + 1) % vehicles.m_size;
                    return;
                }
            }
            m_seekStart = 0;
        }

        protected void OnSelectedItemChanged(UIComponent component, int i)
        {
            m_seekStart = 0;

            VehicleOptions options = m_fastList.rowsData[i] as VehicleOptions;

            m_removeVehicle.Hide();
            m_optionPanel.Show(options);
            m_followVehicle.isVisible = m_preview.parent.isVisible = true;

            m_previewColor = options.color0;
            m_previewColor.a = 0; // Fixes the wrong lighting on one half of the vehicle
            m_previewRenderer.cameraRotation = -60;// 120f;
            m_previewRenderer.zoom = 3f;
            if (options.useColorVariations)
                m_previewRenderer.RenderVehicle(m_optionPanel.m_options.prefab, m_previewColor);
            else
                m_previewRenderer.RenderVehicle(m_optionPanel.m_options.prefab);
        }

        protected void OnEnableStateChanged(UIComponent component, bool state)
        {
            m_fastList.DisplayAt(m_fastList.listPosition);
        }

        public void ChangePreviewColor(Color color)
        {
            if (m_optionPanel.m_options != null)
            {
                if (m_optionPanel.m_options.useColorVariations && m_previewColor != color)
                {
                    m_previewColor = color;
                    m_previewColor.a = 0; // Fixes the wrong lighting on one half of the vehicle
                    m_previewRenderer.RenderVehicle(m_optionPanel.m_options.prefab, m_previewColor);
                }
                else
                {
                    m_previewRenderer.RenderVehicle(m_optionPanel.m_options.prefab);
                }
            }
        }
    }

}
