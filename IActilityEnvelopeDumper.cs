using System.Threading.Tasks;

namespace IoTHub
{
    public interface IActilityEnvelopeDumper
    {
        public Task Dump(string envelope);
    }
}
