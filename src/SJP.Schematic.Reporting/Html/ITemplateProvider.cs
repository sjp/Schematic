namespace SJP.Schematic.Reporting.Html
{
    public interface ITemplateProvider
    {
        string GetTemplate(SchemaSpyTemplate template);
    }
}
