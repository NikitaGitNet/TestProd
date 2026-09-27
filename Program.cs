using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Serialization;
using TestProd.Models;
using TestProd.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.WriteIndented = true;
        options.JsonSerializerOptions.Converters.Add(
            new JsonStringEnumConverter());
    });

builder.Services.AddValidatorsFromAssemblyContaining<InputModelValidator>();
builder.Services.AddScoped<IParserService, ParserService>();
builder.Services.AddScoped<IElementRepository, ElementRepository>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        return new BadRequestObjectResult(new ResponseModel
        {
            IsError = 1,
            ErrorCode = ErrorCode.ValidationError,
            ErrorMessage = string
            .Join(
                "\n",
                context.ModelState.Values
                .SelectMany(x => x.Errors)
                .Select(x => x.ErrorMessage)
            )
        });
    };
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.RoutePrefix = "api/swagger";
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "TestProd API v1");
});

// app.UseAuthorization();

app.MapControllers();

app.Run();
