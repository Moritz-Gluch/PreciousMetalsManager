using System;

namespace PreciousMetalsManager.Models
{
    public static class MetalTypeHelper
    {
        public static Array GetValues => Enum.GetValues(typeof(MetalType));
    }
}