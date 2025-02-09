namespace AspireNetFunctions.Common.Serialization;
 
[JsonSerializable(typeof(WeatherForecastResult))]
public sealed partial class WeatherForecastResultSerializationContext : JsonSerializerContext
{
}