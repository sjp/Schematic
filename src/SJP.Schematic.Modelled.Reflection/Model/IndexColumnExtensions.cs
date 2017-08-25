using SJP.Schema.Core;

namespace SJP.Schema.Modelled.Reflection.Model
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
            return new IndexColumn(column, IndexColumnOrder.Descending);
        }

        public static IndexColumn Descending(this ExpressionColumn column)
        {
            return new IndexColumn(column, IndexColumnOrder.Descending);
        }
    }
}
