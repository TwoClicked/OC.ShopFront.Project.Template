// OC.LUAC.ApiLayer/Controllers/CategoryController.cs
using Microsoft.AspNetCore.Mvc;
using OC.LUAC.ApiLayer.DTO.Category;
using OC.LUAC.ObjectLayer.Entities;
using OC.LUAC.ServiceLayer.Interfaces;

namespace OC.LUAC.ApiLayer.Controllers
{
    [ApiController]
    [Route("api/categories")] // plural route
    public class CategoryController : ControllerBase
    {
        private readonly ICategoryService _categories;

        public CategoryController(ICategoryService categories)
        {
            _categories = categories;
        }

        // GET /api/categories
        [HttpGet]
        public async Task<ActionResult<List<Category>>> GetAll()
        {
            var items = await _categories.GetAllCategoriesAsync();
            return Ok(items);
        }

        // GET /api/categories/{id}
        [HttpGet("{id:int}")]
        public async Task<ActionResult<Category>> GetById(int id)
        {
            var item = await _categories.GetCategoryByIdAsync(id);
            if (item == null)
            {
                return NotFound();
            }
            return Ok(item);
        }

        // POST /api/categories
        [HttpPost]
        [ProducesResponseType(typeof(Category), 201)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<Category>> Create([FromBody] CreateCategoryDto dto)
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            var category = new Category
            {
                Name_en = dto.Name_en,
                Name_de = dto.Name_de
            };

            var created = await _categories.AddCategoryAsync(category);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        // PUT /api/categories/{id}
        [HttpPut("{id:int}")]
        [ProducesResponseType(typeof(Category), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<Category>> Update(int id, [FromBody] UpdateCategoryDto dto)
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            var existing = await _categories.GetCategoryByIdAsync(id);
            if (existing == null)
            {
                return NotFound();
            }

            existing.Name_en = dto.Name_en;
            existing.Name_de = dto.Name_de;

            var updated = await _categories.UpdateCategoryAsync(existing);
            return Ok(updated);
        }

        // DELETE /api/categories/{id}  (soft delete)
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> SoftDelete(int id)
        {
            var ok = await _categories.SoftDeleteCategoryAsync(id);
            if (!ok)
            {
                return NotFound();
            }
            return NoContent();
        }
    }
}
