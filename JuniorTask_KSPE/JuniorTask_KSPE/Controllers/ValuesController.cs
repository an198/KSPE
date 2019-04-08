using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace JuniorTask_KSPE.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        // GET api/values

        [HttpGet]
        public ActionResult <string> Quadratic(double a = 0, double b = 0, double c = 0)
        {
            double x1, x2;
            double d = b*b - 4*a*c;

            if ((a == 0) && (b == 0) && (c == 0)) return "No solution";

            if (d > 0)
            {
                x1 = (-b + Math.Sqrt(d)) / (2 * a);
                x2 = (-b - Math.Sqrt(d)) / (2 * a);
                return "X1 = " + x1 + ";   X2 = " + x2;
            }

            if (d == 0)
            {
                x1 = -b / (2 * a);
                return "X = " + x1;
            }

            return "No solution";
        }
    }
}