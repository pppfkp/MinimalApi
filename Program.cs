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


static async Task<IResult> GetTodoItemsAsync(TodoContext db) => TypedResults.Ok(await db.TodoItems.ToListAsync());
static async Task<IResult> GetTodoItemAsync(TodoContext db, int id)
{
    return await db.TodoItems.FindAsync(id) switch
    {
        TodoItem todoItem => TypedResults.Ok(todoItem),
        null => TypedResults.NotFound()
    };
};
static async Task<IResult> PostTodoItemAsync(TodoContext db, TodoItem todoItem)
{
    db.TodoItems.Add(todoItem);
    await db.SaveChangesAsync();
    return TypedResults.Created($"/todoitems/{todoItem.Id}", todoItem);
}
static async Task<IResult> GetCompleteTodoItemsAsync(TodoContext db) => TypedResults.Ok(await db.TodoItems.Where(todoItem => todoItem.IsComplete).ToListAsync());
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
static async Task<IResult> PutTodoItemAsync(TodoContext db, int id, TodoItem todoItem)
{
    var todo = await db.TodoItems.FindAsync(id);
    if (todo is null)
    {
        return TypedResults.NotFound();
    }
    if(todoItem is not null)
    {
        todo.Name = todoItem.Name;
        todo.IsComplete = todoItem.IsComplete;
        await db.SaveChangesAsync();
        return TypedResults.NoContent();
    } else 
    {
        return TypedResults.BadRequest();
    }
}
app.Run();
