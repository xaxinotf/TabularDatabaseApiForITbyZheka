using Microsoft.AspNetCore.Mvc;
using TabularDatabaseApiForITbyZheka.Services;
using TabularDatabaseApiForITbyZheka.Models;

namespace TabularDatabaseApiForITbyZheka.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TableController : ControllerBase
    {
        private readonly DatabaseService _service;

        public TableController(DatabaseService service)
        {
            _service = service;
        }

        // Отримати список таблиць у базі даних
        [HttpGet("{databaseName}/tables")]
        public IActionResult GetTables(string databaseName)
        {
            var db = _service.GetDatabase(databaseName);
            if (db == null)
                return NotFound($"Database '{databaseName}' not found.");

            return Ok(db.Tables);
        }

        // Створити нову таблицю у базі даних
        [HttpPost("{databaseName}/tables")]
        public IActionResult CreateTable(string databaseName, [FromBody] TableModel model)
        {
            if (model == null || string.IsNullOrWhiteSpace(model.Name))
                return BadRequest("Table name cannot be empty.");

            if (!_service.CreateTable(databaseName, model.Name))
                return BadRequest($"Table '{model.Name}' already exists in database '{databaseName}' or database does not exist.");

            return CreatedAtAction(nameof(GetTables), new { databaseName }, $"Table '{model.Name}' created successfully in database '{databaseName}'.");
        }

        // Видалити таблицю з бази даних
        [HttpDelete("{databaseName}/tables/{tableName}")]
        public IActionResult DeleteTable(string databaseName, string tableName)
        {
            if (!_service.DeleteTable(databaseName, tableName))
                return NotFound($"Table '{tableName}' not found in database '{databaseName}' or database does not exist.");

            return NoContent();
        }
    }
}
