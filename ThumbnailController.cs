using dogpixels_viewer.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Size = System.Drawing.Size;

namespace dogpixels_viewer
{
    internal class ThumbnailController
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private static readonly List<string> extensions = new List<string> { ".png", ".gif", ".jpg", ".jpeg", ".bmp", ".ai", ".ps", ".svg", ".tif", ".tiff", ".ico" };
        public static readonly BitmapImage Placeholder = new BitmapImage(new Uri(@"/Resources/nopreview.jpg", UriKind.Relative));
        public static readonly int ThumbSize = 256;

        [Flags]
        public enum SIIGBF
        {
            SIIGBF_RESIZETOFIT = 0x00000000,
            SIIGBF_BIGGERSIZEOK = 0x00000001,
            SIIGBF_MEMORYONLY = 0x00000002,
            SIIGBF_ICONONLY = 0x00000004,
            SIIGBF_THUMBNAILONLY = 0x00000008,
            SIIGBF_INCACHEONLY = 0x00000010,
            SIIGBF_CROPTOSQUARE = 0x00000020,
            SIIGBF_WIDETHUMBNAILS = 0x00000040,
            SIIGBF_ICONBACKGROUND = 0x00000080,
            SIIGBF_SCALEUP = 0x00000100,
        }

        [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
        private static extern int SHCreateItemFromParsingName(string path, IntPtr pbc, [MarshalAs(UnmanagedType.LPStruct)] Guid riid, out IShellItemImageFactory factory);

        [ComImport]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        [Guid("bcc18b79-ba16-442f-80c4-8a59c30c463b")]
        private interface IShellItemImageFactory
        {
            [PreserveSig]
            int GetImage(Size size, SIIGBF flags, out IntPtr phbm);

            //[PreserveSig]
            //int GetThumbnailImage(int thumbWidth, int thumbHeight, System.Drawing.Image.GetThumbnailImageAbort? callback, IntPtr callbackData);
        }

        private static BitmapImage ToBitmapImage(Bitmap bitmap)
        {
            using (var memory = new MemoryStream())
            {
                bitmap.Save(memory, ImageFormat.Png);
                memory.Position = 0;
                var bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                bitmapImage.Freeze();
                return bitmapImage;
            }
        }

        /// <summary>
        /// Retrieves a 256px^2 thumbnail for a file from Shell32 or a hardcoded placeholder.
        /// </summary>
        /// <param name="path"></param>
        /// <returns>A thumbnail or a placeholder usable as Image.Source.</returns>
        public static BitmapImage GetThumbnail(string path)
        {
            if (!extensions.Contains(Path.GetExtension(path)))
            {
                return Placeholder;
            }

            // code mostly based off https://stackoverflow.com/a/20698644/5410292

            IShellItemImageFactory factory;
            IntPtr bitmapPointer;
            int result;

            result = SHCreateItemFromParsingName(path, IntPtr.Zero, typeof(IShellItemImageFactory).GUID, out factory);

            if (result != 0)
            {
                log.Warn($"Failed to query Shell32 item for '{path}': {result}");
                return Placeholder;
            }

            result = factory.GetImage(new Size(ThumbSize, ThumbSize), SIIGBF.SIIGBF_THUMBNAILONLY, out bitmapPointer);

            if (result != 0)
            {
                log.Warn($"Failed to query Shell32 thumbnail for '{path}': {result}");
                return Placeholder;
            }

            if (bitmapPointer != IntPtr.Zero)
            {
                return ToBitmapImage(System.Drawing.Image.FromHbitmap(bitmapPointer));
            }

            return Placeholder;
        }
    }
}
