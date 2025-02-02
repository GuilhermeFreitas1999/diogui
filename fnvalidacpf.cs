using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace httpValidaCpf
{
    public static class fnvalidacpf
    {
        [FunctionName("fnvalidacpf")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("Iniciando a validação do CPF.");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            var cpf = data?.cpf;

            if (cpf == null)
            {
                return new BadRequestObjectResult("CPF não fornecido.");
            }

            string validationMessage = ValidaCPF(cpf.ToString());

            if (validationMessage == "CPF válido")
            {
                return new OkObjectResult(validationMessage);
            }
            else
            {
                return new BadRequestObjectResult(validationMessage);
            }
        }

        public static string ValidaCPF(string cpf)
        {
            // Remove non-numeric characters
            cpf = new string(cpf.Where(char.IsDigit).ToArray());

            // Check if the length is valid
            if (cpf.Length != 11)
                return "CPF inválido: a quantidade de dígitos está incorreta.";

            // Check if the CPF is a sequence of the same digits
            if (cpf.All(c => c == cpf[0]))
                return "CPF inválido: CPF com todos os dígitos iguais.";

            // Calculate and validate the first digit
            int sum1 = 0;
            for (int i = 0; i < 9; i++)
            {
                sum1 += (cpf[i] - '0') * (10 - i);
            }
            int digit1 = (sum1 * 10) % 11;
            if (digit1 == 10 || digit1 == 11)
                digit1 = 0;

            if (cpf[9] - '0' != digit1)
                return "CPF inválido: o primeiro dígito verificador está incorreto.";

            // Calculate and validate the second digit
            int sum2 = 0;
            for (int i = 0; i < 10; i++)
            {
                sum2 += (cpf[i] - '0') * (11 - i);
            }
            int digit2 = (sum2 * 10) % 11;
            if (digit2 == 10 || digit2 == 11)
                digit2 = 0;

            if (cpf[10] - '0' != digit2)
                return "CPF inválido: o segundo dígito verificador está incorreto.";

            return "CPF válido";
        }
    }
}
