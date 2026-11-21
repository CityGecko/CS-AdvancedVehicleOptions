using System;
using ColossalFramework.UI;
using HarmonyLib;

namespace AdvancedVehicleOptionsUID.Patches
{
    /// <summary>
    /// The base game's MainToolbar.RefreshPanel() rebuilds all of its built-in tool tabs (Roads,
    /// Districts, Policies, ...) by walking the shared "MainToolstrip" tabstrip's child list
    /// purely by position (a private running counter, m_ObjectIndex), reusing whatever component
    /// already sits at each index and re-applying that tool's sprites/tooltip/wiring to it.
    ///
    /// AVO adds its own toolbar tab to that same shared list (GUI/UIMainPanel.cs), outside of
    /// that mechanism. If AVO's tab ends up sitting anywhere before the end of the list instead
    /// of strictly after all of the game's own tabs, every built-in tab positioned after it gets
    /// shifted onto the wrong slot and re-wired with the wrong tool's identity - including
    /// Policies, whose toggle buttons then stop working. This isn't a one-off: RefreshPanel() re-runs
    /// on every level load, every milestone unlock, and every locale change, so the misalignment
    /// can appear or reappear at any of those points, not just once at startup.
    ///
    /// Moving AVO's tab back to the very end of the list right after every RefreshPanel() call
    /// keeps it permanently out of the range the built-in sequence ever reaches, so the game's own
    /// tabs are always rebuilt against a clean, correctly-ordered list on the next pass.
    /// </summary>
    [HarmonyPatch(typeof(MainToolbar), "RefreshPanel")]
    public static class MainToolbarRefreshPanelPatch
    {
        [HarmonyPostfix]
        public static void Postfix()
        {
            try
            {
                UIButton button = AdvancedVehicleOptions.m_mainPanel?.m_button;

                // Only act once the tab is actually attached to the toolstrip; harmless no-op otherwise.
                if (button != null && button.parent != null)
                {
                    button.BringToFront();
                }
            }
            catch (Exception e)
            {
                Logging.LogException(e, "exception keeping AVO toolbar tab in place");
            }
        }
    }
}
