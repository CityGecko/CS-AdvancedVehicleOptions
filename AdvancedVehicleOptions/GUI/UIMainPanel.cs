﻿using UnityEngine;
using ColossalFramework.Globalization;
using ColossalFramework.UI;

using System;
using System.Reflection;

using UIUtils = SamsamTS.UIUtils;

namespace AdvancedVehicleOptionsUID.GUI
{
    public class UIMainPanel : UIPanel
    {
        private UITitleBar m_title;
        private UIDropDown m_category;
        private UITextField m_search;
        private UIFastList m_fastList;
        private UIButton m_import;
        private UIButton m_export;
        private UITextureSprite m_preview;
        private UISprite m_followVehicle;
        private UIOptionPanel m_optionPanel;

        public UIButton m_button;

        private VehicleOptions[] m_optionsList;
        private PreviewRenderer m_previewRenderer;
        private Color m_previewColor;
        private CameraController m_cameraController;
        private uint m_seekStart = 0;

        private const int HEIGHT = 710;
        private const int WIDTHLEFT = 470;
        private const int WIDTHRIGHT = 390;

        public static readonly string[] categoryList = { "All", "Citizen", "Bicycle",
            "Forestry", "Farming", "Ore", "Oil", "Fishing", "Industry", "Player Industry",
            "Police", "Prison", "Fire Safety", "Disaster",
            "Healthcare", "Deathcare", "Garbage", "Maintenance", "Postal Service",
            "Taxi", "Bus", "Metro", "Tram", "Monorail", "Cable Car",
            "Cargo Train", "Passenger Train",
            "Cargo Ship", "Passenger Ship","Ferry", "Cargo Plane", "Passenger Plane", "Blimp", "Tours",
            "Launch Site", "Natural" };

        public static readonly string[] vehicleIconList = { "IconCitizenVehicle", "IconCitizenBicycleVehicle",
              "IconPolicyForest", "IconPolicyFarming", "IconPolicyOre", "IconPolicyOil", "SubBarIndustryFishing", "IconPolicyNone", "SubBarIndustryUniqueFactory",
              "ToolbarIconPolice", "IconPolicyDoubleSentences", "InfoIconFireSafety", "ToolbarIconFireDepartmentHovered",
              "ToolbarIconHealthcare", "ToolbarIconHealthcareHovered", "InfoIconGarbage", "InfoIconMaintenance", "SubBarPublicTransportPost",
              "SubBarPublicTransportTaxi", "SubBarPublicTransportBus", "SubBarPublicTransportMetro", "SubBarPublicTransportTram", "SubBarPublicTransportMonorail", "SubBarPublicTransportCableCar",
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

                name = "AdvancedVehicleOptionsUID";
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

                m_button.normalBgSprite = "InfoIconTrafficCongestion"; 		// previously: "IconCitizenVehicle";
                m_button.focusedFgSprite = "ToolbarIconGroup6Focused";
                m_button.hoveredFgSprite = "ToolbarIconGroup6Hovered";

                m_button.size = new Vector2(43f, 47f);					    // previously: new Vector2(43f, 49f);
			    m_button.name = "Advanced Vehicle Options";
                m_button.tooltip = "Vehicle Options"; 						// previously: "Advanced Vehicle Options " + ModInfo.version; 
                m_button.relativePosition = new Vector3(0, 5);
			   
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

                optionList = AdvancedVehicleOptionsUID.config.options;
                DebugUtils.Log("UI initialized.");
            }
            catch (Exception e)
            {
                DebugUtils.Log("UI initialization failed.");
                DebugUtils.LogException(e);

                if (m_button != null) Destroy(m_button.gameObject);

                Destroy(gameObject);
            }
        }

        /*public override void OnDestroy()
        {
            base.OnDestroy();

            DebugUtils.Log("Destroying UIMainPanel");

            if (m_button != null) GameObject.Destroy(m_button.gameObject);
            GameObject.Destroy(m_optionPanel.gameObject);
        }*/

