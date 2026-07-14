using MeuCrudCsharp.Data;
using MeuCrudCsharp.Features.Courses.Domain.Entities;
using MeuCrudCsharp.Features.Courses.Domain.Interfaces;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace MeuCrudCsharp.Features.Courses.Infrastructure.Persistence.Repositories
{
    public class CourseRepository : ICourseRepository
    {
        private readonly IMongoCollection<Course> _courses;

        public CourseRepository(IMongoDbContext context)
        {
            _courses = context.GetCollection<Course>("courses");
        }

        public async Task<Course?> GetByPublicIdAsync(Guid publicId)
        {
            return await _courses.Find(c => c.PublicId == publicId).FirstOrDefaultAsync();
        }

        public async Task<Course?> GetByPublicIdWithVideosAsync(Guid publicId)
        {
            return await GetByPublicIdAsync(publicId);
        }

        public async Task<Course?> GetByNameAsync(string name)
        {
            return await _courses.Find(c => c.Name.ToLower() == name.ToLower()).FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<Course>> SearchByNameAsync(string name)
        {
            return await _courses.Find(c => c.Name.ToLower().Contains(name.ToLower())).ToListAsync();
        }

        public async Task<bool> ExistsByNameAsync(string name)
        {
            var count = await _courses.CountDocumentsAsync(c => c.Name.ToLower() == name.ToLower());
            return count > 0;
        }

        public async Task<(IEnumerable<Course> Items, int TotalCount)> GetPaginatedWithVideosAsync(
            int pageNumber,
            int pageSize,
            string? name = null
        )
        {
            var filter = string.IsNullOrWhiteSpace(name) 
                ? FilterDefinition<Course>.Empty 
                : Builders<Course>.Filter.Regex(c => c.Name, new MongoDB.Bson.BsonRegularExpression(name, "i"));

            var totalCount = (int)await _courses.CountDocumentsAsync(filter);

            var items = await _courses.Find(filter)
                .SortBy(c => c.Name)
                .Skip((pageNumber - 1) * pageSize)
                .Limit(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }

        public async Task AddAsync(Course course)
        {
            await _courses.InsertOneAsync(course);
        }

        public void Update(Course course)
        {
            _courses.ReplaceOne(x => x.PublicId == course.PublicId, course);
        }

        public void Delete(Course course)
        {
            _courses.DeleteOne(x => x.PublicId == course.PublicId);
        }
    }
}
