using AutoMapper;
using DTOs;
using Models;

public class MappingProfile : Profile
{
    public MappingProfile()
    {

        CreateMap<Request, RequestDto>();
        CreateMap<CreateRequestDto, Request>()
            .ForMember(dest => dest.FileId, opt => opt.Ignore());

        CreateMap<Report, ReportDto>();
        CreateMap<CreateReportDto, Report>();

        CreateMap<User, UserDto>();
        CreateMap<CreateUserDto, User>();
        CreateMap<UserDto, OutUserDto>();
        CreateMap<User, UpdateUserDto>();
        CreateMap<User, OutUserDto>()
            .ForMember(dest => dest.ProfilePhotoId, opt => opt.MapFrom(src => src.ProfilePhotoId));


        CreateMap<ReqCategory, ReqCategoryDto>();
        CreateMap<CreateCategoryDto, ReqCategory>();

        CreateMap<ReqPriority, ReqPriorityDto>();

        CreateMap<ReqType, ReqTypeDto>();

        CreateMap<UserRole, UserRoleDto>();
        CreateMap<Role, RoleDto>();



        CreateMap<ReqStatus, ReqStatusDto>();
        CreateMap<ResStatus, ResStatusDto>();


        CreateMap<Response, ResponseDto>();
        CreateMap<CreateResponseDto, Response>();
        CreateMap<Response, OutResponseDto>()
            .ForMember(dest => dest.Text, opt => opt.MapFrom(src => src.Text))
            .ForMember(dest => dest.Username, opt => opt.MapFrom(src => src.User != null ? src.User.Name : null))
            .ForMember(dest => dest.Usersurname, opt => opt.MapFrom(src => src.User != null ? src.User.Surname : null))
            .ForMember(dest => dest.ResStatusName, opt => opt.MapFrom(src => src.ResStatus != null ? src.ResStatus.Name : null));

        CreateMap<Comment, CommentDto>();
        CreateMap<CreateCommentDto, Comment>();


    }
}


