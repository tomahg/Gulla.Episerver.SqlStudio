using System.Collections.Generic;
using System.Linq;
using EPiServer.Data;
using Gulla.Episerver.SqlStudio.Configuration;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Options;

namespace Gulla.Episerver.SqlStudio.Controllers
{
    public abstract class BaseSqlController : Controller
    {
        protected readonly DataAccessOptions _dataAccessOptions;
        protected readonly SqlStudioOptions _configuration;

        public BaseSqlController(DataAccessOptions dataAccessOptions, IOptions<SqlStudioOptions> options)
        {
            _dataAccessOptions = dataAccessOptions;
            _configuration = options.Value;
        }

        protected List<SelectListItem> GetConnectionStringList(DataAccessOptions dataAccessOptions, SqlStudioOptions configuration)
        {
            if (!string.IsNullOrEmpty(configuration.ConnectionString))
            {
                return
                [
                    new SelectListItem
                    {
                        Text = "Default",
                        Value = configuration.ConnectionString
                    }
                ];
            }
            else
            {
                return dataAccessOptions.ConnectionStrings
                    .DistinctBy(x => x.Name)
                    .OrderByDescending(x => x.Name == _dataAccessOptions.DefaultConnectionStringName)
                    .Select(x => new SelectListItem { Text = x.Name, Value = x.ConnectionString }).ToList();
            }
        }
    }
}
