using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.Common;
using API.DTO.Category;

namespace API.Interfaces.Category
{
    public interface ICategoryService
    {
        Task<Result<CategoryDTO>> CreateAsync(CreateCategoryDTO createCategoryDTO);
        Task<Result<CategoryDTO?>> GetByIdAsync(Guid id);
        Task<Result<List<CategoryDTO>>> GetAllAsync();
        Task<Result<CategoryDTO>> UpdateNameAsync(Guid id, string name);
    }
}