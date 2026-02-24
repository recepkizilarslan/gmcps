
namespace Gmcps.Tests;

public class GmpXmlParserTests
{
    [Fact]
    public void ParseTargets_ValidXml_ReturnsTargetList()
    {
        var xml = XDocument.Parse("""
            <get_targets_response status="200">
                <target id="t1-uuid">
                    <name>Windows Servers</name>
                    <comment>windows,production</comment>
                    <max_hosts>10</max_hosts>
                    <hosts>192.168.1.0/24</hosts>
                </target>
                <target id="t2-uuid">
                    <name>Linux Web Servers</name>
                    <comment>linux,web</comment>
                    <max_hosts>5</max_hosts>
                    <hosts>10.0.0.0/24</hosts>
                </target>
            </get_targets_response>
            """);

        var targets = UnixSocketClient.ParseTargets(xml);

        Assert.Equal(2, targets.Count);
        Assert.Equal("t1-uuid", targets[0].Id);
        Assert.Equal("Windows Servers", targets[0].Name);
        Assert.Equal(10, targets[0].HostsCount);
        Assert.Equal("Windows", targets[0].OsHint);
        Assert.Contains("windows", targets[0].Tags);
        Assert.Contains("production", targets[0].Tags);

        Assert.Equal("t2-uuid", targets[1].Id);
        Assert.Equal("Linux", targets[1].OsHint);
    }

    [Fact]
    public void ParseTargets_EmptyResponse_ReturnsEmptyList()
    {
        var xml = XDocument.Parse("<get_targets_response status=\"200\"/>");
        var targets = UnixSocketClient.ParseTargets(xml);
        Assert.Empty(targets);
    }

    [Fact]
    public void ParseReportSummary_ValidXml_ReturnsSummary()
    {
        var xml = XDocument.Parse("""
            <get_reports_response status="200">
                <report id="r1-uuid">
                    <report id="r1-uuid">
                        <task id="task1-uuid"/>
                        <timestamp>2024-01-15T10:30:00Z</timestamp>
                        <results>
                            <result id="res1"><severity>9.8</severity><threat>high</threat></result>
                            <result id="res2"><severity>5.5</severity><threat>medium</threat></result>
                            <result id="res3"><severity>2.1</severity><threat>low</threat></result>
                            <result id="res4"><severity>0.0</severity><threat>log</threat></result>
                            <result id="res5"><severity>8.0</severity><threat>high</threat></result>
                        </results>
                    </report>
                </report>
            </get_reports_response>
            """);

        var result = UnixSocketClient.ParseReportSummary(xml, "r1-uuid");

        Assert.True(result.IsSuccess);
        Assert.Equal("r1-uuid", result.Value.Id);
        Assert.Equal(2, result.Value.Summary.High);
        Assert.Equal(1, result.Value.Summary.Medium);
        Assert.Equal(1, result.Value.Summary.Low);
        Assert.Equal(1, result.Value.Summary.Log);
    }

