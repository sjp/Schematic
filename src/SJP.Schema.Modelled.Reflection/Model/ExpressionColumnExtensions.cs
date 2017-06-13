using System;

namespace SJP.Schema.Modelled.Reflection.Model
{
    public static class ExpressionColumnExtensions
    {
        public static ExpressionColumn Lower(this ModelledColumn column)
        {
            if (column == null)
                throw new ArgumentNullException(nameof(column));

            return new ExpressionColumn(Sql.Lower(column));
        }

        public static ExpressionColumn Upper(this ModelledColumn column)
        {
            if (column == null)
                throw new ArgumentNullException(nameof(column));

            return new ExpressionColumn(Sql.Upper(column));
        }
    }
}
