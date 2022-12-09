using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<TodoContext>(options => options.UseInMemoryDatabase("TodoList"));
var app = builder.Build();

var todoItemsGroup = app.MapGroup("/todoitems");
todoItemsGroup.MapGet("/", GetTodoItemsAsync);
todoItemsGroup.MapGet("/{id}", GetTodoItemAsync);
todoItemsGroup.MapPost("/", PostTodoItemAsync);
todoItemsGroup.MapGet("/complete", GetCompleteTodoItemsAsync);
todoItemsGroup.MapDelete("/{id}", DeleteTodoItemAsync);
todoItemsGroup.MapPut("/{id}", PutTodoItemAsync);


static async Task<IResult> GetTodoItemsAsync(TodoContext db) => TypedResults.Ok(await db.TodoItems.Select(td => new TodoItemDTO(td)).ToListAsync());
static async Task<IResult> GetTodoItemAsync(TodoContext db, int id)
{
    return await db.TodoItems.FindAsync(id) switch
    {
        TodoItem todoItem => TypedResults.Ok(new TodoItemDTO(todoItem)),
        null => TypedResults.NotFound()
    };
};
static async Task<IResult> PostTodoItemAsync(TodoContext db, TodoItemDTO todoItemDTO)
{
    var todoItem = new TodoItem { Name = todoItemDTO.Name, IsComplete = todoItemDTO.IsComplete };
    db.TodoItems.Add(todoItem);
    await db.SaveChangesAsync();
    return TypedResults.Created($"/todoitems/{todoItem.Id}", todoItem);
}
static async Task<IResult> GetCompleteTodoItemsAsync(TodoContext db) => TypedResults.Ok(await db.TodoItems.Where(todoItem => todoItem.IsComplete).Select(td => new TodoItemDTO(td)).ToListAsync());
static async Task<IResult> DeleteTodoItemAsync(TodoContext db, int id)
{
    var todoItem = await db.TodoItems.FindAsync(id);
    if (todoItem is null)
    {
        return TypedResults.NotFound();
    }
    db.TodoItems.Remove(todoItem);
    await db.SaveChangesAsync();
    return TypedResults.NoContent();
}
static async Task<IResult> PutTodoItemAsync(TodoContext db, int id, TodoItemDTO todoItemDTO)
{
    var todo = await db.TodoItems.FindAsync(id);
    if (todo is null)
    {
        return TypedResults.NotFound();
    }
    if(todoItemDTO is not null)
    {
        todo.Name = todoItemDTO.Name;
        todo.IsComplete = todoItemDTO.IsComplete;
        await db.SaveChangesAsync();
        return TypedResults.NoContent();
    } else 
    {
        return TypedResults.BadRequest();
    }
}
app.Run();
