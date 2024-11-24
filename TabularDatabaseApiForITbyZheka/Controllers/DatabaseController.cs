using Microsoft.AspNetCore.Mvc;
using TabularDatabaseApiForITbyZheka.Services;

namespace TabularDatabaseApiForITbyZheka.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DatabaseController : ControllerBase
    {
        private readonly DatabaseService _service;

        public DatabaseController(DatabaseService service)
        {
            _service = service;
        }

        // Отримати список баз даних
        [HttpGet]
        public IActionResult GetDatabases()
        {
            var databases = _service.GetDatabases();
            return Ok(databases);
        }

        // Створити нову базу даних
        [HttpPost]
        public IActionResult CreateDatabase([FromBody] string databaseName)
        {
            if (string.IsNullOrWhiteSpace(databaseName))
                return BadRequest("Database name cannot be empty.");

            if (!_service.CreateDatabase(databaseName))
                return BadRequest($"Database with name '{databaseName}' already exists.");

            return CreatedAtAction(nameof(GetDatabases), new { name = databaseName }, $"Database '{databaseName}' created successfully.");
        }

        // Видалити базу даних
        [HttpDelete("{databaseName}")]
        public IActionResult DeleteDatabase(string databaseName)
        {
            if (!_service.DeleteDatabase(databaseName))
                return NotFound($"Database '{databaseName}' not found.");

            return NoContent();
        }
    }
}
