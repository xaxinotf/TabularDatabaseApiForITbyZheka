using Microsoft.AspNetCore.Mvc;
using TabularDatabaseApiForITbyZheka.Models;
using TabularDatabaseApiForITbyZheka.Services;

namespace TabularDatabaseApiForITbyZheka.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RowController : ControllerBase
    {
        private readonly DatabaseService _service;

        public RowController(DatabaseService service)
        {
            _service = service;
        }

        // Отримати список рядків у таблиці
        [HttpGet("{databaseName}/tables/{tableName}/rows")]
        public IActionResult GetRows(string databaseName, string tableName)
        {
            var table = _service.GetTable(databaseName, tableName);
            if (table == null)
                return NotFound($"Table '{tableName}' in database '{databaseName}' not found.");

            return Ok(table.Rows);
        }

        // Додати новий рядок до таблиці
        [HttpPost("{databaseName}/tables/{tableName}/rows")]
        public IActionResult AddRow(string databaseName, string tableName, [FromBody] RowModel row)
        {
            if (row == null || row.Values == null)
                return BadRequest("Row values are required.");

            if (!_service.AddRow(databaseName, tableName, row))
                return BadRequest("Failed to add row. Ensure data validity and table existence.");

            return CreatedAtAction(nameof(GetRows), new { databaseName, tableName }, "Row added successfully.");
        }

        // Редагувати існуючий рядок за індексом
        [HttpPut("{databaseName}/tables/{tableName}/rows/{rowIndex}")]
        public IActionResult EditRow(string databaseName, string tableName, int rowIndex, [FromBody] RowModel updatedRow)
        {
            if (updatedRow == null || updatedRow.Values == null)
                return BadRequest("Row values are required.");

            if (!_service.EditRow(databaseName, tableName, rowIndex, updatedRow))
                return BadRequest("Failed to edit row. Ensure data validity and row index.");

            return Ok("Row updated successfully.");
        }

        // Видалити рядок за індексом
        [HttpDelete("{databaseName}/tables/{tableName}/rows/{rowIndex}")]
        public IActionResult DeleteRow(string databaseName, string tableName, int rowIndex)
        {
            if (!_service.DeleteRow(databaseName, tableName, rowIndex))
                return BadRequest("Failed to delete row. Ensure row index is correct.");

            return NoContent();
        }
    }
}
