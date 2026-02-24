namespace Gmcps.Domain.Models;

public sealed record TlsCertificateAsset(
    string Id,
    string Name,
    string SubjectDn,
    string IssuerDn,
    string TimeStatus,
    string Sha256Fingerprint,
    string LastSeen);
