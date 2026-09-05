using API.Common;
using API.DTO.Category;
using API.Interfaces.Category;
using CategoryModel = API.Models.Category;

namespace API.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly ILogger<CategoryService> _logger;

        public CategoryService(ICategoryRepository categoryRepository, ILogger<CategoryService> logger)
        {
            _categoryRepository = categoryRepository;
            _logger = logger;
        }

        public async Task<Result<CategoryDTO>> CreateAsync(CreateCategoryDTO createCategoryDTO)
        {
            var errors = new List<string>();

            var existedCategory = await _categoryRepository.GetByName(createCategoryDTO.Name);

            if (existedCategory is not null)
                errors.Add("Already exists a category with this name");

            if (errors.Any())
                return Result<CategoryDTO>.Failure(errors);

            var newCategory = new CategoryModel
            {
                Name = createCategoryDTO.Name
            };

            var created = await _categoryRepository.AddAsync(newCategory);

            var resultDto = new CategoryDTO
            {
                Id = created.Id,
                Name = created.Name
            };

            return Result<CategoryDTO>.Success(resultDto);
        }

        public async Task<Result<CategoryDTO?>> GetByIdAsync(Guid id)
        {
            var category = await _categoryRepository.GetByIdAsync(id);

            if (category is null)
                return Result<CategoryDTO?>.Failure("Category not found");

            var dto = new CategoryDTO
            {
                Id = category.Id,
                Name = category.Name
            };

            return Result<CategoryDTO?>.Success(dto);
        }

        public async Task<Result<List<CategoryDTO>>> GetAllAsync()
        {
            var categories = await _categoryRepository.GetAllAsync();

            var dtos = categories.Select(c => new CategoryDTO
            {
                Id = c.Id,
                Name = c.Name
            }).ToList();

            return Result<List<CategoryDTO>>.Success(dtos);
        }

        public async Task<Result<CategoryDTO>> UpdateNameAsync(Guid id, string name)
        {
            var category = await _categoryRepository.GetByIdAsync(id);

            if (category is null)
                return Result<CategoryDTO>.Failure("Category not found");

            category.Name = name;

            await _categoryRepository.UpdateAsync(category);

            var dto = new CategoryDTO
            {
                Id = category.Id,
                Name = category.Name
            };

            return Result<CategoryDTO>.Success(dto);
        }
    }
}
