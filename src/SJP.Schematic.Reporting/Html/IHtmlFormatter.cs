namespace SJP.Schematic.Reporting.Html
{
    public interface IHtmlFormatter
    {
        string RenderTemplate(ITemplateParameter templateParameter);
    }
}
