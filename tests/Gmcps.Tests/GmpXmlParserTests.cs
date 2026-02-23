using System.Xml.Linq;
using Gmcps.Infrastructure.Gmp;

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

        var targets = GmpXmlParser.ParseTargets(xml);

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
        var targets = GmpXmlParser.ParseTargets(xml);
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

        var result = GmpXmlParser.ParseReportSummary(xml, "r1-uuid");

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

        var findings = GmpXmlParser.ParseFindings(xml);

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

        var tasks = GmpXmlParser.ParseTasks(xml);

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

        var configs = GmpXmlParser.ParseScanConfigs(xml);

        Assert.Equal(2, configs.Count);
        Assert.Equal("cfg1-uuid", configs[0].Id);
        Assert.Equal("Full and fast", configs[0].Name);
        Assert.Equal("Most common scan config", configs[0].Comment);
        Assert.Equal("cfg2-uuid", configs[1].Id);
    }
}
