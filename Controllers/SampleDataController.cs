using AydinWyldePortfolioX.Models;
using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using DXApplication3.Models;
using Microsoft.AspNetCore.Mvc;

namespace AydinWyldePortfolioX.Controllers
{
    [Route("api/[controller]")]
    public class SampleDataController : Controller
    {

        [HttpGet]
        public object Get(DataSourceLoadOptions loadOptions)
        {
            return DataSourceLoader.Load(SampleData.Orders, loadOptions);
        }

    }
}