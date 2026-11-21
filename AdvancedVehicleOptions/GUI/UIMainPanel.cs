using ColossalFramework;
using ColossalFramework.Globalization;
using ColossalFramework.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
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

        internal List<string> englishCategory = new List<string>();

        internal UIButton m_button;

        // Decided once when the panel is created: with "Hide GUI button" the panel still exists (so the
        // hotkey can open it, as the option's tooltip promises), it is just not integrated into the toolbar.
        // Changing the option rebuilds the panel (AdvancedVehicleOptions.UpdateGUI).
        private readonly bool m_withToolbarTab = !AdvancedVehicleOptions.HideGUIbutton;
        internal bool withToolbarTab => m_withToolbarTab;

        // Shared game state touched by the toolbar-tab integration; every field here is
        // undone again in OnDestroy so panel recreation cannot accumulate drift.
        private UITabstrip m_toolStrip;
        private UIPanel m_tsPlaceholder;
        private bool m_objectIndexBumped;

        public ushort FollowVehicleInstanceID;

        internal VehicleOptions[] m_optionsList;
        internal PreviewRenderer m_previewRenderer;
        internal Color m_previewColor;
        internal CameraController m_cameraController;
        internal uint m_seekStart = 0;

        private const int HEIGHT = 710;
        private const int WIDTHLEFT = 470;
        private const int WIDTHRIGHT = 390;


        public static readonly string[] categoryList = CategoryEnumToArray();


        public static readonly string[] vehicleIconList = {
                                                            "IconCitizenVehicle", "IconCitizenBicycleVehicle", "IconPolicyForest",
                                                            "IconPolicyFarming", "IconPolicyOre", "IconPolicyOil", "SubBarIndustryFishing",
                                                            "IconPolicyNone", "SubBarIndustryUniqueFactory", "ToolbarIconPolice", "IconPolicyDoubleSentences",
                                                            "InfoIconFireSafety", "ToolbarIconFireDepartmentHovered", "ToolbarIconHealthcare", "ToolbarIconHealthcareHovered",
                                                            "InfoIconGarbage", "InfoIconGarbage", "InfoIconMaintenance", "SubBarPublicTransportPost",
                                                            "SubBarPoliceBank", "SubBarPublicTransportBus", "SubBarPublicTransportBus", "SubBarPublicTransportTrolleybus",
                                                            "SubBarPublicTransportTaxi", "SubBarPublicTransportMetro", "SubBarPublicTransportTram", "SubBarPublicTransportMonorail",
                                                            "SubBarPublicTransportCableCar", "SubBarPublicTransportTrain", "SubBarPublicTransportTrain", "IconCargoShip",
                                                            "SubBarPublicTransportShip", "IconPolicyPreferFerries", "IconPolicyPreferFerries", "SubBarPublicTransportPlane",
                                                            "SubBarPublicTransportPlane", "IconPolicyHelicopterPriority", "IconPolicyHelicopterPriority","IconPolicyEducationalBlimps",
                                                            "IconPolicyAirplaneTours", "SubBarPublicTransportTours", "SubBarBeautificationRacesAndParades", "SubBarBeautificationRacesAndParades",
                                                            "ToolbarIconMonuments", "SubBarFireDepartmentDisaster"
                                                          };

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
                // Centre on the real UI area: fixedWidth/fixedHeight are always 1920x1080, GetScreenResolution
                // respects aspect ratio and UI scaling (otherwise left of centre on 21:9)
                Vector2 screen = view.GetScreenResolution();
                relativePosition = new Vector3(Mathf.Floor((screen.x - width) / 2), Mathf.Floor((screen.y - height) / 2));

                // Get camera controller
                GameObject go = GameObject.FindGameObjectWithTag("MainCamera");
                if (go != null)
                {
                    m_cameraController = go.GetComponent<CameraController>();
                }

                // Setting up UI
                SetupControls();

                // Adding main button - integrated into the main toolstrip as a tab, as before.
                // Injecting a foreign tab requires compensating tweaks (tab-index shift,
                // container placeholder, adviser suppression). The original mod applied them
                // once and never undid them, so every panel recreation stacked another copy
                // and desynced toolbar routing. Each tweak is now tracked and reverted in
                // OnDestroy, and stale leftovers are cleaned up here.
                UITabstrip toolStrip = m_withToolbarTab ? view.FindUIComponent<UITabstrip>("MainToolstrip") : null;
                if (!m_withToolbarTab)
                {
                    Logging.Message("Toolbar button hidden by option; panel is reachable via hotkey.");
                    m_button = null;
                }
                else if (toolStrip == null)
                {
                    Logging.Error("Main toolstrip not found; AVO button not created (hotkey still works).");
                    m_button = null;
                }
                else
                {
                    // Self-healing guard: a tab left over from a previous panel instance
                    // would otherwise be duplicated, shifting MainToolbar.m_ObjectIndex twice.
                    List<GameObject> leftovers = new List<GameObject>();
                    foreach (UIButton child in toolStrip.GetComponentsInChildren<UIButton>(true))
                    {
                        if (child != null && child.name == AVOMod.ModName) leftovers.Add(child.gameObject);
                    }
                    foreach (GameObject leftover in leftovers) GameObject.Destroy(leftover);

                    m_toolStrip = toolStrip;

                    m_button = AddUIComponent<UIButton>();

                    m_button.normalBgSprite = "InfoIconTrafficCongestion";
                    m_button.focusedFgSprite = "ToolbarIconGroup6Focused";
                    m_button.hoveredFgSprite = "ToolbarIconGroup6Hovered";

                    m_button.size = new Vector2(43f, 47f);
				    m_button.name = AVOMod.ModName;
                    m_button.tooltip = "Modify various vehicle properties";
                    m_button.relativePosition = new Vector3(0, 5);

                    // GUI Button is pressed in game
                    m_button.eventButtonStateChanged += OnButtonStateChanged;

                    UIComponent tab = toolStrip.AddTab("Advanced Vehicle Options", m_button.gameObject, null, null);

                    // Remove the tutorial adviser popup the game automatically attaches to toolbar tabs - AVO has no adviser text registered for it, so it would otherwise show raw untranslated locale keys.
                    TutorialUITag tutorialTag = tab.GetComponent<TutorialUITag>();
                    if (tutorialTag != null)
                    {
                        Destroy(tutorialTag);
                    }

                    tutorialTag = m_button.GetComponent<TutorialUITag>();
                    if (tutorialTag != null)
                    {
                        Destroy(tutorialTag);
                    }

                    // Compensate for the extra tab so the game's tool routing stays aligned.
                    FieldInfo m_ObjectIndex = typeof(MainToolbar).GetField("m_ObjectIndex", BindingFlags.Instance | BindingFlags.NonPublic);
                    m_ObjectIndex.SetValue(ToolsModifierControl.mainToolbar, (int)m_ObjectIndex.GetValue(ToolsModifierControl.mainToolbar) + 1);
                    m_objectIndexBumped = true;

                    m_title.closeButton.eventClick += OnTitleCloseClicked;

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

                    // Selecting our tab makes the game look up a TSContainer child by index;
                    // add an invisible placeholder so real tools keep their slots.
                    m_tsPlaceholder = view.FindUIComponent<UITabContainer>("TSContainer").AddUIComponent<UIPanel>();
                    m_tsPlaceholder.color = new Color32(0, 0, 0, 0);
                }

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

            // Undo everything Start() did to shared game state. Every step is defensive:
            // an exception here used to leave the compensation tweaks applied forever, and
            // the next panel instance stacked ANOTHER copy on top (cumulative toolbar
            // corruption). Uses only cached references - no UIView lookups, which are not
            // safe during teardown.
            try
            {
                if (m_title != null && m_title.closeButton != null)
                {
                    m_title.closeButton.eventClick -= OnTitleCloseClicked;
                }

                if (m_tsPlaceholder != null)
                {
                    GameObject.Destroy(m_tsPlaceholder.gameObject);
                    m_tsPlaceholder = null;
                }

                if (m_objectIndexBumped && ToolsModifierControl.mainToolbar != null)
                {
                    FieldInfo m_ObjectIndex = typeof(MainToolbar).GetField("m_ObjectIndex", BindingFlags.Instance | BindingFlags.NonPublic);
                    m_ObjectIndex.SetValue(ToolsModifierControl.mainToolbar, (int)m_ObjectIndex.GetValue(ToolsModifierControl.mainToolbar) - 1);
                    m_objectIndexBumped = false;
                }
            }
            catch (Exception e)
            {
                Logging.LogException(e, "exception reverting toolbar integration");
            }

            if (m_button != null) GameObject.Destroy(m_button.gameObject);
            GameObject.Destroy(m_optionPanel.gameObject);

            m_toolStrip = null;
        }

        private void OnButtonStateChanged(UIComponent c, UIButton.ButtonState s)
        {
            if (s == UIButton.ButtonState.Focused)
            {
                // Focusing a toolbar tab makes the game show its own tutorial adviser popup for it; AVO has no adviser content for this, so close it immediately.
                TutorialAdvisorPanel.instance?.Hide();

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
                if (m_button != null) m_button.Unfocus();
            }
        }

        private void OnTitleCloseClicked(UIComponent component, UIMouseEventParameter param)
        {
            // The title bar itself hides the panel (UITitleBar). Deselect AVO's toolbar tab too, but only if it is
            // actually selected - simulating a click on the toolstrip's own close button while no tab was open
            // reached the game's toolbar logic for no reason.
            if (IsToolbarTabSelected()) m_toolStrip.selectedIndex = -1;
        }

        // Index of AVO's tab in the main toolstrip, or -1 (no tab, e.g. "Hide GUI button").
        private int GetToolbarTabIndex()
        {
            if (m_toolStrip == null || m_button == null) return -1;
            for (int i = 0; i < m_toolStrip.tabCount; i++)
            {
                if (m_toolStrip.tabs[i] == m_button) return i;
            }
            return -1;
        }

        private bool IsToolbarTabSelected()
        {
            int index = GetToolbarTabIndex();
            return index >= 0 && m_toolStrip.selectedIndex == index;
        }

        /// <summary>
        /// Toggle the main panel visibility; used by the hotkey. With a toolbar tab, the tab selection is driven
        /// as well, so panel and tab never disagree (before, a hotkey-opened panel left the tab unselected: the
        /// next tab click seemingly did nothing and closing went through the toolstrip's close button).
        /// </summary>
        internal void TogglePanel()
        {
            int tabIndex = GetToolbarTabIndex();

            if (isVisible)
            {
                if (IsToolbarTabSelected()) m_toolStrip.selectedIndex = -1;   // -> OnButtonStateChanged hides the panel
                isVisible = false;
                return;
            }

            if (tabIndex >= 0)
            {
                m_toolStrip.selectedIndex = tabIndex;                         // -> OnButtonStateChanged shows the panel
                if (isVisible) return;
            }

            isVisible = true;
            m_fastList.DisplayAt(m_fastList.listPosition);
            m_optionPanel.Show(m_fastList.rowsData[m_fastList.selectedIndex] as VehicleOptions);
            m_followVehicle.isVisible = m_preview.parent.isVisible = true;
            AdvancedVehicleOptions.UpdateOptionPanelInfo();
        }

        public static string[] CategoryEnumToArray()
        {
           return  Enum.GetValues(typeof(VehicleOptions.Category))
                    .Cast<VehicleOptions.Category>()
                    .OrderBy(c => (int)c) // add the enum values in order the none is first
                    .Select(c => c.ToString())
                    .ToArray();
        }

        public static bool CategoryAvailability(string category)
        {
            switch (category)
            {
                case "Bicycle":
                case "TransportTaxi":
                case "Prison":
                    if (!AdvancedVehicleOptions.hasAfterDarkDLC)
                        return false;
                    break;

                case "TransportTram" when !AdvancedVehicleOptions.hasSnowfallDLC:
                    return false;

                case "Disaster":
                case "Natural":
                    if (!AdvancedVehicleOptions.HasNaturalDisastersDLC)
                        return false;
                    break;

                case "TransportMonorail":
                case "TransportCableCar":
                case "TransportFerry":
                case "TransportBlimp":
                    if (!AdvancedVehicleOptions.hasMassTransitDLC)
                        return false;
                    break;

                case "TransportTours" when !AdvancedVehicleOptions.hasParkLifeDLC:
                    return false;

                case "IndustryPlayer":
                case "PostalService":
                    if (!AdvancedVehicleOptions.hasIndustriesDLC)
                        return false;
                    break;

                case "Maintenance" when !AdvancedVehicleOptions.hasSnowfallDLC && !AdvancedVehicleOptions.hasParkLifeDLC:
                    return false;

                case "CargoPlane" when !AdvancedVehicleOptions.hasIndustriesDLC && !AdvancedVehicleOptions.hasAirportDLC:
                    return false;

                case "BankService" when !AdvancedVehicleOptions.hasFinancialDistricsDLC:
                    return false;

                case "RaceDayCars" when !AdvancedVehicleOptions.hasRaceDayDLC:
                    return false;

                case "RaceDayBicycles" when !AdvancedVehicleOptions.hasRaceDayDLC:
                    return false;

                case "Fishing":
                case "LocalAirTraffic":
                case "TransportTrolleyBus":
                case "TransportHelicopter":
                case "TransportIntercityBus":
                case "WasteTransfer":
                    if (!AdvancedVehicleOptions.hasSunsetHarborDLC)
                        return false;
                    break;

                case "CargoFerry" when !AdvancedVehicleOptions.hasMassTransitDLC || ModUtils.GetEnabledAssembly("AdditionalFreightTransporters") == null:
                    return false;

                case "CargoHelicopter" when !AdvancedVehicleOptions.hasSunsetHarborDLC || ModUtils.GetEnabledAssembly("AdditionalFreightTransporters") == null:
                    return false;
            }
            return true;
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
            label.text = Translations.Translate("AVO_MOD_MP61");

            m_category = UIUtils.CreateDropDown(this);
            m_category.width = 175;

            var additionalFreightTransporters = ModUtils.GetEnabledAssembly("AdditionalFreightTransporters");

            for (int i = 0; i < categoryList.Length; i++)
            {
                if (!CategoryAvailability(categoryList[i]))
                {
                    continue;
                }
                string name = "";
                if (i < 10)
                {
                    name = "AVO_MOD_CAT0" + i;
                }
                else
                {
                    name = "AVO_MOD_CAT" + i;
                }
                if(i == 0)
                {
                    englishCategory.Add("All");
                }
                else
                {
                    englishCategory.Add(categoryList[i]);
                }
                m_category.AddItem(Translations.Translate(name));
            }
                
            m_category.selectedIndex = 0;
            m_category.tooltip = Translations.Translate("AVO_MOD_MP62");
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
            m_search.tooltip = Translations.Translate("AVO_MOD_MP45");
            m_search.relativePosition = new Vector3(WIDTHLEFT - m_search.width, offset);

            m_search.eventTextChanged += (c, t) => PopulateList();

            label = AddUIComponent<UILabel>();
            label.textScale = 0.8f;
            label.padding = new RectOffset(0, 0, 8, 0);
            label.relativePosition = m_search.relativePosition - new Vector3(60f, 0f);
            label.text = Translations.Translate("AVO_MOD_MP60");

            // FastList
            m_fastList = UIFastList.Create<UIVehicleItem>(this);
            m_fastList.backgroundSprite = "UnlockingPanel";
            m_fastList.width = WIDTHLEFT - 5;
            m_fastList.height = height - offset - 110;
            m_fastList.canSelect = true;
            m_fastList.relativePosition = new Vector3(5, offset + 35);

            // Configuration file buttons
            UILabel configLabel = this.AddUIComponent<UILabel>();
            configLabel.text = Translations.Translate("AVO_MOD_MP46");
            configLabel.textScale = 0.8f;
            configLabel.relativePosition = new Vector3(16, height - 65);

            m_import = UIUtils.CreateButton(this);
            m_import.text = Translations.Translate("AVO_MOD_MP47");
            m_import.width = 80;
            m_import.tooltip = Translations.Translate("AVO_MOD_MP48");
            m_import.relativePosition = new Vector3(10, height - 45);

            m_export = UIUtils.CreateButton(this);
            m_export.text = Translations.Translate("AVO_MOD_MP49");
            m_export.width = 80;
            m_export.tooltip = Translations.Translate("AVO_MOD_MP50");
            m_export.relativePosition = new Vector3(95, height - 45);

            m_resetall = UIUtils.CreateButton(this);
            m_resetall.text = Translations.Translate("AVO_MOD_MP51");
            m_resetall.width = 80;
            m_resetall.tooltip = Translations.Translate("AVO_MOD_MP52");
            m_resetall.relativePosition = new Vector3(180, height - 45);

            m_autosave = AddUIComponent<UILabel>();
            m_autosave.textScale = 0.6f;
            m_autosave.text = Translations.Translate("AVO_MOD_MP53");
            m_autosave.relativePosition = new Vector3(275, height - 40);
            m_autosave.autoSize = true;
            m_autosave.textAlignment = UIHorizontalAlignment.Center;
            m_autosave.textColor = Color.green;
            m_autosave.tooltip = Translations.Translate("AVO_MOD_MP54");
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
                m_followVehicle.tooltip = Translations.Translate("AVO_MOD_MP55");
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
                m_removeVehicle.tooltip = Translations.Translate("AVO_MOD_MP56");
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
                // Same window style as the vehicle class warning; the file path is shown so it can be copied
                UIConfirmModal.ShowQuestion(Translations.Translate("AVO_MOD_MP47"),
                    Translations.Translate("AVO_MOD_MP63") + "\n\n" + AdvancedVehicleOptions.m_VehicleSettingsFile, yes =>
                {
                    if (!yes)
                        return;

                    DefaultOptions.RestoreAll();
                    AdvancedVehicleOptions.ImportVehicleDataConfig();
                    optionList = AdvancedVehicleOptions.config.options;
                });
            };

            m_export.eventClick += (c, t) => AdvancedVehicleOptions.ExportVehicleDataConfig(true);

            m_resetall.eventClick += (c, t) =>
            {
                UIConfirmModal.ShowQuestion(Translations.Translate("AVO_MOD_MP57"), Translations.Translate("AVO_MOD_MP58"), yes =>
                {
                    if (!yes)
                        return;

                    DefaultOptions.RestoreAll();
                    AdvancedVehicleOptions.ResetVehicleDataConfig();
                    optionList = AdvancedVehicleOptions.config.options;

                    UIConfirmModal.ShowMessage("Advanced Vehicle Options", Translations.Translate("AVO_MOD_MP59"), showCopy: false);
                }, showCopy: false);

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
                    (m_category.selectedIndex == 0 || m_optionsList[i].category.ToString() == englishCategory[m_category.selectedIndex]) &&
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
