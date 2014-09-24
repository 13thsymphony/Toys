using System;
using System.Collections.Generic;
using System.Text;
using Windows.UI;

using Microsoft.Graphics.Canvas;

namespace Fireworks
{
    static class Constants
    {
        // Age at which the firework will stop being simulated in seconds.
        public const float FireworkMaxAge = 3.0f;
        // What fraction of velocity should remain each second.
        // The equation for calculating remaining amount based on dt:
        // e^(ln[coefficient] * dt) * [current amount]
        public const float FireworkDragCoeff = 0.3f;
        // How much is added to DY each second (applied before drag).
        public const float FireworkGravityTerm = 20.0f;
        // What fraction of each RGB channel should remain each second.
        public const float FireworkBrightnessCoeff = 0.5f;
    }
    /// <summary>
    /// Encapsulates a single "sparkle". Manages and updates its internal state.
    /// </summary>
    class Firework
    {
        /// <summary>
        /// X position in DIPs.
        /// </summary>
        private float m_x;
        /// <summary>
        /// Y position in DIPs.
        /// </summary>
        private float m_y;
        /// <summary>
        /// m_x velocity in DIPs/second.
        /// </summary>
        private float m_dx;
        /// <summary>
        /// Y velocity in DIPs/second.
        /// </summary>
        private float m_dy;
        /// <summary>
        /// Radius in DIPs.
        /// </summary>
        private float m_radius;
        /// <summary>
        /// Current color.
        /// </summary>
        private Color m_color;
        /// <summary>
        /// How old the firework is in seconds.
        /// </summary>
        private float m_age;

        /// <summary>
        /// Create and initialize a new firework.
        /// </summary>
        /// <param name="x">In DIPs.</param>
        /// <param name="y">In DIPs.</param>
        /// <param name="dx">In DIPs/second.</param>
        /// <param name="dy">In DIPs/second.</param>
        /// <param name="radius">In DIPs.</param>
        /// <param name="color"></param>
        public Firework(float x, float y, float dx, float dy, float radius, Color color)
        {
            m_x = x;
            m_y = y;
            m_dx = dx;
            m_dy = dy;
            m_radius = radius;
            m_color = color;
            m_age = 0;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="timeDelta">In seconds.</param>
        public void UpdateState(float timeDelta)
        {
            m_x += m_dx * timeDelta;
            m_y += m_dy * timeDelta;

            m_dx *= (float)Math.Exp(Math.Log(Constants.FireworkDragCoeff) * timeDelta);
            // Gravity acts before drag. This ensures that gravity's contribution is always bounded.
            m_dy += Constants.FireworkGravityTerm;
            m_dy *= (float)Math.Exp(Math.Log(Constants.FireworkDragCoeff) * timeDelta);

            m_age += timeDelta;

            // TODO: I think I can't use the *= operator because it would force conversion to byte before the multiplication.
            // TODO: Premultiplied colors??!?!?!??!?!?
            //m_color.A = (byte)(m_color.A * Math.Exp(Math.Log(Constants.FireworkDragCoeff) * timeDelta));
            m_color.R = (byte)(m_color.R * Math.Exp(Math.Log(Constants.FireworkDragCoeff) * timeDelta));
            m_color.G = (byte)(m_color.G * Math.Exp(Math.Log(Constants.FireworkDragCoeff) * timeDelta));
            m_color.B = (byte)(m_color.B * Math.Exp(Math.Log(Constants.FireworkDragCoeff) * timeDelta));
        }

        public void Render(CanvasDrawingSession ds)
        {
            ds.FillCircle(m_x, m_y, m_radius, m_color);
        }

        /// <summary>
        /// Whether or not the firework should be disposed and not simulated anymore.
        /// </summary>
        /// <returns></returns>
        public bool ShouldDispose()
        {
            if (m_age > Constants.FireworkMaxAge)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }

    class FireworksController
    {
        private List<Firework> m_fireworks;

        public FireworksController()
        {
            m_fireworks = new List<Firework>();
        }

        public void AddFirework(Firework f)
        {
            m_fireworks.Add(f);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="timeDelta">Delta time in seconds.</param>
        public void UpdateFireworks(float timeDelta)
        {
            // Walk the list backwards so the order isn't affected by deleting an element.
            for (int i = m_fireworks.Count - 1; i >= 0; i--)
            {
                m_fireworks[i].UpdateState(timeDelta);

                if (m_fireworks[i].ShouldDispose())
                {
                    m_fireworks.RemoveAt(i);
                }
            }
        }

        public void RenderFireworks(CanvasDrawingSession ds)
        {
            foreach (var firework in m_fireworks)
            {
                firework.Render(ds);
            }
        }
    }
}
