// "Open Source copyrights apply - All code can be reused DO NOT remove author tags"




using Microsoft.Extensions.Options;



namespace MediaRecycler.Modules.Options;


public class OptionsMonitorStub<T> : IOptionsMonitor<T>
{



    public OptionsMonitorStub(T currentValue)
    {
        CurrentValue = currentValue;
    }






    public T CurrentValue { get; }

    public T Get(string name) => CurrentValue;






    public IDisposable OnChange(Action<T, string> listener)
    {
        // No change tracking in this stub
        return new NoopDisposable();
    }






    private class NoopDisposable : IDisposable
    {

        public void Dispose() { }

    }

}
