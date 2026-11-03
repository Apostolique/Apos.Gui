namespace Apos.Gui {
    /// <summary>
    /// Where textboxes copy to and paste from. Set <see cref="GuiHelper.Clipboard"/> to share text with other apps.
    /// MonoGame doesn't have a clipboard that works on every platform, so each game plugs in the one its platform has.
    /// </summary>
    public interface IClipboard {
        /// <summary>Gets the clipboard's text.</summary>
        /// <returns>The clipboard's text, or null when it doesn't hold any.</returns>
        string? GetText();
        /// <summary>Puts text in the clipboard.</summary>
        /// <param name="text">The text to copy.</param>
        void SetText(string text);
    }

    /// <summary>
    /// Keeps copied text inside the game. It works on every platform, though other apps can't see it.
    /// </summary>
    public class GameClipboard : IClipboard {
        /// <inheritdoc/>
        public string? GetText() => _text;
        /// <inheritdoc/>
        public void SetText(string text) => _text = text;

        private string? _text;
    }
}
