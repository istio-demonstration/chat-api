using System.Linq;
using API.DTOs.Account;
using API.DTOs.Member;
using API.DTOs.Message;
using API.DTOs.Photo;
using API.Entities;
using API.Extensions;
using Mapster;

namespace API.Helper
{
    public static class MapsterProfile
    {
        public static TypeAdapterConfig GetConfiguredMappingConfig()
        {
            var config = new TypeAdapterConfig
            {
                //Compiler = exp => exp.CompileWithDebugInfo(new ExpressionCompilationOptions { EmitFile = true, ThrowOnFailedCompilation = true })
            };

            //config.NewConfig<Enrollment, EnrollmentDto>()
            //    .AfterMappingAsync(async dto =>
            //    {
            //        var context = MapContext.Current.GetService<SchoolContext>();
            //        var course = await context.Courses.FindAsync(dto.CourseID);
            //        if (course != null)
            //            dto.CourseTitle = course.Title;
            //        var student = await context.Students.FindAsync(dto.StudentID);
            //        if (student != null)
            //            dto.StudentName = MapContext.Current.GetService<NameFormatter>().Format(student.FirstMidName, student.LastName);
            //    });
            //config.NewConfig<Student, StudentDto>()
            //    .Map(dest => dest.Name, src => MapContext.Current.GetService<NameFormatter>().Format(src.FirstMidName, src.LastName));
            //config.NewConfig<Course, CourseDto>()
            //    .Map(dest => dest.CourseIDDto, src => src.CourseID)
            //    .Map(dest => dest.CreditsDto, src => src.Credits)
            //    .Map(dest => dest.TitleDto, src => src.Title)
            //.Map(dest => dest.EnrollmentsDto, src => src.Enrollments);
            config.NewConfig<AppUser, MemberToReturnDto>()
                .Map(desc=> desc.MainPhotoUrl, src => src.Photos.Any(x => x.IsMain) ? src.Photos.FirstOrDefault(x => x.IsMain).Url : "")
                .Map(desc => desc.Age, src => src.DateOfBirth.CalculateAge());
            config.NewConfig<Photo, PhotoDto>();
            config.NewConfig<MemberUpdateDto, AppUser>();
            config.NewConfig<RegisterDto, AppUser>();
            config.NewConfig<Message, MessageDto>()
                .Map(dest => dest.SenderMainPhotoUrl, src => src.Sender.Photos.Any(x => x.IsMain) ? src.Sender.Photos.FirstOrDefault(x => x.IsMain).Url : null)
                .Map(dest => dest.RecipientMainPhotoUrl, src => src.Recipient.Photos.Any(x => x.IsMain) ? src.Recipient.Photos.FirstOrDefault(x => x.IsMain).Url : null);
            return config;
        }
    }
}