    [Fact]
    public void ParseFindings_ValidXml_ReturnsGroupedFindings()
    {
        var xml = XDocument.Parse("""
            <get_reports_response status="200">
                <report id="r1">
                    <report id="r1">
                        <results>
                            <result id="res1">
                                <name>Critical RCE</name>
                                <severity>9.8</severity>
                                <host>192.168.1.10</host>
                                <port>443/tcp</port>
                                <nvt oid="1.3.6.1.4.1.25623.1.0.100001">
                                    <name>Critical RCE</name>
                                    <cve>CVE-2024-1234,CVE-2024-5678</cve>
                                </nvt>
                                <qod><value>80</value></qod>
                                <description>Remote code execution found</description>
                            </result>
                            <result id="res2">
                                <name>Info Disclosure</name>
                                <severity>5.3</severity>
                                <host>192.168.1.20</host>
                                <port>80/tcp</port>
                                <nvt oid="1.3.6.1.4.1.25623.1.0.100002">
                                    <name>Info Disclosure</name>
                                    <cve>NOCVE</cve>
                                </nvt>
                                <qod><value>70</value></qod>
                            </result>
                        </results>
                    </report>
                </report>
            </get_reports_response>
            """);

        var findings = UnixSocketClient.ParseFindings(xml);

        Assert.Equal(2, findings.Count);

        var critical = findings[0];
        Assert.Equal("Critical RCE", critical.Name);
        Assert.Equal(9.8, critical.Severity);
        Assert.Equal(80, critical.Qod);
        Assert.Equal(2, critical.Cves.Count);
        Assert.Contains("CVE-2024-1234", critical.Cves);
        Assert.Contains("CVE-2024-5678", critical.Cves);
        Assert.Equal("192.168.1.10", critical.Host);
        Assert.Equal("443/tcp", critical.Port);
        Assert.Equal("1.3.6.1.4.1.25623.1.0.100001", critical.NvtOid);
        Assert.Equal("Remote code execution found", critical.Description);

        var info = findings[1];
        Assert.Empty(info.Cves); // NOCVE should produce empty list
    }

    [Fact]
    public void ParseTasks_ValidXml_ReturnsTaskList()
    {
        var xml = XDocument.Parse("""
            <get_tasks_response status="200">
                <task id="task1-uuid">
                    <name>Daily Scan</name>
                    <target id="t1-uuid"/>
                    <config id="cfg1-uuid"/>
                    <status>Done</status>
                    <progress>-1</progress>
                    <last_report>
                        <report id="r1-uuid"/>
                    </last_report>
                </task>
                <task id="task2-uuid">
                    <name>Running Scan</name>
                    <target id="t2-uuid"/>
                    <config id="cfg1-uuid"/>
                    <status>Running</status>
                    <progress>45</progress>
                </task>
            </get_tasks_response>
            """);

        var tasks = UnixSocketClient.ParseTasks(xml);

        Assert.Equal(2, tasks.Count);
        Assert.Equal("task1-uuid", tasks[0].Id);
        Assert.Equal("Daily Scan", tasks[0].Name);
        Assert.Equal("t1-uuid", tasks[0].TargetId);
        Assert.Equal("Done", tasks[0].Status);
        Assert.Equal("r1-uuid", tasks[0].LastReportId);

        Assert.Equal("task2-uuid", tasks[1].Id);
        Assert.Equal("Running", tasks[1].Status);
        Assert.Equal(45, tasks[1].Progress);
        Assert.Null(tasks[1].LastReportId);
    }

    [Fact]
    public void ParseScanConfigs_ValidXml_ReturnsConfigList()
    {
        var xml = XDocument.Parse("""
            <get_configs_response status="200">
                <config id="cfg1-uuid">
                    <name>Full and fast</name>
                    <comment>Most common scan config</comment>
                </config>
                <config id="cfg2-uuid">
                    <name>Discovery</name>
                </config>
            </get_configs_response>
            """);

        var configs = UnixSocketClient.ParseScanConfigs(xml);

        Assert.Equal(2, configs.Count);
        Assert.Equal("cfg1-uuid", configs[0].Id);
        Assert.Equal("Full and fast", configs[0].Name);
        Assert.Equal("Most common scan config", configs[0].Comment);
        Assert.Equal("cfg2-uuid", configs[1].Id);
    }

