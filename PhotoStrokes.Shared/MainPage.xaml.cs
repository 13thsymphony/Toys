using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage.Pickers;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

using Microsoft.Graphics.Canvas;
using Windows.Graphics.Imaging;
using Windows.UI;

// Code-behind shared by Windows and Windows Phone projects.
// Project-specific code is found in MainPage[foo].xaml.cs.

namespace PhotoStrokes
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private float m_strokeLength = 12.0f;
        private float m_strokeWidth = 4.0f;
        private int m_numStrokes = 100;

        private bool m_isResourceLoadingDone;
        private CanvasBitmap m_sourceBitmap;
        private CanvasRenderTarget m_targetBitmap;
        private byte[] m_pixelArray;
        private uint m_pixelArrayWidth;
        private uint m_pixelArrayHeight;
        private Random m_rnd;

        public MainPage()
        {
            this.InitializeComponent();

#if WINDOWS_PHONE_APP
            this.NavigationCacheMode = NavigationCacheMode.Required;
#endif

            mainCanvas.CreateResources += mainCanvas_CreateResources;
            mainCanvas.Draw += mainCanvas_Draw;
        }

        async void mainCanvas_CreateResources(Microsoft.Graphics.Canvas.CanvasControl sender, object args)
        {
            m_isResourceLoadingDone = false;

            // WIN2D: resource loading from WinRT types including StorageFile and IRAS
            m_sourceBitmap = await CanvasBitmap.LoadAsync(sender, "Chrysanthemum.jpg");
            var sourceFile = await Package.Current.InstalledLocation.GetFileAsync("Chrysanthemum.jpg");

            // Win2D: because we can't lock/read pixels we rely on BitmapDecoder
            var stream = await sourceFile.OpenReadAsync();
            var decoder = await BitmapDecoder.CreateAsync(stream);

            // Technically these should always be identical to m_sourceBitmap.SizeInPixels;
            m_pixelArrayHeight = decoder.PixelHeight;
            m_pixelArrayWidth = decoder.PixelWidth;
            var pixelProvider = await decoder.GetPixelDataAsync(
                BitmapPixelFormat.Bgra8,
                BitmapAlphaMode.Premultiplied,
                new BitmapTransform(),
                ExifOrientationMode.IgnoreExifOrientation, // Must do this.
                ColorManagementMode.ColorManageToSRgb
                );

            m_pixelArray = pixelProvider.DetachPixelData();

            m_targetBitmap = new CanvasRenderTarget(sender, new Size(m_pixelArrayWidth, m_pixelArrayHeight));

            m_rnd = new Random();

            m_isResourceLoadingDone = true;

            mainCanvas.Invalidate();
        }

        void mainCanvas_Draw(Microsoft.Graphics.Canvas.CanvasControl sender, Microsoft.Graphics.Canvas.CanvasDrawEventArgs args)
        {
            if (m_isResourceLoadingDone)
            {
                //args.DrawingSession.DrawImage(m_sourceBitmap);

                using (var ds = m_targetBitmap.CreateDrawingSession())
                {
                    // Draw a bunch of solid color strokes.
                    // - Centered at a random point
                    // - Color is determined by the bitmap value at that point
                    // - Fixed stroke length and width
                    // - Random angle
                    for (int i = 0; i < m_numStrokes; i++)
                    {
                        double centerXFactor = m_rnd.NextDouble();
                        double centerYFactor = m_rnd.NextDouble();
                        float centerX = (float)(centerXFactor * m_sourceBitmap.Size.Width);
                        float centerY = (float)(centerYFactor * m_sourceBitmap.Size.Height);
                        float angle = (float)(m_rnd.NextDouble() * 2 * Math.PI);
                        Color color = getColorFromBitmapCoordinates(centerXFactor, centerYFactor);

                        // Convert the stroke definition to bounding box for line.
                        float x1 = centerX + (float)Math.Cos(angle) * m_strokeLength / 2;
                        // Technically the coordinate systems don't match for Y, but it doesn't matter here.
                        float y1 = centerY + (float)Math.Sin(angle) * m_strokeLength / 2;
                        float x2 = 2 * centerX - x1;
                        float y2 = 2 * centerY - y1;

                        ds.DrawLine(x1, y1, x2, y2, color, m_strokeWidth);
                    }
                }

                args.DrawingSession.DrawImage(m_targetBitmap);
            }

            // Render looping.
            mainCanvas.Invalidate();
        }

        /// <summary>
        /// Gets the Color stored at specific coordinates. Reads from the pixel array obtained from BitmapDecoder.
        /// This is to workaround the fact that we can't read-back from a CanvasBitmap yet.
        /// </summary>
        /// <param name="x">Value from 0.0 to 1.0, multiply by PixelWidth to get the x coordinate.</param>
        /// <param name="y">Value from 0.0 to 1.0, multiply by PixelHeight to get the y coordinate.</param>
        /// <returns></returns>
        private Color getColorFromBitmapCoordinates(double xFactor, double yFactor)
        {
            uint x = (uint)(xFactor * m_pixelArrayWidth);
            uint y = (uint)(yFactor * m_pixelArrayHeight);
            uint offset = (y * m_pixelArrayWidth + x) * 4;

            // The pixel array is stored in BGRA channel order.
            return Color.FromArgb(m_pixelArray[offset + 3], m_pixelArray[offset + 2], m_pixelArray[offset + 1], m_pixelArray[offset]);
        }

        private void renderOneButton_Click(object sender, RoutedEventArgs e)
        {
            mainCanvas.Invalidate();
        }
    }
}
