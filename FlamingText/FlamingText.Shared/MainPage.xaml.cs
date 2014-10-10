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
using Microsoft.Graphics.Canvas.Effects;
using System.Numerics;

// Code-behind shared by Windows and Windows Phone projects.
// Project-specific code is found in MainPage[foo].xaml.cs.

namespace FlamingText
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private bool m_AreResourcesLoaded;

        private Transform2DEffect m_flameAnimation;
        private CompositeEffect m_composite;
        private int m_flameOffset;

        public MainPage()
        {
            this.InitializeComponent();

#if WINDOWS_PHONE_APP
            this.NavigationCacheMode = NavigationCacheMode.Required;
#endif

            MainCanvas.CreateResources += MainCanvas_CreateResources;
            MainCanvas.Draw += MainCanvas_Draw;
        }

        void MainCanvas_Draw(CanvasControl sender, CanvasDrawEventArgs args)
        {
            if (m_AreResourcesLoaded)
            {
                using (var ds = args.DrawingSession)
                {
                    ds.DrawImage(m_composite);
                }
            }
        }

        async void MainCanvas_CreateResources(CanvasControl sender, object args)
        {
            m_AreResourcesLoaded = false;

            var bitmap = await CanvasBitmap.LoadAsync(sender, "HelloWorldWhite.png");

            // Dilate the text bitmap.
            var dilate = new MorphologyEffect();
            dilate.Source = bitmap;
            dilate.Mode = MorphologyEffectMode.Dilate;
            dilate.Width = 7;
            dilate.Height = 1;

            // Blur the dilated text.
            var blur = new GaussianBlurEffect();
            blur.Source = dilate;
            blur.BlurAmount = 3.0f;

            // Colorize the text, setting the interior orange and fade to red as the alpha increases.
            // TODO: THE EFFECT DOESN'T EXIST?!?!?
            var colorize = new GaussianBlurEffect(); // dummy thing for now
            colorize.Source = blur;
            //colorize.Matrix =     {
            //    0, 0, 0, 0,
            //    0, 0, 0, 0,
            //    0, 0, 0, 0,
            //    0, 1, 0, 1,
            //    1, -0.5, 0, 0
            //};

            // The turbulence effect is the source of the flame's movements.
            var turbulence = new TurbulenceEffect();
            turbulence.Frequency = new Vector2(0.109f, 0.109f);
            turbulence.Size = new Vector2(1000.0f, 80.0f); // TODO: This should be tuned

            // Repeat the turbulence with the border effect.
            // TODO: do we really need this? can't we just tile in the Turbulence effect?
            var border = new BorderEffect();
            border.Source = turbulence;
            border.ExtendX = CanvasEdgeBehavior.Mirror;
            border.ExtendY = CanvasEdgeBehavior.Mirror;

            // The 2D affine transform animates the flame by shifting the turbulence upward.
            m_flameAnimation = new Transform2DEffect();
            m_flameAnimation.Source = border;
            // TODO: need to set the matrix (increment it) with every animated frame

            // Displacement map applies the turbulence to the blurred text.
            var displacement = new DisplacementMapEffect();
            displacement.Source = colorize;
            displacement.Displacement = m_flameAnimation;
            displacement.Amount = 40.0f;

            // TODO: for multiline we need another Transform2D here

            // Composite the text over the flames.
            m_composite = new CompositeEffect();
            m_composite.Inputs.Add(displacement);
            m_composite.Inputs.Add(bitmap);

            m_AreResourcesLoaded = true;
            sender.Invalidate();
        }
    }
}
