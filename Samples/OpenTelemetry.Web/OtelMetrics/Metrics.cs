using System.Diagnostics.Metrics;
using ivp.edm.apm;

namespace OpenTelemetrySample.Controllers;

public class SampleMeters
{
    private readonly Counter<long>? weatherMethodCounter;
    private readonly Counter<long>? blockCounter;
    public SampleMeters(Observability apmHelper){
        weatherMethodCounter = apmHelper?.ServiceMeter?.CreateCounter<long>("weather.service.counter", "ServiceCall");
        blockCounter = apmHelper?.ServiceMeter?.CreateCounter<long>("block.counter", "ServiceCall");
    }

    public void WeatherMethodInvoked() => weatherMethodCounter?.Add(1);
    public void WeatherMethodRevoked() => weatherMethodCounter?.Add(-1);
    public void BlockInvoked() => blockCounter?.Add(1);

}