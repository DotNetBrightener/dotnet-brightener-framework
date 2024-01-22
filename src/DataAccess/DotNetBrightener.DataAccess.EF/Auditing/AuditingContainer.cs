using DotNetBrightener.DataAccess.Auditing;
using DotNetBrightener.DataAccess.Services;
using System.Collections.Generic;

namespace DotNetBrightener.DataAccess.EF.Auditing;


public class AuditingContainer : IAuditingContainer
{
    public List<AuditTrail> AuditTrails => new List<AuditTrail>();
}