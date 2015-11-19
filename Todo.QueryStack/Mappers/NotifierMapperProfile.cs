using AutoMapper;
using Todo.QueryStack.Model;

namespace Todo.QueryStack.Mappers
{
    class NotifierMapperProfile : Profile
    {
        protected override void Configure()
        {
            CreateMap<ToDoList, NotifiedToDoList>()
                .ForMember(dest => dest.Items, opt => opt.Ignore());
            CreateMap<ToDoItem, NotifiedToDoItem>();
        }
    }
}
