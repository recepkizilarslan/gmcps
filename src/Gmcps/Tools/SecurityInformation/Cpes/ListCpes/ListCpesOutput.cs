using Gmcps.Domain.SecurityInformation.Shared;

namespace Gmcps.Domain.SecurityInformation.Cpes.Outputs;

public sealed record ListCpesOutput(
    IReadOnlyList<SecurityInfoItemOutput> Cpes);
