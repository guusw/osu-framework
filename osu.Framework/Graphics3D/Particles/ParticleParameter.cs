// Copyright(c) 2016 Guus Waals (guus_waals@live.nl)
// Licensed under the MIT License(MIT)
// See "LICENSE.txt" for more information

namespace osu.Framework.Graphics3D.Particles
{
    /// <summary>
    /// A parameter that controls how particles behave
    /// </summary>
    public abstract class ParticleParameter
    {
        public ParticleSystem System { get; private set; }

        public void OnAttach(ParticleSystem system)
        {
            System = system;
        }

        public void OnDetach()
        {
            
        }
    }
}