using System.Collections.Generic;

namespace TabularDatabaseApiForITbyZheka.Models
{
    public class TableModel
    {
        public string Name { get; set; }
        public List<ColumnModel> Columns { get; set; } = new();
        public List<RowModel> Rows { get; set; } = new();
    }
}
