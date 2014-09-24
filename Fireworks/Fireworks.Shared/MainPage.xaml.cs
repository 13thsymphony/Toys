using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Effects;

// Code-behind shared by Windows and Windows Phone projects.
// Project-specific code is found in MainPage[foo].xaml.cs.

namespace Fireworks
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private Point m_pointerPos;
        private CanvasRenderTarget[] m_targets; // TODO: We can't copy from a DrawingSession so we need two targets.
        private CanvasRenderTarget m_fadeToBlack; // This is a semitransparent black surface, blend this with a bitmap to darken it.
        private Random m_rnd;

        private bool m_drawNewFirework;
        private bool m_areResourcesReady;
        private int m_targetIndex;

        private const byte FADE_ALPHA = 0; // Controls how quickly the screenbuffer fades to black (0 to 255, lower is slower).
        private const float FIREWORK_RADIUS = 5.0f;
        private const float FIREWORK_OFFSET_DIPS = 20.0f; // "Radius" of how many DIPs the firework can be offset.
        private const float BLUR_STDDEV = 2.0f;

        private Size m_currentSize;

        public MainPage()
        {
            this.InitializeComponent();

#if WINDOWS_PHONE_APP
            this.NavigationCacheMode = NavigationCacheMode.Required;
#endif

            m_rnd = new Random();

            MainCanvas.CreateResources += MainCanvas_CreateResources;
            MainCanvas.Draw += MainCanvas_Draw;
            MainCanvas.PointerMoved += MainCanvas_PointerMoved;
            MainCanvas.SizeChanged += MainCanvas_SizeChanged;
        }

        void MainCanvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            CreateSizeDependentResources(e.NewSize);
        }

        void CreateSizeDependentResources(Size NewSize)
        {
            m_targets = new CanvasRenderTarget[]
            {
                new CanvasRenderTarget(MainCanvas, NewSize),
                new CanvasRenderTarget(MainCanvas, NewSize)
            };

            m_fadeToBlack = new CanvasRenderTarget(MainCanvas, NewSize);
            using (var ds = m_fadeToBlack.CreateDrawingSession())
            {
                ds.Clear(Windows.UI.Color.FromArgb(FADE_ALPHA, 0, 0, 0));
            }

            m_currentSize = NewSize;
            m_areResourcesReady = true;
        }

        void MainCanvas_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            m_pointerPos = e.GetCurrentPoint((UIElement)sender).Position;

            m_drawNewFirework = e.GetCurrentPoint((UIElement)sender).IsInContact;
        }

        byte GetClampedRandomByte(byte min, byte max)
        {
            return (byte)(m_rnd.Next(min, max));
        }

        float GetClampedRandomFloat(float min, float max)
        {
            return (float)m_rnd.NextDouble() * (max - min) + min;
        }

        void MainCanvas_Draw(CanvasControl sender, CanvasDrawEventArgs args)
        {
            var ds = args.DrawingSession;

            using (var rtds = m_targets[m_targetIndex].CreateDrawingSession())
            {
                // Emulate a fade-to-black effect by blurring the previous render target
                // and blending with semitransparent black.
                var blend = new BlendEffect();
                var blur = new GaussianBlurEffect();
                blend.Background = m_fadeToBlack;
                blend.Foreground = m_targets[1 - m_targetIndex];
                blur.Source = blend;
                blur.StandardDeviation = BLUR_STDDEV;
                rtds.DrawImage(blur);

                if (m_drawNewFirework == true)
                {
                    var brush = new CanvasSolidColorBrush(sender, Windows.UI.Color.FromArgb(
                        255,
                        GetClampedRandomByte(0, 255),
                        GetClampedRandomByte(0, 255),
                        GetClampedRandomByte(0, 255)
                        ));

                    rtds.FillCircle(
                        (float)m_pointerPos.X + GetClampedRandomFloat(-FIREWORK_OFFSET_DIPS, FIREWORK_OFFSET_DIPS),
                        (float)m_pointerPos.Y + GetClampedRandomFloat(-FIREWORK_OFFSET_DIPS, FIREWORK_OFFSET_DIPS),
                        GetClampedRandomFloat(0.0f, FIREWORK_RADIUS),
                        brush
                        );
                }
            }

            ds.DrawImage(m_targets[m_targetIndex]);

            // Transition state machine
            if (m_targetIndex == 0)
            {
                m_targetIndex = 1;
            }
            else if (m_targetIndex == 1)
            {
                m_targetIndex = 0;
            }
            else // error state
            {
                throw new ArgumentOutOfRangeException(m_targetIndex.ToString());
            }

            // Render loop
            sender.Invalidate();
        }

        void MainCanvas_CreateResources(CanvasControl sender, object args)
        {
            // TODO: Why do we need to have CanvasControl sender, can we just refer to MainCanvas?

            // TODO: Why is this workaround necessary?
            // The first time this is called, the control hasn't yet been sized properly.
            // Don't create size-dependent resources yet.
            if ((m_currentSize.Height != 0) && (m_currentSize.Width != 0))
            {
                CreateSizeDependentResources(m_currentSize);
            }
        }
    }
}
