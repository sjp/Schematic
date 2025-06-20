namespace SJP.Schematic.Reporting.Html;

public interface ITemplateProvider
{
    string GetTemplate(ReportTemplate template);
}