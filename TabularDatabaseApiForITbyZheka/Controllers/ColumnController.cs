using Microsoft.AspNetCore.Mvc;
using TabularDatabaseApiForITbyZheka.Models;
using TabularDatabaseApiForITbyZheka.Services;

namespace TabularDatabaseApiForITbyZheka.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ColumnController : ControllerBase
    {
        private readonly DatabaseService _service;

        public ColumnController(DatabaseService service)
        {
            _service = service;
        }

        // Отримати список стовпців у таблиці
        [HttpGet("{databaseName}/tables/{tableName}/columns")]
        public IActionResult GetColumns(string databaseName, string tableName)
        {
            var table = _service.GetTable(databaseName, tableName);
            if (table == null)
                return NotFound($"Table '{tableName}' in database '{databaseName}' not found.");

            return Ok(table.Columns);
        }

        // Додати новий стовпець до таблиці
        [HttpPost("{databaseName}/tables/{tableName}/columns")]
        public IActionResult AddColumn(string databaseName, string tableName, [FromBody] ColumnModel column)
        {
            if (column == null || string.IsNullOrWhiteSpace(column.Name) || string.IsNullOrWhiteSpace(column.DataType))
                return BadRequest("Column name and data type are required.");

            if (!_service.AddColumn(databaseName, tableName, column))
                return BadRequest($"Column '{column.Name}' already exists in table '{tableName}' or invalid data.");

            return CreatedAtAction(nameof(GetColumns), new { databaseName, tableName }, $"Column '{column.Name}' added successfully to table '{tableName}' in database '{databaseName}'.");
        }

        // Перейменувати стовпець у таблиці
        [HttpPut("{databaseName}/tables/{tableName}/columns/rename")]
        public IActionResult RenameColumn(string databaseName, string tableName, [FromBody] RenameColumnRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.OldName) || string.IsNullOrWhiteSpace(request.NewName))
                return BadRequest("OldName and NewName are required.");

            if (!_service.RenameColumn(databaseName, tableName, request.OldName, request.NewName))
                return BadRequest($"Failed to rename column '{request.OldName}' to '{request.NewName}' in table '{tableName}'.");

            return Ok($"Column '{request.OldName}' renamed to '{request.NewName}' successfully in table '{tableName}' of database '{databaseName}'.");
        }

        // Переставити порядок стовпців у таблиці
        [HttpPut("{databaseName}/tables/{tableName}/columns/reorder")]
        public IActionResult ReorderColumns(string databaseName, string tableName, [FromBody] List<string> newOrder)
        {
            if (newOrder == null || newOrder.Count == 0)
                return BadRequest("New order of columns is required.");

            if (!_service.ReorderColumns(databaseName, tableName, newOrder))
                return BadRequest("Failed to reorder columns. Ensure all column names are correct and count matches.");

            return Ok($"Columns reordered successfully in table '{tableName}' of database '{databaseName}'.");
        }
    }

    public class RenameColumnRequest
    {
        public string OldName { get; set; }
        public string NewName { get; set; }
    }
}