    [Fact]
    public void ParseReports_ValidXml_ReturnsReportList()
    {
        var xml = XDocument.Parse("""
            <get_reports_response status="200">
                <report id="r1-uuid">
                    <report id="r1-uuid">
                        <task id="task1-uuid"/>
                        <timestamp>2026-02-23T09:15:00Z</timestamp>
                        <results>
                            <result id="res1"><severity>9.0</severity><threat>high</threat></result>
                            <result id="res2"><severity>5.1</severity><threat>medium</threat></result>
                        </results>
                    </report>
                </report>
                <report id="r2-uuid">
                    <report id="r2-uuid">
                        <task id="task2-uuid"/>
                        <creation_time>2026-02-22T18:00:00Z</creation_time>
                        <results>
                            <result id="res3"><severity>2.2</severity><threat>low</threat></result>
                        </results>
                    </report>
                </report>
            </get_reports_response>
            """);

        var reports = UnixSocketClient.ParseReports(xml);

        Assert.Equal(2, reports.Count);
        Assert.Equal("r1-uuid", reports[0].Id);
        Assert.Equal("task1-uuid", reports[0].TaskId);
        Assert.Equal(new DateTime(2026, 2, 23, 9, 15, 0, DateTimeKind.Utc), reports[0].Timestamp);
        Assert.Equal(1, reports[0].Summary.High);
        Assert.Equal(1, reports[0].Summary.Medium);

        Assert.Equal("r2-uuid", reports[1].Id);
        Assert.Equal("task2-uuid", reports[1].TaskId);
        Assert.Equal(1, reports[1].Summary.Low);
    }

    [Fact]
    public void ParseComplianceAuditReports_ValidXml_ReturnsComplianceReportList()
    {
        var xml = XDocument.Parse("""
            <get_reports_response status="200">
                <report id="audit-report-1">
                    <report id="audit-report-1">
                        <task id="audit-task-1"/>
                        <timestamp>2026-02-21T10:00:00Z</timestamp>
                        <compliance_count>
                            <yes>15</yes>
                            <no>3</no>
                            <incomplete>2</incomplete>
                        </compliance_count>
                    </report>
                </report>
            </get_reports_response>
            """);

        var reports = UnixSocketClient.ParseComplianceAuditReports(xml);

        Assert.Single(reports);
        Assert.Equal("audit-report-1", reports[0].Id);
        Assert.Equal("audit-task-1", reports[0].TaskId);
        Assert.Equal("2026-02-21T10:00:00Z", reports[0].Timestamp);
        Assert.Equal(15, reports[0].Yes);
        Assert.Equal(3, reports[0].No);
        Assert.Equal(2, reports[0].Incomplete);
    }

    [Fact]
    public void ParseHostAssets_ValidXml_ReturnsHostAssets()
    {
        var xml = XDocument.Parse("""
            <get_assets_response status="200">
                <asset id="a1">
                    <name>Host A</name>
                    <host>
                        <detail>
                            <name>ip</name>
                            <value>10.0.0.10</value>
                        </detail>
                        <detail>
                            <name>os</name>
                            <value>Linux</value>
                        </detail>
                        <severity>
                            <value>7.5</value>
                        </severity>
                    </host>
                </asset>
            </get_assets_response>
            """);

        var assets = UnixSocketClient.ParseHostAssets(xml);

        Assert.Single(assets);
        Assert.Equal("a1", assets[0].Id);
        Assert.Equal("Host A", assets[0].Name);
        Assert.Equal("10.0.0.10", assets[0].Ip);
        Assert.Equal("Linux", assets[0].OperatingSystem);
        Assert.Equal(7.5, assets[0].Severity);
    }

    [Fact]
    public void ParseOperatingSystemAssets_ValidXml_ReturnsOperatingSystemAssets()
    {
        var xml = XDocument.Parse("""
            <get_assets_response status="200">
                <asset id="os1">
                    <name>cpe:/o:linux:kernel</name>
                    <os>
                        <title>Linux Kernel</title>
                        <hosts>4</hosts>
                        <all_hosts>6</all_hosts>
                        <average_severity>
                            <value>4.2</value>
                        </average_severity>
                        <highest_severity>
                            <value>9.1</value>
                        </highest_severity>
                    </os>
                </asset>
            </get_assets_response>
            """);

        var assets = UnixSocketClient.ParseOperatingSystemAssets(xml);

        Assert.Single(assets);
        Assert.Equal("os1", assets[0].Id);
        Assert.Equal("Linux Kernel", assets[0].Title);
        Assert.Equal(4, assets[0].Hosts);
        Assert.Equal(6, assets[0].AllHosts);
        Assert.Equal(4.2, assets[0].AverageSeverity);
        Assert.Equal(9.1, assets[0].HighestSeverity);
    }

