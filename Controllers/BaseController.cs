using System.Net.Mime;

using Microsoft.AspNetCore.Mvc;

namespace QuestionApi.Controllers;

[ApiController]
[Route("[controller]")]
[Produces(MediaTypeNames.Application.Json)]
public class BaseController : ControllerBase {
}