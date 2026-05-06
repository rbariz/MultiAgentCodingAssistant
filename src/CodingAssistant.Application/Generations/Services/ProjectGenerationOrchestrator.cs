using CodingAssistant.Application.Abstractions;
using CodingAssistant.Domain.Entities;
using CodingAssistant.Domain.Enums;

namespace CodingAssistant.Application.Generations.Services
{
    public sealed class ProjectGenerationOrchestrator : IProjectGenerationOrchestrator
    {
        private readonly IProjectGenerationRepository _repository;
        private readonly IUnitOfWork _unitOfWork;

        public ProjectGenerationOrchestrator(
            IProjectGenerationRepository repository,
            IUnitOfWork unitOfWork)
        {
            _repository = repository;
            _unitOfWork = unitOfWork;
        }

        public async Task RunAsync(Guid generationId, CancellationToken cancellationToken = default)
        {
            var generation = await _repository.GetByIdAsync(generationId, cancellationToken);

            if (generation is null)
                return;

            generation.Status = GenerationStatus.Generating;
            generation.StartedAtUtc = DateTime.UtcNow;

            CompleteStep(generation, AgentRole.Planner);
            AddMessage(generation, AgentRole.Planner, "Project plan created.");

            CompleteStep(generation, AgentRole.Architect);
            AddMessage(generation, AgentRole.Architect, "Project structure selected: index.html, style.css, script.js.");

            CompleteStep(generation, AgentRole.Developer);
            AddMessage(generation, AgentRole.Developer, "Source files generated.");

            generation.ProjectName = BuildProjectName(generation.UserPrompt);

            var files = CreateCalculatorFiles(generation.Id);

            foreach (var file in files)
            {
                await _repository.AddFileAsync(file, cancellationToken);
            }

            CompleteStep(generation, AgentRole.Reviewer);
            AddMessage(generation, AgentRole.Reviewer, "Generated project reviewed successfully.");

            generation.Status = GenerationStatus.Completed;
            generation.CompletedAtUtc = DateTime.UtcNow;

            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        private static void CompleteStep(ProjectGeneration generation, AgentRole role)
        {
            var step = generation.Steps.FirstOrDefault(x => x.AgentRole == role);

            if (step is null)
                return;

            step.Status = StepStatus.Completed;
            step.StartedAtUtc ??= DateTime.UtcNow;
            step.CompletedAtUtc = DateTime.UtcNow;
        }

        private static void AddMessage(ProjectGeneration generation, AgentRole role, string content)
        {
            generation.Messages.Add(new AgentMessage
            {
                ProjectGenerationId = generation.Id,
                Role = role,
                Content = content
            });
        }

        private static string BuildProjectName(string prompt)
        {
            if (prompt.Contains("calculator", StringComparison.OrdinalIgnoreCase))
                return "calculator-app";

            return "generated-app";
        }

        private static List<GeneratedFile> CreateCalculatorFiles(Guid generationId)
        {
            return
            [
                new GeneratedFile
            {
                ProjectGenerationId = generationId,
                RelativePath = "index.html",
                FileName = "index.html",
                Language = "html",
                Kind = GeneratedFileKind.SourceCode,
                Order = 1,
                Content = """
                <!DOCTYPE html>
                <html lang="en">
                <head>
                    <meta charset="UTF-8">
                    <meta name="viewport" content="width=device-width, initial-scale=1.0">
                    <title>Calculator App</title>
                    <link rel="stylesheet" href="style.css">
                </head>
                <body>
                    <main class="calculator">
                        <h1>Calculator</h1>

                        <input id="firstNumber" type="number" placeholder="First number">
                        <input id="secondNumber" type="number" placeholder="Second number">

                        <div class="buttons">
                            <button onclick="calculate('add')">Add</button>
                            <button onclick="calculate('subtract')">Subtract</button>
                            <button onclick="calculate('multiply')">Multiply</button>
                            <button onclick="calculate('divide')">Divide</button>
                        </div>

                        <p id="result">Result: -</p>
                    </main>

                    <script src="script.js"></script>
                </body>
                </html>
                """
            },
            new GeneratedFile
            {
                ProjectGenerationId = generationId,
                RelativePath = "style.css",
                FileName = "style.css",
                Language = "css",
                Kind = GeneratedFileKind.SourceCode,
                Order = 2,
                Content = """
                * {
                    box-sizing: border-box;
                }

                body {
                    margin: 0;
                    min-height: 100vh;
                    display: flex;
                    align-items: center;
                    justify-content: center;
                    font-family: Arial, sans-serif;
                    background: #f3f4f6;
                }

                .calculator {
                    width: 100%;
                    max-width: 360px;
                    padding: 24px;
                    border-radius: 16px;
                    background: white;
                    box-shadow: 0 10px 30px rgba(0, 0, 0, 0.08);
                }

                h1 {
                    margin-top: 0;
                    text-align: center;
                }

                input {
                    width: 100%;
                    padding: 12px;
                    margin-bottom: 12px;
                    border: 1px solid #d1d5db;
                    border-radius: 8px;
                }

                .buttons {
                    display: grid;
                    grid-template-columns: 1fr 1fr;
                    gap: 10px;
                }

                button {
                    padding: 12px;
                    border: none;
                    border-radius: 8px;
                    cursor: pointer;
                    background: #2563eb;
                    color: white;
                    font-weight: 600;
                }

                button:hover {
                    background: #1d4ed8;
                }

                #result {
                    margin-bottom: 0;
                    font-weight: 700;
                    text-align: center;
                }
                """
            },
            new GeneratedFile
            {
                ProjectGenerationId = generationId,
                RelativePath = "script.js",
                FileName = "script.js",
                Language = "javascript",
                Kind = GeneratedFileKind.SourceCode,
                Order = 3,
                Content = """
                function calculate(operation) {
                    const firstNumber = Number(document.getElementById("firstNumber").value);
                    const secondNumber = Number(document.getElementById("secondNumber").value);
                    const resultElement = document.getElementById("result");

                    let result;

                    switch (operation) {
                        case "add":
                            result = firstNumber + secondNumber;
                            break;
                        case "subtract":
                            result = firstNumber - secondNumber;
                            break;
                        case "multiply":
                            result = firstNumber * secondNumber;
                            break;
                        case "divide":
                            if (secondNumber === 0) {
                                resultElement.textContent = "Result: Cannot divide by zero";
                                return;
                            }

                            result = firstNumber / secondNumber;
                            break;
                        default:
                            resultElement.textContent = "Result: Unknown operation";
                            return;
                    }

                    resultElement.textContent = `Result: ${result}`;
                }
                """
            }
            ];
        }
    }
}
