using Accounting;
using Companies;
using Scalar.AspNetCore;
using Users;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddUsersModule(builder.Configuration);
builder.Services.AddCompaniesModule(builder.Configuration);
builder.Services.AddAccountingModule(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();

app.Run();

public partial class Program;