    [Fact]
    public void ParseTlsCertificates_ValidXml_ReturnsCertificates()
    {
        var xml = XDocument.Parse("""
            <get_tls_certificates_response status="200">
                <tls_certificate id="cert1">
                    <name>api.example.com</name>
                    <subject_dn>CN=api.example.com</subject_dn>
                    <issuer_dn>CN=Root CA</issuer_dn>
                    <time_status>valid</time_status>
                    <sha256_fingerprint>abc123</sha256_fingerprint>
                    <last_seen>2026-02-20T10:00:00Z</last_seen>
                </tls_certificate>
            </get_tls_certificates_response>
            """);

        var certs = UnixSocketClient.ParseTlsCertificates(xml);

        Assert.Single(certs);
        Assert.Equal("cert1", certs[0].Id);
        Assert.Equal("api.example.com", certs[0].Name);
        Assert.Equal("valid", certs[0].TimeStatus);
        Assert.Equal("abc123", certs[0].Sha256Fingerprint);
    }

    [Fact]
    public void ParseSecurityInfos_ValidXml_ReturnsEntries()
    {
        var xml = XDocument.Parse("""
            <get_info_response status="200">
                <info id="CVE-2025-0001">
                    <name>CVE-2025-0001</name>
                    <cve>
                        <score>9.8</score>
                        <description>Remote code execution</description>
                    </cve>
                </info>
            </get_info_response>
            """);

        var infos = UnixSocketClient.ParseSecurityInfos(xml, "CVE");

        Assert.Single(infos);
        Assert.Equal("CVE-2025-0001", infos[0].Id);
        Assert.Equal("CVE-2025-0001", infos[0].Name);
        Assert.Equal("CVE", infos[0].Type);
        Assert.Equal(9.8, infos[0].Score);
        Assert.Equal("Remote code execution", infos[0].Summary);
    }

    [Fact]
    public void ParseResults_ValidXml_ReturnsResultItems()
    {
        var xml = XDocument.Parse("""
            <get_results_response status="200">
                <result id="r1">
                    <name>Open Port</name>
                    <host>10.0.0.5</host>
                    <port>22/tcp</port>
                    <severity>5.0</severity>
                    <threat>medium</threat>
                    <nvt oid="1.3.6.1.4.1.25623.1.0.100000">
                        <name>OpenSSH Detection</name>
                    </nvt>
                </result>
            </get_results_response>
            """);

        var results = UnixSocketClient.ParseResults(xml);

        Assert.Single(results);
        Assert.Equal("r1", results[0].Id);
        Assert.Equal("Open Port", results[0].Name);
        Assert.Equal("10.0.0.5", results[0].Host);
        Assert.Equal("22/tcp", results[0].Port);
        Assert.Equal(5.0, results[0].Severity);
        Assert.Equal("medium", results[0].Threat);
    }

    [Fact]
    public void ParseNotes_ValidXml_ReturnsNotes()
    {
        var xml = XDocument.Parse("""
            <get_notes_response status="200">
                <note id="n1">
                    <text>Reviewed by SOC team</text>
                    <active>1</active>
                </note>
            </get_notes_response>
            """);

        var notes = UnixSocketClient.ParseNotes(xml);

        Assert.Single(notes);
        Assert.Equal("n1", notes[0].Id);
        Assert.Equal("Reviewed by SOC team", notes[0].Text);
        Assert.True(notes[0].Active);
    }

