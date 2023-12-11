using Microsoft.AspNetCore.Mvc;

namespace QuestionApi.Controllers;

[ApiController]
[Route("[controller]")]
public class QuestionController : ControllerBase {
    private readonly ILogger<QuestionController> _logger;

    public QuestionController(ILogger<QuestionController> logger) {
        _logger = logger;
    }
}