using System.Collections.Generic;
using DotNetBrightener.DataAccess.Auditing;

namespace DotNetBrightener.DataAccess.Services;

public interface IAuditingContainer
{
    List<AuditTrail> AuditTrails { get; }
}