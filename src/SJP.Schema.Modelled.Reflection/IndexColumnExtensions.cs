namespace SJP.Schema.Modelled.Reflection
{
    public static class IndexColumnExtensions
    {
        public static IndexColumn Ascending(this ModelledColumn column)
        {
            return new IndexColumn(column);
        }

        public static IndexColumn Ascending(this ExpressionColumn column)
        {
            return new IndexColumn(column);
        }

        public static IndexColumn Descending(this ModelledColumn column)
        {
            return new IndexColumn(column, false);
        }

        public static IndexColumn Descending(this ExpressionColumn column)
        {
            return new IndexColumn(column, false);
        }
    }
}
