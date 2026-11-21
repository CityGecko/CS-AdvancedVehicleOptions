using System;
using ColossalFramework;
using ColossalFramework.UI;
using UnityEngine;

using UIUtils = SamsamTS.UIUtils;

namespace AdvancedVehicleOptionsUID.GUI
{
    /// <summary>
    /// Message window for longer texts (the game's ConfirmPanel has a fixed height and cuts them off):
    /// title, scrollable text in a smaller font, Yes/No or OK on the left and a "Copy text" button (clipboard) on the
    /// right. Escape counts as "No" (or OK). The question (Yes/No) is modal, the message (OK) is not, so the check on
    /// game load does not block the game or other mods.
    /// </summary>
    public class UIConfirmModal : UIPanel
    {
        private const float WIDTH = 640f;
        private const float TEXT_MAX_HEIGHT = 320f;
        private const float MARGIN = 12f;

        private string m_titleText;
        private string m_messageText;
        private bool m_yesNo;
        private bool m_showCopy;
        private bool m_modal;
        private Action<bool> m_callback;
        private bool m_closed;

        private UIButton m_copy;

        /// <summary>Shows a modal question with Yes/No. The callback gets true for "Yes".</summary>
        public static void ShowQuestion(string title, string message, Action<bool> callback, bool showCopy = true)
        {
            Create(title, message, true, showCopy, true, callback);
        }

        /// <summary>Shows a message with OK - by default not modal (information only, blocks nothing).</summary>
        public static void ShowMessage(string title, string message, bool showCopy = true, bool modal = false)
        {
            Create(title, message, false, showCopy, modal, null);
        }

        private static void Create(string title, string message, bool yesNo, bool showCopy, bool modal, Action<bool> callback)
        {
            UIConfirmModal window = UIView.GetAView().AddUIComponent(typeof(UIConfirmModal)) as UIConfirmModal;
            window.m_titleText = title;
            window.m_messageText = message;
            window.m_yesNo = yesNo;
            window.m_showCopy = showCopy;
            window.m_modal = modal;
            window.m_callback = callback;
        }

