using AutoMapper;
using Todo.QueryStack.Model;

namespace Todo.QueryStack.Mappers
{
    class NotifierMapperProfile : Profile
    {
        protected override void Configure()
        {
            CreateMap<ToDoList, NotifiedToDoList>();
            CreateMap<ToDoItem, NotifiedToDoItem>();
        }
    }
}
