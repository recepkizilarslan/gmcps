
namespace Gmcps.Tools.Core;

public static class OutputMappingExtensions
{
    public static Result<TOutput> ToOutput<TOutput>(this IGmcpsResult response)
    {
        ArgumentNullException.ThrowIfNull(response);
        return GenericOutputMapper.Map<TOutput>(response.ValueObject);
    }
}
