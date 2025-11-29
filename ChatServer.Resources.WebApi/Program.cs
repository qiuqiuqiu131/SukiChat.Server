var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                     .AddEnvironmentVariables();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();

builder.WebHost.ConfigureKestrel(options => {
    options.Limits.MaxRequestBodySize = 524288000; // 500 MB
});

builder.Services.AddCors(cor =>
{
    var cors = builder.Configuration.GetSection("CorsUrls").GetChildren().Select(p => p.Value);
    cor.AddPolicy("Cors", policy =>
    {
        policy.WithOrigins(cors.ToArray())//设置允许的请求头
        .WithExposedHeaders("x-custom-header")//设置公开的响应头
        .AllowAnyHeader()//允许所有请求头
        .AllowAnyMethod()//允许任何方法
        .AllowCredentials()//允许跨源凭据----服务器必须允许凭据
        .SetIsOriginAllowed(_ => true);
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI();

app.UseCors("Cors");
app.UseAuthorization();

app.MapControllers();

app.Run();
