
namespace Gmcps.Tools.Core;

public static class OutputMappingExtensions
{
    public static Result<TOutput> ToOutput<TOutput>(this IGmcpsResult response)
    {
        ArgumentNullException.ThrowIfNull(response);
        if (response.IsFailure)
        {
            return Result<TOutput>.Failure(response.Error);
        }

        return GenericOutputMapper.Map<TOutput>(response.ValueObject);
    }
}
