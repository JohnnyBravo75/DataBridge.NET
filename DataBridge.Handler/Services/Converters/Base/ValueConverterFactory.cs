using DataBridge.Helper;

namespace DataBridge.Handler.Services.Converters
{
    public class ValueConverterFactory : GenericFactory
    {
        public static ValueConverterBase GetInstance(string typeName)
        {
            return GenericFactory.GetInstance<ValueConverterBase>(typeName);
        }
    }
}