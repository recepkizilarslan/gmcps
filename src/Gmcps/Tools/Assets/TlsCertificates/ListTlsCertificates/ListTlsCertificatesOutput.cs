namespace Gmcps.Tools.Assets.TlsCertificates.ListTlsCertificates;

public sealed record TlsCertificateOutput(
    string Id,
    string Name,
    string SubjectDn,
    string IssuerDn,
    string TimeStatus,
    string Sha256Fingerprint,
    string LastSeen);

public sealed record ListTlsCertificatesOutput(
    IReadOnlyList<TlsCertificateOutput> TlsCertificates);
