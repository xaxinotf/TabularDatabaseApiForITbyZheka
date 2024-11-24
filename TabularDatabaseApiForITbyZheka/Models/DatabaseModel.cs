using System.Collections.Generic;

namespace TabularDatabaseApiForITbyZheka.Models
{
    public class DatabaseModel
    {
        public string Name { get; set; }
        public List<TableModel> Tables { get; set; } = new();
    }
}
