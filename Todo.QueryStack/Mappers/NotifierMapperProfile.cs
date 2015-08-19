using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