        private void SetupControls()
        {
            float offset = 40f;

            // Title Bar
            m_title = AddUIComponent<UITitleBar>();
            m_title.iconSprite = "InfoIconTrafficCongestion";
            m_title.title = "Advanced Vehicle Options " + ModInfo.version;

            // Category DropDown
            UILabel label = AddUIComponent<UILabel>();
            label.textScale = 0.8f;
            label.padding = new RectOffset(0, 0, 8, 0);
            label.relativePosition = new Vector3(10f, offset);
            label.text = "Category :";

            m_category = UIUtils.CreateDropDown(this);
            m_category.width = 150;

            for (int i = 0; i < categoryList.Length; i++)
                m_category.AddItem(categoryList[i]);

            m_category.selectedIndex = 0;
            m_category.tooltip = "Select a category to display\nTip: Use the mouse wheel to switch between categories faster";
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
            m_search.tooltip = "Type the name of a vehicle type";
            m_search.relativePosition = new Vector3(WIDTHLEFT - m_search.width, offset);

            m_search.eventTextChanged += (c, t) => PopulateList();

            label = AddUIComponent<UILabel>();
            label.textScale = 0.8f;
            label.padding = new RectOffset(0, 0, 8, 0);
            label.relativePosition = m_search.relativePosition - new Vector3(60f, 0f);
            label.text = "Search :";

            // FastList
            m_fastList = UIFastList.Create<UIVehicleItem>(this);
            m_fastList.backgroundSprite = "UnlockingPanel";
            m_fastList.width = WIDTHLEFT - 5;
            m_fastList.height = height - offset - 110;
            m_fastList.canSelect = true;
            m_fastList.relativePosition = new Vector3(5, offset + 35);

            // Configuration file buttons
            UILabel configLabel = this.AddUIComponent<UILabel>();
            configLabel.text = "Configuration file:";
            configLabel.textScale = 0.8f;
            configLabel.relativePosition = new Vector3(10, height - 60);

            m_import = UIUtils.CreateButton(this);
            m_import.text = "Import";
            m_import.tooltip = "Import the configuration";
            m_import.relativePosition = new Vector3(10, height - 40);

            m_export = UIUtils.CreateButton(this);
            m_export.text = "Export";
            m_export.tooltip = "Export the configuration";
            m_export.relativePosition = new Vector3(105, height - 40);

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

            // Follow
            if (m_cameraController != null)
            {
                m_followVehicle = AddUIComponent<UISprite>();
                m_followVehicle.spriteName = "LocationMarkerFocused";
                m_followVehicle.width = m_followVehicle.spriteInfo.width;
                m_followVehicle.height = m_followVehicle.spriteInfo.height;
                m_followVehicle.tooltip = "Click here to cycle through the existing vehicles of that type";
                m_followVehicle.relativePosition = new Vector3(panel.relativePosition.x + panel.width - m_followVehicle.width - 5, panel.relativePosition.y + 5);

                m_followVehicle.eventClick += (c, p) => FollowNextVehicle();
            }

            // Option panel
            m_optionPanel = AddUIComponent<UIOptionPanel>();
            m_optionPanel.relativePosition = new Vector3(WIDTHLEFT, height - 370);

            // Event handlers
            m_fastList.eventSelectedIndexChanged += OnSelectedItemChanged; 
            m_optionPanel.eventEnableCheckChanged += OnEnableStateChanged;
            m_import.eventClick += (c, t) =>
            {
                DefaultOptions.RestoreAll();
                AdvancedVehicleOptionsUID.ImportConfig();
                optionList = AdvancedVehicleOptionsUID.config.options;
            };
            m_export.eventClick += (c, t) => AdvancedVehicleOptionsUID.ExportConfig();

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
        }

        private void FollowNextVehicle()
        {
            Array16<Vehicle> vehicles = VehicleManager.instance.m_vehicles;
            VehicleOptions options = m_optionPanel.m_options;

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

                    m_cameraController.SetTarget(instanceID, ToolsModifierControl.cameraController.transform.position, false);

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
