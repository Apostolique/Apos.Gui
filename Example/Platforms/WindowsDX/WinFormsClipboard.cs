#nullable enable
using System.Windows.Forms;
using Apos.Gui;

namespace GameProject {
    /// <summary>
    /// Shares text with other apps through WinForms, which WindowsDX already runs on. It needs Main to be [STAThread].
    /// </summary>
    public class WinFormsClipboard : IClipboard {
        public string? GetText() => Clipboard.ContainsText() ? Clipboard.GetText() : null;
        public void SetText(string text) => Clipboard.SetText(text);
    }
}
