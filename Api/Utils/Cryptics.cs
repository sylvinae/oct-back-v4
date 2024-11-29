using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Data.Entities.User;
using Microsoft.IdentityModel.Tokens;

namespace API.Utils;

public class Cryptics
{
    public static string ComputeHash(object itemDto)
    {
        var propertiesToInclude = new[]
        {
            "Barcode",
            "Brand",
            "Generic",
            "Classification",
            "Formulation",
            "Location",
            "Wholesale",
            "Retail",
            "Stock",
            "LowThreshold",
            "Company",
            "HasExpiry",
            "Expiry",
            "IsReagent",
            "UsesMax",
            "IsExpired",
        };

        var propertyValues = itemDto
            .GetType()
            .GetProperties()
            .Where(p => propertiesToInclude.Contains(p.Name))
            .Select(p => p.GetValue(itemDto)?.ToString() ?? string.Empty);

        var input = string.Join("-", propertyValues);
        var hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
    }
}
