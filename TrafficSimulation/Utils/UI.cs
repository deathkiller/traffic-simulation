using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;

namespace TrafficSimulation.Utils
{
    /// <summary>
    /// Contains helper methods for UI
    /// </summary>
    public class UI
    {
        /// <summary>
        /// Enables close button of specified window
        /// </summary>
        /// <param name="window">Window</param>
        /// <param name="enabled">True to enable; otherwise disable</param>
        public static void EnableCloseBox(IWin32Window window, bool enabled)
        {
            IntPtr hWnd = window.Handle;
            IntPtr hMenu = GetSystemMenu(hWnd, false);
            if (hMenu == IntPtr.Zero) {
                throw new InvalidOperationException("The Window must be shown before changing menu");
            }

            EnableMenuItem(hMenu, 0xf060 /*SC_CLOSE*/, (uint)(enabled ? 0 : 1));
            if (!DrawMenuBar(hWnd)) {
                throw new InvalidOperationException("The Close menu does not exist");
            }

            GC.KeepAlive(window);
        }

        /// <summary>
        /// Gets system font used as main instruction font in some dialogs
        /// </summary>
        /// <returns>Main instruction font</returns>
        public static Font GetMainInstructionFont()
        {
            try {
                VisualStyleRenderer renderer = new VisualStyleRenderer("TEXTSTYLE", 0x1, 0x0);

                LOGFONT lFont;
                if (GetThemeFont(renderer.Handle, IntPtr.Zero, 0x1 /*TEXT_MAININSTRUCTION*/, 0, 0xD2 /*TMT_FONT*/, out lFont) != 0)
                    throw new InvalidOperationException(); // Fallback to SystemFonts.CaptionFont

                return new Font(lFont.lfFaceName, Math.Abs(lFont.lfHeight), FontStyle.Regular, GraphicsUnit.Pixel);
            } catch {
                return SystemFonts.CaptionFont;
            }
        }

        /// <summary>
        /// Paints gradient used in some dialogs
        /// </summary>
        /// <param name="g">Graphics</param>
        /// <param name="x">X</param>
        /// <param name="y">Y</param>
        /// <param name="width">Width</param>
        /// <param name="height">Height</param>
        /// <param name="gradientSize">Gradient size</param>
        /// <param name="alpha">Alpha</param>
        public static void PaintDirtGradient(Graphics g, int x, int y, int width, int height, int gradientSize, int alpha)
        {
            Region oldClip = g.Clip;
            g.SetClip(new Rectangle(x, y, width, height), CombineMode.Intersect);

            PixelOffsetMode oldPixelOffsetMode = g.PixelOffsetMode;
            g.PixelOffsetMode = PixelOffsetMode.Half;

            SmoothingMode oldSmoothingMode = g.SmoothingMode;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            for (int i = 0; i < gradientSize; i += 3) {
                double curve = (Math.Sin((1.5 + ((double)i / gradientSize)) * Math.PI) + 1) / 2;
                using (Pen pen = new Pen(Color.FromArgb((int)((1 - curve) * 60) * alpha / 255, 0x55, 0x44, 0x44))) {
                    g.DrawLine(pen, width, i, width - gradientSize, i - gradientSize);
                }
            }

            g.Clip = oldClip;
            g.PixelOffsetMode = oldPixelOffsetMode;
            g.SmoothingMode = oldSmoothingMode;
        }

        #region Native Methods
        [DllImport("user32")]
        private static extern IntPtr GetSystemMenu(IntPtr hWnd, [MarshalAs(UnmanagedType.Bool)] bool bRevert);
        [DllImport("user32")]
        private static extern int EnableMenuItem(IntPtr hMenu, uint uIDEnableItem, uint uEnable);
        [DllImport("user32", SetLastError = true)]
        private static extern bool DrawMenuBar(IntPtr hWnd);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        private struct LOGFONT
        {
            public const int LF_FACESIZE = 32;

            public int lfHeight;
            public int lfWidth;
            public int lfEscapement;
            public int lfOrientation;
            public int lfWeight;
            public byte lfItalic;
            public byte lfUnderline;
            public byte lfStrikeOut;
            public byte lfCharSet;
            public byte lfOutPrecision;
            public byte lfClipPrecision;
            public byte lfQuality;
            public byte lfPitchAndFamily;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = LF_FACESIZE)]
            public string lfFaceName;
        }

        [DllImport("uxtheme", ExactSpelling = true, CharSet = CharSet.Unicode)]
        private static extern int GetThemeFont(IntPtr hTheme, IntPtr hdc, int iPartId, int iStateId, int iPropId, out LOGFONT pFont);
        #endregion
    }
}