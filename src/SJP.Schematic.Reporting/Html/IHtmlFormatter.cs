namespace SJP.Schematic.Reporting.Html
{
    public interface IHtmlFormatter
    {
        string RenderTemplate<T>(T templateParameter) where T : ITemplateParameter;
    }
}
