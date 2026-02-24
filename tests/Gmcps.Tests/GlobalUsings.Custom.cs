global using System;
global using System.Collections;
global using System.Collections.Generic;
global using System.Linq;
global using System.Text.Json;
global using System.Threading;
global using System.Threading.Tasks;
global using System.Xml.Linq;

global using Microsoft.Extensions.Logging.Abstractions;
global using Microsoft.Extensions.Options;

global using Xunit;

global using Gmcps.Configuration;
global using Gmcps.Domain;
global using Gmcps.Domain.Scans.Vulnerabilities.Inputs;
global using Gmcps.Domain.Validation;
global using Gmcps.Infrastructure.Clients.Gvm;
global using Gmcps.Infrastructure.Security;
global using Gmcps.Infrastructure.Stores;
global using Gmcps.Domain.Models;
global using Gmcps.Tools.Configuration.Targets.CreateTarget;
global using Gmcps.Tools.Resilience.Compliance.IsTargetCompliant;
global using Gmcps.Tools.Scans.Targets.ListCriticalTargets;
global using Gmcps.Tools.Scans.Vulnerabilities.ListCriticalPackages;
