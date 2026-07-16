using MeuCrudCsharp.Data;
using MeuCrudCsharp.Features.Courses.Domain.Entities;
using MeuCrudCsharp.Features.Courses.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MeuCrudCsharp.Features.Courses.Infrastructure.Persistence.Repositories
{
    public class CourseRepository : ICourseRepository
    {
        private readonly ApplicationDbContext _context;

        public CourseRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Course?> GetByPublicIdAsync(Guid publicId)
        {
            return await _context.Courses.FirstOrDefaultAsync(c => c.Id == publicId);
        }

        public async Task<Course?> GetByPublicIdWithModulesAsync(Guid publicId)
        {
            return await _context.Courses
                .Include(c => c.Modules)
                    .ThenInclude(m => m.Lessons)
                .FirstOrDefaultAsync(c => c.Id == publicId);
        }

        public async Task<Course?> GetByNameAsync(string name)
        {
            return await _context.Courses.FirstOrDefaultAsync(c => c.Name.ToLower() == name.ToLower());
        }

        public async Task<IEnumerable<Course>> SearchByNameAsync(string name)
        {
            return await _context.Courses
                .Where(c => c.Name.ToLower().Contains(name.ToLower()))
                .ToListAsync();
        }

        public async Task<bool> ExistsByNameAsync(string name)
        {
            return await _context.Courses.AnyAsync(c => c.Name.ToLower() == name.ToLower());
        }

        public async Task<(IEnumerable<Course> Items, int TotalCount)> GetPaginatedWithModulesAsync(
            int pageNumber,
            int pageSize,
            string? name = null,
            bool onlyPublished = false
        )
        {
            var query = _context.Courses.AsQueryable();

            if (!string.IsNullOrWhiteSpace(name))
            {
                query = query.Where(c => c.Name.ToLower().Contains(name.ToLower()));
            }
            if (onlyPublished)
            {
                query = query.Where(c => c.IsPublished);
            }

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderBy(c => c.Name)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }

        public async Task AddAsync(Course course)
        {
            await _context.Courses.AddAsync(course);
            await _context.SaveChangesAsync();
        }

        public void Update(Course course)
        {
            _context.Courses.Update(course);
        }

        public void Delete(Course course)
        {
            _context.Courses.Remove(course);
        }
    }
}
