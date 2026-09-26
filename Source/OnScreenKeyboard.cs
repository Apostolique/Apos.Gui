using System.Threading.Tasks;
using Microsoft.Xna.Framework.Input;

namespace Apos.Gui {
    /// <summary>
    /// Types into a textbox when there's no physical keyboard, like on phones and consoles.
    /// Set <see cref="GuiHelper.OnScreenKeyboard"/> to turn it on. A textbox opens it when it's tapped, or when
    /// <see cref="Bindings.OpenKeyboard"/> is pressed while it has focus.
    /// </summary>
    public interface IOnScreenKeyboard {
        /// <summary>Opens the keyboard for a textbox. The textbox waits for the task and puts the text in on the game's thread.</summary>
        /// <returns>The text once the player is done, or null when they cancel.</returns>
        Task<string?> Show(Textbox textbox);
    }

    /// <summary>
    /// The platform's own keyboard, through KeyboardInput. It pops up over the game on phones and consoles.
    /// Desktop platforms don't have one.
    /// </summary>
    public class SystemKeyboard : IOnScreenKeyboard {
        /// <summary>Shown at the top of the keyboard's dialog.</summary>
        public string Title { get; set; } = "";
        /// <summary>Shown under the title.</summary>
        public string Description { get; set; } = "";

        public async Task<string?> Show(Textbox textbox) {
            return await KeyboardInput.Show(Title, Description, textbox.Text);
        }
    }
}
