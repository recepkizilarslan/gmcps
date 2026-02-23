using Gmcps.Domain;

namespace Gmcps.Core;

public interface IExample<in TRaw, TParsed>
{
    Result<TParsed> Parse(TRaw raw);
}
