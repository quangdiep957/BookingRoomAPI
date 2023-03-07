using Microsoft.AspNetCore.Builder;
using RoomBooking.Common.Exception;
using RoomBooking.DAL.Interfaces;
using RoomBooking.DAL.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers().AddNewtonsoftJson(
    option =>
    {
        option.UseMemberCasing();
    }
    );
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
//Tr??c build
//X? lý v? DI - Dependency Injection:
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IDepartmentRepository, DepartmentRepository>();
builder.Services.AddScoped<IJobPositionRepository, JobPositionRepository>();
builder.Services.AddScoped<IRoleRepository, RoleRepository>();
builder.Services.AddScoped<IRoomRepository, RoomRepository>();
builder.Services.AddScoped<IBuildingRepository, BuildingRepository>();
builder.Services.AddScoped<IEquipmentRepository, EquipmentRepository>();
builder.Services.AddScoped<ISemesterRepository, SemesterRepository>();
//builder.Services.AddScoped<IUserService, UserService>();
//builder.Services.AddScoped<IRoleService, RoleService>();
//builder.Services.AddScoped<IDepartmentService, DepartmentService>();
////builder.Services.AddScoped<IJobPositionService, JobPositionService>();
builder.Services.AddScoped(typeof(IBaseRepository<>), typeof(BaseRepository<>));
//builder.Services.AddScoped(typeof(IBaseService<>), typeof(BaseService<>));

builder.Services.AddControllers(options =>
{
    options.Filters.Add<HttpResponseExceptionFilter>();
});

builder.Services.AddCors(o =>
{
    o.AddPolicy("CorsPolicy", b =>
    {
        b.AllowAnyMethod().AllowAnyOrigin().AllowAnyHeader().Build();
    }
    );
}
);
var app = builder.Build();


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();
app.UseCors("CorsPolicy");
app.Run();
