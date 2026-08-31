using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CategoryModel = API.Models.Category;

namespace API.Interfaces.Category
{
    public interface ICategoryRepository
    {
        Task<CategoryModel?> GetByIdAsync(Guid id);
        Task<CategoryModel?> GetByName(string name);
        Task<List<CategoryModel>> GetAllAsync();
        Task<CategoryModel> AddAsync(CategoryModel category);
        Task UpdateAsync(CategoryModel category);
        Task DeleteAsync(Guid id);
    }
}