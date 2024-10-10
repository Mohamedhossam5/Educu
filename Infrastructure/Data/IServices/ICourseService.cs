using System.Collections;
using Core.Entities;
using Infrastructure.Base;
using Infrastructure.Data.Models;
using Infrastructure.Dtos;

namespace Infrastructure.Data.IServices;

public interface ICourseService
{
    Task<Course> GetCourseByIdAsync(Guid Id);

    Task<IList<CourseCardDto>> GetPopularCoursesPaged();

    Task<IEnumerable<Course>> GetCoursesByCategory(ISpecification<Course> spec);

    // Task<Course> AddCourse(AddCourseModel model);
    
}

