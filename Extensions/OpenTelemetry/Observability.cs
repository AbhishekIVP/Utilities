using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace ivp.edm.apm
{
    public class Observability
    {
        public ActivitySource? ServiceActivity { get; set; }
        public Meter? ServiceMeter { get; set; }
    }
}