    [Fact]
    public void ParseOverrides_ValidXml_ReturnsOverrides()
    {
        var xml = XDocument.Parse("""
            <get_overrides_response status="200">
                <override id="o1">
                    <text>Accepted risk</text>
                    <new_severity>0.0</new_severity>
                    <active>0</active>
                </override>
            </get_overrides_response>
            """);

        var overrides = UnixSocketClient.ParseOverrides(xml);

        Assert.Single(overrides);
        Assert.Equal("o1", overrides[0].Id);
        Assert.Equal("Accepted risk", overrides[0].Text);
        Assert.Equal(0.0, overrides[0].NewSeverity);
        Assert.False(overrides[0].Active);
    }

    [Fact]
    public void ParseTickets_ValidXml_ReturnsTickets()
    {
        var xml = XDocument.Parse("""
            <get_tickets_response status="200">
                <ticket id="ticket-1">
                    <name>Fix OpenSSL issue</name>
                    <status>Open</status>
                    <severity>9.1</severity>
                    <host>10.10.10.5</host>
                    <location>443/tcp</location>
                    <result id="result-1" />
                    <assigned_to>
                        <user id="user-1" />
                    </assigned_to>
                </ticket>
            </get_tickets_response>
            """);

        var tickets = UnixSocketClient.ParseTickets(xml);

        Assert.Single(tickets);
        Assert.Equal("ticket-1", tickets[0].Id);
        Assert.Equal("Fix OpenSSL issue", tickets[0].Name);
        Assert.Equal("Open", tickets[0].Status);
        Assert.Equal(9.1, tickets[0].Severity);
        Assert.Equal("10.10.10.5", tickets[0].Host);
        Assert.Equal("443/tcp", tickets[0].Location);
        Assert.Equal("result-1", tickets[0].ResultId);
        Assert.Equal("user-1", tickets[0].AssignedToUserId);
    }

    [Fact]
    public void ParsePortLists_ValidXml_ReturnsPortLists()
    {
        var xml = XDocument.Parse("""
            <get_port_lists_response status="200">
                <port_list id="pl-1">
                    <name>All IANA assigned TCP</name>
                    <comment>Default list</comment>
                </port_list>
            </get_port_lists_response>
            """);

        var portLists = UnixSocketClient.ParsePortLists(xml);

        Assert.Single(portLists);
        Assert.Equal("pl-1", portLists[0].Id);
        Assert.Equal("All IANA assigned TCP", portLists[0].Name);
        Assert.Equal("Default list", portLists[0].Comment);
    }

    [Fact]
    public void ParseCredentials_ValidXml_ReturnsCredentials()
    {
        var xml = XDocument.Parse("""
            <get_credentials_response status="200">
                <credential id="cred-1">
                    <name>SSH Root</name>
                    <type>up</type>
                    <comment>Linux root credential</comment>
                </credential>
            </get_credentials_response>
            """);

        var credentials = UnixSocketClient.ParseCredentials(xml);

        Assert.Single(credentials);
        Assert.Equal("cred-1", credentials[0].Id);
        Assert.Equal("SSH Root", credentials[0].Name);
        Assert.Equal("up", credentials[0].Type);
        Assert.Equal("Linux root credential", credentials[0].Comment);
    }

    [Fact]
    public void ParseScanners_ValidXml_ReturnsScanners()
    {
        var xml = XDocument.Parse("""
            <get_scanners_response status="200">
                <scanner id="sc-1">
                    <name>OpenVAS Default</name>
                    <type>OpenVAS</type>
                    <active>1</active>
                </scanner>
            </get_scanners_response>
            """);

        var scanners = UnixSocketClient.ParseScanners(xml);

        Assert.Single(scanners);
        Assert.Equal("sc-1", scanners[0].Id);
        Assert.Equal("OpenVAS Default", scanners[0].Name);
        Assert.Equal("OpenVAS", scanners[0].Type);
        Assert.True(scanners[0].Active);
    }
}
