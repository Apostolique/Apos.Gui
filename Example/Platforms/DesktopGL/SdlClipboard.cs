#nullable enable
using System;
using System.Reflection;
using System.Runtime.InteropServices;
using Apos.Gui;

namespace GameProject {
    /// <summary>
    /// Shares text with other apps through SDL, which DesktopGL already runs on.
    /// </summary>
    public class SdlClipboard : IClipboard {
        static SdlClipboard() {
            // SDL's file name is different on each OS.
            NativeLibrary.SetDllImportResolver(typeof(SdlClipboard).Assembly, (name, assembly, path) => {
                if (name != "SDL2") return IntPtr.Zero;
                foreach (string candidate in new[] { "SDL2", "libSDL2-2.0.so.0", "libSDL2-2.0.0.dylib" }) {
                    if (NativeLibrary.TryLoad(candidate, assembly, path, out IntPtr handle)) return handle;
                }
                return IntPtr.Zero;
            });
        }

        public string? GetText() {
            if (SDL_HasClipboardText() == 0) return null;

            IntPtr text = SDL_GetClipboardText();
            try {
                return Marshal.PtrToStringUTF8(text);
            } finally {
                SDL_free(text);
            }
        }
        public void SetText(string text) {
            SDL_SetClipboardText(text);
        }

        [DllImport("SDL2", CallingConvention = CallingConvention.Cdecl)]
        private static extern int SDL_HasClipboardText();
        [DllImport("SDL2", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr SDL_GetClipboardText();
        [DllImport("SDL2", CallingConvention = CallingConvention.Cdecl)]
        private static extern int SDL_SetClipboardText([MarshalAs(UnmanagedType.LPUTF8Str)] string text);
        [DllImport("SDL2", CallingConvention = CallingConvention.Cdecl)]
        private static extern void SDL_free(IntPtr memory);
    }
}
