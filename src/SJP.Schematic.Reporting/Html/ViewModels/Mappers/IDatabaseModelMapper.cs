namespace SJP.Schematic.Reporting.Html.ViewModels.Mappers
{
    internal interface IDatabaseModelMapper<TDbObject, TModel>
    {
        TModel Map(TDbObject dbObject);
    }
}
