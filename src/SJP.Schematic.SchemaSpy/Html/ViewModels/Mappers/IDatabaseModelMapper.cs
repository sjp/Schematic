namespace SJP.Schematic.SchemaSpy.Html.ViewModels.Mappers
{
    internal interface IDatabaseModelMapper<TDbObject, TModel>
    {
        TModel Map(TDbObject dbObject);
    }
}
