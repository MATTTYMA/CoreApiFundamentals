using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using CoreCodeCamp.Controllers;
using CoreCodeCamp.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.AspNetCore.Mvc.Versioning.Conventions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CoreCodeCamp
{
  public class Startup
  {
    public void ConfigureServices(IServiceCollection services)
    {
      services.AddDbContext<CampContext>();
      services.AddScoped<ICampRepository, CampRepository>();
      services.AddAutoMapper(Assembly.GetExecutingAssembly());
      services.AddApiVersioning(opt =>
      {
          opt.AssumeDefaultVersionWhenUnspecified = true;
          opt.DefaultApiVersion = new ApiVersion(1, 1);
          opt.ReportApiVersions = true;
          //Enable following line if query string versioning
          //opt.ApiVersionReader = new QueryStringApiVersionReader("ver");
          //Enable following line if Header versioning
          //opt.ApiVersionReader = new HeaderApiVersionReader("X-Version");
          //Enable following line if combine 
          opt.ApiVersionReader = ApiVersionReader.Combine(
              new HeaderApiVersionReader("X-Version"),
              new QueryStringApiVersionReader("ver")
              );
          //Enable Following line if url versioning and you will to need to change all in controller
          //opt.ApiVersionReader = new UrlSegmentApiVersionReader();
         /** opt.Conventions.Controller<TalksController>()
          .HasApiVersion(new ApiVersion(1, 1))
          .HasApiVersion(new ApiVersion(1, 0))
          .Action(c => c.Delete(default(string), default(int)))
          .MaptoApiVersion(1,1);**/

      });
      services.AddControllers();
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
      if (env.IsDevelopment())
      {
        app.UseDeveloperExceptionPage();
      }

      app.UseRouting();

      app.UseAuthentication();
      app.UseAuthorization();

      app.UseEndpoints(cfg =>
      {
        cfg.MapControllers();
      });
    }
  }
}
