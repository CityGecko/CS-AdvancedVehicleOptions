using ColossalFramework;
using ColossalFramework.UI;
using UnityEngine;

using UIUtils = SamsamTS.UIUtils;

namespace AdvancedVehicleOptionsUID.GUI
{
    public class UIWarningModal : UIPanel
    {
        private UITitleBar m_title;
        private UISprite m_warningIcon;
        private UILabel m_messageLabel;
        private UIButton m_ok;

        private string m_message;

        private static UIWarningModal _instance;

        private static UIWarningModal instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = UIView.GetAView().AddUIComponent(typeof(UIWarningModal)) as UIWarningModal;
                }
                return _instance;
            }
        }

        public static string message
        {
            get { return instance.m_message; }
            set
            {
                var warning = instance;

                warning.m_message = value;
                if(warning.m_messageLabel != null)
                {
                    warning.m_messageLabel.text = warning.m_message;
                    warning.m_messageLabel.autoHeight = true;
                    warning.m_messageLabel.width = warning.width - warning.m_warningIcon.width - 15;

                    warning.height = 200;

                    if ((warning.m_title.height + 40 + warning.m_messageLabel.height) > (warning.height - 40))
                    {
                        warning.height = warning.m_title.height + 40 + warning.m_messageLabel.height;
                    }

                    warning.m_warningIcon.relativePosition = new Vector3(5, warning.m_title.height + (warning.height - warning.m_title.height - 40 - warning.m_warningIcon.height) / 2);
                    warning.m_ok.relativePosition = new Vector3((warning.width - warning.m_ok.width) / 2, warning.height - warning.m_ok.height - 5);
                    warning.m_messageLabel.relativePosition = new Vector3(warning.m_warningIcon.width + 10, warning.m_title.height + (warning.height - warning.m_title.height - 40 - warning.m_messageLabel.height) / 2);
                }
            }
        }

        public override void Start()
        {
            base.Start();

            backgroundSprite = "UnlockingPanel2";
            isVisible = false;
            canFocus = true;
            isInteractive = true;
            clipChildren = true;
            width = 600;
            height = 200;
            relativePosition = new Vector3(Mathf.Floor((GetUIView().fixedWidth - width) / 2), Mathf.Floor((GetUIView().fixedHeight - height) / 2));

            // Title Bar
            m_title = AddUIComponent<UITitleBar>();
            m_title.title = "Advanced Vehicle Options - " + Translations.Translate("AVO_MOD_WM01");
            m_title.iconSprite = "IconCitizenVehicle";
            m_title.isModal = true;

            // Icon
            m_warningIcon = AddUIComponent<UISprite>();
            m_warningIcon.size = new Vector2(90, 90);
            m_warningIcon.spriteName = "IconWarning";

            // Message
            m_messageLabel = AddUIComponent<UILabel>();
            m_messageLabel.wordWrap = true;

            // Ok
            m_ok = UIUtils.CreateButton(this);
            m_ok.text = "OK";

            m_ok.eventClick += (c, p) =>
            {
                HideWarning();
            };

            message = m_message;

            isVisible = true;
        }

        protected override void OnKeyDown(UIKeyEventParameter p)
        {
            if (Input.GetKey(KeyCode.Escape) || Input.GetKey(KeyCode.Return))
            {
                p.Use();
                HideWarning();
            }

            base.OnKeyDown(p);
        }

        public static void ShowWarning()
        {
            var warning = instance;

            UIView.PushModal(warning);
            warning.Show(true);
            warning.Focus();

            if (UIView.GetAView().panelsLibraryModalEffect is UIComponent modalEffect)
            {
                modalEffect.FitTo(null);
                if (!modalEffect.isVisible || modalEffect.opacity != 1f)
                {
                    modalEffect.Show(false);
                    ValueAnimator.Cancel("ModalEffect67419");
                    ValueAnimator.Animate("ModalEffect67419", val => modalEffect.opacity = val, new AnimatedFloat(0f, 1f, 0.7f, EasingType.CubicEaseOut));
                }
            }
        }
        public static void HideWarning()
        {
            var warning = instance;
            if (warning == null || UIView.GetModalComponent() != warning)
                return;

            UIView.PopModal();

            if (UIView.GetAView().panelsLibraryModalEffect is UIComponent modalEffect)
            {
                if (!UIView.HasModalInput())
                {
                    ValueAnimator.Cancel("ModalEffect67419");
                    ValueAnimator.Animate("ModalEffect67419", val => modalEffect.opacity = val, new AnimatedFloat(1f, 0f, 0.7f, EasingType.CubicEaseOut), () => modalEffect.Hide());
                }
                else
                    modalEffect.zOrder = UIView.GetModalComponent().zOrder - 1;
            }

            warning.Hide();
        }
    }
}