        public override void Start()
        {
            base.Start();

            backgroundSprite = "UnlockingPanel2";
            canFocus = true;
            isInteractive = true;
            clipChildren = true;
            width = WIDTH;

            // Title
            UILabel title = AddUIComponent<UILabel>();
            title.text = m_titleText;
            title.textScale = 1.1f;
            title.relativePosition = new Vector3(MARGIN, MARGIN);

            // Scrollable text
            UIScrollablePanel scroll = AddUIComponent<UIScrollablePanel>();
            scroll.relativePosition = new Vector3(MARGIN, 50f);
            scroll.width = WIDTH - 2 * MARGIN - 24f;
            scroll.clipChildren = true;
            scroll.scrollWheelDirection = UIOrientation.Vertical;
            scroll.builtinKeyNavigation = true;

            UILabel text = scroll.AddUIComponent<UILabel>();
            text.wordWrap = true;
            text.autoSize = false;
            text.width = scroll.width - 4f;
            text.autoHeight = true;
            text.textScale = 0.85f;
            text.text = m_messageText;
            text.relativePosition = Vector3.zero;

            scroll.height = Mathf.Min(text.height + 4f, TEXT_MAX_HEIGHT);

            if (text.height + 4f > TEXT_MAX_HEIGHT)
            {
                UIScrollbar scrollbar = AddUIComponent<UIScrollbar>();
                scrollbar.width = 20f;
                scrollbar.height = scroll.height;
                scrollbar.orientation = UIOrientation.Vertical;
                scrollbar.relativePosition = new Vector3(WIDTH - MARGIN - 20f, 50f);
                scrollbar.incrementAmount = 30f;

                UISlicedSprite track = scrollbar.AddUIComponent<UISlicedSprite>();
                track.relativePosition = Vector2.zero;
                track.autoSize = true;
                track.size = scrollbar.size;
                track.fillDirection = UIFillDirection.Vertical;
                track.spriteName = "ScrollbarTrack";
                scrollbar.trackObject = track;

                UISlicedSprite thumb = track.AddUIComponent<UISlicedSprite>();
                thumb.relativePosition = Vector2.zero;
                thumb.fillDirection = UIFillDirection.Vertical;
                thumb.autoSize = true;
                thumb.width = track.width - 8f;
                thumb.spriteName = "ScrollbarThumb";
                scrollbar.thumbObject = thumb;

                scroll.verticalScrollbar = scrollbar;
            }

            // Buttons: answer buttons on the left, "Copy text" on the right
            float buttonsY = 50f + scroll.height + MARGIN;

            if (m_yesNo)
            {
                UIButton yes = UIUtils.CreateButton(this);
                yes.text = Translations.Translate("AVO_MOD_OP59");
                yes.relativePosition = new Vector3(MARGIN, buttonsY);
                yes.eventClick += (c, p) => Close(true);

                UIButton no = UIUtils.CreateButton(this);
                no.text = Translations.Translate("AVO_MOD_OP60");
                no.relativePosition = new Vector3(yes.relativePosition.x + yes.width + 10f, buttonsY);
                no.eventClick += (c, p) => Close(false);
            }
            else
            {
                UIButton ok = UIUtils.CreateButton(this);
                ok.text = "OK";
                ok.relativePosition = new Vector3(MARGIN, buttonsY);
                ok.eventClick += (c, p) => Close(true);
            }

            if (m_showCopy)
            {
                m_copy = UIUtils.CreateButton(this);
                m_copy.width = 200f;
                m_copy.text = Translations.Translate("AVO_MOD_OP57");
                m_copy.relativePosition = new Vector3(WIDTH - MARGIN - m_copy.width, buttonsY);
                m_copy.eventClick += (c, p) =>
                {
                    GUIUtility.systemCopyBuffer = m_titleText + "\n\n" + m_messageText;
                    m_copy.text = Translations.Translate("AVO_MOD_OP58");
                };
            }

            height = buttonsY + 30f + MARGIN;
            relativePosition = new Vector3(Mathf.Floor((GetUIView().fixedWidth - width) / 2), Mathf.Floor((GetUIView().fixedHeight - height) / 2));

            // Questions are modal (they must be answered); information messages are not by default, so they do not
            // block the game or other mods' windows.
            if (m_modal) ShowModal();
            else
            {
                Show(true);
                BringToFront();
                Focus();
            }
        }

        protected override void OnKeyDown(UIKeyEventParameter p)
        {
            if (p.keycode == KeyCode.Escape)
            {
                p.Use();
                Close(false);
                return;
            }
            base.OnKeyDown(p);
        }

        private void ShowModal()
        {
            UIView.PushModal(this);
            Show(true);
            Focus();

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

        private void Close(bool result)
        {
            if (m_closed) return;
            m_closed = true;

            bool wasModal = UIView.GetModalComponent() == this;
            if (wasModal) UIView.PopModal();

            if (wasModal && UIView.GetAView().panelsLibraryModalEffect is UIComponent modalEffect)
            {
                if (!UIView.HasModalInput())
                {
                    ValueAnimator.Cancel("ModalEffect67419");
                    ValueAnimator.Animate("ModalEffect67419", val => modalEffect.opacity = val, new AnimatedFloat(1f, 0f, 0.7f, EasingType.CubicEaseOut), () => modalEffect.Hide());
                }
                else
                {
                    modalEffect.zOrder = UIView.GetModalComponent().zOrder - 1;
                }
            }

            Hide();

            try
            {
                m_callback?.Invoke(result);
            }
            catch (Exception e)
            {
                Logging.LogException(e, "exception in confirm dialog callback");
            }

            Destroy(gameObject);
        }
    }
}
