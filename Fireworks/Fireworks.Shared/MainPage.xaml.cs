using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Effects;
using System.Diagnostics;

// Code-behind shared by Windows and Windows Phone projects.
// Project-specific code is found in MainPage[foo].xaml.cs.

namespace Fireworks
{
    static partial class Constants
    {
        public const int NumPerFrame = 6;
        public const float RadiusMax = 2.0f;
        public const float RadiusMin = 2.0f;
        // Randomization factor - how many DIPs can we deviate from pointer position when creating the firework.
        public const float PosRndMax = 0.0f;
        // Randomization factor - how many DIPs/sec can we deviate from pointer velocity when creating the firework.
        public const float VelRndMax = 600.0f;
        // Multiply this by the pointer velocity to get base firework velocity.
        public const float PointerVelCoeff = 0.5f;
    }

    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private Point m_pointerPos;
        private Point m_pointerPrevPos;

        private Random m_rnd;
        private bool m_wasPointerPressed;
        private Size m_currentSize;
        private FireworksController m_controller;
        private Stopwatch m_stopwatch;

        public MainPage()
        {
            this.InitializeComponent();

#if WINDOWS_PHONE_APP
            this.NavigationCacheMode = NavigationCacheMode.Required;
#endif

            m_rnd = new Random();
            m_controller = new FireworksController();
            m_stopwatch = new Stopwatch();
            m_stopwatch.Start();

            MainCanvas.CreateResources += MainCanvas_CreateResources;
            MainCanvas.Draw += MainCanvas_Draw;
            MainCanvas.PointerMoved += MainCanvas_PointerMoved;
            MainCanvas.PointerPressed += MainCanvas_PointerPressed;
            MainCanvas.PointerReleased += MainCanvas_PointerReleased;
            MainCanvas.SizeChanged += MainCanvas_SizeChanged;
        }

        void MainCanvas_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            m_pointerPos = e.GetCurrentPoint((UIElement)sender).Position;
        }

        void MainCanvas_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            m_wasPointerPressed = false;
        }

        void MainCanvas_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            // We need to get the pointer position here as well in case the user
            // tapped without having ever moved the pointer (e.g. mouse click without mouse move).
            m_pointerPos = e.GetCurrentPoint((UIElement)sender).Position;
            m_wasPointerPressed = true;
        }

        void MainCanvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            CreateSizeDependentResources(e.NewSize);
        }

        void CreateSizeDependentResources(Size NewSize)
        {
            m_currentSize = NewSize;
        }

        byte RndByte(byte min, byte max)
        {
            return (byte)(m_rnd.Next(min, max));
        }

        float RndFloat(float min, float max)
        {
            return (float)m_rnd.NextDouble() * (max - min) + min;
        }

        void MainCanvas_Draw(CanvasControl sender, CanvasDrawEventArgs args)
        {
            var ds = args.DrawingSession;
            ds.Clear(Colors.Black);

            float dt = m_stopwatch.ElapsedMilliseconds / 1000.0f;
            m_stopwatch.Restart();

            // We are abusing the Draw call as our "game loop" = create new fireworks here
            // independent of the timing of input events.
            if (m_wasPointerPressed == true)
            {
                for (int i = 0; i < Constants.NumPerFrame; i++)
                {
                    var firework = new Firework(
                        (float)m_pointerPos.X + RndFloat(-Constants.PosRndMax, Constants.PosRndMax),
                        (float)m_pointerPos.Y + RndFloat(-Constants.PosRndMax, Constants.PosRndMax),
                        (float)(m_pointerPos.X - m_pointerPrevPos.X) / dt * Constants.PointerVelCoeff + RndFloat(-Constants.VelRndMax, Constants.VelRndMax),
                        (float)(m_pointerPos.Y - m_pointerPrevPos.Y) / dt * Constants.PointerVelCoeff + RndFloat(-Constants.VelRndMax, Constants.VelRndMax),
                        RndFloat(Constants.RadiusMin, Constants.RadiusMax),
                        Color.FromArgb(
                            255,
                            RndByte(0, 255),
                            RndByte(0, 255),
                            RndByte(0, 255)
                        ));

                    m_controller.AddFirework(firework);
                }
            }

            m_controller.UpdateFireworks(dt);
            m_controller.RenderFireworks(ds);

            // We snap the pointer position with each Draw call, independent of the frequency of input events.
            m_pointerPrevPos = m_pointerPos;

            // Render loop.
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
