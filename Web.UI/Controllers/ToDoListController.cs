using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.ModelBinding;
using Todo.Infrastructure;
using Todo.QueryStack.Model;
using Web.UI.Models.TodoList;
using Web.UI.Worker;

namespace Web.UI.Controllers
{
    public class ToDoListController : ApiController
    {
        private readonly ToDoWorker Worker;

        public ToDoListController(ToDoWorker worker)
        {
            Contract.Requires<System.ArgumentNullException>(worker != null, "worker");
            Worker = worker;
        }

        ///////////////////////
        /// TODO -LISTS
        ///////////////////////
        [Route("api/TodoList/List")]
        [HttpGet]
        public Task<List<ToDoList>> List()
        {
            return Worker.GetLists();
        }

        [Route("api/TodoList/ChangeDescription")]
        [HttpPost]
        public IHttpActionResult ChangeDescription(ChangeTodoListDescriptionCommandModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                Worker.ChangeToDoListDescription(model);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Route("api/TodoList/CreateNewList")]
        [HttpPost]
        public IHttpActionResult CreateNewList(CreateTodoListCommandModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                Worker.CreateToDoList(model);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // PUT api/todolist/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/todolist/5
        public void Delete(int id)
        {
        }

        ///////////////////////
        /// TODO -ITEMS
        ///////////////////////
        [Route("api/TodoList/Items/{Id}")]
        [HttpGet]
        public Task<ToDoList> Items(string Id)
        {
            return Worker.GetListItems(Id);
        }

        [Route("api/TodoList/Items/{Id}/Add")]
        [HttpPost]
        public IHttpActionResult AddItemToList(AddNewToDoItemCommandModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                Worker.AddNewToDoItem(model);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Route("api/TodoItems/MarkAsComplete")]
        [HttpPost]
        public IHttpActionResult MarkToDoItemAsComplete(MarkToDoItemAsCompleteModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                Worker.MarkToDoItemAsComplete(model);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Route("api/TodoItems/ReOpen")]
        [HttpPost]
        public IHttpActionResult ReOpenToDoItem(ReOpenToDoItemModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                Worker.ReOpenToDoItem(model);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        [Route("api/TodoItems/ChangeDescription")]
        [HttpPost]
        public IHttpActionResult ChangeDescription(ChangeToDoItemDescriptionModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                Worker.ChangeDescription(model);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Route("api/TodoItems/ChangeImportance")]
        [HttpPost]
        public IHttpActionResult ReOpenToDoItem(ChangeToDoItemImportanceModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                Worker.ChangeImportance(model);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Route("api/TodoItems/ChangeDueDate")]
        [HttpPost]
        public IHttpActionResult ChangeDueDate(ChangeToDoItemDueDateModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                Worker.ChangeDueDate(model);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        
    }
}
